using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace twtichBot
{
	public class foobar2k
    {
		private string url;
		private static readonly HttpClient httpClient = new HttpClient();
		private List<song> _playlist;
		private Library _library;

		public List<song> Playlist { get => _playlist; }
		public Library Library { get => _library; }

		public foobar2k(string IP, string Port)
		{
			url = string.Format("http://{0}:{1}", IP, Port);
			System.Threading.Tasks.Task.Run(async () =>
			{
				_playlist = new List<song>();
				_library = new Library();
				_playlist.AddRange(await GetPlaylist());
				_library.Process(_playlist);
			});

		}

		async public Task<string> getUrlAsync(string path)
		{
			using (HttpResponseMessage res = await httpClient.GetAsync(url + path))
			using (HttpContent content = res.Content)
			{
				string data = await content.ReadAsStringAsync();
				if (data != null)
				{
					
					return data;
				}
				else
				{
					throw new HttpRequestException("No Data");
				}
			}
		}

		async public Task<int> updatePlaylist()
		{
			_playlist.Clear(); //GC? get gud
			_playlist.AddRange(await GetPlaylist());
			_library.Albums.Clear();
			_library.Process(_playlist);
			return _playlist.Count;
		}

		async public Task<string> GetCurrentSong()
		{
			return JsonConvert.DeserializeObject<song>(await getUrlAsync("/nowPlaying/?param3=nowPlaying.json")).ToString();
		}

		async public Task<List<song>> GetPlaylist()
		{
			var playlist = await getUrlAsync("/playlistView/?param3=playlist.json");
			return JsonConvert.DeserializeObject<List<song>>(playlist);
		}

		async public Task<song> queueSong(int index)
		{
			await getUrlAsync("/default/?cmd=QueueItems&param1=" + index.ToString());
			return Playlist.ToArray()[index];
		}

		async public Task<song> queueSong(song Song)
		{
			await getUrlAsync("/default/?cmd=QueueItems&param1=" + Playlist.FindIndex(song=> song.ToString().Equals(Song.ToString())));
			return Song;
		}

		async public void nextSong()
		{
			await getUrlAsync("/default/?cmd=StartNext");
		}

		async public Task<List<song>> searchLibrary(string searchQuery)
		{
			var searchPlaylist = await (getUrlAsync("/playlistView/?cmd=SearchMediaLibrary&param1=" + searchQuery));
			await (getUrlAsync("/default/?cmd=SwitchPlaylist&param1=0"));
			return JsonConvert.DeserializeObject<List<song>>(searchPlaylist);
		}
	}

	public class song
	{
		public string Artist { get; set; }
		public string Track { get; set; }
		public string Album { get; set; }
		public int Index { get; set; }

		public override string ToString()
		{
			return string.Format("{0} -- {1} [{2}]", Artist, Track, Album);
		}

		public bool ShouldSerializeAlbum()
		{
			return false;
		}
	}

	public class Album
	{

		private string title;
		private List<song> songs;
		public string Title { get => title; }
		public List<song> Songs { get => songs; }

		public Album(string Title)
		{
			title = Title;
			songs = new List<song>();
		}

		public Album(string Title, List<song> Songs)
		{
			title = Title;
			songs = Songs;
		}



		public void AddSong(song Song)
		{
			songs.Add(Song);
		}

		public override string ToString()
		{
			return string.Format("[{0}]", title);
		}
	}

	public class Library
	{
		private List<Album> _albums;
		public Library()
		{
			_albums = new List<Album>();
		}

		public List<Album> Albums { get => _albums; }

		public void AddAlbum(Album album)
		{
			Albums.Add(album);
		}

		public string DumpAlbums()
		{
			var logPath = System.IO.Path.GetTempFileName();
			var logFile = System.IO.File.Create(logPath);
			var logWriter = new System.IO.StreamWriter(logFile);
			logWriter.WriteLine(JsonConvert.SerializeObject(Albums));
			logWriter.Dispose();
			return logFile.Name;
		}

		public void Process(List<song> Songs)
		{
			int i = 0;
			foreach (song Song in Songs)
			{
				try
				{
					Song.Index = i;
					Albums.Find(album => album.Title.Equals(Song.Album)).AddSong(Song);
				} catch(NullReferenceException)
				{
					AddAlbum(new Album(Song.Album));
					Albums.Find(album => album.Title.Equals(Song.Album)).AddSong(Song);
				}
				i++;
			}
			Console.WriteLine(@"
███████╗██████╗ ██████╗ ██╗  ██╗
██╔════╝██╔══██╗╚════██╗██║ ██╔╝
█████╗  ██████╔╝ █████╔╝█████╔╝ 
██╔══╝  ██╔══██╗██╔═══╝ ██╔═██╗ 
██║     ██████╔╝███████╗██║  ██╗
╚═╝     ╚═════╝ ╚══════╝╚═╝  ╚═╝
");

		}
	}
}

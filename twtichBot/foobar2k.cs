using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace twtichBot
{
    public class foobar2k
    {
		private string url;
		private static readonly HttpClient httpClient = new HttpClient();
		private List<song> _playlist;

		public List<song> Playlist { get => _playlist; }

		public foobar2k(string IP, string Port)
		{
			url = string.Format("http://{0}:{1}", IP, Port);
			System.Threading.Tasks.Task.Run(async () =>
			{
				_playlist = new List<song>();
				_playlist.AddRange(await GetPlaylist());
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

		async public void updatePlaylist()
		{
			_playlist.Clear(); //GC? get gud
			_playlist.AddRange(await GetPlaylist());
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

		async public Task<song> playSong(int index)
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
#pragma warning disable IDE1006 // Naming Styles
		public string artist { get; set; }
#pragma warning disable IDE1006 // Naming Styles
		public string track { get; set; }
#pragma warning restore IDE1006 // Naming Styles
		public string album { get; set; }

		public override string ToString()
		{
			return string.Format("{0} -- {1} [{2}]", artist, track, album);
		}
	}
}

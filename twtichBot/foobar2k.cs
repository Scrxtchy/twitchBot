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
		private playlist _playlist;

		public playlist Playlist { get => _playlist;}

		public foobar2k(string IP, string Port)
		{
			url = string.Format("http://{0}:{1}", IP, Port);
			System.Threading.Tasks.Task.Run(async () =>
			{
				_playlist = new playlist(await GetPlaylist());
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

		async public Task<string> GetCurrentSong()
		{
			return JsonConvert.DeserializeObject<song>(await getUrlAsync("/nowPlaying/?param3=nowPlaying.json")).ToString();
		}

		async public Task<List<song>> GetPlaylist()
		{
			var result1 = await getUrlAsync("/playlistView/?param3=playlist.json");
			return JsonConvert.DeserializeObject<List<song>>(result1);
		}

		async public Task<song> playSong(int index)
		{
			await getUrlAsync("/default/?cmd=QueueItems&param1=" + index.ToString());
			return Playlist.List.ToArray()[index];
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

	public class playlist
	{
		private List<song> _list;

		public List<song> List { get => _list; }

		public playlist(){}

		public playlist(List<song> Songs)
		{
			_list = Songs;
		}

		public void appendSong(song Song)
		{
			_list.Add(Song);
		}

		public void appendSong(List<song> Songs)
		{
			foreach (var Song in Songs)
			{
				List.Add(Song);
			}
		}
	}
}

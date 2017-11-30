using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib;
using TwitchLib.Events.Client;
using TwitchLib.Models.Client;



namespace twtichBot
{
	class Colligo
	{
		private TwitchClient irc;
		private TwitchAPI api;
		private List<string> channelList = new List<string>();
		private foobar2k fb2k;

		public foobar2k Fb2k { get => fb2k; }

		public Colligo(string username, string oauth, string channel, string clientID, string IP, string port)
		{
			irc = new TwitchClient(new ConnectionCredentials(username, oauth), channel, '!', logging: true);
			api = new TwitchAPI(clientID);
			fb2k = new foobar2k(IP, port);
			channelList.Add(channel);
			irc.OnJoinedChannel += onJoinedChannel;
			irc.OnChatCommandReceived += ChatCommandHandler;
			irc.Connect();
		}

		private void ChatCommandHandler(object sender, OnChatCommandReceivedArgs e)
		{
			System.Threading.Tasks.Task.Run(async () =>
			{
				switch (e.Command.CommandText.ToLower())
				{
					case "song":
						var currentSong = await fb2k.GetCurrentSong();
						irc.SendMessage("Now Playing: " + currentSong);
						break;
					case "random":
						Random rand = new Random();
						int target = rand.Next(Fb2k.Playlist.Count);
						song random = await Fb2k.queueSong(target);
						irc.SendMessage("Queued Random Song: " + random);
						break;
					case "skip":
						Fb2k.nextSong();
						irc.SendMessage("Track Skipped");
						break;
					case "request":
						if (int.TryParse(e.Command.ArgumentsAsString, out int index)) {
							irc.SendMessage("Queued " + await Fb2k.queueSong(index));
						}
						else {

							//song request = await Fb2k.playSong(int.Parse(e.Command.ArgumentsAsString));
							//irc.SendMessage("Requested " + request);
							//Search(e.Command.ArgumentsAsString)
							List<song> requestQuery = await Fb2k.searchLibrary(e.Command.ArgumentsAsString);
							if (requestQuery.Count < 8)
							{
								await Fb2k.queueSong(requestQuery[0]);
								irc.SendMessage("Queued " + requestQuery[0]);
							} else
							{
								irc.SendMessage("Too many results, Nothing queued");
							}
						}
						break;
					case "search":
						var searchQuery = await Fb2k.searchLibrary(e.Command.ArgumentsAsString);
						irc.SendMessage(string.Format("Search returned {0} results", searchQuery.Count));
						break;
					case "uptime":
						//await api.Users.helix.GetUsersAsync(logins: channelList);
						TimeSpan uptime = (TimeSpan) await api.Streams.v5.GetUptimeAsync("90707410"); //TODO: Needs to be made dynamic
						irc.SendMessage(string.Format("Streaming for {0} Hours and {1} Minutes", uptime.Hours, uptime.Minutes));
						break;
					case "dump":
						irc.SendMessage("Dumped to: " + Fb2k.Library.DumpAlbums());
						break;
					case "update":
						irc.SendRaw(string.Format("PRIVMSG #{0} :{2}ACTION playlist's updated with {1} songs{2}", e.Command.ChatMessage.Channel ,await fb2k.updatePlaylist(), '\x01'));
						break;
					case "trello":
						irc.SendMessage("Bot Trello Link: https://trello.com/b/99XsqKc7");
						break;
					case "github":
						irc.SendMessage("Github Link: https://github.com/scrxtchy/twitchBot");
						break;
					case "list":
						irc.SendMessage("Songs List: https://scrxtchy.github.io/twitchBot/?list=QmTNsPd8zVDGuZGM3SD8sZZySZBM2uPM1x3es6eRap8zSB/libraryList.json");
						break;
				}
			});
		}

		private void onJoinedChannel(object sender, OnJoinedChannelArgs e)
		{
			irc.SendRaw(string.Format("PRIVMSG #{0} :{2}ACTION now online with {1} songs{2}", e.Channel, Fb2k.Playlist.Count, '\x01'));
		}

		public void kill()
		{
			irc.Disconnect();
		}
	}
}

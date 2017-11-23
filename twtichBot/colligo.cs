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

						var x = await fb2k.GetCurrentSong();
						irc.SendMessage("Now Playing: " + x);
						break;
					case "random":
						Random rand = new Random();
						int target = rand.Next(Fb2k.Playlist.List.Count);
						song random = await Fb2k.playSong(target);
						irc.SendMessage("Queued Random Song: " + random);
						break;

				}
			});
		}

		private void onJoinedChannel(object sender, OnJoinedChannelArgs e)
		{
			//irc.SendMessage("Kappa");
		}

		public void kill()
		{
			irc.Disconnect();
		}
	}
}

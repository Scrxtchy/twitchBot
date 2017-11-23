using System;
using Microsoft.Extensions.Configuration;



namespace twtichBot
{
	class Program
	{
		static void Main(string[] args)
		{

			Console.WriteLine(@"
 _________  ___       __   ___  _________  ________  ___  ___          ________  ________  _________   
|\___   ___\\  \     |\  \|\  \|\___   ___\\   ____\|\  \|\  \        |\   __  \|\   __  \|\___   ___\ 
\|___ \  \_\ \  \    \ \  \ \  \|___ \  \_\ \  \___|\ \  \\\  \       \ \  \|\ /\ \  \|\  \|___ \  \_| 
     \ \  \ \ \  \  __\ \  \ \  \   \ \  \ \ \  \    \ \   __  \       \ \   __  \ \  \\\  \   \ \  \  
      \ \  \ \ \  \|\__\_\  \ \  \   \ \  \ \ \  \____\ \  \ \  \       \ \  \|\  \ \  \\\  \   \ \  \ 
       \ \__\ \ \____________\ \__\   \ \__\ \ \_______\ \__\ \__\       \ \_______\ \_______\   \ \__\
        \|__|  \|____________|\|__|    \|__|  \|_______|\|__|\|__|        \|_______|\|_______|    \|__|
");

			IConfiguration config = new ConfigurationBuilder().AddJsonFile("config.json", false).Build();

			Colligo twitchIRC = new Colligo(config["irc:username"], config["irc:oauth"], config["irc:channel"], config["api:Client-ID"], config["fb2k:ip"], config["fb2k:port"]);
			Console.ReadLine();
			twitchIRC.kill();
			

		}
	}
}

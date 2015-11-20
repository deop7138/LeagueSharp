using System;

using LeagueSharp;

namespace Cassio_Sharpy
{
    class Initializer
    {
        internal static void Initialize()
        {
            Console.WriteLine("Cassio Sharpy: HelloWorld!");

            MenuProvider.initialize();

            if (PluginLoader.LoadPlugin(ObjectManager.Player.ChampionName))
            {
                MenuProvider.Champion.Drawings.addItem(" ");
            }

            AutoQuit.Load();

            Console.WriteLine("Cassio Sharpy: Initialized.");
        }
    }
}

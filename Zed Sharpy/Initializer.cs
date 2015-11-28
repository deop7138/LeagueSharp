using System;

using LeagueSharp;

namespace Zed_Sharpy
{
    class Initializer
    {
        internal static void Initialize()
        {
            Console.WriteLine("Zed Sharpy: HelloWorld!");

            MenuProvider.initialize();

            if (PluginLoader.LoadPlugin(ObjectManager.Player.ChampionName))
            {
                MenuProvider.Champion.Drawings.addItem(" ");
            }

            AutoQuit.Load();

            Console.WriteLine("Zed Sharpy: Initialized.");
        }
    }
}

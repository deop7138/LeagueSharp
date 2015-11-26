using System;

using LeagueSharp;

namespace Annie_Sharpy
{
    class Initializer
    {
        internal static void Initialize()
        {
            Console.WriteLine("Annie Sharpy: HelloWorld!");

            MenuProvider.initialize();

            if (PluginLoader.LoadPlugin(ObjectManager.Player.ChampionName))
            {
                MenuProvider.Champion.Drawings.addItem(" ");
            }

            AutoQuit.Load();

            Console.WriteLine("Annie Sharpy: Initialized.");
        }
    }
}

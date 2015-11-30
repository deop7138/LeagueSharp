using System;

using LeagueSharp;

namespace Herrari_488_GTB
{
    class Initializer
    {
        internal static void Initialize()
        {
            Console.WriteLine("Herrari 488 GTB: HelloWorld!");

            MenuProvider.initialize();

            if (PluginLoader.LoadPlugin(ObjectManager.Player.ChampionName))
            {
                MenuProvider.Champion.Drawings.addItem(" ");
            }

            AutoQuit.Load();

            Console.WriteLine("Herrari 488 GTB: Initialized.");
        }
    }
}

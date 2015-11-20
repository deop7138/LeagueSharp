using System;

using LeagueSharp.Common;

namespace Cassio_Sharpy
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Initializer.Initialize();
        }
    }
}

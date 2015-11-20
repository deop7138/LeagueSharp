using System;

using LeagueSharp;

namespace Cassio_Sharpy
{
    class PluginLoader
    {
        internal static bool LoadPlugin(string PluginName)
        {
            if (CanLoadPlugin(PluginName))
            {
                DynamicInitializer.NewInstance(Type.GetType("Cassio_Sharpy.Plugins." + ObjectManager.Player.ChampionName));
                return true;
            }

            return false;
        }

        internal static bool CanLoadPlugin(string PluginName)
        {
            return Type.GetType("Cassio_Sharpy.Plugins." + ObjectManager.Player.ChampionName) != null;
        }
    }
}

using System;

using LeagueSharp;

namespace Zed_Sharpy
{
    class PluginLoader
    {
        internal static bool LoadPlugin(string PluginName)
        {

            if (CanLoadPlugin(PluginName))
            {
                DynamicInitializer.NewInstance(Type.GetType("Zed_Sharpy.Plugins." + ObjectManager.Player.ChampionName));
                return true;
            }

            return false;
        }

        internal static bool CanLoadPlugin(string PluginName)
        {
            return Type.GetType("Zed_Sharpy.Plugins." + ObjectManager.Player.ChampionName) != null;
        }
    }
}

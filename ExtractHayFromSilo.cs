using StardewModdingAPI;
using HarmonyLib;

namespace ExtractHayFromSilo
{
    public class ExtractHayMod : Mod
    {
        internal static IMonitor monitor;
        public override void Entry(IModHelper helper)
        {
            monitor = Monitor;
            Harmony harmony = new("xyz.nekogaming.cat2002.stardewvalley.extracthayfromsilo");
            harmony.PatchAll();
        }
    }
}

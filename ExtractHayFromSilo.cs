using StardewModdingAPI;
using HarmonyLib;
using StardewValley;
using System;
using xTile.Dimensions;

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
            Patches patches = Harmony.GetPatchInfo(AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction), new Type[] { typeof(string[]), typeof(Farmer), typeof(Location) }));
        }
    }
}

using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile.Dimensions;
using SObject = StardewValley.Object;

namespace ExtractHayFromSilo
{
    [HarmonyPatch(typeof(GameLocation),nameof(GameLocation.performAction), new Type[] { typeof(string[]), typeof(Farmer), typeof(Location) })]
    public static class GameLocation_performAction_Patch
    {
        public static bool Prefix(GameLocation __instance, string[] action, Farmer who, ref bool __result)
        {
            if (action.Length >= 1 && action[0] == "BuildingSilo")
            {
                if (who.ActiveItem == null)
                {
                    int piecesOfHayToRemove = Math.Min(ItemRegistry.Create<SObject>("(O)178").maximumStackSize(), __instance.piecesOfHay.Value);
                    SObject hayStack = ItemRegistry.Create<SObject>("(O)178", piecesOfHayToRemove);
                    if (piecesOfHayToRemove > 0 && Game1.player.couldInventoryAcceptThisItem(hayStack))
                    {
                        hayStack = (SObject)who.addItemToInventory(hayStack);
                        if (hayStack == null || hayStack.Stack < piecesOfHayToRemove)
                        {
                            __instance.piecesOfHay.Value -= piecesOfHayToRemove - (hayStack?.Stack ?? 0);
                            Game1.playSound("shwip");
                            __result = true;
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
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
    [HarmonyDebug]
    public static class GameLocation_performAction_Patch
    {
        static readonly MethodInfo activeObjectInfo = AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.ActiveObject));
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            Type type = AccessTools.Inner(typeof(GameLocation), "<>c__DisplayClass389_0");
            List<CodeInstruction> il = instructions.ToList();
            bool found1 = false;
            bool found2 = false;
            bool found3 = false;
            Label? label1 = null;
            for (int i = 0; i < il.Count; i++)
            {
                if (!found1 && il[i].Is(OpCodes.Ldstr, "BuildingSilo"))
                {
                    found1 = true;
                    label1 = (Label)il[i + 2].operand;
                }
                else if (found1 && il[i].labels.Contains(label1.Value))
                {
                    found2 = true;
                }
                if (found2 && !found3 && il[i-1].Calls(activeObjectInfo))
                {
                    Label endLabel = generator.DefineLabel();
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return CodeInstruction.LoadField(type, "who");
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.ActiveItem)));
                    yield return new CodeInstruction(OpCodes.Brtrue, endLabel);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return CodeInstruction.LoadField(type, "who");
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return CodeInstruction.Call(typeof(GameLocation_performAction_Patch), nameof(GiveHay));
                    yield return new CodeInstruction(OpCodes.Brfalse, endLabel);
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Ret);
                    il[i].labels.Add(endLabel);
                    found3 = true;
                }
                yield return il[i];
            }
            if (!found3)
            {
                ExtractHayMod.monitor.Log("Could not patch method.", LogLevel.Error);
            }
        }
        private static bool GiveHay(Farmer farmer, GameLocation location)
        {
            int piecesOfHayToRemove = Math.Min(ItemRegistry.Create<SObject>("(O)178").maximumStackSize(),location.piecesOfHay.Value);
            SObject hayStack = ItemRegistry.Create<SObject>("(O)178", piecesOfHayToRemove);
            if (piecesOfHayToRemove > 0 && Game1.player.couldInventoryAcceptThisItem(hayStack))
            {
                hayStack = (SObject)farmer.addItemToInventory(hayStack);
                if (hayStack == null || hayStack.Stack < piecesOfHayToRemove)
                {
                    location.piecesOfHay.Value -= piecesOfHayToRemove - (hayStack?.Stack ?? 0);
                    Game1.playSound("shwip");
                    return true;
                }
            }
            return false;
        }
    }
}
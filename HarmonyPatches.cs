using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using SObject = StardewValley.Object;

namespace ExtractHayFromSilo
{
    [HarmonyPatch(typeof(Building),nameof(Building.doAction))]
    [HarmonyDebug]
    public static class Building_doAction_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> il = instructions.ToList();
            bool found = false;
            for (int i = 0; i < il.Count; i++)
            {
                if (!found && il[i+1].Is(OpCodes.Ldstr, "Strings\\Buildings:PiecesOfHay"))
                {
                    Label endLabel = (Label)il[i - 1].operand;
                    Label label = generator.DefineLabel();
                    yield return new CodeInstruction(OpCodes.Ldarg_2).WithLabels(il[i].ExtractLabels());
                    yield return CodeInstruction.Call(typeof(Farmer), AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.ActiveObject)).Name);
                    yield return new CodeInstruction(OpCodes.Brtrue, label);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return CodeInstruction.Call(typeof(Building_doAction_Patch), nameof(GetHandEmpty));
                    yield return new CodeInstruction(OpCodes.Brfalse, label);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return CodeInstruction.Call(typeof(Building_doAction_Patch),nameof(GiveHay));
                    yield return new CodeInstruction(OpCodes.Br, endLabel);
                    il[i].WithLabels(label);
                    found = true;
                }
                yield return il[i];
            }
            if (!found)
            {
                ExtractHayMod.monitor.Log("Could not patch method.", LogLevel.Error);
            }
        }
        private static bool GetHandEmpty(Farmer farmer)
        {
            return farmer.Items[farmer.CurrentToolIndex] == null;
        }
        private static void GiveHay(Farmer farmer)
        {
            Farm farm = Game1.getLocationFromName("Farm") as Farm;
            int piecesOfHayToRemove = Math.Min(new SObject(178, 1, false, -1, 0).maximumStackSize(),farm.piecesOfHay.Value);
            if (piecesOfHayToRemove != 0 && Game1.player.couldInventoryAcceptThisObject(178,piecesOfHayToRemove, 0))
            {
                farm.piecesOfHay.Value -= piecesOfHayToRemove;
                farmer.addItemToInventoryBool(new SObject(178, piecesOfHayToRemove, false, -1, 0), false);
                Game1.playSound("shwip");
            }
        }
    }
}

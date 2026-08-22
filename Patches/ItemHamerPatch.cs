using Allumeria.Blocks.Blocks;
using Allumeria.EntitySystem.Entities;
using Allumeria.Items.ItemTypes;
using Allumeria.UI.UINodes;
using HarmonyLib;

namespace ShapingExtendedMod.Patches;

[HarmonyPatch(typeof(ItemHammer))]
internal static class ItemHammerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ItemHammer.OnRadialOpen))]
    private static bool OnRadialOpen(PlayerEntity player, UIRadialMenu menu) =>
        BlockVariantsPatch.OnRadialOpen(player, menu);

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ItemHammer.GetBlockVariantBasedOnInt))]
    private static bool GetBlockVariantBasedOnInt(
        ItemHammer __instance,
        int mode,
        Block originalBlock,
        ref Block __result
    ) => BlockVariantsPatch.GetBlockVariantBasedOnInt(originalBlock, mode, ref __result);
}

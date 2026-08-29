using System.Reflection;
using System.Reflection.Emit;
using Allumeria.Items;
using Allumeria.Rendering;
using Allumeria.UI.Menus;
using HarmonyLib;
using OpenTK.Mathematics;

namespace ShapingExtended.Patches;

[HarmonyPatch]
internal static class InGameHudVariantIconPatch
{
    private static readonly MethodInfo AddQuadScaledMethod = AccessTools.DeclaredMethod(
        typeof(TextureBatcher),
        nameof(TextureBatcher.AddQuadScaled)
    );

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(InGameHUD), nameof(InGameHUD.Render))]
    private static IEnumerable<CodeInstruction> InGameHUDRender(
        IEnumerable<CodeInstruction> instructions
    ) =>
        new CodeMatcher(instructions)
            .MatchStartForward(
                new CodeMatch(
                    OpCodes.Ldsfld,
                    AccessTools.DeclaredField(typeof(Item), nameof(Item.placeMode))
                )
            )
            .ThrowIfInvalid($"No {nameof(Item.placeMode)} access in {nameof(InGameHUD)}.Render")
            .MatchStartForward(new CodeMatch(instruction => instruction.Calls(AddQuadScaledMethod)))
            .ThrowIfInvalid($"No place mode icon draw in {nameof(InGameHUD)}.Render")
            .Set(
                OpCodes.Call,
                AccessTools.DeclaredMethod(
                    typeof(InGameHudVariantIconPatch),
                    nameof(DrawPlaceModeIcon)
                )
            )
            .InstructionEnumeration();

    private static void DrawPlaceModeIcon(
        TextureBatcher batcher,
        int x,
        int y,
        int w,
        int h,
        int umin,
        int vmin,
        int umax,
        int vmax,
        int scale,
        Vector4 color
    )
    {
        var variantIndex = Item.placeMode - 1;
        if (variantIndex < 0 || variantIndex >= BlockVariantsPatch.Variants.Length)
        {
            batcher.AddQuadScaled(x, y, w, h, umin, vmin, umax, vmax, scale, color);
            return;
        }

        // Replace the texture coordinates with the custom texture for the current block variant
        var textureId = BlockVariantsPatch.Variants[variantIndex].TextureId;
        batcher.AddQuadScaled(
            x,
            y,
            w,
            h,
            UIRendererPatch.TextureMarker,
            textureId,
            umax,
            vmax,
            scale,
            color
        );
    }
}

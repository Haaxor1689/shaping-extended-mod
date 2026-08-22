using System.Reflection;
using System.Reflection.Emit;
using Allumeria.DataManagement.AssetLoading;
using Allumeria.Items;
using Allumeria.Rendering;
using Allumeria.UI.Menus;
using Allumeria.UI.UINodes;
using HarmonyLib;
using OpenTK.Mathematics;

namespace ShapingExtendedMod.Patches;

[HarmonyPatch]
internal static class VariantIconRenderPatch
{
    private static readonly MethodInfo AddQuadScaledMethod = AccessTools.DeclaredMethod(
        typeof(TextureBatcher),
        nameof(TextureBatcher.AddQuadScaled)
    );

    // Overrides the active place mode icon rendering in bottom right corner of the UI
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
                    typeof(VariantIconRenderPatch),
                    nameof(DrawPlaceModeIcon)
                )
            )
            .InstructionEnumeration();

    // Overrides the radial menu icon rendering for block variants
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(UIRadialMenu), nameof(UIRadialMenu.Render))]
    private static IEnumerable<CodeInstruction> UIRadialMenuRender(
        IEnumerable<CodeInstruction> instructions
    ) =>
        new CodeMatcher(instructions)
            .MatchStartForward(
                new CodeMatch(
                    OpCodes.Ldfld,
                    AccessTools.DeclaredField(typeof(RadialItem), nameof(RadialItem.textureX))
                )
            )
            .ThrowIfInvalid(
                $"No {nameof(RadialItem.textureX)} access in {nameof(UIRadialMenu)}.Render"
            )
            .MatchStartForward(new CodeMatch(instruction => instruction.Calls(AddQuadScaledMethod)))
            .ThrowIfInvalid($"No radial icon draw in {nameof(UIRadialMenu)}.Render")
            .Set(
                OpCodes.Call,
                AccessTools.DeclaredMethod(typeof(VariantIconRenderPatch), nameof(DrawRadialIcon))
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
        if (!TryGetVariantTexture(Item.placeMode - 1, out var texture))
        {
            // Draw original as a fallback
            batcher.AddQuadScaled(x, y, w, h, umin, vmin, umax, vmax, scale, color);
            return;
        }

        DrawAtlasIcon(batcher, x, y, w, h, texture, scale, color);
    }

    private static void DrawRadialIcon(
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
        if (umin != -1 || !TryGetVariantTexture(vmin, out var texture))
        {
            // Draw original as a fallback
            batcher.AddQuadScaled(x, y, w, h, umin, vmin, umax, vmax, scale, color);
            return;
        }

        DrawAtlasIcon(batcher, x, y, w, h, texture, scale, color);
    }

    private static bool TryGetVariantTexture(int index, out AtlasTexture texture)
    {
        texture = null!;
        if (index < 0 || index >= BlockVariantsPatch.Variants.Length)
            return false;

        texture = BlockVariantsPatch.Variants[index].Texture!;
        return texture != null;
    }

    private static void DrawAtlasIcon(
        TextureBatcher batcher,
        int x,
        int y,
        int w,
        int h,
        AtlasTexture texture,
        int scale,
        Vector4 color
    )
    {
        // We need to finzalize current batch and start a new one because the icons are in a different texture atlas
        batcher.Finalise();
        batcher.DrawBatch();

        batcher.Start(AssetManager.itemAtlas.generatedTexture);
        batcher.AddQuadScaled(x, y, w, h, texture.x, texture.y, texture.w, texture.h, scale, color);
        batcher.Finalise();
        batcher.DrawBatch();

        batcher.Start(Drawing.uiTexture);
    }
}

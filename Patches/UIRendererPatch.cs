using Allumeria.DataManagement.AssetLoading;
using Allumeria.Rendering;
using HarmonyLib;
using OpenTK.Mathematics;
using Logger = Allumeria.Logger;

namespace ShapingExtended.Patches;

[HarmonyPatch]
internal static class UIRendererPatch
{
    // Unique texture marker to identify custom textures in the texture batcher
    internal const int TextureMarker = -20002;

    private static readonly Dictionary<int, RegisteredTexture> CustomTextures = [];

    private static int nextTextureId;

    // Registers a custom texture and returns its unique texture ID.
    internal static int RegisterTexture(string atlasKey)
    {
        var textureId = nextTextureId++;
        CustomTextures.Add(textureId, new RegisteredTexture(Mod.UiSpriteKey(atlasKey)));
        return textureId;
    }

    internal static void InitializeTextures()
    {
        foreach (var registeredTexture in CustomTextures.Values)
        {
            Logger.Info($"Loading texture for UIRendererPatch: {registeredTexture.AtlasKey}");
            registeredTexture.Texture =
                AssetManager.itemAtlas.atlasTexturesByString.GetValueOrDefault(
                    registeredTexture.AtlasKey
                );
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TextureBatcher), nameof(TextureBatcher.AddQuadScaled))]
    private static bool AddQuadScaledPrefix(
        TextureBatcher __instance,
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
        if (umin != TextureMarker || !TryGetCustomTexture(vmin, out var texture))
            return true;

        DrawAtlasIcon(__instance, x, y, w, h, texture, scale, color);
        return false;
    }

    private static bool TryGetCustomTexture(int textureId, out AtlasTexture texture)
    {
        texture = null!;
        if (!CustomTextures.TryGetValue(textureId, out var registeredTexture))
            return false;

        texture = registeredTexture.Texture!;
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
        // We need to finalize the current batch and start a new one because the icons are in a different texture atlas.
        batcher.Finalise();
        batcher.DrawBatch();

        batcher.Start(AssetManager.itemAtlas.generatedTexture);
        batcher.AddQuadScaled(x, y, w, h, texture.x, texture.y, texture.w, texture.h, scale, color);
        batcher.Finalise();
        batcher.DrawBatch();

        batcher.Start(Drawing.uiTexture);
    }

    private sealed class RegisteredTexture(string atlasKey)
    {
        internal string AtlasKey { get; } = atlasKey;
        internal AtlasTexture? Texture { get; set; }
    }
}

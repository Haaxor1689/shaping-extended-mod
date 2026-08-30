using Allumeria.Items;
using Allumeria.Items.ItemTypes;
using HarmonyLib;
using HarmonyLib.Tools;
using Ignitron.Aluminium.Assets;
using Ignitron.Aluminium.Events;
using Ignitron.Loader;
using ShapingExtended.Patches;

namespace ShapingExtended;

public sealed class Mod : IModEntrypoint
{
    public const string ModId = "shaping_extended_mod";

    public void Main(ModBox box)
    {
#if DEBUG
        HarmonyFileLog.Enabled = true;
#endif
        // Apply harmony patches
        new Harmony($"{box.Metadata.Contributors.First().Name}.{box.Metadata.Id}").PatchAll();

        // Initialize asset manager for loading resources
        var assetManager = AssetManager.CreateDefault(box.RootPath, $"ignitron/{ModId}");

        Allumeria.DataManagement.AssetLoading.AssetManager.itemAtlas.ScanDirectory(
            assetManager,
            "textures/atlas/ui",
            16
        );

        ContentRegistryEvents.Items += () =>
        {
            // Enable left click use for hammers so that they can be used to cycle shaped block state
            foreach (var item in Item.items)
                if (item is ItemHammer)
                    item.leftClickUse = true;
        };

        ClientLoopEvents.Loaded += (game) =>
        {
            UIRendererPatch.InitializeTextures();
        };
    }

    internal static string ItemSpriteKey(string name) =>
        $"ignitron.{ModId}.textures.atlas.items.{name}";

    internal static string BlockSpriteKey(string name) =>
        $"ignitron.{ModId}.textures.atlas.blocks.{name}";

    internal static string UiSpriteKey(string name) => $"ignitron.{ModId}.textures.atlas.ui.{name}";

    internal static string ModelKey(string name) => $"ignitron.{ModId}.models.{name}";

    internal static string TextureKey(string name) => $"ignitron.{ModId}.textures.{name}";
}

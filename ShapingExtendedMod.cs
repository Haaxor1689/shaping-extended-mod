using Allumeria.DataManagement.Permissions;
using Allumeria.Items;
using Allumeria.Items.ItemTypes;
using HarmonyLib;
using HarmonyLib.Tools;
using Ignitron.Aluminium.Assets;
using Ignitron.Aluminium.Events;
using Ignitron.Loader;
using ShapingExtendedMod.Patches;
using Logger = Allumeria.Logger;

namespace ShapingExtendedMod;

public sealed class ShapingExtendedMod : IModEntrypoint
{
    public void Main(ModBox box)
    {
#if DEBUG
        HarmonyFileLog.Enabled = true;
#endif
        // Apply harmony patches
        new Harmony($"{box.Metadata.Contributors.First().Name}.{box.Metadata.Id}").PatchAll();

        // Initialize asset manager for loading resources
        var assetManager = AssetManager.CreateDefault(box.RootPath, $"ignitron/{box.Metadata.Id}");

        // Register resources
        Allumeria.DataManagement.AssetLoading.AssetManager.itemAtlas.ScanDirectory(
            assetManager,
            BlockVariantsPatch.IconDirectory,
            16
        );

        ContentRegistryEvents.Items += () =>
        {
            BlockVariantsPatch.Initialize(box.Metadata.Id);

            // Enable left click use for hammers so that they can be used to cycle shaped block state
            foreach (var item in Item.items)
                if (item is ItemHammer)
                    item.leftClickUse = true;
        };

#if DEBUG
        // Enable creative menu and noclip for dev
        PlayerEvents.Spawned += (player, world) =>
        {
            player.permissions.permissions.TryGetValue(
                PermissionRegistry.allow_creative_menu.shortID,
                out var creativePerm
            );
            creativePerm?.SetValue(true);

            player.permissions.permissions.TryGetValue(
                PermissionRegistry.allow_noclip.shortID,
                out var noclipPerm
            );
            noclipPerm?.SetValue(true);

            player.permissions.permissions.TryGetValue(
                PermissionRegistry.instant_break.shortID,
                out var instantBreakPerm
            );
            instantBreakPerm?.SetValue(true);
        };
#endif

        Logger.Init($"Initializing {box.Metadata.DisplayName}!");
    }
}

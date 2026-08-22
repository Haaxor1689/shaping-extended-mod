using Allumeria.Blocks.BlockModels;
using Allumeria.Blocks.Blocks;
using Allumeria.DataManagement.AssetLoading;
using Allumeria.EntitySystem.Entities;
using Allumeria.UI.UINodes;
using HarmonyLib;
using ShapingExtendedMod.BlockModels;
using ShapingExtendedMod.Blocks;
using Logger = Allumeria.Logger;

namespace ShapingExtendedMod.Patches;

internal struct BlockVariantData
{
    public string Name;

    public AtlasTexture? Texture;

    public BlockModel Model;

    public Type BlockConstructor;
}

[HarmonyPatch(typeof(Block))]
internal static class BlockVariantsPatch
{
    internal static readonly BlockVariantData[] Variants =
    [
        new BlockVariantData
        {
            Name = "Stair",
            Model = BlockModelQuads.stair_model,
            BlockConstructor = typeof(BlockStairs),
        },
        new BlockVariantData
        {
            Name = "Inner Corner Stair",
            Model = BlockModelRegistry.inner_corner_stairs,
            BlockConstructor = typeof(BlockInnerCornerStairs),
        },
        new BlockVariantData
        {
            Name = "Outer Corner Stair",
            Model = BlockModelRegistry.outer_corner_stairs,
            BlockConstructor = typeof(BlockOuterCornerStairs),
        },
        new BlockVariantData
        {
            Name = "Side Stair",
            Model = BlockModelRegistry.side_stairs,
            BlockConstructor = typeof(BlockSideStairs),
        },
        new BlockVariantData
        {
            Name = "Slab",
            Model = BlockModelQuads.slab_model,
            BlockConstructor = typeof(BlockSlab),
        },
        new BlockVariantData
        {
            Name = "Step",
            Model = BlockModelRegistry.step,
            BlockConstructor = typeof(BlockStep),
        },
        new BlockVariantData
        {
            Name = "Side Step",
            Model = BlockModelRegistry.side_step,
            BlockConstructor = typeof(BlockSideStep),
        },
        new BlockVariantData
        {
            Name = "Panel",
            Model = BlockModelQuads.panel,
            BlockConstructor = typeof(BlockPanel),
        },
        new BlockVariantData
        {
            Name = "Fence",
            Model = BlockModelQuads.fence,
            BlockConstructor = typeof(BlockFence),
        },
        new BlockVariantData
        {
            Name = "Flooring",
            Model = BlockModelRegistry.flooring,
            BlockConstructor = typeof(BlockFlooring),
        },
        new BlockVariantData
        {
            Name = "Siding",
            Model = BlockModelRegistry.siding,
            BlockConstructor = typeof(BlockSiding),
        },
        new BlockVariantData
        {
            Name = "Mini Block",
            Model = BlockModelRegistry.mini_block,
            BlockConstructor = typeof(BlockMini),
        },
    ];

    internal static readonly Dictionary<Block, Block[]> AddedVariants = [];

    internal const string IconDirectory = "textures/atlas/ui";

    // Load all the variant icons into the texture atlas manually
    internal static void Initialize(string modId)
    {
        for (var i = 0; i < Variants.Length; i++)
        {
            var spriteString =
                $"ignitron/{modId}/{IconDirectory}/{ToSnakeCase(Variants[i].Name)}".Replace(
                    '/',
                    '.'
                );

            Variants[i].Texture = AssetManager.itemAtlas.atlasTexturesByString.GetValueOrDefault(
                spriteString
            );

            if (Variants[i].Texture == null)
                Logger.Warn($"Missing icon '{spriteString}' for variant '{Variants[i].Name}'");
        }
    }

    private static string ToSnakeCase(string name) => name.ToLowerInvariant().Replace(' ', '_');

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Block.AutoGenVariants))]
    private static bool AutoGenVariants(Block __instance, ref Block __result)
    {
        __instance.item.usesRadialMenu = true;
        // Generate variants
        AddedVariants[__instance] = Variants
            .Select(variant =>
            {
                var variantInstance = (Block)
                    Activator.CreateInstance(
                        variant.BlockConstructor,
                        __instance.strID + "_" + ToSnakeCase(variant.Name)
                    )!;
                return variantInstance
                    .MakeCopy(__instance)
                    .SetDropItem(__instance.item)
                    .SetBlockModel(variant.Model)
                    .Hide()
                    .MakeSemiSolid();
            })
            .ToArray();

        // Workaround for the game using this variant to determine if full block has variants
        __instance.slabVariant = AddedVariants[__instance][1];

        __result = __instance;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Block.OnRadialOpen))]
    internal static bool OnRadialOpen(PlayerEntity player, UIRadialMenu menu)
    {
        menu.AddItem(new RadialItem(0, 224, nameof(Block)), 60);
        for (var i = 0; i < Variants.Length; i++)
        {
            var variant = Variants[i];
            menu.AddItem(new RadialItem(-1, i, variant.Name), 60);
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Block.GetBlockVariantBasedOnInt))]
    internal static bool GetBlockVariantBasedOnInt(Block __instance, int mode, ref Block __result)
    {
        __result = mode switch
        {
            >= 1 when AddedVariants.TryGetValue(__instance, out Block[]? variants) => variants[
                mode - 1
            ],
            _ => __instance,
        };
        return false;
    }
}

using Allumeria.Blocks.BlockModels;
using Allumeria.Blocks.Blocks;
using Allumeria.EntitySystem.Entities;
using Allumeria.UI.UINodes;
using HarmonyLib;
using ShapingExtended.BlockModels;
using ShapingExtended.Blocks;

namespace ShapingExtended.Patches;

internal struct BlockVariantData
{
    public string Name;
    public int TextureId;
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
            TextureId = UIRendererPatch.RegisterTexture("stair"),
            Model = BlockModelQuads.stair_model,
            BlockConstructor = typeof(BlockStairs),
        },
        new BlockVariantData
        {
            Name = "Inner Corner Stair",
            TextureId = UIRendererPatch.RegisterTexture("inner_corner_stair"),
            Model = BlockModelRegistry.inner_corner_stairs,
            BlockConstructor = typeof(BlockInnerCornerStairs),
        },
        new BlockVariantData
        {
            Name = "Outer Corner Stair",
            TextureId = UIRendererPatch.RegisterTexture("outer_corner_stair"),
            Model = BlockModelRegistry.outer_corner_stairs,
            BlockConstructor = typeof(BlockOuterCornerStairs),
        },
        new BlockVariantData
        {
            Name = "Vertical Stair",
            TextureId = UIRendererPatch.RegisterTexture("side_stair"),
            Model = BlockModelRegistry.side_stairs,
            BlockConstructor = typeof(BlockSideStairs),
        },
        new BlockVariantData
        {
            Name = "Slab",
            TextureId = UIRendererPatch.RegisterTexture("slab"),
            Model = BlockModelQuads.slab_model,
            BlockConstructor = typeof(BlockSlab),
        },
        new BlockVariantData
        {
            Name = "Vertical Slab",
            TextureId = UIRendererPatch.RegisterTexture("side_slab"),
            Model = BlockModelQuads.slab_model,
            BlockConstructor = typeof(BlockSlab),
        },
        new BlockVariantData
        {
            Name = "Step",
            TextureId = UIRendererPatch.RegisterTexture("step"),
            Model = BlockModelRegistry.step,
            BlockConstructor = typeof(BlockStep),
        },
        new BlockVariantData
        {
            Name = "Vertical Step",
            TextureId = UIRendererPatch.RegisterTexture("side_step"),
            Model = BlockModelRegistry.side_step,
            BlockConstructor = typeof(BlockSideStep),
        },
        new BlockVariantData
        {
            Name = "Panel",
            TextureId = UIRendererPatch.RegisterTexture("panel"),
            Model = BlockModelQuads.panel,
            BlockConstructor = typeof(BlockPanel),
        },
        new BlockVariantData
        {
            Name = "Fence",
            TextureId = UIRendererPatch.RegisterTexture("fence"),
            Model = BlockModelQuads.fence,
            BlockConstructor = typeof(BlockFence),
        },
        new BlockVariantData
        {
            Name = "Flooring",
            TextureId = UIRendererPatch.RegisterTexture("flooring"),
            Model = BlockModelRegistry.flooring,
            BlockConstructor = typeof(BlockFlooring),
        },
        new BlockVariantData
        {
            Name = "Siding",
            TextureId = UIRendererPatch.RegisterTexture("siding"),
            Model = BlockModelRegistry.siding,
            BlockConstructor = typeof(BlockSiding),
        },
        new BlockVariantData
        {
            Name = "Column",
            TextureId = UIRendererPatch.RegisterTexture("column"),
            Model = BlockModelQuads.log,
            BlockConstructor = typeof(Block),
        },
        new BlockVariantData
        {
            Name = "Mini Block",
            TextureId = UIRendererPatch.RegisterTexture("mini_block"),
            Model = BlockModelRegistry.mini_block,
            BlockConstructor = typeof(BlockMini),
        },
    ];

    internal static readonly Dictionary<Block, Block[]> AddedVariants = [];

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
            menu.AddItem(
                new RadialItem(UIRendererPatch.TextureMarker, variant.TextureId, variant.Name),
                60
            );
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

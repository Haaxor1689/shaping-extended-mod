using Allumeria.Blocks.BlockModels;

namespace ShapingExtendedMod.BlockModels;

public static class BlockModelRegistry
{
    public static readonly BlockModelQuads step = (BlockModelQuads)
        new BlockModelQuads()
            .AddCuboid(new Cuboid(0, 0, 0, 16, 8, 8))
            .AddCuboid(new Cuboid(0, 8, 0, 16, 16, 8, flag: 1))
            .AddCollider(0f, 0f, 0f, 16f, 8f, 8f);

    public static readonly BlockModelQuads side_step = (BlockModelQuads)
        new BlockModelQuads()
            .AddCuboid(new Cuboid(8, 0, 0, 16, 16, 8))
            .AddCollider(8f, 0f, 0f, 8f, 16f, 8f);

    public static readonly BlockModelQuads outer_corner_stairs = (BlockModelQuads)
        new BlockModelQuads()
            .AddCuboid(new Cuboid(0, 0, 0, 16, 8, 16))
            .AddCuboid(new Cuboid(8, 8, 0, 16, 16, 8))
            .AddCuboid(new Cuboid(0, 8, 0, 16, 16, 16, flag: 1))
            .AddCuboid(new Cuboid(8, 0, 0, 16, 8, 8, flag: 1))
            .AddCollider(0f, 0f, 0f, 16f, 8f, 16f)
            .AddCollider(8f, 8f, 0f, 8f, 8f, 8f);

    public static readonly BlockModelQuads inner_corner_stairs = (BlockModelQuads)
        new BlockModelQuads()
            .AddCuboid(new Cuboid(0, 0, 0, 16, 8, 16))
            .AddCuboid(new Cuboid(0, 8, 0, 16, 16, 8))
            .AddCuboid(new Cuboid(8, 8, 8, 16, 16, 16))
            .AddCuboid(new Cuboid(0, 8, 0, 16, 16, 16, flag: 1))
            .AddCuboid(new Cuboid(0, 0, 0, 16, 8, 8, flag: 1))
            .AddCuboid(new Cuboid(8, 0, 8, 16, 8, 16, flag: 1))
            .AddCollider(0f, 0f, 0f, 16f, 8f, 16f)
            .AddCollider(0f, 8f, 0f, 16f, 8f, 8f)
            .AddCollider(8f, 8f, 8f, 8f, 8f, 8f);

    public static readonly BlockModelQuads side_stairs = (BlockModelQuads)
        new BlockModelQuads()
            .AddCuboid(new Cuboid(0, 0, 0, 16, 16, 8))
            .AddCuboid(new Cuboid(8, 0, 8, 16, 16, 16))
            .AddCollider(0f, 0f, 0f, 16f, 16f, 8f)
            .AddCollider(8f, 0f, 8f, 8f, 16f, 8f);

    public static readonly BlockModelQuads flooring = (BlockModelQuads)
        new BlockModelQuads()
            .AddCuboid(new Cuboid(0, 0, 0, 16, 1, 16))
            .AddCuboid(new Cuboid(0, 15, 0, 16, 16, 16, flag: 1))
            .AddCollider(0f, 0f, 0f, 16f, 1f, 16f);

    public static readonly BlockModelQuads siding = (BlockModelQuads)
        new BlockModelQuads()
            .AddCuboid(new Cuboid(0, 0, 0, 16, 16, 1))
            .AddCollider(0f, 0f, 0f, 16f, 16f, 1f);

    public static readonly BlockModelQuads mini_block = (BlockModelQuads)
        new BlockModelQuads()
            .AddCuboid(new Cuboid(4, 0, 4, 12, 8, 12))
            .AddCuboid(new Cuboid(4, 8, 4, 12, 16, 12, flag: 1))
            .AddCollider(4f, 0f, 4f, 8f, 8f, 8f);
}

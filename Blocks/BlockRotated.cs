using Allumeria;
using Allumeria.Blocks.BlockModels;
using Allumeria.Blocks.Blocks;
using Allumeria.ChunkManagement;
using Allumeria.EntitySystem;
using Allumeria.EntitySystem.Entities;
using Allumeria.Settings;
using OpenTK.Mathematics;
using ShapingExtended.BlockModels;

namespace ShapingExtended.Blocks;

public abstract class BlockRotated(string strID) : Block(strID)
{
    internal virtual bool IsFlippable() => false;

    internal static BlockStateStairs _state = new();

    protected virtual byte GetPlacementFacing(PlayerEntity player) =>
        GetFacingBasedOnOrientationNoNormal(player);

    public override uint GetFacing(uint metadata)
    {
        _state.SetFromInt(metadata);
        return _state.facing;
    }

    public override int GetModelFlag(uint metadata)
    {
        if (!IsFlippable())
            return 0;

        _state.SetFromInt(metadata);
        return _state.upside_down;
    }

    public override bool HasRotation() => true;

    public override void OnPlace(PlayerEntity player, int x, int y, int z, World world)
    {
        base.OnPlace(player, x, y, z, world);
        _state.facing = GetPlacementFacing(player);

        // Mirrors existing behavior of stairs and slabs from vanilla
        _state.upside_down = (byte)(
            !IsFlippable() ? 0
            : GameSettings.slab_placement_mode.value != 1
                ? (
                    GameSettings.slab_placement_mode.value != 2
                        ? (
                            player.targetedBlockPosNormal.Y >= 0
                                ? (
                                    player.targetedBlockPosNormal.Y <= 0
                                        ? (
                                            player.raycastHitPositionPrecise.Y
                                                - player.targetedBlockPos.Y
                                            <= 0.5
                                                ? 0
                                                : 1
                                        )
                                        : 0
                                )
                                : 1
                        )
                        : 1
                )
            : 0
        );

        world.chunkManager.SetBlockMaintainFluid(
            x,
            y,
            z,
            intID,
            _state.ConvertToInt(),
            keepPaint: true
        );
    }

    public override bool DoesThisOcclude(Block block, uint metadata, AxisDir dir) => false;

    public override uint GetRotatedMetadata(uint metadata, int rotation) =>
        (uint)((metadata + (2 * rotation)) % 8);

    public static List<Collider>[] GenerateColliders(BlockModelQuads model, bool flipped = false)
    {
        List<Collider>[] colliders = new List<Collider>[8];

        for (int facing = 0; facing < 8; ++facing)
        {
            float angle = MathHelper.DegreesToRadians(facing * 45f);
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);

            colliders[facing] =
            [
                .. model.colliders.Select(collider => RotateCollider(collider, cos, sin, flipped)),
            ];
        }
        return colliders;
    }

    public static List<Collider> FlippedCollider(BlockModelQuads model) =>
        model.colliders.Select(collider => RotateCollider(collider, 1f, 0f, true)).ToList();

    private static Collider RotateCollider(Collider collider, float cos, float sin, bool flipped)
    {
        var position = collider.position;
        var size = collider.size;

        if (flipped)
            position.Y = 1f - (position.Y + size.Y);

        float minX = position.X - 0.5f;
        float minZ = position.Z - 0.5f;
        float maxX = minX + size.X;
        float maxZ = minZ + size.Z;

        Span<Vector2> corners =
        [
            new(minX, minZ),
            new(maxX, minZ),
            new(minX, maxZ),
            new(maxX, maxZ),
        ];

        float rotatedMinX = float.MaxValue;
        float rotatedMinZ = float.MaxValue;
        float rotatedMaxX = float.MinValue;
        float rotatedMaxZ = float.MinValue;

        foreach (Vector2 corner in corners)
        {
            float x = (corner.X * cos) + (corner.Y * sin);
            float z = (-corner.X * sin) + (corner.Y * cos);
            rotatedMinX = MathF.Min(rotatedMinX, x);
            rotatedMaxX = MathF.Max(rotatedMaxX, x);
            rotatedMinZ = MathF.Min(rotatedMinZ, z);
            rotatedMaxZ = MathF.Max(rotatedMaxZ, z);
        }

        return new Collider(
            new Vector3(rotatedMinX + 0.5f, position.Y, rotatedMinZ + 0.5f),
            new Vector3(rotatedMaxX - rotatedMinX, size.Y, rotatedMaxZ - rotatedMinZ),
            collider.colliderType
        );
    }
}

public class BlockStep(string strID) : BlockRotated(strID)
{
    internal override bool IsFlippable() => true;

    private static readonly List<Collider>[] _colliders = GenerateColliders(
        BlockModelRegistry.step
    );
    private static readonly List<Collider>[] _colliders_flipped = GenerateColliders(
        BlockModelRegistry.step,
        flipped: true
    );

    public override List<Collider> GetColliders(uint metadata = 0)
    {
        _state.SetFromInt(metadata);
        return _state.upside_down == 1
            ? _colliders_flipped[_state.facing % 8]
            : _colliders[_state.facing % 8];
    }
}

public class BlockOuterCornerStairs(string strID) : BlockRotated(strID)
{
    internal override bool IsFlippable() => true;

    private static readonly List<Collider>[] _colliders = GenerateColliders(
        BlockModelRegistry.outer_corner_stairs
    );
    private static readonly List<Collider>[] _colliders_flipped = GenerateColliders(
        BlockModelRegistry.outer_corner_stairs,
        flipped: true
    );

    public override List<Collider> GetColliders(uint metadata = 0)
    {
        _state.SetFromInt(metadata);
        return _state.upside_down == 1
            ? _colliders_flipped[_state.facing % 8]
            : _colliders[_state.facing % 8];
    }
}

public class BlockInnerCornerStairs(string strID) : BlockRotated(strID)
{
    internal override bool IsFlippable() => true;

    private static readonly List<Collider>[] _colliders = GenerateColliders(
        BlockModelRegistry.inner_corner_stairs
    );
    private static readonly List<Collider>[] _colliders_flipped = GenerateColliders(
        BlockModelRegistry.inner_corner_stairs,
        flipped: true
    );

    public override List<Collider> GetColliders(uint metadata = 0)
    {
        _state.SetFromInt(metadata);
        return _state.upside_down == 1
            ? _colliders_flipped[_state.facing % 8]
            : _colliders[_state.facing % 8];
    }
}

public sealed class BlockSideStairs(string strID) : BlockRotated(strID)
{
    private static readonly List<Collider>[] _colliders = GenerateColliders(
        BlockModelRegistry.side_stairs
    );

    public override List<Collider> GetColliders(uint metadata = 0)
    {
        _state.SetFromInt(metadata);
        return _colliders[_state.facing % 8];
    }
}

public sealed class BlockFlooring(string strID) : BlockRotated(strID)
{
    internal override bool IsFlippable() => true;

    private static readonly List<Collider> _colliders = BlockModelRegistry.flooring.colliders;
    private static readonly List<Collider> _colliders_flipped = FlippedCollider(
        BlockModelRegistry.flooring
    );

    public override List<Collider> GetColliders(uint metadata = 0)
    {
        _state.SetFromInt(metadata);
        return _state.upside_down == 1 ? _colliders_flipped : _colliders;
    }
}

public sealed class BlockSiding(string strID) : BlockRotated(strID)
{
    private static readonly List<Collider>[] _colliders = GenerateColliders(
        BlockModelRegistry.siding
    );

    public override List<Collider> GetColliders(uint metadata = 0)
    {
        _state.SetFromInt(metadata);
        return _colliders[_state.facing % 8];
    }
}

public sealed class BlockSideStep(string strID) : BlockRotated(strID)
{
    private static readonly List<Collider>[] _colliders = GenerateColliders(
        BlockModelRegistry.side_step
    );

    public override List<Collider> GetColliders(uint metadata = 0)
    {
        _state.SetFromInt(metadata);
        return _colliders[_state.facing % 8];
    }
}

public sealed class BlockMini(string strID) : BlockRotated(strID)
{
    internal override bool IsFlippable() => true;

    private static readonly List<Collider> _colliders = BlockModelRegistry.mini_block.colliders;
    private static readonly List<Collider> _colliders_flipped = FlippedCollider(
        BlockModelRegistry.mini_block
    );

    protected override byte GetPlacementFacing(PlayerEntity player)
    {
        var direction = Game.camera.front.Xz;
        direction.Normalize();

        double eighthTurn = Math.PI / 4.0;
        double angle = Math.Atan2(-direction.X, direction.Y);
        return (byte)((int)Math.Round(angle / eighthTurn) & 7);
    }

    public override List<Collider> GetColliders(uint metadata = 0)
    {
        _state.SetFromInt(metadata);
        return _state.upside_down == 1 ? _colliders_flipped : _colliders;
    }
}

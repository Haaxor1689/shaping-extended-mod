using Allumeria;
using Allumeria.Audio;
using Allumeria.Blocks.Blocks;
using Allumeria.ChunkManagement;
using Allumeria.DataManagement.AssetLoading;
using Allumeria.EntitySystem.Entities;
using Allumeria.Input;
using Allumeria.Items;
using Allumeria.Items.ItemTagTypes;
using Allumeria.Items.ItemTypes;
using HarmonyLib;
using ShapingExtendedMod.Blocks;

[HarmonyPatch(typeof(Item))]
public static class OnLeftClickUsePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Item.OnLeftClickUse))]
    private static bool OnLeftClickUse(Item __instance, PlayerEntity player, World world)
    {
        if (__instance is not ItemHammer hammer)
            return true;

        var lookingAtBlock = player.lookingAtBlock;
        if (!player.IsSelf() || lookingAtBlock.isVariantOf == null)
            return false;

        if (
            !hammer.GetTag(ItemTag.hammer, out var entry)
            || lookingAtBlock.blockMaterial.hammerLevel > entry.data
        )
            return false;

        uint metadata;
        bool withUpdate = true;
        switch (lookingAtBlock)
        {
            case BlockSlab:
                metadata = RotateOrFlip(
                    BlockSlab.state,
                    player.lookingAtMetadata,
                    allowFlip: false
                );
                break;
            case BlockStairs:
                metadata = RotateOrFlip(
                    BlockStairs.state,
                    player.lookingAtMetadata,
                    allowFlip: true
                );
                break;
            case BlockMini mini:
                metadata = RotateOrFlip(
                    BlockRotated._state,
                    player.lookingAtMetadata,
                    mini.IsFlippable(),
                    rotationStep: 1
                );
                break;
            case BlockRotated rotated:
                metadata = RotateOrFlip(
                    BlockRotated._state,
                    player.lookingAtMetadata,
                    rotated.IsFlippable()
                );
                break;
            case BlockFence fence:
                if (!TryToggleBlockedDirection(fence.state, player.lookingAtMetadata, out metadata))
                    return false;
                break;
            case BlockPanel panel:
                if (!TryToggleBlockedDirection(panel.state, player.lookingAtMetadata, out metadata))
                    return false;
                break;
            default:
                return false;
        }

        Logger.Info(
            $"Hammer used on {lookingAtBlock.strID} at {player.targetedBlockPos} with metadata {player.lookingAtMetadata}. New metadata: {metadata}"
        );

        world.chunkManager.SetBlockWithLight(
            player.targetedBlockPos.X,
            player.targetedBlockPos.Y,
            player.targetedBlockPos.Z,
            lookingAtBlock.intID,
            true,
            metadata,
            maintainFluid: true,
            maintainPaint: true
        );

        if (withUpdate)
            world.chunkManager.UpdateSelfAndNeighbours(
                player.targetedBlockPos.X,
                player.targetedBlockPos.Y,
                player.targetedBlockPos.Z
            );

        world.chunkManager.MarkNeighboursDirty(
            player.targetedBlockPos.X,
            player.targetedBlockPos.Y,
            player.targetedBlockPos.Z
        );

        AudioPlayer.PlaySoundPlayer(AssetManager.GetSound("effects.bow_release"), 1f);

        return false;
    }

    private static uint RotateOrFlip(
        BlockStateStairs state,
        uint currentMetadata,
        bool allowFlip,
        byte rotationStep = 2
    )
    {
        state.SetFromInt(currentMetadata);

        if (allowFlip && InputManager.sneak.IsDown())
            state.upside_down = (byte)(1 - state.upside_down);
        else
            state.facing = (byte)((state.facing + rotationStep) % 8);

        return state.ConvertToInt();
    }

    private static bool TryToggleBlockedDirection(
        BlockStateFence blockState,
        uint currentMetadata,
        out uint metadata
    )
    {
        metadata = currentMetadata;
        if (blockState is not BlockStateFenceExtended state)
            return false;

        state.SetFromInt(currentMetadata);

        var facing = Game.camera.front.Xz;
        bool invert = InputManager.sneak.IsDown();

        if (MathF.Abs(facing.X) >= MathF.Abs(facing.Y))
        {
            if (facing.X > 0 != invert)
                state.xPosBlock = !state.xPosBlock;
            else
                state.xNegBlock = !state.xNegBlock;
        }
        else
        {
            if (facing.Y > 0 != invert)
                state.zPosBlock = !state.zPosBlock;
            else
                state.zNegBlock = !state.zNegBlock;
        }

        state.ApplyBlocking();
        metadata = state.ConvertToInt();
        return true;
    }
}

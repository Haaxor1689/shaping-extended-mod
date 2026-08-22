using Allumeria;
using Allumeria.Blocks.Blocks;
using Allumeria.ChunkManagement;
using HarmonyLib;
using ShapingExtendedMod.Blocks;

namespace ShapingExtendedMod.Patches;

internal static class FenceConnections
{
    internal static bool UpdateState(
        Block block,
        BlockStateFence blockState,
        int x,
        int y,
        int z,
        World world
    )
    {
        if (blockState is not BlockStateFenceExtended state)
            return true;

        // Vanilla never reads the stored metadata, so the blocked flags have to be reloaded here.
        state.SetFromInt(world.chunkManager.GetBlockWithMetadata(x, y, z).metadata);

        state.xPosRaw = CanConnect(world, x + 1, y, z);
        state.xNegRaw = CanConnect(world, x - 1, y, z);
        state.zPosRaw = CanConnect(world, x, y, z + 1);
        state.zNegRaw = CanConnect(world, x, y, z - 1);
        state.ApplyBlocking();

        world.chunkManager.SetBlockMaintainFluid(
            x,
            y,
            z,
            block.intID,
            state.ConvertToInt(),
            markMeshDirty: true,
            keepPaint: true
        );

        return false;
    }

    internal static bool GetRotatedMetadata(
        BlockStateFence blockState,
        uint metadata,
        int rotation,
        ref uint __result
    )
    {
        if (blockState is not BlockStateFenceExtended state)
            return true;

        state.SetFromInt(metadata);
        state.Rotate(rotation);
        __result = state.ConvertToInt();

        return false;
    }

    private static bool CanConnect(World world, int x, int y, int z)
    {
        Block neighbour = world.chunkManager.GetBlock(x, y, z);
        return neighbour.solid
            && !neighbour.canWalkThrough
            && (!neighbour.semisolid || neighbour is BlockFence || neighbour is BlockPanel);
    }
}

[HarmonyPatch(typeof(BlockFence))]
internal static class BlockFenceConnectionPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(MethodType.Constructor, typeof(string))]
    private static void Constructor(BlockFence __instance) =>
        __instance.state = new BlockStateFenceExtended();

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BlockFence.UpdateState))]
    private static bool UpdateState(BlockFence __instance, int x, int y, int z, World world) =>
        FenceConnections.UpdateState(__instance, __instance.state, x, y, z, world);

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BlockFence.GetRotatedMetadata))]
    private static bool GetRotatedMetadata(
        BlockFence __instance,
        uint metadata,
        int rotation,
        ref uint __result
    ) => FenceConnections.GetRotatedMetadata(__instance.state, metadata, rotation, ref __result);
}

[HarmonyPatch(typeof(BlockPanel))]
internal static class BlockPanelConnectionPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(MethodType.Constructor, typeof(string))]
    private static void Constructor(BlockPanel __instance) =>
        __instance.state = new BlockStateFenceExtended();

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BlockPanel.UpdateState))]
    private static bool UpdateState(BlockPanel __instance, int x, int y, int z, World world) =>
        FenceConnections.UpdateState(__instance, __instance.state, x, y, z, world);

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BlockPanel.GetRotatedMetadata))]
    private static bool GetRotatedMetadata(
        BlockPanel __instance,
        uint metadata,
        int rotation,
        ref uint __result
    ) => FenceConnections.GetRotatedMetadata(__instance.state, metadata, rotation, ref __result);
}

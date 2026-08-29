using Allumeria.Blocks.Blocks;
using Allumeria.Blocks.BlockVariants;
using Allumeria.ChunkManagement;
using HarmonyLib;
using ShapingExtended.BlockStates;

namespace ShapingExtended.Patches;

internal static class FenceConnections
{
    internal static bool UpdateState(Block block, int x, int y, int z, World world)
    {
        // BlockFence/BlockPanel's own state field is a fixed struct, so the extended state
        // used to derive the effective (post-blocking) connection bits is only ever local.
        var state = new BlockStateFenceExtended();

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

    internal static bool GetRotatedMetadata(uint metadata, int rotation, ref uint __result)
    {
        var state = new BlockStateFenceExtended();
        state.SetFromInt(metadata);
        state.Rotate(rotation);
        __result = state.ConvertToInt();

        return false;
    }

    /// <summary>
    /// Vanilla's own BlockStateFence only ever reads bits 0-3 of metadata as the connections,
    /// so rewriting those bits to the blocked-aware values before GetColliders/GetModelFlags run
    /// makes the unpatched vanilla code display the effective (post-blocking) state correctly.
    /// </summary>
    internal static void RewriteToEffectiveConnections(ref uint metadata)
    {
        var state = new BlockStateFenceExtended();
        state.SetFromInt(metadata);

        uint effective = 0;
        if (state.xpos)
            effective |= 1u;
        if (state.xneg)
            effective |= 2u;
        if (state.zpos)
            effective |= 4u;
        if (state.zneg)
            effective |= 8u;

        metadata = (metadata & ~0xFu) | effective;
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
    [HarmonyPrefix]
    [HarmonyPatch(nameof(BlockFence.UpdateState))]
    private static bool UpdateState(BlockFence __instance, int x, int y, int z, World world) =>
        FenceConnections.UpdateState(__instance, x, y, z, world);

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BlockFence.GetRotatedMetadata))]
    private static bool GetRotatedMetadata(uint metadata, int rotation, ref uint __result) =>
        FenceConnections.GetRotatedMetadata(metadata, rotation, ref __result);

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BlockFence.GetColliders))]
    private static void GetColliders(ref uint metadata) =>
        FenceConnections.RewriteToEffectiveConnections(ref metadata);

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BlockFence.GetModelFlags))]
    private static void GetModelFlags(ref uint metadata) =>
        FenceConnections.RewriteToEffectiveConnections(ref metadata);
}

[HarmonyPatch(typeof(BlockPanel))]
internal static class BlockPanelConnectionPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(BlockPanel.UpdateState))]
    private static bool UpdateState(BlockPanel __instance, int x, int y, int z, World world) =>
        FenceConnections.UpdateState(__instance, x, y, z, world);

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BlockPanel.GetRotatedMetadata))]
    private static bool GetRotatedMetadata(uint metadata, int rotation, ref uint __result) =>
        FenceConnections.GetRotatedMetadata(metadata, rotation, ref __result);

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BlockPanel.GetColliders))]
    private static void GetColliders(ref uint metadata) =>
        FenceConnections.RewriteToEffectiveConnections(ref metadata);

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BlockPanel.GetModelFlags))]
    private static void GetModelFlags(ref uint metadata) =>
        FenceConnections.RewriteToEffectiveConnections(ref metadata);
}

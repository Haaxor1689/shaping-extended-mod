using Allumeria.Blocks.Blocks;

namespace ShapingExtendedMod.Blocks;

/// <summary>
/// Fence/panel state that additionally tracks per-direction connection blocking.
/// Bits 0-3 keep the unblocked (raw) connections so blocking never destroys them; the blocked flags
/// live in bits 4-7. The inherited xpos/xneg/zpos/zneg fields hold the effective connection, because
/// vanilla reads those fields directly.
/// </summary>
internal class BlockStateFenceExtended : BlockStateFence
{
    private const int BlockedShift = 4;

    public bool xPosRaw;
    public bool xNegRaw;
    public bool zPosRaw;
    public bool zNegRaw;

    public bool xPosBlock;
    public bool xNegBlock;
    public bool zPosBlock;
    public bool zNegBlock;

    public override uint ConvertToInt()
    {
        uint value = 0;
        if (xPosRaw)
            value |= 1u;
        if (xNegRaw)
            value |= 2u;
        if (zPosRaw)
            value |= 4u;
        if (zNegRaw)
            value |= 8u;

        if (xPosBlock)
            value |= 1u << BlockedShift;
        if (xNegBlock)
            value |= 2u << BlockedShift;
        if (zPosBlock)
            value |= 4u << BlockedShift;
        if (zNegBlock)
            value |= 8u << BlockedShift;

        return value;
    }

    public override void SetFromInt(uint value)
    {
        xPosRaw = (value & 1u) != 0;
        xNegRaw = (value & 2u) != 0;
        zPosRaw = (value & 4u) != 0;
        zNegRaw = (value & 8u) != 0;

        uint blocked = value >> BlockedShift;
        xPosBlock = (blocked & 1u) != 0;
        xNegBlock = (blocked & 2u) != 0;
        zPosBlock = (blocked & 4u) != 0;
        zNegBlock = (blocked & 8u) != 0;

        ApplyBlocking();
    }

    /// <summary>Recomputes the inherited connection fields from the raw and blocked flags.</summary>
    public void ApplyBlocking()
    {
        xpos = xPosRaw && !xPosBlock;
        xneg = xNegRaw && !xNegBlock;
        zpos = zPosRaw && !zPosBlock;
        zneg = zNegRaw && !zNegBlock;
    }

    /// <summary>Rotates connections and blocked flags together, 90 degrees per step.</summary>
    public void Rotate(int rotation)
    {
        for (int step = 0; step < (rotation & 3); step++)
        {
            (xPosRaw, zPosRaw, xNegRaw, zNegRaw) = (zPosRaw, xNegRaw, zNegRaw, xPosRaw);
            (xPosBlock, zPosBlock, xNegBlock, zNegBlock) = (
                zPosBlock,
                xNegBlock,
                zNegBlock,
                xPosBlock
            );
        }

        ApplyBlocking();
    }
}

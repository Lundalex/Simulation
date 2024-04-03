using Unity.Mathematics;
public struct StickynessRequestStruct
{
    public int pIndex;
    public int StickyLineIndex;
    public float3 StickyLineDst;
    public float absDstToLineSqr;
    public float RBStickyness;
    public float RBStickynessRange;
};
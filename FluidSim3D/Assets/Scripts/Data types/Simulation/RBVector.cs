using Unity.Mathematics;
public struct RBVectorStruct
{
    public float2 Position;
    public float2 LocalPosition;
    public float3 ParentImpulse;
    public int ParentRBIndex;
    public int WallCollision;
};
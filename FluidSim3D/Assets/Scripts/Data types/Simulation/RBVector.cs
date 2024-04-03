using Unity.Mathematics;
public struct RBVectorStruct
{
    public float3 Position;
    public float3 LocalPosition;
    public float3 ParentImpulse;
    public int ParentRBIndex;
    public int WallCollision;
};
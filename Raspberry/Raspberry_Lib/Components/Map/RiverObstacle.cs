using Microsoft.Xna.Framework;

namespace Raspberry_Lib.Components;

internal class RiverObstacle
{
    public RiverObstacle(Vector2 iPosition, int iRockIndex, float iRotationRadians)
    {
        Position = iPosition;
        RockIndex = iRockIndex;
        RotationRadians = iRotationRadians;
    }

    public Vector2 Position { get; }
    public int RockIndex { get; }
    public float RotationRadians { get; }
}
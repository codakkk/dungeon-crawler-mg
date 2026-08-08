using System;

namespace DungeonCrawler.Core.Maths;

public static class MathUtils
{
    public static float RadToDeg => 180f / MathF.PI;
    public static float DegToRad => MathF.PI / 180f;
}
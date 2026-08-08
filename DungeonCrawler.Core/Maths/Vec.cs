// using System;
// using System.Numerics;
//
// namespace DungeonCrawler.Core.Maths;
//
// /// <summary>
// /// A Vector2 representation with X and Z instead of X/Y to avoid confusion (there's no real Y in world logic)
// /// </summary>
// /// <param name="X"></param>
// /// <param name="Z"></param>
// public readonly record struct Vec<T>(T X, T Z) where T : IFloatingPointIeee754<T>
// {
//     public static readonly Vec<T> Zero = new Vec<T>(T.Zero, T.Zero);
//     public static readonly Vec<T> One = new Vec<T>(T.One, T.One);
//     public static readonly Vec<T> UnitX = new Vec<T>(T.One, T.Zero);
//     public static readonly Vec<T> UnitZ = new Vec<T>(T.Zero, T.One);
//
//     public static Vec<T> operator +(Vec<T> a, Vec<T> b) => new(a.X + b.X, a.Z + b.Z);
//     public static Vec<T> operator -(Vec<T> a, Vec<T> b) => new(a.X - b.X, a.Z - b.Z);
//     public static Vec<T> operator -(Vec<T> a) => new(-a.X, -a.Z);
//     public static Vec<T> operator *(Vec<T> a, T b) => new(a.X * b, a.Z * b);
//     public static Vec<T> operator *(T b, Vec<T> a) => a * b;
//     public static Vec<T> operator /(Vec<T> a, T b) => new(a.X / b, a.Z / b);
//
//     public T LengthSquared => X * X + Z * Z;
//     
//     public T Length => T.Sqrt(LengthSquared);
//
//     public Vec<T> Normalized => this / Length;
//     
//     public Vec<T> Perpendicular => new (Z, -X);
//     
//     public static T Dot(Vec<T> a, Vec<T> b) => a.X * b.X + a.Z * b.Z;
//     
//     /// <summary>
//     /// 2D cross product - equals to |a||b|*sin(theta).
//     /// Sign of return tells which side b lies on relative to a
//     /// </summary>
//     public static T Cross(Vec<T> a, Vec<T> b) => a.X * b.Z - a.Z * b.X;
//
//     public Vec<T> Rotated(T radians)
//     {
//         var cos = T.Cos(radians);
//         var sin = T.Sin(radians);
//         return new Vec<T>(X * cos - Z * sin, X * sin + Z * cos);
//     }
//     
//     public override string ToString() => $"({X:0.###}, {Z:0.###})";
//
//     public Vector<T> ToNumerics()
//     {
//         return new Vector<T>([X, Z]); 
//     }
// }
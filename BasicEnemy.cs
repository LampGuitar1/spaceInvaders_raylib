using System;
using System.Numerics;
using Raylib_cs;

namespace LampGuitar.SInvadersClone
{
    public struct BasicEnemy
    {
        public Vector2 position;
        public Vector2 velocity;
        public float speed;
        public bool isAlive;
        public bool facing_right;
    }
}
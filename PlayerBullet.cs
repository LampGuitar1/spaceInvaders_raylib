using System;
using Raylib_cs;
using System.Numerics;

namespace LampGuitar.SInvadersClone
{
    public struct PlayerBullet
    {
        public Vector2 position;
        public Vector2 velocity;
        public float speed;
        public bool isActive;
    }

    public struct EnemyBullet
    {
        public Vector2 position;
        public Vector2 velocity;
        public bool isActive;
    }
}
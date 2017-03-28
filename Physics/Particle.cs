using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
    public class Particle : Unit
    {
        private float Lifetime;
        public bool Wreck;
        public float AngVel;

        public Particle(float drag, float elasticity)
            : base("Particle", 2, 2, 0.99f, 0.95f)
        {
        }

        protected override void RunLogic(float cycleTime)
        {
            Lifetime -= cycleTime;
            if (Lifetime < 0.0f || Velocity.Length() < 5.0f)
                Active = false;

            if (Wreck)
            {
                Facing = Gizmo.WrapAngle(Facing + AngVel);
            }
        }

        public void Activate(Vector3 pos, Vector3 mom, float lifetime)
        {
            LastPos = pos;
            Position = pos;
            Velocity = mom;
            Lifetime = lifetime;
            Active = true;
            Wreck = false;
        }

        public void ActWreck(float facing, float angVel)
        {
            Facing = facing;
            AngVel = angVel;
            Wreck = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Wreck)
                DrawWreck(spriteBatch);
            else
                Registry.CenterLine(spriteBatch, 4.0f, Color.White, Registry.CalcRenderPos(Position), 
                    Registry.CalcRenderPos(LastPos), Registry.GetDepth(Position.Z));
        }

        private void DrawWreck(SpriteBatch spriteBatch)
        {
            float dep = 32;
            float z = Position.Z - dep / 2;

            Color fill = Color.DarkGray;
            Vector2 pos = new Vector2(Position.X, Position.Y);

            for (int i = 0; i < 4; i++)
            {
                spriteBatch.Draw(
                    Registry.Spritesheet,
                    Registry.CalcRenderPos(new Vector3(pos, z)),
                    new Rectangle(i * 32, 0, 32, 32), Color.White, Facing, new Vector2(16),
                    2.0f, SpriteEffects.None, Registry.GetDepth(z));

                z += dep / 4;
                fill = Color.Lerp(fill, Color.Black, 0.25f);
            }
        }
    }
}

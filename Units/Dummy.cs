using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
    class Dummy : Unit
    {
        public Dummy(Vector3 pos)
            : base("Dummy", 32, 24, 0.9f, 0.0f)
        {
            Facing = (float)Math.Atan2(pos.Y - Registry.Avatar.Position.Y,
                pos.X - Registry.Avatar.Position.X);
            Position = pos;
            Active = true;
            Chassis = new Sprite(this);
        }

        protected override void RunLogic(float cycleTime)
        {
            Vector2 avaPos = 
                new Vector2(Registry.Avatar.Position.X, Registry.Avatar.Position.Y);
            Facing = Gizmo.TurnToFace(new Vector2(Position.X, Position.Y),
                avaPos, Facing, 0.1f);

            Velocity += new Vector3((float)Math.Cos(Facing) * 1.75f,
                (float)Math.Sin(Facing) * 1.75f, 0.0f);
            
            base.RunLogic(cycleTime);
        }

        public override void Draw(SpriteBatch batch)
        {
            Chassis.Draw(batch);
        }
    }
}

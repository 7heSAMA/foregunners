using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
    public class Particle : SimFrame
    {
        private float Lifetime;

		public Particle() : base(2, 2) { }

		protected override void RunLogic(float cycleTime)
        {
            Lifetime -= cycleTime;
            if (Lifetime < 0.0f || Velocity.Length() < 5.0f)
                Active = false;
        }

		public override void Draw(SpriteBatch spriteBatch)
        {
			Registry.CenterLine(spriteBatch, 4.0f, Color.White, Registry.CalcRenderPos(Position),
				Registry.CalcRenderPos(LastPos), Registry.GetDepth(Position.Z));
        }
    }
}

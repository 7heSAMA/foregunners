using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
    public class Sprite : IVisible
    {
		// implement factory pattern for array settings - 
		// either [,,] for stacked complex fl at sprites or 
		// simply [] for stacks flat sprites 

        protected IReal Parent { get; private set; }
        protected List<Rectangle> Textures;

        public Sprite(ISpatial parent)
        {
            Parent = parent;
            Textures = new List<Rectangle>();
            for (int i = 0; i < 4; i++)
                Textures.Add(new Rectangle(i * 32, 0, 32, 32));
        }

        public Sprite(ISpatial parent, List<Rectangle> textures)
        {
            Parent = parent;
            Textures = textures;
        }

        public void Draw(SpriteBatch batch)
        {
            int gap = 12; // GAP - WRONG

            Vector3 pos = Parent.Position;
            pos.Z -= gap * 2;
			
            foreach (Rectangle rect in Textures)
            {
				batch.Draw(
					Registry.Spritesheet,
					Registry.CalcRenderPos(pos),
					rect,
					Registry.Stage.LerpColor(Color.White, Parent.Position), 
					Parent.Facing, new Vector2(16),
                    2.0f, SpriteEffects.None, Registry.GetDepth(pos.Z));
                
                pos.Z += gap; 
            }
        }
    }
}

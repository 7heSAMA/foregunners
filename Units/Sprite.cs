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
            int gap = 8; // GAP - WRONG

            Vector3 pos = Parent.Position;
            pos.Z -= gap * 2;

            float lerp = new Vector2(pos.X, pos.Y).Length() / (Tile.FOOT * 20);

            foreach (Rectangle rect in Textures)
            {
                batch.Draw(
                    Registry.Spritesheet,
                    Registry.CalcRenderPos(pos),
                    rect,
                    Color.Lerp(Color.White, Registry.Burn, lerp), Parent.Facing, new Vector2(16),
                    2.0f, SpriteEffects.None, Registry.GetDepth(pos.Z));
                
                pos.Z += gap; 
            }
        }
    }
}

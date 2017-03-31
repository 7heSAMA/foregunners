using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
    public enum TileCollision
    {
        Empty,
        Solid,
        Slope,
        Landing,
    }
	
    [Flags]
    public enum SeamStyle
    {
        None = 0,
        Flat = 1 << 0,	//1
        Slope = 1 << 1,	//2
        Sty3 = 1 << 2,	//4
        Sty4 = 1 << 3,	//8
        Sty5 = 1 << 4,	//16
        Sty6 = 1 << 5,	//32
        Sty7 = 1 << 6,	//64
        Sty8 = 1 << 7,	//128
    }

    [Flags]
    public enum Neighbors
    {
        None = 0,
        TopLeft = 1 << 0,		//1
        TopCenter = 1 << 1,		//2
        TopRight = 1 << 2,		//4
        CenterLeft = 1 << 3,	//8
        CenterRight = 1 << 4,	//16
        BottomLeft = 1 << 5,	//32
        BottomCenter = 1 << 6,	//64
        BottomRight = 1 << 7,	//128
    }

    public class Subtile
    {
        public Rectangle Source { get; protected set; }
        public SpriteEffects Fx { get; protected set; }
        public float Rotation { get; protected set; }

        public Subtile (Rectangle source, float rads)
        {
            Source = source;
            Rotation = rads;
        }
    }

    public class Tile : IWorld
    {
        #region static fields
        public const int DEPTH = 48;
        public const int FOOT = 96;
        public const int DIVS = 4;

        public static readonly Vector2 Print = new Vector2(FOOT);
        public static readonly Vector2 Origin = new Vector2(FOOT / 2);
        public static readonly Vector3 Dimensions = new Vector3(Print, DEPTH);

        public static int GetArrayXY(float pos)
        { return (int)Math.Floor(pos / FOOT); }

        public static int GetArrayZ(float pos)
        { return (int)Math.Floor(pos / DEPTH); }
        
        public static BoundingBox Bounds(int x, int y, int z)
        {
            Vector3 min = new Vector3(x * FOOT, y * FOOT, z * DEPTH);
            return new BoundingBox(min, min + Dimensions);
        }

        public static BoundingBox Bounds(Vector3 pos)
        { return Bounds(GetArrayXY(pos.X), GetArrayXY(pos.Y), GetArrayZ(pos.Z)); }
        #endregion
		
        public Vector3 Position { get; protected set; }
        public TileCollision Collision;
        public SeamStyle Style;
        public Subtile[,,] Sprites;
        public Color Fill;
		
		public Tile()
		{
			Collision = TileCollision.Empty;
			Style = SeamStyle.None;

			// TODO: tile drawing method is still drawing these transparent tiles, 
			// which is a huge resource drain 
			Fill = Color.Transparent;
		}

        public Tile(Vector3 pos, TileCollision coll, SeamStyle style, Color fill)
        {
            Style = style;
            Collision = coll;
            Fill = fill;
        }

        public virtual void LoadContextualSource(Neighbors hood)
        {
			int edge = DIVS - 1;		// last index 
            int quarter = FOOT / DIVS;	// for size of sprite sources 
            Sprites = new Subtile[DIVS, DIVS, DIVS];
            
            Rectangle smooth = new Rectangle(0, 0, quarter, quarter);
            Rectangle outside = new Rectangle(quarter, 0, quarter, quarter);
            Rectangle straight = new Rectangle(quarter * 2, 0, quarter, quarter);
            Rectangle inside = new Rectangle(quarter * 3, 0, quarter, quarter);
			
			// Make the four edge subtiles straight sprites if no like tile above 
            if ((hood & Neighbors.TopCenter) != Neighbors.TopCenter)
            {
                for (int i = 0; i < Sprites.GetLength(0); i++)
                    Sprites[i, 0, edge] = new Subtile(straight, 0.0f);
            }
            if ((hood & Neighbors.BottomCenter) != Neighbors.BottomCenter)
            {
                for (int i = 0; i < Sprites.GetLength(0); i++)
                    Sprites[i, Sprites.GetLength(1) - 1, edge] = new Subtile(straight, MathHelper.Pi);
            }
            if ((hood & Neighbors.CenterLeft) != Neighbors.CenterLeft)
            {
                for (int i = 0; i < Sprites.GetLength(1); i++)
                    Sprites[0, i, edge] = new Subtile(straight, -MathHelper.Pi / 2.0f);
            }
            if ((hood & Neighbors.CenterRight) != Neighbors.CenterRight)
            {
                for (int i = 0; i < Sprites.GetLength(1); i++)
                    Sprites[Sprites.GetLength(0) - 1, i, edge] = new Subtile(straight, MathHelper.Pi / 2.0f);
            }

			// Make corner subtiles inside corners if adjacent tiles alike, but diagonal not
			if ((hood & (Neighbors.TopCenter | Neighbors.CenterRight)) == (Neighbors.TopCenter | Neighbors.CenterRight) &&
                (hood & Neighbors.TopRight) == Neighbors.None)
            {
                Sprites[Sprites.GetLength(0) - 1, 0, edge] = new Subtile(inside, 0.0f);
            }
            if ((hood & (Neighbors.BottomCenter | Neighbors.CenterRight)) == (Neighbors.BottomCenter | Neighbors.CenterRight) &&
                (hood & Neighbors.BottomRight) == Neighbors.None)
            {
                Sprites[Sprites.GetLength(0) - 1, Sprites.GetLength(1) - 1, 0] = new Subtile(inside, MathHelper.Pi / 2.0f);
            }
            if ((hood & (Neighbors.BottomCenter | Neighbors.CenterLeft)) == (Neighbors.BottomCenter | Neighbors.CenterLeft) &&
                (hood & Neighbors.BottomLeft) == Neighbors.None)
            {
                Sprites[0, Sprites.GetLength(1) - 1, edge] = new Subtile(inside, MathHelper.Pi);
            }
            if ((hood & (Neighbors.TopCenter | Neighbors.CenterLeft)) == (Neighbors.TopCenter | Neighbors.CenterLeft) &&
                (hood & Neighbors.TopLeft) == Neighbors.None)
            {
                Sprites[0, 0, edge] = new Subtile(inside, -MathHelper.Pi / 2.0f);
            }

			// Make corner subtiles outside corners if like-tiles adjacent on opposite sides
            if ((hood & (Neighbors.TopCenter | Neighbors.CenterRight)) == Neighbors.None)
            {
                Sprites[Sprites.GetLength(0) - 1, 0, edge] = new Subtile(outside, 0.0f);
            }
            if ((hood & (Neighbors.BottomCenter | Neighbors.CenterRight)) == Neighbors.None)
            {
                Sprites[Sprites.GetLength(0) - 1, Sprites.GetLength(1) - 1, edge] = new Subtile(outside, MathHelper.Pi / 2.0f);
            }
            if ((hood & (Neighbors.BottomCenter | Neighbors.CenterLeft)) == Neighbors.None)
            {
                Sprites[0, Sprites.GetLength(1) - 1, edge] = new Subtile(outside, MathHelper.Pi);
            }
            if ((hood & (Neighbors.TopCenter | Neighbors.CenterLeft)) == Neighbors.None)
            {
                Sprites[0, 0, edge] = new Subtile(outside, -MathHelper.Pi / 2.0f);
            }

            for (int subZ = 0; subZ < Sprites.GetLength(2); subZ++)
                for (int subY = 0; subY < Sprites.GetLength(1); subY++)
                    for (int subX = 0; subX < Sprites.GetLength(0); subX++)
                        if (Sprites[subX, subY, subZ] == null)
                            Sprites[subX, subY, subZ] = new Subtile(smooth, 0.0f);
        }
		
        public virtual void Draw(SpriteBatch batch, int x, int y, int z)
        {
            Vector2 basePos = new Vector2(x * FOOT, y * FOOT);

            if (Sprites == null)
            {
                float scaler = (z + 1) * DEPTH;

                for (int i = 0; i < DIVS; i++)
                {
                    Registry.DrawQuad(batch, basePos + Registry.Spin * scaler,
                        Color.Lerp(Fill, Registry.Burn, Registry.Lerp(scaler)),
                        0.0f, Print, Registry.GetDepth(scaler - i * (DEPTH / DIVS)), false);
                    scaler -= DEPTH / DIVS;
                }
            }
            else
            {
                Vector2 origin = new Vector2(FOOT / (DIVS * 2));
                basePos += origin;

                for (int subZ = 0; subZ < DIVS; subZ++)
                {
					float scaler = z * DEPTH + (subZ * DEPTH / DIVS);
                    float depth = Registry.GetDepth(scaler);
                    Vector2 offset = Vector2.Zero;

                    for (int subY = 0; subY < DIVS; subY++)
                    {
                        offset.X = 0.0f;
                        for (int subX = 0; subX < DIVS; subX++)
                        {
							if (Sprites[subX, subY, subZ] != null)
								batch.Draw(
									Registry.Tilesheet,
									basePos + offset + Registry.Spin * scaler,
									Sprites[subX, subY, subZ].Source,
									Color.Lerp(Fill, Registry.Burn, Registry.Lerp(scaler)),
									Sprites[subX, subY, subZ].Rotation,
									origin,
									1.0f,
									SpriteEffects.None,
									depth);
                            offset.X += FOOT / DIVS;
                        }
                        offset.Y += FOOT / DIVS;
                    }
                }
            }
        }

        public void Draw(SpriteBatch batch)
        {
            // TODO : Implement Sprite methods in Tile
            throw new NotImplementedException();
        }
    }

    public class Slope : Tile
    {
        private Point Incline;
        private Vector3 Center;

        public Slope(Vector3 pos, Point incline, SeamStyle style, Color fill)
            : base(pos, TileCollision.Slope, style, fill)
        {
            if (incline.X < -1 || incline.X > 1 || incline.Y < -1 || incline.Y > 1)
                throw new NotSupportedException();

            Center = pos + Dimensions / 2;
            Incline = incline;
        }

        public float GetHeight(Vector3 pos)
        {
            float z;

            if (Incline.X != 0 && Incline.Y != 0)
            {
                Vector2 peak = new Vector2(Center.X + (Incline.X * FOOT / 2),
                    Center.Y + (Incline.Y * FOOT / 2));

                z = Center.Z - DEPTH / 2;
                float distance = Vector2.Distance(peak, new Vector2(pos.X, pos.Y));
                if (distance <= FOOT)
                    z += (1.0f - (distance / FOOT)) * DEPTH;
            }
            else
            {
                z = Center.Z;
                int ratio = FOOT / DEPTH;

                z -= (Center.X - pos.X) * Incline.X / ratio;
                z -= (Center.Y - pos.Y) * Incline.Y / ratio;
            }

            z = MathHelper.Clamp(z, Center.Z - DEPTH / 2, Center.Z + DEPTH / 2);
            return z;
        }

		public override void LoadContextualSource(Neighbors hood)
		{
			base.LoadContextualSource(hood);

			for (int z = 0; z < DIVS; z++)
				for (int y = 0; y < DIVS; y++)
					for (int x = 0; x < DIVS; x++)
					{
						if (Incline.X == -1 && z + x > DIVS - 1 || Incline.X == 1 && z > x ||
							Incline.Y == -1 && z + y > DIVS - 1 || Incline.Y == 1 && z > y)
							Sprites[x, y, z] = null;
					}
		}
    }
}

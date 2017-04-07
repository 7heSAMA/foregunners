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
    public enum TileStyle
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
    public enum TileNeighbors
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
		// 1/4 the size of a tile 
		public static readonly int Quarter = Tile.FOOT / Tile.DIVS;
		// 1/8 the size of a tile
		public static readonly Vector2 Origin = new Vector2(Quarter / 2);

		public static Rectangle Smooth = new Rectangle(0, 0, Quarter, Quarter);
		public static Rectangle Outside = new Rectangle(Quarter, 0, Quarter, Quarter);
		public static Rectangle Straight = new Rectangle(Quarter * 2, 0, Quarter, Quarter);
		public static Rectangle Inside = new Rectangle(Quarter * 3, 0, Quarter, Quarter);

		public Rectangle Source { get; protected set; }
        public SpriteEffects Fx { get; protected set; }
        public float Rotation { get; protected set; }

        public Subtile (Rectangle source, float rads)
        {
            Source = source;
            Rotation = rads;
        }

		public Subtile()
		{
			Source = Rectangle.Empty;
			Rotation = 0.0f;
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
        public TileStyle Style;
        public Subtile[,,] Sprites;
        public Color Fill;
		
		public Tile()
		{
			Collision = TileCollision.Empty;
			Style = TileStyle.None;
			Sprites = null;
		}

        public Tile(Vector3 pos, TileCollision coll, TileStyle style, Color fill)
        {
			Position = pos;
            Collision = coll;
			Style = style;
			Sprites = new Subtile[DIVS, DIVS, DIVS];
			Fill = fill;
        }

		public virtual void LoadContextualSource(TileNeighbors hood)
		{
			if (Registry.Stage.GetStyle(Position + new Vector3(Vector2.Zero, DEPTH)).HasFlag(Style))
			{
				SmoothSubtiles();
				return;
			}

			// index of top top subtile by depth 
			int edge = DIVS - 1;
			
			// Make the four edge subtiles straight sprites if no like tile above 
            if ((hood & TileNeighbors.TopCenter) != TileNeighbors.TopCenter)
            {
                for (int i = 0; i < Sprites.GetLength(0); i++)
                    Sprites[i, 0, edge] = new Subtile(Subtile.Straight, 0.0f);
            }
            if ((hood & TileNeighbors.BottomCenter) != TileNeighbors.BottomCenter)
            {
                for (int i = 0; i < Sprites.GetLength(0); i++)
                    Sprites[i, Sprites.GetLength(1) - 1, edge] = new Subtile(Subtile.Straight, MathHelper.Pi);
            }
            if ((hood & TileNeighbors.CenterLeft) != TileNeighbors.CenterLeft)
            {
                for (int i = 0; i < Sprites.GetLength(1); i++)
                    Sprites[0, i, edge] = new Subtile(Subtile.Straight, -MathHelper.Pi / 2.0f);
            }
            if ((hood & TileNeighbors.CenterRight) != TileNeighbors.CenterRight)
            {
                for (int i = 0; i < Sprites.GetLength(1); i++)
                    Sprites[Sprites.GetLength(0) - 1, i, edge] = new Subtile(Subtile.Straight, MathHelper.Pi / 2.0f);
            }

			// Make corner subtiles inside corners if adjacent tiles alike, but diagonal not
			if ((hood & (TileNeighbors.TopCenter | TileNeighbors.CenterRight)) == (TileNeighbors.TopCenter | TileNeighbors.CenterRight) &&
                (hood & TileNeighbors.TopRight) == TileNeighbors.None)
            {
                Sprites[Sprites.GetLength(0) - 1, 0, edge] = new Subtile(Subtile.Inside, 0.0f);
            }
            if ((hood & (TileNeighbors.BottomCenter | TileNeighbors.CenterRight)) == (TileNeighbors.BottomCenter | TileNeighbors.CenterRight) &&
                (hood & TileNeighbors.BottomRight) == TileNeighbors.None)
            {
                Sprites[Sprites.GetLength(0) - 1, Sprites.GetLength(1) - 1, edge] = new Subtile(Subtile.Inside, MathHelper.Pi / 2.0f);
            }
            if ((hood & (TileNeighbors.BottomCenter | TileNeighbors.CenterLeft)) == (TileNeighbors.BottomCenter | TileNeighbors.CenterLeft) &&
                (hood & TileNeighbors.BottomLeft) == TileNeighbors.None)
            {
                Sprites[0, Sprites.GetLength(1) - 1, edge] = new Subtile(Subtile.Inside, MathHelper.Pi);
            }
            if ((hood & (TileNeighbors.TopCenter | TileNeighbors.CenterLeft)) == (TileNeighbors.TopCenter | TileNeighbors.CenterLeft) &&
                (hood & TileNeighbors.TopLeft) == TileNeighbors.None)
            {
                Sprites[0, 0, edge] = new Subtile(Subtile.Inside, -MathHelper.Pi / 2.0f);
            }

			// Make corner subtiles outside corners if like-tiles adjacent on opposite sides
            if ((hood & (TileNeighbors.TopCenter | TileNeighbors.CenterRight)) == TileNeighbors.None)
            {
                Sprites[Sprites.GetLength(0) - 1, 0, edge] = new Subtile(Subtile.Outside, 0.0f);
            }
            if ((hood & (TileNeighbors.BottomCenter | TileNeighbors.CenterRight)) == TileNeighbors.None)
            {
                Sprites[Sprites.GetLength(0) - 1, Sprites.GetLength(1) - 1, edge] = new Subtile(Subtile.Outside, MathHelper.Pi / 2.0f);
            }
            if ((hood & (TileNeighbors.BottomCenter | TileNeighbors.CenterLeft)) == TileNeighbors.None)
            {
                Sprites[0, Sprites.GetLength(1) - 1, edge] = new Subtile(Subtile.Outside, MathHelper.Pi);
            }
            if ((hood & (TileNeighbors.TopCenter | TileNeighbors.CenterLeft)) == TileNeighbors.None)
            {
                Sprites[0, 0, edge] = new Subtile(Subtile.Outside, -MathHelper.Pi / 2.0f);
            }

			SmoothSubtiles();
        }

		private void SmoothSubtiles()
		{
			for (int subZ = 0; subZ < Sprites.GetLength(2); subZ++)
				for (int subY = 0; subY < Sprites.GetLength(1); subY++)
					for (int subX = 0; subX < Sprites.GetLength(0); subX++)
						if (Sprites[subX, subY, subZ] == null)
						{
							if (subZ == DIVS - 1 || (
								subX == 0 || subX == DIVS - 1 || subY == 0 || subY == DIVS - 1))
								Sprites[subX, subY, subZ] = new Subtile(Subtile.Smooth, 0.0f);
							else
								Sprites[subX, subY, subZ] = new Subtile();
						}
		}
		
        public virtual void Draw(SpriteBatch batch)
        {
            Vector2 basePos = new Vector2(Position.X, Position.Y);
			basePos += Subtile.Origin;
			
			// created here so we don't make 64 temp variables
			Vector2 offset, oriPos;
			Rectangle source;

			for (int subZ = 0; subZ < DIVS; subZ++)
			{
				// TODO: increase subZ by 1 so sprites push up against bounding area? 
				float scaler = Position.Z + ((subZ + 1) * DEPTH / DIVS);
				float depth = Registry.GetDepth(scaler);
				offset = Vector2.Zero;

				for (int subY = 0; subY < DIVS; subY++)
				{
					offset.X = 0.0f;
					for (int subX = 0; subX < DIVS; subX++)
					{
						source = Sprites[subX, subY, subZ].Source;
						if (source != Rectangle.Empty)
						{
							oriPos = 
								Registry.CalcRenderPos(new Vector3(basePos + offset, scaler));
							batch.Draw(
								Registry.Tilesheet,
								oriPos,
								source,
								Registry.Stage.LerpColor(Fill, new Vector3(oriPos, scaler)),
								Sprites[subX, subY, subZ].Rotation,
								Subtile.Origin,
								1.0f,
								SpriteEffects.None,
								depth);
						}
						offset.X += Subtile.Quarter;
					}
					offset.Y += Subtile.Quarter;
				}
			}
        }

		public static void DrawBG(SpriteBatch batch, int x, int y, int z)
		{
			Vector2 basePos = new Vector2(x * FOOT, y * FOOT);
			basePos += Subtile.Origin;

			Vector2 offset, oriPos;

			float scaler = (z * DEPTH);
			float depth = Registry.GetDepth(scaler);
			offset = Vector2.Zero;

			for (int subY = 0; subY < DIVS; subY++)
			{
				offset.X = 0;
				for (int subX = 0; subX < DIVS; subX++)
				{
					oriPos = basePos + offset + Registry.Spin * scaler;
					batch.Draw(
						Registry.Tilesheet,
						oriPos,
						Subtile.Smooth,
						Registry.Stage.LerpColor(new Color(100, 90, 80), new Vector3(oriPos, scaler)),
						0.0f,
						Subtile.Origin,
						1.0f,
						SpriteEffects.None,
						depth);
					offset.X += Subtile.Quarter;
				}
				offset.Y += Subtile.Quarter;
			}
		}
    }

    public class TileSlope : Tile
    {
        private Point Incline;
        private Vector3 Center;

        public TileSlope(Vector3 pos, Point incline, TileStyle style, Color fill)
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

		public override void LoadContextualSource(TileNeighbors hood)
		{
			base.LoadContextualSource(hood);

			int mapX = GetArrayXY(Position.X);
			int mapY = GetArrayXY(Position.Y);
			int mapZ = GetArrayZ(Position.Z);

			for (int z = DIVS - 1; z >= 0; z--)
				for (int y = 0; y < DIVS; y++)
					for (int x = 0; x < DIVS; x++)
					{
						//	Check against the line of the slope
						if (Incline.X == -1 && z + x >= DIVS - 1 || Incline.X == 1 && z >= x ||
							Incline.Y == -1 && z + y >= DIVS - 1 || Incline.Y == 1 && z >= y)
						{
							// Check to see if an X slope is by nothing north/south 
							// or a Y slope by nothing east/west, to clone edge sprites
							if (z > 0 && (
								(y == 0 && Registry.Stage.GetStyle(mapX, mapY - 1, mapZ) == TileStyle.None) ||
								(y == DIVS - 1 && Registry.Stage.GetStyle(mapX, mapY + 1, mapZ) == TileStyle.None) ||
								(x == 0 && Registry.Stage.GetStyle(mapX - 1, mapY, mapZ) == TileStyle.None) ||
								(x == DIVS - 1 && Registry.Stage.GetStyle(mapX + 1, mapY, mapZ) == TileStyle.None)))
								Sprites[x, y, z - 1] = Sprites[x, y, z];
							
							Sprites[x, y, z] = new Subtile();
						}
					}
		}
    }
}

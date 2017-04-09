using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Foregunners
{
    public class Level
    {
        protected Tile[,,] Tiles;
		private int GroundLevel;

		private List<Vector3> Lights = new List<Vector3>();

		public Level(string map, IServiceProvider serviceProvider)
		{
			GroundLevel = 0;
			LoadTiles(map);
		}

		public void Initialize()
		{
			LoadContextSource();
		}

		#region Bounds and Collision
		public TileCollision GetCollision(Vector3 pos)
        {
            return GetCollision(
                Tile.GetArrayXY(pos.X), 
                Tile.GetArrayXY(pos.Y), 
                Tile.GetArrayZ(pos.Z));
        }

		public TileCollision GetCollision(int x, int y, int z)
		{
			if (z < 0)
				return TileCollision.Solid;
			else if (z > Depth - 1)
				return TileCollision.Empty;

			else if (x < 0 || x > Width - 1 || y > Height - 1 || y < 0)
			{
				if (z < GroundLevel)
					return TileCollision.Solid;
				else
					return TileCollision.Empty;
			}
			else
				return Tiles[x, y, z].Collision;
		}

		public TileStyle GetStyle(Vector3 pos)
		{
			return GetStyle(
				Tile.GetArrayXY(pos.X), 
				Tile.GetArrayXY(pos.Y), 
				Tile.GetArrayZ(pos.Z));
		}

		public TileStyle GetStyle(int x, int y, int z)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Depth)
                return TileStyle.None;

            return Tiles[x, y, z].Style;
        }

        public float GetSlope(Vector3 pos)
        {
            return GetSlope(pos, 
                Tile.GetArrayXY(pos.X), Tile.GetArrayXY(pos.Y), Tile.GetArrayZ(pos.Z));
        }

        public float GetSlope(Vector3 pos, int x, int y, int z)
        {
            if (GetCollision(x, y, z) == TileCollision.Landing)
            {
				// if this is a platform under another slope, 
				// return that slope's height 
                if (GetCollision(x, y, z + 1) == TileCollision.Slope)
                    return (Tiles[x, y, z + 1] as TileSlope).GetHeight(pos);
                else
                    return (z + 1) * Tile.DEPTH;
            }
            else
                return (Tiles[x, y, z] as TileSlope).GetHeight(pos);
        }

        /// <summary>
        /// modify to take input pos to vary gravity? 
        /// convert to V3 for horizontal/vertical forces? 
        /// </summary>
        public float Gravity
        { get { return -5.0f; } }

        /// <summary>
        /// Width of the level, in Tiles.
        /// </summary>
        public int Width
        {
            get { return Tiles.GetLength(0); }
        }

        /// <summary>
        /// Height of the level, in Tiles.
        /// </summary>
        public int Height
        {
            get { return Tiles.GetLength(1); }
        }

        /// <summary>
        /// Depth of the level, in Tiles.
        /// </summary>
        public int Depth
        {
            get { return Tiles.GetLength(2); }
        }
        #endregion

        protected void LoadContextSource()
        {
            for (int z = 0; z < Depth; z++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
						if (Tiles[x, y, z].Sprites == null)
							continue;

                        TileNeighbors hood = TileNeighbors.None;
						TileStyle style = Tiles[x, y, z].Style;
						
						if ((GetStyle(x - 1, y - 1, z) & style) != TileStyle.None)
							hood = hood | TileNeighbors.TopLeft;
						if ((GetStyle(x, y - 1, z) & style) != TileStyle.None)
							hood = hood | TileNeighbors.TopCenter;
						if ((GetStyle(x + 1, y - 1, z) & style) != TileStyle.None)
							hood = hood | TileNeighbors.TopRight;
						if ((GetStyle(x - 1, y, z) & style) != TileStyle.None)
							hood = hood | TileNeighbors.CenterLeft;
						if ((GetStyle(x + 1, y, z) & style) != TileStyle.None)
							hood = hood | TileNeighbors.CenterRight;
						if ((GetStyle(x - 1, y + 1, z) & style) != TileStyle.None)
							hood = hood | TileNeighbors.BottomLeft;
						if ((GetStyle(x, y + 1, z) & style) != TileStyle.None)
							hood = hood | TileNeighbors.BottomCenter;
						if ((GetStyle(x + 1, y + 1, z) & style) != TileStyle.None)
							hood = hood | TileNeighbors.BottomRight;
						
						Tiles[x, y, z].LoadContextualSource(hood);
                    }
                }
            }
        }
		
        public Vector3 CastMousePos(Vector3 mouse, Vector3 dir)
        {
            Vector3 result = mouse;

            while (mouse.Z < Depth * Tile.DEPTH)
            {
                TileCollision coll = GetCollision(mouse);
                if (coll == TileCollision.Solid || coll == TileCollision.Landing)
                    result = mouse + dir;
                else if (coll == TileCollision.Slope)
                {
                    int x = Tile.GetArrayXY(mouse.X);
                    int y = Tile.GetArrayXY(mouse.Y);
                    int z = Tile.GetArrayZ(mouse.Z);

                    float height = ((TileSlope)Tiles[x, y, z]).GetHeight(mouse) % Tile.DEPTH;
                    
                    Vector3 multipled = new Vector3(dir.X, dir.Y, 0.0f);
                    multipled *= (height / Tile.DEPTH);
                    multipled.Z = height;
                    
                    result = mouse + multipled;
                }
                
                mouse += dir;
            }
            
            return result;
        }
        
        public virtual void Update() { }

        public Vector2 Center
        {
            get { return new Vector2(Width * Tile.FOOT / 2, Height * Tile.FOOT / 2); }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
			Vector3 topLeft = Registry.OverlayToWorld(new Point(0, 0));
			Vector3 topRight = Registry.OverlayToWorld(new Point(Registry.Viewport.Width, 0));
			Vector3 botLeft = Registry.OverlayToWorld(new Point(0, Registry.Viewport.Height));
			Vector3 botRight = Registry.OverlayToWorld(new Point(Registry.Viewport.Width, 
				Registry.Viewport.Height));

			int xStart = Tile.GetArrayXY(Math.Min(
				Math.Min(topLeft.X, topRight.X), Math.Min(botLeft.X, botRight.X))) - 2;
			int xEnd = Tile.GetArrayXY(Math.Max(
				Math.Max(topLeft.X, topRight.X), Math.Max(botLeft.X, botRight.X))) + 2;

			if (xStart < 0)
				xStart = 0;
			if (xEnd > Width)
				xEnd = Width;

			int yStart = Tile.GetArrayXY(Math.Min(
				Math.Min(topLeft.Y, topRight.Y), Math.Min(botLeft.Y, botRight.Y))) - 2;
			int yEnd = Tile.GetArrayXY(Math.Max(
				Math.Max(topLeft.Y, topRight.Y), Math.Max(botLeft.Y, botRight.Y))) + 2;

			if (yStart < 0)
				yStart = 0;
			if (yEnd > Height)
				yEnd = Height;
			
			for (int z = 0; z < Depth; z++)
				for (int y = yStart; y < yEnd; y++)
					for (int x = xStart; x < xEnd; x++)
					{
						if (Tiles[x, y, z].Sprites != null)
						{
							Tiles[x, y, z].Draw(spriteBatch);
						}

						if (z == GroundLevel)
							Tile.DrawBG(spriteBatch, x, y, z);
					}
		}

		public Color LerpColor(Color color, Vector3 pos)
		{
			return Color.Lerp(color, Registry.DarkPurple,
				1.0f - pos.Z / (Depth * Tile.DEPTH));
		}

		private void LoadTiles(string mapName)
		{
			int i = 0;
			string root = "Content/Maps/" + mapName + "/";
			string path = root + mapName + i + ".txt";
			List<string> paths = new List<string>();

			Console.WriteLine("populating paths");

			while (System.IO.File.Exists(path))
			{
				paths.Add(path);
				path = root + mapName + (++i) + ".txt";
			}

			for (int z = 0; z < paths.Count; z++)
			{
				int width;
				List<string> Lines = new List<string>();

				using (System.IO.StreamReader reader = new System.IO.StreamReader(paths[z]))
				{
					string line = reader.ReadLine();
					width = line.Length;
					while (line != null)
					{
						Lines.Add(line);
						if (line.Length != width)
							throw new NotSupportedException(string.Format(
								"The length of line {0} is different from all preceding lines.", Lines.Count));
						line = reader.ReadLine();
					}
				}

				if (Tiles == null)
					Tiles = new Tile[width, Lines.Count, paths.Count];

				for (int y = 0; y < Height; ++y)
				{
					for (int x = 0; x < Width; ++x)
					{
						char TileType = Lines[y][x];
						Tiles[x, y, z] = LoadTile(TileType, x, y, z);
					}
				}
			}
		}

		private Tile LoadTile(char icon, int x, int y, int z)
		{
			Vector3 minCorner = new Vector3(x * Tile.FOOT, y * Tile.FOOT, z * Tile.DEPTH);
			Color BoneWhite = new Color(100, 90, 80);
			//BoneWhite = Color.Lerp(Color.LightGray, Color.MonoGameOrange, 0.1f);

			switch (icon)
			{
				case '.':
				case '-':
					return new Tile();

				case '#':
				case '_':
				case '"':
				case '~':
					return new Tile(minCorner, TileCollision.Solid, TileStyle.Flat, BoneWhite);

				case '0':
				case 'T':
					return new Tile(minCorner, TileCollision.Landing, TileStyle.Flat | TileStyle.Slope, BoneWhite);
					
				case '1':
					return new TileSlope(minCorner, new Point(1, -1), TileStyle.Slope, BoneWhite);
				case '2':
					return new TileSlope(minCorner, new Point(1, 0), TileStyle.Slope, BoneWhite);
				case '3':
					return new TileSlope(minCorner, new Point(1, 1), TileStyle.Slope, BoneWhite);
				case '4':
					return new TileSlope(minCorner, new Point(0, 1), TileStyle.Slope, BoneWhite);
				case '5':
					return new TileSlope(minCorner, new Point(-1, 1), TileStyle.Slope, BoneWhite);
				case '6':
					return new TileSlope(minCorner, new Point(-1, 0), TileStyle.Slope, BoneWhite);
				case '7':
					return new TileSlope(minCorner, new Point(-1, -1), TileStyle.Slope, BoneWhite);
				case '8':
					return new TileSlope(minCorner, new Point(0, -1), TileStyle.Slope, BoneWhite);
					
				case '@':
					Player player = new Player(minCorner + new Vector3(Tile.Origin, Tile.DEPTH));

					Registry.Avatar = player;
					Registry.UnitMan.Add(player);

					return new Tile();

				case 'x':
					Registry.UnitMan.Add(new Beetle(minCorner + new Vector3(
						Tile.Origin, Tile.DEPTH * 2)));
					return new Tile();

				default:
					throw new NotSupportedException(string.Format(
						"Unsupported character type {0} at {1}, {2}, depth of {3}.",
						icon, x, y, z));
			}
		}
	}
}

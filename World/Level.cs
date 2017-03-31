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

		public FollowCin Focus { get; private set; }
		public LanderCin Intro { get; private set; }
		public TrackingCin Tracking { get; private set; }

		public Level(string map, IServiceProvider serviceProvider)
		{
			LoadTiles(map);
			LoadContextSource();

			if (map != "dusk")
			{
				Intro = new LanderCin();
				Focus = new FollowCin(Registry.Avatar, true);
				Tracking = new TrackingCin(Registry.Avatar);

				Registry.Scripts.Add(Intro);
				Registry.Scripts.Add(Focus);
				Registry.Scripts.Add(Tracking);
			}

			if (map == "harbinger")
			{
				List<Tile> hide = new List<Tile>();
				for (int y = 11; y < 21; y++)
					for (int x = 17; x < 24; x++)
						hide.Add(Tiles[x, y, Depth - 2]);
				Rectangle area = new Rectangle(15 * Tile.FOOT, 11 * Tile.FOOT,
					9 * Tile.FOOT, 10 * Tile.FOOT);
			}
		}

		#region Bounds and Collision
		public TileCollision GetCollision(Vector3 pos)
        {
            return GetCollision(
                Tile.GetArrayXY(pos.X), 
                Tile.GetArrayXY(pos.Y), 
                Tile.GetArrayZ(pos.Z));
        }

        public SeamStyle GetStyle(int x, int y, int z)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Depth)
                return SeamStyle.None;

            return Tiles[x, y, z].Style;
        }

        public TileCollision GetCollision(int x, int y, int z)
        {
            int groundLevel = 0; 

            if (z < 0)
                return TileCollision.Solid;
            else if (z > Depth - 1)
                return TileCollision.Empty;

            else if (x < 0 || x > Width - 1 || y > Height - 1 || y < 0)
            {
                if (z < groundLevel)
                    return TileCollision.Solid;
                else
                    return TileCollision.Empty;
            }
            else
                return Tiles[x, y, z].Collision;
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
                if (GetCollision(x, y, z + 1) == TileCollision.Slope)
                    return (Tiles[x, y, z + 1] as Slope).GetHeight(pos);
                else
                    return (z + 1) * Tile.DEPTH;
            }
            else
                return (Tiles[x, y, z] as Slope).GetHeight(pos);
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
						// TODO: remove this once all tiles implement SeamStyle and subtiles 
                        //if (!(Tiles[x, y, z].Collision == TileCollision.Solid ||
                        //    Tiles[x, y, z].Collision == TileCollision.Landing))
                        //    continue;

                        Neighbors hood = Neighbors.None;
						SeamStyle style = Tiles[x, y, z].Style;
						
						if ((GetStyle(x - 1, y - 1, z) & style) != SeamStyle.None)
							hood = hood | Neighbors.TopLeft;
						if ((GetStyle(x, y - 1, z) & style) != SeamStyle.None)
							hood = hood | Neighbors.TopCenter;
						if ((GetStyle(x + 1, y - 1, z) & style) != SeamStyle.None)
							hood = hood | Neighbors.TopRight;
						if ((GetStyle(x - 1, y, z) & style) != SeamStyle.None)
							hood = hood | Neighbors.CenterLeft;
						if ((GetStyle(x + 1, y, z) & style) != SeamStyle.None)
							hood = hood | Neighbors.CenterRight;
						if ((GetStyle(x - 1, y + 1, z) & style) != SeamStyle.None)
							hood = hood | Neighbors.BottomLeft;
						if ((GetStyle(x, y + 1, z) & style) != SeamStyle.None)
							hood = hood | Neighbors.BottomCenter;
						if ((GetStyle(x + 1, y + 1, z) & style) != SeamStyle.None)
							hood = hood | Neighbors.BottomRight;
						
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

                    float height = ((Slope)Tiles[x, y, z]).GetHeight(mouse) % Tile.DEPTH;
                    
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

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            float angle = -Main.Cam.Rotation - MathHelper.Pi / 2.0f;
            Vector2 spin = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            
            spin *= Cinema.Perspective;
            // TODO: transfer this to a script
            Registry.Spin = spin;

            for (int z = 0; z < Depth; z++)
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        if (Tiles[x, y, z].Fill != Color.Transparent)
                            Tiles[x, y, z].Draw(spriteBatch, x, y, z);
        }

		private void LoadTiles(string mapName)
		{
			int i = 0;
			string root = "Content/Maps/" + mapName + "/";
			string path = root + mapName + i + ".txt";
			List<String> paths = new List<string>();

			Console.WriteLine("populating paths");

			while (System.IO.File.Exists(path))
			{
				paths.Add(path);
				path = root + mapName + (++i) + ".txt";
			}

			for (int z = 0; z < paths.Count; z++)
			{
				int width;
				List<String> Lines = new List<string>();

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
			Color BoneWhite = new Color(180, 170, 160);
			BoneWhite = Color.Lerp(Color.LightGray, Color.MonoGameOrange, 0.1f);

			switch (icon)
			{
				case '.':
					return new Tile();

				case '#':
				case '_':
				case '"':
				case '~':
					return new Tile(minCorner, TileCollision.Solid, SeamStyle.Flat, BoneWhite);

				case '0':
				case 'T':
					return new Tile(minCorner, TileCollision.Landing, SeamStyle.Flat | SeamStyle.Slope, BoneWhite);
					
				case '1':
					return new Slope(minCorner, new Point(1, -1), SeamStyle.Slope, BoneWhite);
				case '2':
					return new Slope(minCorner, new Point(1, 0), SeamStyle.Slope, BoneWhite);
				case '3':
					return new Slope(minCorner, new Point(1, 1), SeamStyle.Slope, BoneWhite);
				case '4':
					return new Slope(minCorner, new Point(0, 1), SeamStyle.Slope, BoneWhite);
				case '5':
					return new Slope(minCorner, new Point(-1, 1), SeamStyle.Slope, BoneWhite);
				case '6':
					return new Slope(minCorner, new Point(-1, 0), SeamStyle.Slope, BoneWhite);
				case '7':
					return new Slope(minCorner, new Point(-1, -1), SeamStyle.Slope, BoneWhite);
				case '8':
					return new Slope(minCorner, new Point(0, -1), SeamStyle.Slope, BoneWhite);
					
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

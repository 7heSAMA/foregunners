using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
    class TextMap : Level
    {
        public FollowCin Focus { get; private set; }
        public LanderCin Intro { get; private set; }
        public TrackingCin Tracking { get; private set; }

        public TextMap(string map, IServiceProvider serviceProvider)
        {
            LoadTiles(map);
            LoadContextSource2();

            if (map == "drill")
            {
                Registry.Scripts.Add(new DrillCin());
            }

            else if (map != "dusk")
            {
                Intro = new LanderCin();
                Focus = new FollowCin(Registry.Avatar, true);
                Tracking = new TrackingCin(Registry.Avatar);

                Registry.Scripts.Add(Intro);
                Registry.Scripts.Add(Focus);
                Registry.Scripts.Add(Tracking);
            }

            //if (map == "depot")
            //    Registry.Scripts.Add(new BasicWave(new Vector3(Center, 0), Width * Tile.FOOT));

            if (map == "harbinger")
            {
                List<Tile> hide = new List<Tile>();
                for (int y = 11; y < 21; y++)
                    for (int x = 17; x < 24; x++)
                        hide.Add(Tiles[x, y, Depth - 2]);
                Rectangle area = new Rectangle(15 * Tile.FOOT, 11 * Tile.FOOT,
                    9 * Tile.FOOT, 10 * Tile.FOOT);

                Registry.Scripts.Add(new TileHider(hide, Registry.Avatar, area));
            }
        }

        #region Loading

        #region procedural
        private void GenerateMap()
        {
            Random RNG = new Random();
            Tiles = new Tile[64, 32, 4];

            GenRiver(RNG);
            FillAroundRiver();
            BuildUp();
            MakePlains();
            FillSpace();
            GenRoad(RNG);

            Registry.Avatar = new Player(new Vector3(
                Width * Tile.FOOT / 2,
                Height * Tile.FOOT / 2,
                Depth * Tile.DEPTH));
        }

        private void GenRoad(Random rng)
        {
            for (int x = 0; x < Width; x++)
            {
                Tiles[x, 15, 2] = new Tile(TileCollision.Solid, Color.DimGray);
                Tiles[x, 16, 2] = new Tile(TileCollision.Solid, Color.DimGray);
            }
        }

        private void GenRiver(Random rng)
        {
            int upHeight = rng.Next(2, 5);
            int upWidth = rng.Next(4, 9);

            for (int x = 0; x < 32; x++)
                for (int y = upWidth; y < upWidth + upHeight; y++)
                    Tiles[x, y, 0] = new Tile(TileCollision.Solid, Color.LightSteelBlue);

            int downHeight = rng.Next(2, 5);
            int downWidth = rng.Next(4, 9);

            for (int x = 32; x < 64; x++)
                for (int y = 32 - (downWidth + downHeight); y < 32 - downWidth; y++)
                    Tiles[x, y, 0] = new Tile(TileCollision.Solid, Color.LightSteelBlue);

            int rivWidth = rng.Next(1, 4);

            for (int x = 32 - rivWidth; x < 32 + rivWidth; x++)
                for (int y = upWidth; y < 32 - downWidth; y++)
                    Tiles[x, y, 0] = new Tile(TileCollision.Solid, Color.LightSteelBlue);
        }

        private void FillAroundRiver()
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (Tiles[x, y, 0] == null && AnyNeighbors(x, y, 0, Color.LightSteelBlue))
                        Tiles[x, y, 0] = new Tile(TileCollision.Solid, Color.Gray);
        }

        private void BuildUp()
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (Tiles[x, y, 0] != null)
                    {
                        Tile toPlace;
                        if (NullNeighbors(x, y, 0))
                            toPlace = new Tile(TileCollision.Solid, Color.Gray);
                        else
                            toPlace = new Tile();

                        for (int z = 1; z < Depth - 1; z++)
                            Tiles[x, y, z] = toPlace;
                    }
        }

        private void MakePlains()
        {
            Color main = Color.Lerp(Color.IndianRed, Color.Black, 0.6f);
            Color cliff = Color.Lerp(main, Color.Lerp(Color.Indigo, Color.Black, 0.7f), 0.3f);
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    if (Tiles[x, y, 2] == null)
                        Tiles[x, y, 2] = new Tile(TileCollision.Solid, main);
                    else if (Tiles[x, y, 2].Fill == Color.Gray)
                        Tiles[x, y, 2] = new Tile(TileCollision.Solid, cliff);
                }
        }

        private void FillSpace()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    for (int z = 0; z < Depth; z++)
                        if (Tiles[x, y, z] == null)
                            Tiles[x, y, z] = new Tile();
        }

        private bool AnyNeighbors(int x, int y, int z, Color col)
        {
            Tile left = Tiles[Math.Max(0, x - 1), y, z];
            Tile right = Tiles[Math.Min(Width - 1, x + 1), y, z];
            Tile up = Tiles[x, Math.Max(0, y - 1), z];
            Tile down = Tiles[x, Math.Min(Height - 1, y + 1), z];

            if ((left != null && left.Fill == col) ||
                (right != null && right.Fill == col) ||
                (up != null && up.Fill == col) ||
                (down != null && down.Fill == col))
                return true;
            else
                return false;
        }

        private bool AnyNeighbors(int x, int y, int z)
        {
            if (Tiles[Math.Max(0, x - 1), y, z] != null ||
                Tiles[Math.Min(Width - 1, x + 1), y, z] != null ||
                Tiles[x, Math.Max(0, y - 1), z] != null ||
                Tiles[x, Math.Min(Height - 1, y + 1), z] != null)
                return true;
            else
                return false;
        }

        private bool NullNeighbors(int x, int y, int z)
        {
            if (Tiles[Math.Max(0, x - 1), y, z] == null ||
                Tiles[Math.Min(Width - 1, x + 1), y, z] == null ||
                Tiles[x, Math.Max(0, y - 1), z] == null ||
                Tiles[x, Math.Min(Height - 1, y + 1), z] == null)
                return true;
            else
                return false;
        }
        #endregion

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
                    return new Tile(TileCollision.Solid, Color.BurlyWood); 
                case '_':
                    return new Tile(TileCollision.Solid, Color.DimGray);
                case '"':
                    return new Tile(TileCollision.Solid, Color.DimGray);
                case '~':
                    return new Tile(TileCollision.Solid, Color.LightSteelBlue);

                case 'W':
                    return new Tile(TileCollision.Landing, Color.LightSteelBlue);

                case 'P':
                    return new Tile(TileCollision.Solid, Color.DarkSlateGray);
                case 'T':
                    return new Tile(TileCollision.Landing, BoneWhite);
                case 't':
                    return new Tile(TileCollision.Landing, Color.BurlyWood);
                    
                case '0':
                    return new Tile(TileCollision.Landing, BoneWhite);

                case '*':
                    Tile tile = new Tile(TileCollision.Solid, SeamStyle.Earth, Registry.BoneWhite);
                    return tile;
                    //return new Tile(TileCollision.Solid, BoneWhite);

                case '$':
                    return new Tile(TileCollision.Solid, Color.IndianRed);

                case '1':
                    return new Slope(BoneWhite, new Point(1, -1), minCorner);
                case '2':
                    return new Slope(BoneWhite, new Point(1, 0), minCorner);
                case '3':
                    return new Slope(BoneWhite, new Point(1, 1), minCorner);
                case '4':
                    return new Slope(BoneWhite, new Point(0, 1), minCorner);
                case '5':
                    return new Slope(BoneWhite, new Point(-1, 1), minCorner);
                case '6':
                    return new Slope(BoneWhite, new Point(-1, 0), minCorner);
                case '7':
                    return new Slope(BoneWhite, new Point(-1, -1), minCorner);
                case '8':
                    return new Slope(BoneWhite, new Point(0, -1), minCorner);

                case ')':
                    return new Diagonal(BoneWhite, new Point(1, 1));
                case '(':
                    return new Diagonal(BoneWhite, new Point(-1, 1));
                case ']':
                    return new Diagonal(BoneWhite, new Point(1, -1));
                case '[':
                    return new Diagonal(BoneWhite, new Point(-1, -1));

                case '-':
                    return new Tile();

                case '+':
                    return new Tile();

                case '@':
                    Player player = new Player(minCorner + new Vector3(
                        0, 0, Tile.DEPTH));

                    Registry.Avatar = player;
                    Registry.UnitMan.Add(player);

                    return new Tile();

                case 'x':
                    Registry.UnitMan.Add(new Beetle(minCorner + new Vector3(
                        Tile.FOOT / 2, Tile.FOOT / 2, Tile.DEPTH * 2)));
                    return new Tile();

                default:
                    throw new NotSupportedException(string.Format(
                        "Unsupported character type {0} at {1}, {2}, depth of {3}.",
                        icon, x, y, z));
            }
        }
        #endregion
    }
}

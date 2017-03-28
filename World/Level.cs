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
    public abstract class Level
    {
        protected Tile[,,] Tiles;
        
        #region Bounds and Collision
        public TileCollision GetCollision(Vector3 pos)
        {
            return GetCollision(
                Tile.GetArrayXY(pos.X), 
                Tile.GetArrayXY(pos.Y), 
                Tile.GetArrayZ(pos.Z));
        }

        public SeamStyle GetStlye(int x, int y, int z)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Depth)
                return SeamStyle.Lone;

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

        protected void LoadContextSource2()
        {
            for (int z = 0; z < Depth; z++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        if (Tiles[x, y, z].Collision != TileCollision.Solid &&
                            Tiles[x, y, z].Collision != TileCollision.Landing)
                            continue;

                        Neighbors hood = Neighbors.None;

                        if (GetStlye(x - 1, y - 1, z) == Tiles[x, y, z].Style)
                            hood = hood | Neighbors.TopLeft;
                        if (GetStlye(x, y - 1, z) == Tiles[x, y, z].Style)
                            hood = hood | Neighbors.TopCenter;
                        if (GetStlye(x + 1, y - 1, z) == Tiles[x, y, z].Style)
                            hood = hood | Neighbors.TopRight;
                        if (GetStlye(x - 1, y, z) == Tiles[x, y, z].Style)
                            hood = hood | Neighbors.CenterLeft;
                        if (GetStlye(x + 1, y, z) == Tiles[x, y, z].Style)
                            hood = hood | Neighbors.CenterRight;
                        if (GetStlye(x - 1, y + 1, z) == Tiles[x, y, z].Style)
                            hood = hood | Neighbors.BottomLeft;
                        if (GetStlye(x, y + 1, z) == Tiles[x, y, z].Style)
                            hood = hood | Neighbors.BottomCenter;
                        if (GetStlye(x + 1, y + 1, z) == Tiles[x, y, z].Style)
                            hood = hood | Neighbors.BottomRight;

                        Tiles[x, y, z].LoadContextualSource(hood);
                    }
                }
            }
        }

        protected void LoadContextSource()
        {
            for (int z = 0; z < Depth; z++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        if (Tiles[x, y, z].Collision != TileCollision.Solid)
                            continue;

                        Neighbors hood = Neighbors.None;

                        if (GetCollision(x - 1, y - 1, z) == TileCollision.Solid)
                            hood = hood | Neighbors.TopLeft;
                        if (GetCollision(x, y - 1, z) == TileCollision.Solid)
                            hood = hood | Neighbors.TopCenter;
                        if (GetCollision(x + 1, y - 1, z) == TileCollision.Solid)
                            hood = hood | Neighbors.TopRight;
                        if (GetCollision(x - 1, y, z) == TileCollision.Solid)
                            hood = hood | Neighbors.CenterLeft;
                        if (GetCollision(x + 1, y, z) == TileCollision.Solid)
                            hood = hood | Neighbors.CenterRight;
                        if (GetCollision(x - 1, y + 1, z) == TileCollision.Solid)
                            hood = hood | Neighbors.BottomLeft;
                        if (GetCollision(x, y + 1, z) == TileCollision.Solid)
                            hood = hood | Neighbors.BottomCenter;
                        if (GetCollision(x + 1, y + 1, z) == TileCollision.Solid)
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
            // should transfer this to a script, too 
            Registry.Spin = spin;

            for (int z = 0; z < Depth; z++)
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        if (Tiles[x, y, z].Fill != Color.Transparent)
                            Tiles[x, y, z].Draw(spriteBatch, x, y, z);
        }
    }
}

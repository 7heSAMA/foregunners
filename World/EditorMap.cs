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
    public class EditorMap : Level
    {
        public GuiGrid Gui;
        public EditorCin EditCin;

        public EditorMap() : this(16, 16, 2) { }

        public EditorMap(int Width, int Height, int Depth)
        {
            Gui = new GuiGrid();
            Registry.Scripts.Add(Gui);

            // very bad practice here 
            Gui.Placer.Map = this;

            EditCin = new EditorCin();
            Registry.Scripts.Add(EditCin);

            //Main.overlay.Editor = Gui;

            Tiles = new Tile[Width, Height, Depth];

            for (int z = 0; z < Depth; z++)
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        Tiles[x, y, z] = new Tile();
        }
        
        private void ExtendTiles(Gizmo.Axis axis, Gizmo.Side createOn, int amount)
        {
            // create an array of the dimensions of the old tilemap 
            int[] dim = new int[3] { Width, Height, Depth };

            // increment the axis by the parameter amount 
            dim[(int)axis] += amount;
            
            // create an array of tiles with the new dimensions 
            Tile[,,] newTiles = new Tile[dim[0], dim[1], dim[2]];

            // create starting points to copy the old tiles over and 
            // bump up the extension axis by the creation amount if the blank tiles 
            // are going to be on the low side 
            int[] counters = new int[3] { 0, 0, 0 };
            if (createOn == Gizmo.Side.Low)
                counters[(int)axis] += amount;

            // copy over all old tiles to their new positions 
            for (int z = counters[2]; z < dim[2]; z++)
                for (int y = counters[1]; y < dim[1]; y++)
                    for (int x = counters[0]; x < dim[0]; x++)
                        newTiles[x, y, z] = extensionTile(x, y, z);

            // assign new tiles
            Tiles = newTiles;

            // fill in holes 
            for (int z = 0; z < Depth; z++)
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        if (Tiles[x, y, z] == null)
                            Tiles[x, y, z] = new Tile();
        }

        private Tile extensionTile(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0)
                throw new NotSupportedException("What the fuck are you doing?");

            if (x >= Width || y >= Height || z >= Depth)
                return null;
            else
                return Tiles[x, y, z];
        }

        public override void Update()
        {
            if (Registry.KeyJustPressed(Keys.D1))
                ExtendTiles(Gizmo.Axis.X, Gizmo.Side.Low, 1);
            if (Registry.KeyJustPressed(Keys.D2))
                ExtendTiles(Gizmo.Axis.X, Gizmo.Side.High, 1);

            if (Registry.KeyJustPressed(Keys.D3))
                ExtendTiles(Gizmo.Axis.Y, Gizmo.Side.Low, 1);
            if (Registry.KeyJustPressed(Keys.D4))
                ExtendTiles(Gizmo.Axis.Y, Gizmo.Side.High, 1);

            // editor tools in separate pane 
            // color, shape, rotation, and map depth as UI widgets 
            // when you click to place, grabs info from widgets 
            // displays transparent tile 
            // eventually needs some way to differentiate platforms and non platforms 
            // along with a total overhaul of the tile system in general 

            // for extending map size: +- [axis] -+ times 3, with "clone" toggle 
            // (duplicates neighboring values) at top 
        }

        public void InjectTile(int x, int y, int z, Tile tile)
        {
            Tiles[x, y, z] = tile;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Registry.DrawQuad(spriteBatch, Vector2.Zero, Color.LightGray,
                0.0f, new Vector2(Width * Tile.FOOT, Height * Tile.FOOT), 1.0f, false);

            Gui.Placer.DrawPreview(spriteBatch);

            base.Draw(spriteBatch);
        }
    }
}

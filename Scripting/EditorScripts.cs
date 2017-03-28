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
    public class Resize //: Script
    {

    }
    
    public class Palette<T> : GuiScript
    {
        #region arrers 
        public abstract class Increment : BaScript
        {
            protected Palette<T> Parent;

            public Increment(Palette<T> parent)
            { Parent = parent; }
        }

        public class Next : Increment
        {
            public Next(Palette<T> parent) : base(parent) { }
            
            public override void Update()
            {
                Parent.Index -= 1;
                if (Parent.Index < 0)
                    Parent.Index = Parent.Keys.Count - 1;
            }
        }

        public class Prev : Increment
        {
            public Prev(Palette<T> parent) : base(parent) { }

            public override void Update()
            {
                Parent.Index += 1;
                if (Parent.Index >= Parent.Keys.Count)
                    Parent.Index = 0;
            }
        }
        #endregion

        public int Index { get; protected set; }
        public List<string> Keys { get; protected set; }
        public Dictionary<string, T> Settings { get; protected set; }

        public Prev PrevScript;
        public Next NextScript;
        public Button PrevButton, NextButton;
        Vector2 Origin;

        public Palette(Vector2 origin)
        {
            Index = 0;
            Keys = new List<string>();

            PrevScript = new Prev(this);
            NextScript = new Next(this);
            PrevButton = new Button(PrevScript, origin, "[<]");
            NextButton = new Button(PrevScript, 
                new Vector2(origin.X + GuiGrid.GRID * 5, origin.Y), "[>]");

            Origin = new Vector2(origin.X + GuiGrid.GRID, origin.Y);
        }

        public override void Update()
        {
            PrevButton.Update();
            NextButton.Update();
        }

        public T Current()
        {
            return Settings[Keys[Index]];
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            PrevButton.Draw(spriteBatch);
            NextButton.Draw(spriteBatch);
            
            spriteBatch.DrawString(Registry.Header, Keys[Index], Origin, Color.White, 0.0f,
                Vector2.Zero, 0.75f, SpriteEffects.None, 0.0f);
        }
    }

    #region palettes
    public class ColorPal : Palette<Color>
    {
        public ColorPal(Vector2 origin)
            : base(origin)
        {
            Settings = new Dictionary<string, Color>();
            Settings.Add("Bone", Color.Lerp(Color.LightGray, Color.MonoGameOrange, 0.1f));
            Settings.Add("Water", Color.LightSteelBlue);
            Settings.Add("Ground", Color.BurlyWood);
            Settings.Add("Metal", Color.DimGray);
            Settings.Add("Gravel", Color.LightGray);
            Settings.Add("Shadow", Registry.Burn);

            Keys = new List<string>(Settings.Keys);
        }
    }

    public class CollisionPal : Palette<TileCollision>
    {
        public CollisionPal(Vector2 origin)
            : base(origin)
        {
            Settings = new Dictionary<string, TileCollision>();
            Settings.Add("Solid", TileCollision.Solid);
            Settings.Add("Landing", TileCollision.Landing);
            Settings.Add("Incline", TileCollision.Slope);
            Settings.Add("Corner", TileCollision.Slope);

            Keys = new List<string>(Settings.Keys);
        }
    }

    public class RotatePal : Palette<Point>
    {
        public RotatePal(Vector2 origin)
            : base(origin)
        {
            Settings = new Dictionary<string, Point>();
            Settings.Add("Up X", new Point(1, 0));
            Settings.Add("Up XY", new Point(1, 1));
            Settings.Add("Up Y", new Point(0, 1));
            Settings.Add("dX - uY", new Point (-1, 1));
            Settings.Add("Down X", new Point(-1, 0));
            Settings.Add("Down XY", new Point(-1, -1));
            Settings.Add("Down Y", new Point(0, -1));
            Settings.Add("uX - dY", new Point(1, -1));

            Keys = new List<string>(Settings.Keys);
        }
    }
    #endregion

    public class PlaceTile : GuiScript
    {
        private GuiGrid Parent;
        private Point MouseGrid;
        public EditorMap Map;

        public PlaceTile(GuiGrid parent)
        {
            Parent = parent;
        }

        public override void Update()
        {
            Vector2 mousePos = //Registry.MouseTransformed() + Main.cam.Pos;
                Vector2.Zero;
            MouseGrid = new Point(Tile.GetArrayXY(mousePos.X), Tile.GetArrayXY(mousePos.Y));

            if (Registry.LeftClick() && Mouse.GetState().Position.X > GuiGrid.GRID * 8 && 
                MouseGrid.X >= 0 && MouseGrid.X < Map.Width &&
                MouseGrid.Y >= 0 && MouseGrid.Y < Map.Height)
            {
                Map.InjectTile(MouseGrid.X, MouseGrid.Y, 0, MakeTile());
            }
        }

        private Tile MakeTile()
        {
            switch (Parent.Collisions.Current())
            {
                case TileCollision.Empty:
                    return new Tile();
                case TileCollision.Landing:
                    return new Tile(TileCollision.Landing, Parent.Colors.Current());
                case TileCollision.Solid:
                    return new Tile(TileCollision.Solid, Parent.Colors.Current());
                case TileCollision.Slope:
                    Vector3 minCorner = new Vector3(
                        MouseGrid.X * Tile.FOOT, MouseGrid.Y * Tile.FOOT, 0);
                    return new Slope(
                        Parent.Colors.Current(), Parent.Rotations.Current(), minCorner);

                default:
                    throw new NotSupportedException();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
        }

        public void DrawPreview(SpriteBatch spriteBatch)
        {
            Vector2 mousePos = Vector2.Zero;
                //Registry.MouseTransformed();
            mousePos += Main.Cam.Pos;
            int x = Tile.GetArrayXY(mousePos.X);
            int y = Tile.GetArrayXY(mousePos.Y);

            Registry.DrawQuad(spriteBatch, new Vector2(x * Tile.FOOT, y * Tile.FOOT),
                Color.White, 0.0f, Tile.Print, 0.0f, false);
        }
    }
}

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
    public abstract class TextBox
    {
        public abstract string Contents { get; }
        public bool Active { get; protected set; }
        public SpriteFont Font { get; protected set; }
        public IScript OnClick;

        public Vector2 Size
        { get { return Font.MeasureString(Pad(Contents)); } }
        public Vector2 Position { get; protected set; }

        public TextBox(SpriteFont font)
        {
            Font = font;
        }

        public void Update()
        {
            if (Active && Registry.LeftClick() && 
                new Rectangle(
                    (int)Math.Floor(Position.X), 
                    (int)Math.Floor(Position.Y),
                    (int)Math.Floor(Size.X),
                    (int)Math.Floor(Size.Y)).Contains(Mouse.GetState().Position))
            {
            }
        }

        public void Reposition(Vector2 pos)
        {
            Position = pos;
        }

        public void Toggle()
        {
            Active = !Active;
        }

        public void Draw(SpriteBatch batch, Vector2 pos)
        {
            batch.DrawString(Font, Pad(Contents), pos, Color.White);
        }
        
        private string Pad(string data)
        {
            return " " + data + " ";
        }
    }

    public class Container
    {
    }

    public class ManualBox : TextBox
    {
        private string _contents;
        public override string Contents
        { get { return _contents; } }

        public ManualBox(string contents, SpriteFont font)
            :base(font)
        {
            _contents = contents;
        }
    }

    public class AutoBox : TextBox
    {
        public override string Contents
        { get { return Property.GetValue(Target).ToString(); } }

        private System.Reflection.PropertyInfo Property;
        private object Target;

        public AutoBox(object target, string propName, SpriteFont font)
            :base(font)
        {
            Target = target;
            Property = Target.GetType().GetProperty(propName);
        }
    }
    
    public class Tooltip : IComparable<Tooltip>
    {
        public const float LINEWIDTH = 2.0f;
        
        protected List<TextBox> Lines;
        protected IReal Target;

        protected Vector2 TitleSize;
        protected Vector2 BodySize;

        private bool _dropdown;
        public bool Dropdown
        {
            get { return _dropdown; }
            set
            {
                _dropdown = value;
                if (!value)
                    Size = TitleSize;
                else
                    Size = new Vector2(
                        Math.Max(TitleSize.X, BodySize.X),
                        TitleSize.Y + LINEWIDTH + BodySize.Y);
            }
        }

        public Vector2 Bulb
        { get; private set; }

        public Vector2 ScreenPos
        { get; private set; }

        public Vector2 PushedPos
        { get; private set; }

        public Vector2 Size
        { get; private set; }

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(
                    (int)Math.Floor(ScreenPos.X), 
                    (int)Math.Floor(ScreenPos.Y),
                    (int)Math.Floor(Size.X), 
                    (int)Math.Floor(Size.Y));
            }
        }
        
        public Tooltip(IReal target, TextBox title)
        {
            Target = target;
            Lines = new List<TextBox>();
            Lines.Add(title);

            TitleSize = Lines[0].Size;
            Dropdown = false;
        }

        public void AddEntry(TextBox toAdd)
        {
            Lines.Add(toAdd);
            MeasureBody();
            Dropdown = Dropdown;
        }

        private void MeasureBody()
        {
            BodySize = new Vector2(TitleSize.X, 0.0f);

            for (int i = 1; i < Lines.Count; i++)
            {
                Vector2 size = Lines[i].Size;
                BodySize.X = Math.Max(size.X, BodySize.X);
                BodySize.Y = Math.Max(size.Y, BodySize.Y);
            }

            BodySize.Y *= (Lines.Count - 1);
        }

        public void Update()
        {
            Bulb = Registry.WorldOnOverlay(Target.Position);
            Recalc(); 

            if (Registry.LeftClick() && Bounds.Contains(Registry.MouseV2))
            {
                Dropdown = !Dropdown;
                for (int i = 1; i < Lines.Count; i++)
                    Lines[i].Toggle();
                Recalc();
            }
        }

        private void Recalc()
        {
            float yPos = -TitleSize.Y;
            if (Dropdown)
                yPos -= BodySize.Y / 2;

            ScreenPos = Bulb + new Vector2(GuiGrid.GRID, yPos);
        }

        public void Draw(SpriteBatch batch)
        {
            if (Registry.Debug)
                Registry.DrawQuad(batch, new Vector2(Bounds.X, Bounds.Y), 
                    Color.Red, 0.0f, new Vector2(Bounds.Width, Bounds.Height), 0.0f, false);

            // draw bg
            Registry.DrawQuad(batch, ScreenPos, Registry.BurnThru, 0.0f, TitleSize, 0.0f, false);
            if (Dropdown)
                Registry.DrawQuad(batch, ScreenPos + new Vector2(0.0f, TitleSize.Y + LINEWIDTH), 
                    Registry.BurnThru, 0.0f, BodySize, 0.0f, false);

            // draw text 
            Vector2 pos = ScreenPos;

            int limit = 1;
            if (Dropdown)
                limit = Lines.Count;

            for (int i = 0; i < limit; i++)
            {
                Lines[i].Draw(batch, pos);
                pos.Y += Lines[i].Size.Y;
            }

            // draw lines 
            Vector2 tagStart = Bulb + new Vector2(GuiGrid.GRID, 0.0f);
            if (Dropdown)
                tagStart.Y -= BodySize.Y / 2;

            Vector2 tagEnd = tagStart + new Vector2(TitleSize.X, 0.0f);
            Registry.DrawLine(batch, LINEWIDTH, Color.White, Bulb, tagStart, 0.0f);
            Registry.DrawLine(batch, LINEWIDTH, Color.White, tagStart, tagEnd, 1.0f);
        }
        
        public int CompareTo(Tooltip other)
        {
            // compare by Bounds.Y
            throw new NotImplementedException();
        }
    }

    public static class TipSorter
    {
        private static List<Tooltip> Elements;
        private static Dictionary<Rectangle, List<Tooltip>> AreaSets;

        static TipSorter()
        {
            Elements = new List<Tooltip>();
            AreaSets = new Dictionary<Rectangle, List<Tooltip>>();
        }

        public static void Update()
        {
            foreach (Tooltip tip in Elements)
                tip.Update();

            // check for input 
            // if tooltip added/changed, sort
        }
        
        public static void Add(Tooltip toAdd)
        {
            Elements.Add(toAdd);
            Sort();
        }

        // should only recalculate when a tooltip goes on/offline 
        // or a dropdown is opened/closed
        // store offsets in individual tooltips 
        static private void Sort()
        {
            foreach (Tooltip tip in Elements)
            {
                bool inArea = false;
                Rectangle bounds;
                List<Tooltip> list;

                foreach (Rectangle area in AreaSets.Keys)
                {
                    if (area.Intersects(tip.Bounds))
                    {
                        inArea = true;
                        int height = area.Height + tip.Bounds.Height;
                        int width = area.Width + tip.Bounds.Width;

                        int count = AreaSets[area].Count();
                        int x = (area.X * count) + tip.Bounds.X;
                        int y = (area.Y * count) + tip.Bounds.Y;
                        x = (int)Math.Floor(x / count + 1.0f);
                        y = (int)Math.Floor(y / count + 1.0f);

                        bounds = new Rectangle(x, y, width, height);

                        list = AreaSets[area];
                        list.Add(tip);
                        AreaSets.Remove(area);
                        AreaSets.Add(bounds, list);
                    }
                }

                if (!inArea)
                {
                    list = new List<Tooltip>();
                    list.Add(tip);
                    bounds = tip.Bounds;
                    AreaSets.Add(bounds, list);
                }
            }

            for (int i = 0; i < AreaSets.Keys.Count; i++)
            {
                Rectangle area = AreaSets.Keys.ElementAt(i);
                for (int n = i + 1; n < AreaSets.Keys.Count; n++)
                {

                }
            }
        }
        
        static public void Draw(SpriteBatch batch)
        {
            foreach (Tooltip tip in Elements)
                tip.Draw(batch);
        }
    }
}

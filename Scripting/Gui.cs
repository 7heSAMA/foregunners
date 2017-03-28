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
    public interface INewGui
    {
        void Draw(SpriteBatch batch);
    }

    public abstract class GuiScript : BaScript, IVisible
    {
        public abstract void Draw(SpriteBatch batch);
    }
    
    public class Frame
    {

    }

    public class Window : GuiScript
    {
        #region fields
        public const int GRID = 32;
        public List<GuiScript> Elements;
        public string Name { get; protected set; }

        public Point TopLeft { get; private set; }
        public Point BotRight { get; private set; }
        protected int Width { get; private set; }
        protected int Height { get; private set; }
        #endregion

        public Window(int x, int y, int width, int height)
            : this(new Point(x, y), new Point(x + width, y + height)) { }

        public Window (Point topLeft, Point botRight)
        {
            TopLeft = topLeft;
            BotRight = botRight;
        }

        public void ChangePos(Point pos)
        {

        }

        protected void ChangeDim(Point topLeft, Point botRight)
        {
            topLeft.X = Math.Max(topLeft.X, 0);
            topLeft.Y = Math.Max(topLeft.Y, 0);
            botRight.X = Math.Min(botRight.X, Main.viewport.Width - GRID);
            botRight.Y = Math.Min(botRight.Y, Main.viewport.Height - GRID);

            if (botRight.X <= topLeft.X)
                throw new ArgumentOutOfRangeException(
                    "TL: " + topLeft.X + ", BR: " + botRight.X);
            if (botRight.Y <= topLeft.Y)
                throw new ArgumentOutOfRangeException(
                    "TL: " + topLeft.Y + ", BR: " + botRight.Y);

            TopLeft = topLeft;
            BotRight = botRight;
            Width = botRight.X - topLeft.X;
            Height = botRight.Y - topLeft.Y;
        }

        public override void Update()
        {
        }

        public override void Draw(SpriteBatch batch)
        {
            Registry.DrawQuad(batch, new Vector2(TopLeft.X, TopLeft.Y), 
                Registry.Burn, 0.0f, new Vector2(Width, Height), 0.0f, false);
        }
    }
    
    public abstract class SidePane : GuiScript
    {
        // metadata (inc. save/load)
        // tiles 
        // scripting 
        // editor tools (
    }

    public class GuiGrid : GuiScript
    {
        public Rectangle Area;
        public List<GuiScript> Elements;
        public const int GRID = 32;

        public ColorPal Colors;
        public CollisionPal Collisions;
        public RotatePal Rotations;
        public PlaceTile Placer;

        // symbols:     _ minimize, ^ maximize, X close 
        //              [<]/[>] prev/next, 

        public GuiGrid()
        {
            editor();
        }

        private void mainMenu()
        {
            Active = true;
            Area = new Rectangle(0, 0, Main.viewport.Height / GRID, GRID * 15);
            Vector2 origin = new Vector2(GRID);
        }

        private void editor()
        {
            Active = true;
            Area = new Rectangle(0, 0, GRID * 8, GRID * 15);
            Vector2 origin = new Vector2(GRID);

            Placer = new PlaceTile(this);

            // depth/width/height modifiers (height 3)
            // tile preview (height 3 with height 1 buffer above/below)

            origin.Y += GRID * 8;
            Colors = new ColorPal(origin);
            origin.Y += GRID;
            Collisions = new CollisionPal(origin);
            origin.Y += GRID;
            Rotations = new RotatePal(origin);
            
            Elements = new List<GuiScript>();

            Elements.Add(Colors);
            Elements.Add(Collisions);
            Elements.Add(Rotations);

            Elements.Add(Placer);
        }

        public override void Update()
        {
            if (!Active)
            {
                if (Registry.KeyJustPressed(Keys.Escape))
                    Active = true;
            }
            else
            {
                if (Registry.KeyJustPressed(Keys.Space))
                {
                    Active = false;
                    return;
                }
                
                foreach (IScript ele in Elements)
                    ele.Update();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
            {
                Registry.DrawQuad(spriteBatch, Vector2.Zero, Color.Black, 0.0f,
                    new Vector2(Area.Width, Area.Height), 0.0f, false);

                // draw preview tile 

                foreach (GuiScript ele in Elements)
                    ele.Draw(spriteBatch);
            }
        }
    }

    public abstract class Clicker : BaScript
    {
        public Vector2 Position { get; protected set; }
        public Rectangle Area { get; protected set; }
        public string Message { get; protected set; }

        public Clicker(Vector2 pos, string message)
        {
            Position = pos;
            Area = new Rectangle(
                (int)(Math.Floor(pos.X)), 
                (int)(Math.Floor(pos.Y)),
                GuiGrid.GRID, GuiGrid.GRID);
            Message = message;
        }

        public abstract void Click();

        public override void Update()
        {
            if (Registry.LeftClick() && Area.Contains(Mouse.GetState().Position))
                Click();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 pos = Position + new Vector2(2.0f);

            Registry.DrawQuad(spriteBatch, Position, Color.DarkSlateGray, 0.0f,
                new Vector2(Area.Width, Area.Height), 0.0f, false);
            spriteBatch.DrawString(Registry.Header, Message, pos, Color.White, 0.0f,
                Vector2.Zero, 0.70f, SpriteEffects.None, 0.0f);
        }
    }

    public class Button : Clicker
    {
        public IScript OnClick { get; protected set; }

        public Button(IScript onClick, Vector2 pos, string message) :
            base(pos, message)
        {
            OnClick = onClick;
        }

        public override void Click()
        {
            OnClick.Update();
        }
    }

    public class Toggle : Clicker
    {
        public bool Checked { get; protected set; }

        public Toggle(IScript onClick, Vector2 pos)
            : base(pos, "[N]")
        {
            Checked = false;
        }

        public override void Click()
        {
            if (Checked)
                Message = "[N]";
            else
                Message = "[Y]";
            Checked = !Checked;
        }
    }
}

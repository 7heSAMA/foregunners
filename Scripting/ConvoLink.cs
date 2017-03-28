using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
    public abstract class ConvoLink
    {
        public static float PSIZE = 64.0f;

        protected bool NPC, TexIn, Closeout;
        protected List<String> Lines;
        public Vector2 TextPos;
        private Vector2 Position;
        public bool InUse { get; protected set; }
        public string Name { get; private set; }
        private float Desired;

        // convo links are set by files that open with a pilot name, 
        // then a number of lines that will be delivered in order 
        protected ConvoLink(string file, bool npc)
        {
            NPC = npc;
            TexIn = false;
            Lines = new List<string>();
            TextPos = new Vector2(PSIZE * 2.5f, 0.0f);

            if (NPC)
            {
                TextPos = new Vector2(PSIZE * 2.0f, PSIZE / 2.0f);
                Position = new Vector2(-PSIZE, PSIZE / 2.0f);
                Desired = PSIZE / 2.0f;
            }
            else
            {
                TextPos = new Vector2(PSIZE, Main.viewport.Height - PSIZE * 1.5f);
                Position = new Vector2(Main.viewport.Width, Main.viewport.Height - PSIZE * 1.5f);
                Desired = Main.viewport.Width - PSIZE * 1.5f;
            }

            using (System.IO.StreamReader reader = new System.IO.StreamReader(file))
            {
                Name = reader.ReadLine();
                string line = reader.ReadLine();
                while (line != null)
                {
                    Lines.Add(line);
                    line = reader.ReadLine();
                }
            }
        }

        public void Update(bool input)
        {
            if (input)
                Input();

            if (!TexIn)
                SlideIn();
            else if (Closeout)
                SlideOut();
            else
                UpdateText();
        }

        protected abstract void UpdateText();

        private void SlideIn()
        {
            if (Math.Abs(Position.X - Desired) < Overlay.SLIDE)
            {
                Position.X = Desired;
                TexIn = true;
            }
            else
                Position.X += MathHelper.Clamp(Desired - Position.X, -Overlay.SLIDE, Overlay.SLIDE);
        }

        private void SlideOut()
        {
            if (Math.Abs(Position.X - Desired) < Overlay.SLIDE)
            {
                Position.X = Desired;
                InUse = false;
                //if (NextLink == null)
                //    Game1.overlay.EndConvo();
            }
            else
                Position.X += MathHelper.Clamp(Desired - Position.X, -Overlay.SLIDE, Overlay.SLIDE);
        }

        public void Activate()
        {
            InUse = true;
            TexIn = false;
            Closeout = false;
        }

        protected void Deactivate(ConvoLink next)
        {
            Closeout = true;

            Console.WriteLine(next);

            //if (next != null)
            //    Main.overlay.Inject(next);

            if (NPC)
                Desired = -PSIZE;
            else
                Desired = Main.viewport.Width + PSIZE;
        }

        // skip on lines, select on choices (potentially deactivate here)
        public abstract void Input();

        public abstract void Draw(SpriteBatch spriteBatch);

        public override string ToString()
        {
            return Name + ", " + Lines.Count + " lines.";
        }

        protected void DrawHeadshot(SpriteBatch spriteBatch)
        {
            Color color = Color.White;
            if (!NPC)
                color = Color.DarkRed;
            Registry.DrawQuad(spriteBatch, Position, color, 0.0f, new Vector2(PSIZE),
                0.95f, false);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Foregunners
{
    public class Overlay
    {
        public bool Injected = false; // this shit

        public GuiGrid Editor;

        public const float SLIDE = 16.0f;
        public bool Conversing { get; private set; }
        private List<ConvoLink> ActiveLinks, toAdd, toCut;
        private ContentManager Content;
        private float BarHeight, DesiredHeight;
        private bool BarSet;

        public Overlay(ContentManager content)
        {
            Content = content;
            ActiveLinks = new List<ConvoLink>();
            toAdd = new List<ConvoLink>();
            toCut = new List<ConvoLink>();

            BarHeight = Main.viewport.Height / 2.0f;
            DesiredHeight = 0.0f;
        }

        public void Update(GameTime gameTime)
        {
            if (!BarSet)
            {
                if (Math.Abs(BarHeight - DesiredHeight) < SLIDE)
                {
                    BarHeight = DesiredHeight;
                    BarSet = true;
                }
                else
                    BarHeight += MathHelper.Clamp(DesiredHeight - BarHeight, 
                        -Overlay.SLIDE, Overlay.SLIDE);
            }
            else
            {
                bool input = false;
                if (Registry.KeyJustPressed(Microsoft.Xna.Framework.Input.Keys.Enter))
                    input = true;
                
                foreach (ConvoLink node in ActiveLinks)
                {
                    node.Update(input);

                    if (!node.InUse)
                        toCut.Add(node);
                }

                foreach (ConvoLink node in toCut)
                    ActiveLinks.Remove(node);
                toCut.Clear();

                foreach (ConvoLink node in toAdd)
                    ActiveLinks.Add(node);
                toAdd.Clear();

                if (ActiveLinks.Count == 0)
                    EndConvo();
            }
        }

        public void Inject(ConvoLink node)
        {
            Injected = true;

            BarHeight = ConvoLink.PSIZE * 2.0f;

            Console.WriteLine("injecting " + node.ToString());

            if (ActiveLinks.Count == 0)
            {
                BarSet = false;
                BarHeight = 0.0f;
                DesiredHeight = ConvoLink.PSIZE * 2.0f;
            }

            node.Activate();
            toAdd.Add(node);
        }

        public void EndConvo()
        {
            DesiredHeight = 0.0f;
            BarSet = false;
            Injected = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawBar(spriteBatch, Vector2.Zero);
            DrawBar(spriteBatch, new Vector2(0.0f, Main.viewport.Height - BarHeight));

            foreach (ConvoLink link in ActiveLinks)
                link.Draw(spriteBatch);

            if (Editor != null)
                Editor.Draw(spriteBatch);
        }

        private void DrawBar(SpriteBatch spriteBatch, Vector2 pos)
        {
            Registry.DrawQuad(spriteBatch, pos, Color.Black, 0.0f, new Vector2(
                Main.viewport.Width, BarHeight), 1.0f, false);
        }
    }
}

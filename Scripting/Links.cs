using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
    public class LineLink : ConvoLink
    {
        private int Index;
        private string Current;
        private ConvoLink Next;

        public LineLink(string file, bool npc, ConvoLink next)
            : base(file, npc)
        {
            Next = next;
            Current = "";
        }

        protected override void UpdateText()
        {
            if (Current.Length < Lines[Index].Length)
                Current = Lines[Index].Substring(0, Current.Length + 1);

            //Console.WriteLine(Game1.reg.Header.MeasureString(Current) + ", vs " + 
            //    (Game1.viewport.Width - PSIZE * 2.5f));
            
        }

        public override void Input()
        {
            Console.WriteLine("Input!");

            if (Current.Length < Lines[Index].Length)
                Current = Lines[Index];
            else
            {
                Index += 1;
                Current = "";
                if (Index >= Lines.Count)
                    Deactivate(Next);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            StringBuilder builder = new StringBuilder(Current);

            string current = Gizmo.WrapWord(builder, Registry.Header,
                new Rectangle(0, 0, (int)(Main.viewport.Width - PSIZE * 3), Main.viewport.Height));
            
            spriteBatch.DrawString(Registry.Header, current, TextPos, Color.White);
            
            DrawHeadshot(spriteBatch);
        }
    }

    // displays 2-3 choices that each correspond to another ConvoLink
    public class ChoiceLink : ConvoLink
    {
        // choices[i] corresponds to Nodes[i] in Overlay
        public ChoiceLink(string file, bool npc, ConvoLink[] choices)
            : base(file, npc)
        { }

        protected override void UpdateText()
        { }

        public override void Input()
        { }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            Vector2 textPos = TextPos; 

            // + (textPos * 2) for each choice 
            // if selected, store length, add "> ", subtract length diff from positon.x

            DrawHeadshot(spriteBatch);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
    public class CamVP : BaScript
    {
        #region fields 
        public Viewport Viewport { get; private set; }
        public Camera2D Cam { get; private set; }

        public Stack<Cinema> Position { get; private set; }
        public Stack<Cinema> Rotation { get; private set; }
        public Stack<Cinema> Perspective { get; private set; }
        public Stack<Cinema> Zoom { get; private set; }
        #endregion

        #region constructors and initializers
        public CamVP()
        {
            /*Viewport = new Viewport(0, 0,
                Game1.Graphics.PreferredBackBufferWidth,
                Game1.Graphics.PreferredBackBufferHeight);
            Cam = new Camera2D(Game1.Graphics);*/
        }

        public CamVP(Viewport vp)
        {
            Viewport = vp;
            //Cam = new Camera2D(Game1.Graphics);
        }

        public CamVP(Viewport vp, Camera2D cam)
        {
            Viewport = vp;
            Cam = cam;
        }

        public void InitDefaultCin()
        {
            GenericCin generic = new GenericCin();
            Zoom.Push(generic);
            Position.Push(generic);
            Rotation.Push(generic);
            Perspective.Push(generic);
        }
        
        public override void Update()
        {
            Zoom.Peek().Update();
        }

        // [Field stack].Push([Field stack].Pop().Next
        // assuming next != null 
        #endregion
    }
}

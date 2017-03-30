using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Foregunners
{
    public abstract class Cinema : IScript
    {
        #region fields
        protected bool DoRot, DoPos, DoZoom;

        /// <summary>
        /// Controls rotation for the Camera2D. 
        /// </summary>
        public static float Rotation
        {
            get { return _rotation; }
            protected set { _rotation = Gizmo.WrapAngle(value); }
        }
        private static float _rotation;

        /// <summary>
        /// Modifies the relation between Z world to XY screen positions.
        /// </summary>
        public static float Perspective
        {
            get { return _perspec; }
            protected set { _perspec = MathHelper.Clamp(value, Bird, Worm); }
        }
        private static float _perspec;
        protected static readonly float Bird, Worm;
        
        /// <summary>
        /// Used for calculations of XYZ world to XY screen positions.
        /// </summary>
        public static float Zoom
        {
            get { return _zoom; }
            protected set { _zoom = MathHelper.Clamp(value, Micro, Macro); }
        }
        private static float _zoom;
        protected static readonly float Micro, Macro;

        /// <summary>
        /// Controls world position for the Camera2D.
        /// </summary>
        public static Vector2 Position
        {
            get { return _position; }
            protected set { _position = value; }
        }
        private static Vector2 _position;

        /// <summary>
        /// Calculates focal lens for the Camera2D. 
        /// </summary>
        public static Vector3 Lens()
        {
            return new Vector3(Zoom, Zoom * (float)Math.Cos(Perspective), 1.0f);
        }
        #endregion

        #region constructors, initialization
        /// <summary>
        /// Static constructor setting Zoom and Perspective boundaries
        /// as well as baselining all cam values.
        /// </summary>
        static Cinema()
        {
            Bird = 0.0f;
            Worm = MathHelper.Pi / 4.0f;
            Micro = 0.25f;
            Macro = 1.0f;

            Rotation = 0.0f;
            Perspective = 0.0f;
            Zoom = 0.5f;
            Position = Vector2.Zero;
        }
        #endregion

        #region actual scripting
        public abstract void Camerawork();

        public void Update()
        {
            Camerawork();

            if (DoRot)
                Main.Cam.Rotation = Rotation;
            if (DoPos)
                Main.Cam.Pos = Position;
            if (DoZoom)
                Main.Cam.Zoom = Lens();
        }
        #endregion
    }

    public class EditorCin : Cinema
    {
        float Zint = 2;

        public EditorCin()
        {
            DoZoom = true;
            DoRot = true;
            DoPos = true;
        }

        public override void Camerawork()
        {
            if (Registry.KeyJustPressed(Keys.Left))
                Zint /= 2;
            if (Registry.KeyJustPressed(Keys.Right))
                Zint *= 2;

            Zint = MathHelper.Clamp(Zint, Micro, Macro);
            Zoom = 1.0f / Zint;

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                Perspective = Bird;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                Perspective = Worm;

            Vector2 pos = Position;
            if (Registry.KeyJustPressed(Keys.A))
                pos.X -= Tile.FOOT;
            if (Registry.KeyJustPressed(Keys.D))
                pos.X += Tile.FOOT;
            if (Registry.KeyJustPressed(Keys.W))
                pos.Y -= Tile.FOOT;
            if (Registry.KeyJustPressed(Keys.S))
                pos.Y += Tile.FOOT;

            pos.X = MathHelper.Clamp(pos.X, 0, (Registry.Stage.Width - 1) * Tile.FOOT);
            pos.Y = MathHelper.Clamp(pos.Y, 0, (Registry.Stage.Height - 1) * Tile.FOOT);

            Position = pos;
        }
    }

    public class LanderCin : Cinema
    {
        public LanderCin()
        {
            DoZoom = true;
            DoRot = true;
        }

        public override void Camerawork()
        {
            Zoom = MathHelper.SmoothStep(
                0.25f, 1.0f, (float)Registry.gameTime.TotalGameTime.TotalSeconds / 7f);
            Perspective = MathHelper.SmoothStep(
                0.0f, MathHelper.Pi / 4.0f, 
                (float)Registry.gameTime.TotalGameTime.TotalSeconds / 5f);
            Rotation = MathHelper.SmoothStep(
                0.0f, MathHelper.Pi / 4.0f, 
                (float)Registry.gameTime.TotalGameTime.TotalSeconds / 6f);
        }
    }
	
    public class GenericCin : Cinema
    {
        public GenericCin()
        {
            DoPos = true;
            DoRot = true;
            DoZoom = true;
        }

        public override void Camerawork()
        {
            Rotation = MathHelper.Pi / 4.0f;
            Perspective = Worm;
            Zoom = 0.5f;
        }
    }

    public class FollowCin : Cinema
    {
        protected Unit Follow;

        public FollowCin(Unit toFollow, bool jumpTo)
        {
            DoPos = true;
            ChangeTarget(toFollow, jumpTo);
        }

        public void ChangeTarget(Unit toFollow, bool jumpTo)
        {
            Follow = toFollow;
            if (jumpTo)
                Position = Registry.CalcRenderPos(toFollow.Position);
        }

        public override void Camerawork()
        {
            Vector2 followPos = Registry.CalcRenderPos(Follow.Position);
            Position = Vector2.SmoothStep(Position, followPos, 0.5f);
        }
    }

    public class TrackingCin : Cinema
    {
        protected enum vp
        {
            Fixed,
            Center,
            Player,
            Straight,
            Topdown,
        }

        protected Player Follow;
        protected vp View;

        public TrackingCin(Player toFollow)
        {
            DoRot = true;
            Follow = toFollow;
        }

        public override void Camerawork()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.D1))
                View = vp.Fixed;
            if (Keyboard.GetState().IsKeyDown(Keys.D2))
                View = vp.Center;
            if (Keyboard.GetState().IsKeyDown(Keys.D3))
                View = vp.Player;
            if (Keyboard.GetState().IsKeyDown(Keys.D9))
                View = vp.Topdown;
            if (Keyboard.GetState().IsKeyDown(Keys.D0))
                View = vp.Straight;

            DoZoom = false;
            switch (View)
            {
                case vp.Fixed:
                    Rotation = MathHelper.Pi / 4.0f;
                    return;
                case vp.Center:
                    Center();
                    return;
                case vp.Player:
                    Rotation = -Follow.Facing - MathHelper.Pi / 2.0f;
                    return;
                case vp.Topdown:
                    Perspective = 0.0f;
                    DoZoom = true;
                    return;
                case vp.Straight:
                    Rotation = 0.0f;
                    return;
            }
        }

        private void Center()
        {
            Vector2 followPos = new Vector2(Follow.Position.X, Follow.Position.Y);
            Vector2 center = new Vector2(Registry.Stage.Width * Tile.FOOT / 4,
                Registry.Stage.Height * Tile.FOOT / 2);
            Rotation = (float)Math.Atan2(center.Y - followPos.Y, center.X - followPos.X);
            Rotation = -Rotation - (MathHelper.Pi / 2.0f);
        }
    }
}

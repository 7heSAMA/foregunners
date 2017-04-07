using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Foregunners
{
    /// <summary>
    /// Tracks universal values across levels, UI states, game saves, etc. 
    /// </summary>
    public class Registry
    {
        public static GameServiceContainer Services;

        public static List<Vector3> Points;
        public static Vector3 MouseCast { get; protected set; }
        public static Vector2 MouseV2 { get; private set; }
        
        public static Random RNG;
        public static Vector2 Spin;
        public const float TargetCycle = 1.0f / 60.0f;

        public static Level Stage;
        public static float cycleTime;
		public static double Seconds;

        private static GraphicsDevice Graphics;
        private static ContentManager Content;
        private static List<IManager> Managers;

        public static ParticleManager PartMan;
        public static UnitManager UnitMan;
        public static MunManager MunMan;

        public static Texture2D Spritesheet, Tilesheet, Blank, Triangle, NASA;
        public static SpriteFont Header, Body, Flobots;

        public static Player Avatar;
		public static ScriptRunner Runner;

        public static KeyboardState LastKeyboard { get; private set; }
        public static MouseState LastMouse { get; private set; }
        public static bool Debug { get; private set; }
		
        public static Color BoneWhite = Color.Lerp(Color.LightGray, Color.MonoGameOrange, 0.1f);
        public static Color DarkPurple = new Color(24, 8, 18);
        public static Color BurnThru = Color.Lerp(DarkPurple, Color.TransparentBlack, 0.25f);
		
        public static void LoadGameServices(
            GraphicsDevice graphics, ContentManager content, GameServiceContainer services)
        {
			Seconds = 0.0f;

            Graphics = graphics;
            Content = content;
            Services = services;
			
            Points = new List<Vector3>();

            RNG = new Random();
            LastKeyboard = Keyboard.GetState();

            Managers = new List<IManager>();

            PartMan = new ParticleManager();
            UnitMan = new UnitManager();
            MunMan = new MunManager();

            Managers.Add(PartMan);
            Managers.Add(UnitMan);
            Managers.Add(MunMan);

			Runner = new ScriptRunner();

            Header = Content.Load<SpriteFont>("Header");
            Body = Content.Load<SpriteFont>("Body");
            Flobots = Content.Load<SpriteFont>("Flobots");

            Spritesheet = Content.Load<Texture2D>("sprites1.png");
            Tilesheet = Content.Load<Texture2D>("tileset.png");

            NASA = Content.Load<Texture2D>("nasa-tumbler.png");
            Blank = new Texture2D(Graphics, 1, 1, false, SurfaceFormat.Color);
            Blank.SetData(new[] { Color.White });

            int i = 0;
            Color[] triColors = new Color[Tile.FOOT * Tile.FOOT];
            for (int y = 0; y < Tile.FOOT; y++)
                for (int x = 0; x < Tile.FOOT; x++)
                {
                    Color color;
                    if (x <= y)
                        color = Color.White;
                    else
                        color = Color.Transparent;
                    triColors[i] = color;
                    i++;
                }

            Triangle = new Texture2D(Graphics, Tile.FOOT, Tile.FOOT, false, SurfaceFormat.Color);
            Triangle.SetData(triColors, 0, Tile.FOOT * Tile.FOOT);
        }
        
        public static void Update(GameTime gameTime)
        {
			Seconds = gameTime.TotalGameTime.TotalSeconds;
            cycleTime = (float)gameTime.ElapsedGameTime.TotalSeconds / (1.0f / 60.0f);
            MouseV2 = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            MouseCast = CastMouseToWorld();
            
            if (KeyJustPressed(Keys.OemQuestion))
                Debug = !Debug;

            if (Stage != null)
                Stage.Update();
			
            foreach (IManager man in Managers)
                man.RunSim(cycleTime);

			Runner.Update();

            LastKeyboard = Keyboard.GetState();
            LastMouse = Mouse.GetState();
        }
        
        public static bool KeyJustPressed(Keys key)
        {
            if (LastKeyboard.IsKeyUp(key) && Keyboard.GetState().IsKeyDown(key))
                return true;
            else
                return false;
        }

        public static bool KeyJustReleased(Keys key)
        {
            if (LastKeyboard.IsKeyDown(key) && Keyboard.GetState().IsKeyUp(key))
                return true;
            else
                return false;
        }

        public static bool LeftClick()
        {
            if (Mouse.GetState().LeftButton == ButtonState.Pressed &&
                LastMouse.LeftButton == ButtonState.Released)
                return true;
            else
                return false;
        }

		#region Render Calculations
		public static Vector2 CalcRenderPos(Vector3 pos)
		{ return new Vector2(pos.X, pos.Y) + Spin * pos.Z; }

		public static Vector2 CalcRenderPos(float x, float y, float z)
		{ return new Vector2(x, y) + Spin * z; }

		public static float GetDepth(float z)
		{
			if (Stage != null)
				return 1.0f - z / (2 * Stage.Depth * Tile.DEPTH);
			else
				return 1.0f - z / (4 * Tile.DEPTH);
		}
		#endregion

		public static Vector2 WorldOnOverlay(Vector3 pos)
        {
            Vector2 flatPos = new Vector2(pos.X, pos.Y) - Camera.Pos;
            
            float angle = (float)Math.Atan2(flatPos.Y, flatPos.X) + Camera.Rotation;
            float len = flatPos.Length();

            flatPos = new Vector2(
                (float)Math.Cos(angle),
                (float)Math.Sin(angle)) * len;
			
			// Equivalent of calculating sprite spin based on z/perspective 
			flatPos.Y -= pos.Z * (float)Math.Cos(Camera.Perspective);
			flatPos *= Camera.Zoom;

            flatPos.X += Main.Viewport.Width / 2;
            flatPos.Y += Main.Viewport.Height / 2;

            flatPos.X = (int)Math.Floor(flatPos.X);
            flatPos.Y = (int)Math.Floor(flatPos.Y);

            return flatPos;
        }

        /// <summary>
        /// Translate a screen position to a world position. 
        /// </summary>
        /// <param name="screen">XY coord with origin in top left.</param>
        /// <param name="z">The world depth we are looking at.</param>
        public static Vector3 OverlayToWorld(Point screen, float z = 0.0f)
        {
            Vector2 pos = new Vector2(
                screen.X - Main.Viewport.Width / 2,
                screen.Y - Main.Viewport.Height / 2);

			Vector3 lens = Camera.Lens();

			pos.X /= lens.X;
			pos.Y /= lens.Y;
            
            float angle = (float)Math.Atan2(pos.Y, pos.X) - Camera.Rotation;
            float length = pos.Length();
            
            pos = new Vector2(
                (float)Math.Cos(angle) * length,
                (float)Math.Sin(angle) * length);

			pos += Camera.Pos;

            return new Vector3(pos, z);
        }
		
        private static Vector3 CastMouseToWorld()
        {
            if (Stage == null)
                return OverlayToWorld(Mouse.GetState().Position);
            else
            {
				float xyMag = (float)(Math.Cos(Camera.Perspective) * Math.Sin(Camera.Perspective));
                float zMag = (float)Math.Cos(Camera.Perspective);

				// convert camera angle to a 1st person 'god's ray' kinda thing 
                float rotation = -Camera.Rotation + MathHelper.Pi / 2.0f;

                float x = (float)Math.Cos(rotation);
                float y = (float)Math.Sin(rotation);

                Vector3 vel = new Vector3(x * xyMag, y * xyMag, zMag);
                vel *= (1.0f / zMag) * Tile.DEPTH;

                return Stage.CastMousePos(OverlayToWorld(Mouse.GetState().Position), vel);
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
			// convert camera angle to a 3rd person kinda view 
			float angle = -Camera.Rotation - MathHelper.Pi / 2.0f;

			// use that and the perspective ratio to determine sprite offset (aka spin) 
			Spin = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
			Spin *= (float)Math.Sin(Camera.Perspective); 
			
			if (Stage != null)
                Stage.Draw(spriteBatch);
            foreach (IManager man in Managers)
                man.DrawSprites(spriteBatch);
            
            foreach (Vector3 v3 in Points)
            {
                float depth = 0.0f; // GetDepth(v3.Z);
                Vector2 v2 = new Vector2(v3.X, v3.Y);
                CenterLine(
                    spriteBatch, 4.0f, Color.MonoGameOrange, v2, CalcRenderPos(v3), depth);
                DrawCircle(
                    spriteBatch, 32.0f, 28.0f, v2, depth, Color.MonoGameOrange);
            }
            Points.Clear();
        }

        #region Drawing
        public static void DrawTri(SpriteBatch spriteBatch, Vector2 pos, Color color, float rotation,
            float scale, float depth, bool centered)
        {
            Vector2 origin;
            if (centered)
                origin = new Vector2(Triangle.Width / 2, Triangle.Height / 2);
            else
                origin = Vector2.Zero;
            spriteBatch.Draw(Triangle, pos, null, color, rotation, origin, scale, SpriteEffects.None, depth);
        }

        public static void DrawQuad(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation,
            Vector2 scale, float depth, bool centered)
        {
            Vector2 origin;
            if (centered)
                origin = new Vector2(0.5f);
            else
                origin = Vector2.Zero;
            spriteBatch.Draw(Blank, position, null, color, rotation, origin, scale, SpriteEffects.None, depth);
        }

        public static void DrawQuad(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation,
            Vector2 scale, float depth, Vector2 origin)
        {
            spriteBatch.Draw(Blank, position, null, color, rotation, origin, scale, SpriteEffects.None, depth);
        }

        public static void CenterLine(SpriteBatch spriteBatch, float width, Color color, Vector2 p1, Vector2 p2, float depth)
        {
            DrawLine(spriteBatch, width / 2, color, p1, p2, depth);
            DrawLine(spriteBatch, width / 2, color, p2, p1, depth);
        }

        public static void DrawLine(SpriteBatch spriteBatch, float width, Color color, Vector2 p1, Vector2 p2, float depth)
        {
            float angle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            float length = Vector2.Distance(p1, p2);

            spriteBatch.Draw(Blank, p1, null, color,
                angle, Vector2.Zero, new Vector2(length, width),
                SpriteEffects.None, depth);
        }
        
        public static void DrawCircle(SpriteBatch spriteBatch, float r, float cutr, Vector2 position, float depth, Color color)
        {
            float width = r - cutr;
            float segments = (float)Math.Floor(r / MathHelper.Pi * 2.0f);
            for (float i = 0; i < segments; i++)
            {
                float sA = (float)(i / segments) * MathHelper.TwoPi;
                Vector2 start = new Vector2((float)Math.Cos(sA) * r, (float)Math.Sin(sA) * r);
                float eA = (float)((i + 1) / segments) * MathHelper.TwoPi;
                Vector2 end = new Vector2((float)Math.Cos(eA) * r, (float)Math.Sin(eA) * r);
                DrawLine(spriteBatch, width, color, start + position, end + position, depth);
            }
        }
        #endregion
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Foregunners
{
    // NOTES 
    // Inversion of Control "Auto Wiring" system - worth looking into to 
    // reduce calldowns/dependency injection? 
    // Implementations of interface properties can change privacy on 
    // unspecified accessors! 

	// TODO: 
	// Merge AutoBox and ManualBox using MakeBox factory pattern 
	// Remove/merge superfluous classes - partially complete 
	// Reimplement binary map files 
	// Update UI - consider starting with tooltip TODO, but there may be a better way 
	
    public class Main : Game
    {
        private GraphicsDeviceManager Graphics;
        private SpriteBatch Batch;
        public static Viewport Viewport { get; private set; }
		private bool Fullscreen = false;

		public Main()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
			SetViewport();
            base.Initialize();
        }
		
		protected void SetViewport()
		{
			if (Fullscreen)
			{
				Viewport = new Viewport(new Rectangle(0, 0, 1366, 768));
				Graphics.PreferredBackBufferWidth = Viewport.Width;
				Graphics.PreferredBackBufferHeight = Viewport.Height;
				Graphics.IsFullScreen = true;
			}
			else
			{
				Viewport = new Viewport(new Rectangle(0, 0, 960, 540));
				Graphics.PreferredBackBufferWidth = Viewport.Width;
				Graphics.PreferredBackBufferHeight = Viewport.Height;
				Graphics.IsFullScreen = false;
			}
			Graphics.ApplyChanges();
		}

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Batch = new SpriteBatch(GraphicsDevice);

			Registry.LoadGameServices(GraphicsDevice, Content, Services);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			Camera.Pos = new Vector2(Registry.Avatar.Position.X, Registry.Avatar.Position.Y);

			TipSorter.Update();

			Registry.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Registry.DarkPurple);
            
            // Draw transformed sprites  
            Batch.Begin(
				SpriteSortMode.BackToFront,
				BlendState.AlphaBlend,
				SamplerState.PointClamp,
				null, null, null,
				Camera.get_transformation(Viewport));
			Registry.Draw(Batch);
			Batch.End();

			// Draw GUI 
			Batch.Begin();
			TipSorter.Draw(Batch);
			Batch.End();

            base.Draw(gameTime);
        }
    }
}

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
        
        // should all be moved to registry or something
        public static Viewport viewport { get; private set; }
        public static Camera2D Cam { get; private set; }

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
            viewport = new Viewport(new Rectangle(0, 0, 960, 540));
            Graphics.PreferredBackBufferWidth = viewport.Width;
            Graphics.PreferredBackBufferHeight = viewport.Height;
            Graphics.IsFullScreen = false;
            Graphics.ApplyChanges();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Batch = new SpriteBatch(GraphicsDevice);
            Cam = new Camera2D(Graphics);
            
            Registry.LoadGameServices(GraphicsDevice, Content, Services);
            
            Registry.Stage = 
                new Level("depot", Services);
			Registry.Stage.Initialize();
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            TipSorter.Update();
            
            Registry.Update(gameTime);
            
            if (Registry.Stage == null)
            {
                Registry.Spin = new Vector2(1, 1);
                Registry.Spin.Normalize();

                Cam.Pos = Vector2.Zero;
                Cam.Zoom = Vector3.One;
                Cam.Rotation = 0.0f;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Clear 
            GraphicsDevice.Clear(Registry.Burn);
            
            // Begin transformed 
            Batch.Begin(SpriteSortMode.BackToFront,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    null, null, null,
                    Cam.get_transformation(viewport));
            
            // Draw transformed 
            Registry.Draw(Batch);
            Batch.End();
            
            // Draw normal 
            Batch.Begin();
            
            TipSorter.Draw(Batch);

            Color trite = Color.Lerp(Color.White, Color.Transparent, 0.5f);

            Registry.CenterLine(Batch, 2.0f, trite, new Vector2(0.0f, Registry.MouseV2.Y),
                new Vector2(viewport.Width, Registry.MouseV2.Y), 0.0f);
            Registry.CenterLine(Batch, 2.0f, trite, new Vector2(Registry.MouseV2.X, 0.0f),
                new Vector2(Registry.MouseV2.X, viewport.Height), 0.0f);
                
            Batch.End();
            base.Draw(gameTime);
        }
    }
}

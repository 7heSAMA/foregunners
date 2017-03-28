using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
    public abstract class BaScript : IScript
    {
        public bool Active { get; protected set; }
        public bool Running { get; protected set; }
        
        public virtual void Enter() { Active = true; }
        public virtual void Pause() { Running = false; }
        public abstract void Update();
        public virtual void Unpause() { Running = true; }
        public virtual void Exit() { Active = false; }
    }

    public abstract class GameState : BaScript, IVisible
    {
        public bool Overlay { get; protected set; } = false;
        public abstract void Draw(SpriteBatch batch);
    }

    public static class GameStateController
    {
        private static Stack<GameState> States;

        static GameStateController()
        {
            States = new Stack<GameState>();
        }

        public static void Update()
        {
            GameState next = States.Peek();
            next.Update();
            if (!next.Active)
                States.Pop().Exit();
        }
        
        public static void Push(GameState state)
        {
            if (States.Count > 0)
                States.Peek().Pause();
            state.Enter();
            States.Push(state);
        }

        public static void Draw(SpriteBatch batch)
        {
            if (States.Count > 0)
                States.Peek().Draw(batch);
        }
    }
    
    public class MenuState : GameState
    {
        public override void Update()
        {
            // MAKE INPUT CLASS 
        }

        public override void Draw(SpriteBatch batch)
        {
            float scale = Math.Max((float)Main.viewport.Width / Registry.NASA.Width,
                (float)Main.viewport.Height / Registry.NASA.Height);

            Vector2 ori = new Vector2(Registry.NASA.Width / 2, Registry.NASA.Height / 2);
            Vector2 center = new Vector2(Main.viewport.Width / 2, Main.viewport.Height / 2);

            batch.Draw(Registry.NASA, center, null, 
                Color.White, 0.0f, ori, scale, SpriteEffects.None, 1.0f);

            string text = "FOREGUNNERS";

            SpriteFont font = Registry.Flobots;
            Vector2 pos = new Vector2(32);

            batch.DrawString(font, text, pos,
                Color.Black, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);

            pos -= new Vector2(2, 4);

            batch.DrawString(font, text, pos,
                Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
        }
    }
}

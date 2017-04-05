using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
    public interface IScript
    {
		string Sid { get; }
		bool Repeat { get; }
		bool Active { get; }

		List<string> InjectOnFinish { get; }

		void Setup();
		void Update();
		void Shutdown();
    }
	
    /// <summary>
    /// Drawable object.
    /// Has Draw method.
    /// </summary>
    public interface IVisible
    {
        void Draw(SpriteBatch batch);
    }

    /// <summary>
    /// In world object. 
    /// Has Position.
    /// </summary>
    public interface IWorld : IVisible
    {
        Vector3 Position { get; }
    }

    /// <summary>
    /// World object.
    /// Has Active property and Draw method.
    /// </summary>
    public interface IReal : IWorld
    {
        bool Active { get; }
        float Facing { get; }
    }
}
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
        //bool Active { get; }
        //bool Running { get; }
		
        void Update();
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
	
    /// <summary>
    /// Has Pos and Bounds properties.
    /// </summary>
    public interface ISpatial : IReal
    {
        int Foot { get; }
        int Depth { get; }
    }
}
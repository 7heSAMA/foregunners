using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
    public interface ILogic
    {
        bool Active { get; }
    }

    public abstract class Chain : ILogic
    {
        public bool Ready { get; protected set; }
        public bool Active { get; protected set; }

        public Chain Cause { get; protected set; }
        public Chain Effect { get; protected set; }
        
        public void Update()
        {
            Cause.Update();

            if (Cause.Ready)
                Effect.Start();
        }

        public abstract void Start();
    }

    // Cause -> Effect chain 
    // 

    // active is whether or not an obj is "relevant" i.e. alive, in world, etc - 
    // not whether it is tripped, triggered, armed, unpaused, or running 

    // scripts run continuously unless interrupted 
    // events happen once and then reset/remove themselves 
    // triggers watch for conditions to update themselves 
    // 

    public interface IScript
    {
        bool Active { get; }
        bool Running { get; }

        void Enter();
        void Pause();
        void Update();
        void Unpause();
        void Exit();
    }

    public interface IEvent
    {
        bool Primed { get; }

        void Run();
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
    /// Moving object. 
    /// Has Active, Pos, Vel properties + Update, Draw methods.
    /// </summary>
    public interface ITemporal : IReal
    {
        Vector3 Velocity { get; }
        void Update(float cycleTime);
    }

    /// <summary>
    /// Has Pos and Bounds properties.
    /// </summary>
    public interface ISpatial : IReal
    {
        int Foot { get; }
        int Depth { get; }

        //Spatial.Relation RelationTo();
        //bool Engulfs(ISpatial other);
        //bool Intersects(ISpatial other);
        //bool Inside(ISpatial other);
    }
    
    /*public interface ITempUnit
    {
        Vector3 Position { get; }
        int Foot { get; }
        int Depth { get; }
        float Facing { get; }
    }*/

    /// <summary>
    /// A world object that is physically simulated.
    /// </summary>
    public interface ICoporeal : ISpatial, ITemporal
    {
        // much, much more needed
    }

    /// <summary>
    /// Killable object.
    /// Has Bounds 
    /// </summary>
    public interface IMortal : ISpatial
    {
        ISpatial Hitbox { get; }
        int Hull { get; }
        int Armor { get; }
        int Shield { get; }

        int MaxHull { get; }
        int MaxArmor { get; }
        int MaxShield { get; }

        void Damage(int ke, int em);
    }

    /// <summary>
    /// Munition object.
    /// </summary>
    public interface IVengeful : ISpatial
    {
        bool Armed { get; }
        void Update();
        void Disarm();
        void Detonate();
    }

    /// <summary>
    /// A world object that thinks.
    /// </summary>
    public interface IThoughtful
    {
        // float Strength(); 
        // Rank + rank in each Area 
        void Update();
    }

    public interface IFull : IMortal, IThoughtful
    {
        Faction Front { get; }
    }
}
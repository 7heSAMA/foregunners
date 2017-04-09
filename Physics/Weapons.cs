using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
    /*
        DESIGN FOR WEAPONS 
        -MunMan controls all projectiles
        -units have ControlModule (abstract <- AI, Player, etc)
            (knows how to use certain parts of weapons, requires further input from units)
        -Units have Weapons fired by ControlModule 
    */

    public sealed class Specs
    {
        public MunManager.Cat EnType { get; private set; }
        public float MuzzleVel { get; private set; }
        public int Damage { get; private set; }

        public float Drag { get; private set; } = 1.0f;
        public bool Gravitized { get; private set; } = false;
        public bool Complex { get; private set; } = false;
        public bool Bouncy { get; private set; } = false;
        public int MaxBounces { get; private set; } = 0;
        public float Elasticity { get; private set; } = 0.0f;
        public int SubNumber { get; private set; } = 0;
        public Specs Submunition { get; private set; } = null;

        public Specs(MunManager.Cat enType, float mv, int dam)
        {
            EnType = enType;
            MuzzleVel = mv;
            Damage = dam;
        }

        public Specs(MunManager.Cat enType, float mv, float drag, int dam)
            : this(enType, mv, dam)
        {
            Drag = drag;
        }
        
        public Specs(MunManager.Cat enType, float mv, float drag, bool grav, bool complex, 
            int dam, bool bouncy, int maxBounce, float elas, int subNum, Specs submun)
        {
            EnType = enType;
            MuzzleVel = mv;
            Drag = drag;
            Gravitized = grav;
            Complex = complex;
            Damage = dam;
            Bouncy = bouncy;
            MaxBounces = maxBounce;
            Elasticity = elas;
            SubNumber = subNum;
            Submunition = submun;
        }
    }
    
    public abstract class Weapon
    {
        #region props/fields
        protected IReal Wielder;
        protected Specs Payload;
        
        // offset 
        protected float Angle, Length, Depth;

        protected Vector3 Offset
        {
            get
            {
                if (Length == 0.0f)
                    return Wielder.Position + new Vector3(
                        Vector2.Zero, Depth);
                else
                    return Wielder.Position + new Vector3(
                        (float)Math.Cos(Angle + Wielder.Facing) * Length,
                        (float)Math.Sin(Angle + Wielder.Facing) * Length,
                        Depth);
            }
        }
        #endregion

        public Weapon(IReal wielder, Specs payload, float depth)
        {
            Wielder = wielder;
            Payload = payload;

            Angle = 0.0f;
            Length = 0.0f;
            Depth = depth;
        }

        public Weapon(IReal wielder, Specs payload, Vector3 offset)
        {
            Wielder = wielder;
            Payload = payload;

            Angle = (float)Math.Atan2(offset.Y, offset.X);
            Length = new Vector2(offset.X, offset.Y).Length();
            Depth = offset.Z;
        }

        public void Update()
        {
            // ordering? 
            //Reposition.Update();
            //Logic.Update();
        }
    }
	
    public class TurretMount : Weapon
    {
        // turret aiming 
        protected float Rotation, Elevation;
        protected float TurnSpeed, EleSpeed;
        
        public TurretMount(IReal wielder, Specs payload, float depth,
            float turnSpeed, float eleSpeed)
            : base (wielder, payload, depth)
        {
            TurnSpeed = turnSpeed;
            EleSpeed = eleSpeed;
        }

        public TurretMount(IReal wielder, Specs payload, Vector3 offset, 
            float turnSpeed, float eleSpeed)
            : base (wielder, payload, offset)
        {
            TurnSpeed = turnSpeed;
            EleSpeed = eleSpeed;
        }

        public void Target(Vector3 position)
        {
            float distance = new Vector2(position.X - Offset.X, position.Y - Offset.Y).Length();

            Elevation = Gizmo.TurnToFace(Vector2.Zero, 
                new Vector2(distance, position.Z - Offset.Z), Elevation, EleSpeed);

            Rotation = Gizmo.TurnToFace(Offset.X, Offset.Y, position.X, position.Y,
                Rotation, TurnSpeed);
        }

        public void Fire()
        {
            Vector3 mv = new Vector3(
                new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation))
                    * (float)Math.Cos(Elevation),
                (float)Math.Sin(Elevation))
                * MunManager.Bullet.MuzzleVel;

            Registry.MunMan.Activate(Wielder, Offset, mv, Payload);
        }
    }
	
    public class Munition : SimFrame
    {
        protected Specs Specification;
        protected IReal Shooter;
        protected bool Struck;
		protected int Bounces;

        public Munition(Specs spec)
            : base (2, 2, spec.Drag, spec.Elasticity, spec.Gravitized)
        {
            Specification = spec;
            Struck = false;
        }

        protected override void RunAxisFull(float cycleTime, string xyz)
        {
            if (xyz == "X")
                Position += Velocity;

            TileCollision collision = Registry.Stage.GetCollision(Position);

            if (collision == TileCollision.Landing || collision == TileCollision.Solid || 
				(collision == TileCollision.Slope && Position.Z < Registry.Stage.GetSlope(
					Position)))
				Struck = true;
        }

        protected override void RunLogic(float cycleTime)
        {
            if (Struck)
            {
				Bounces += 1;

				if (Bounces >= Specification.MaxBounces)
				{
					/*Active = false;
					for (int i = 0; i < 5; i++)
						Registry.PartMan.Activate(Position, new Vector3(
							(float)Registry.RNG.NextDouble() - 0.5f,
							(float)Registry.RNG.NextDouble() - 0.5f,
							(float)Registry.RNG.NextDouble()) * 15.0f);*/
				}
				else
				{
					Velocity = new Vector3(Velocity.X, Velocity.Y, -Velocity.Z);
					Position = LastPos;
				}
            }

            foreach (Unit unit in Registry.UnitMan.Active)
            {
                if (unit.Bounds.Contains(Position) == ContainmentType.Contains && 
                    unit != Shooter)
                {
                    int ke = 0, em = 0;
                    if (Specification.EnType == MunManager.Cat.KE)
                        ke = Specification.Damage;
                    else
                        em = Specification.Damage;
                    unit.Damage(ke, em);
                    Active = false;
                }
            }
        }

        public void Shoot(IReal shooter, Vector3 pos, Vector3 mom)
        {
            Shooter = shooter;
            Position = pos;
            Velocity = mom;
            Active = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Color line;
            if (Specification.EnType == MunManager.Cat.KE)
                line = Color.MonoGameOrange;
            else
                line = Color.Teal;

            Registry.CenterLine(spriteBatch, Specification.Damage, line, 
                Registry.CalcRenderPos(Position),
                Registry.CalcRenderPos(LastPos), Registry.GetDepth(Position.Z));
        }
    }

    public class MunManager: Manager<Munition>
    {
        public enum Cat
        {
            KE,
            EM,
        }

        public static Specs Shrapnel { get; private set; }
        public static Specs Grenade { get; private set; }
        public static Specs Bullet { get; private set; }
        public static Specs Pulse { get; private set; }

        static MunManager()
        {
            Shrapnel = new Specs(Cat.KE, 25.0f, 0.65f, 2);
            Grenade = new Specs(Cat.KE, 100.0f, 0.975f, true, true, 60, true, 2, 0.5f, 0, Shrapnel);
            Bullet = new Specs(Cat.KE, 40.0f, 40);
            Pulse = new Specs(Cat.EM, 32.0f, 20);
        }

        public void Activate(IReal shooter, Vector3 pos, Vector3 mom, Specs spec)
        {
            Munition mun = new Munition(spec);
            mun.Shoot(shooter, pos, mom);
            Active.Add(mun);
        }
    }
}

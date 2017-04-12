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
			: this(wielder, payload)
        {
            Angle = 0.0f;
            Length = 0.0f;
            Depth = depth;
        }

		public Weapon(IReal wielder, Specs payload, Vector3 offset)
			: this(wielder, payload)
		{
			Angle = (float)Math.Atan2(offset.Y, offset.X);
			Length = new Vector2(offset.X, offset.Y).Length();
			Depth = offset.Z;
		}

		protected Weapon(IReal wielder, Specs payload)
		{
			Wielder = wielder;
			Payload = payload;
		}
    }

	public class MountedTurret : Weapon
	{
		// turret aiming 
		protected float Rotation, Elevation;
		protected float TurnSpeed, EleSpeed;

		public MountedTurret(IReal wielder, Specs payload, float depth,
			float turnSpeed, float eleSpeed)
			: base(wielder, payload, depth)
		{
			TurnSpeed = turnSpeed;
			EleSpeed = eleSpeed;
		}

		public MountedTurret(IReal wielder, Specs payload, Vector3 offset,
			float turnSpeed, float eleSpeed)
			: base(wielder, payload, offset)
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
				* Specs.Bullet.MuzzleVel;

			Munition.Fire(Wielder, Offset, mv, Payload);
		}
	}
}

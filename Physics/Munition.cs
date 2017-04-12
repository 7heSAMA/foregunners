using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
	public sealed class Specs 
	{
		#region fields and props
		public Munition.Cat EnType { get; private set; }
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
		#endregion

		#region static implementations 
		public static Specs Shrapnel { get; private set; }
		public static Specs Grenade { get; private set; }
		public static Specs Bullet { get; private set; }
		public static Specs Pulse { get; private set; }

		static Specs()
		{
			Shrapnel = new Specs(Munition.Cat.KE, 25.0f, 0.65f, 2);
			Grenade = new Specs(Munition.Cat.KE, 80.0f, 0.95f, true, true, 60, true, 3, 0.9f, 0, Shrapnel);
			Bullet = new Specs(Munition.Cat.KE, 40.0f, 40);
			Pulse = new Specs(Munition.Cat.EM, 32.0f, 20);
		}
		#endregion

		public Specs(Munition.Cat enType, float mv, int dam)
		{
			EnType = enType;
			MuzzleVel = mv;
			Damage = dam;
		}

		public Specs(Munition.Cat enType, float mv, float drag, int dam)
			: this(enType, mv, dam)
		{
			Drag = drag;
		}

		public Specs(Munition.Cat enType, float mv, float drag, bool grav, bool complex,
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

	public class Munition : SimFrame
	{
		public enum Cat
		{
			KE,
			EM,
		}

		protected Specs Specification;
		protected IReal Shooter;
		protected int Bounces;
		
		public static void Fire(IReal shooter, Vector3 pos, Vector3 mom, Specs spec)
		{
			Munition mun = new Munition(spec);
			mun.Shoot(shooter, pos, mom);
			Registry.MunMan.Add(mun);
		}

		protected Munition(Specs spec)
			: base(2, 2, spec.Drag, spec.Elasticity, spec.Gravitized)
		{
			Specification = spec;
		}

		protected override void RunLogic(float cycleTime)
		{
			if (OnGround || OnWall)
			{
				Bounces += 1;

				if (Bounces >= Specification.MaxBounces)
				{
					Active = false;
					for (int i = 0; i < 5; i++)
						Registry.PartMan.Activate(Position, new Vector3(
							(float)Registry.RNG.NextDouble() - 0.5f,
							(float)Registry.RNG.NextDouble() - 0.5f,
							(float)Registry.RNG.NextDouble()) * 15.0f);
				}
			}

			foreach (Unit unit in Registry.UnitMan.Active)
			{
				if (unit.Bounds.Contains(Position) == ContainmentType.Contains &&
					unit != Shooter)
				{
					int ke = 0, em = 0;
					if (Specification.EnType == Cat.KE)
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
			if (Specification.EnType == Cat.KE)
				line = Color.MonoGameOrange;
			else
				line = Color.Teal;

			Registry.CenterLine(spriteBatch, Specification.Damage, line,
				Registry.CalcRenderPos(Position),
				Registry.CalcRenderPos(LastPos), Registry.GetDepth(Position.Z));
		}
	}
}

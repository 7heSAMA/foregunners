using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
    public abstract class Unit : SimFrame
    {
		public string Name { get; protected set; }

        public int Hull { get; protected set; }
        public int Armor { get; protected set; }
        public int Shield { get; protected set; }

        public int MaxHull { get; protected set; }
        public int MaxArmor { get; protected set; }
        public int MaxShield { get; protected set; }

        public int RadiusShield { get; protected set; }
        public float RechargeSpan { get; protected set; }
        public float RechargeTimer { get; protected set; }
        public float RechargeRate { get; protected set; }
        
        protected Sprite Chassis;
		
        public Unit(string name, int foot, int height, float aero, float elasticity)
            : base(foot, height, aero, elasticity)
        {
            Name = name;
            InitCombat(200, 400, 100);
            InitShields(5.0f, 2.0f);
        }
        
        protected void InitCombat(int hull, int armor, int shield)
        {
            MaxHull = hull;
            MaxArmor = armor;
            MaxShield = shield;

            Hull = hull;
            Armor = armor;
            Shield = shield;
        }

        protected void InitShields(float span, float rate)
        {
            RechargeSpan = span * 60.0f;
            RechargeRate = rate;
            RechargeTimer = RechargeSpan;
        }

        public void Damage(int ke, int em)
        {
            DealEM(em);
            DealKE(ke);
        }

        private void DealEM(int dam)
        {
            // doesn't account for bleedover 
            if (Shield > 0)
                Shield -= dam;
            else if (Armor > 0)
                Armor -= (dam / 2);
            else
                Hull -= dam;

            RechargeTimer = 0.0f;
            if (Hull <= 0)
                Kill();
        }

        private void DealKE(int dam)
        {
            // doesn't account for bleedover 
            if (Shield > 0)
                dam /= 2;

            if (Armor > 0)
                Armor -= dam;
            else
                Hull -= dam;

            if (Hull <= 0)
                Kill();
        }

        private void Kill()
        {
            Active = false;
            Vector3 rando = new Vector3(
                    (float)Registry.RNG.NextDouble() - 0.5f,
                    (float)Registry.RNG.NextDouble() - 0.5f,
                    (float)Registry.RNG.NextDouble() * 0.1f) * 15.0f;
			// TODO: readd particles 
			/*Particle.AddWreck(Position, rando, Facing, 
                (float)Registry.RNG.NextDouble() * 0.25f);*/
        }

        protected override void RunLogic(float cycleTime)
        {
            RechargeTimer += cycleTime;
            if (RechargeTimer > RechargeSpan)
            {
                Shield += (int)Math.Floor(RechargeRate * cycleTime);
                if (Shield > MaxShield)
                    Shield = MaxShield;
            }

            foreach (Unit other in Registry.UnitMan.Active)
            {
                if (other == this)
                    continue;

                float r = other.Foot / 2 + Foot / 2;
                float d = Vector2.Distance(
                    new Vector2(other.Position.X, other.Position.Y),
                    new Vector2(Position.X, Position.Y));

                if (d < r)
                {
                    Vector2 unit = new Vector2(other.Position.X - Position.X,
                        other.Position.Y - Position.Y);
                    unit.Normalize();

                    Push(-unit * (r - d) / Foot * 15.0f);
                    other.Push(unit * (r - d) / other.Foot * 15.0f);
                }
            }
        }

		public override void Draw(SpriteBatch batch)
		{
			Chassis.Draw(batch);
		}
	}
}

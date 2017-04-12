using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
	/*	Alright, here's how this is gonna work:
	 *	
	 *	Jackal holds a static Hunger and Caution variable. 
	 *	Hunger slowly but steadily rises over time, and is reduced by destroying things. 
	 *	Caution is always proportional to enemy weaponry in play. 
	 *	
	 *	Additionally, Jackals have individual Bloodlust and Pain variables. 
	 *	Bloodlust is similar to Hunger, but will only rise - at a faster rate - once the pack decides to attack,
	 *	and is reduced by attacking (regardless of damage dealt). 
	 *	Pain is increased by taking damage and reduces over time. 
	 */
	public class Jackal : Unit
	{
		public static float Hunger { get; protected set; }
		public static float Caution { get; protected set; }

		public float Bloodlust { get; protected set; }
		public float Pain { get; protected set; }

		public Jackal(Vector3 pos)
			: base("Jackal", 48, 32, 0.975f, 0.1f)
		{
			Chassis = new Sprite(this);
		}

		protected override void RunLogic(float cycleTime)
		{
			base.RunLogic(cycleTime);
		}
	}
}
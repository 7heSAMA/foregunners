using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Foregunners
{
	public class Tracker : ScBasic
	{
		protected string Target { get; set; }
		protected float Time { get; set; }

		protected double StartTime { get; set; }
		protected Vector2 StartPos { get; set; }

		public class Data : DataBasic
		{
			public string Target { get; set; }
			public float Time { get; set; }
		}

		public Tracker(Data data)
			: base(data)
		{
			Target = data.Target;
			Time = data.Time;
		}

		public override void Setup()
		{
			StartPos = Camera.Pos;
			StartTime = Registry.Seconds;
		}

		public override void Update()
		{
			// TODO: junk 
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foregunners
{
	public class Tracker : ScBasic
	{
		public string Target { get; set; }
		protected float Time { get; set; }

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

		public override void Update()
		{
			// TODO: 
			// change pos to target pos
			// if time = 0, immediate; else, smoothstep from start time
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Foregunners.Scripting
{
	public class Scenario
	{
		#region data classes
		public class Light
		{
			public int X { get; set; }
			public int Y { get; set; }
			public int Radius { get; set; }
		}
		#endregion

		public string Name { get; set; }
		public string Map { get; set; }

		public float Zoom { get; set; } = float.NaN;
		public float Rotation { get; set; } = float.NaN;
		public float Perspective { get; set; } = float.NaN;

		public List<string> Inject { get; set; } = new List<string>();

		public List<Cinema.Data> Cameras { get; set; } = new List<Cinema.Data>();
		public List<Tracker.Data> Trackers { get; set; } = new List<Tracker.Data>();
		public List<Dialogue.Data> Dialogue { get; set; } = new List<Dialogue.Data>();

		public List<Zone> Zones { get; set; } = new List<Zone>();

		public List<Light> Lights { get; set; } = new List<Light>();
	}
}

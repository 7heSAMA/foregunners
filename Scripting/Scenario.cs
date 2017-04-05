using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Foregunners
{
	public class Scenario
	{
		public string Name { get; set; }
		public string Map { get; set; }

		public float Zoom { get; set; } = 0.0f;
		public float Rotation { get; set; } = 0.0f;
		public float Perspective { get; set; } = 0.0f;

		public List<string> Inject { get; set; } = new List<string>();

		public List<Cinema.Data> Cameras { get; set; } = new List<Cinema.Data>();
		public List<Tracker.Data> Trackers { get; set; } = new List<Tracker.Data>();
		public List<Dialogue.Data> Dialogue { get; set; } = new List<Dialogue.Data>();
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Foregunners.Scripting
{
	public class Zone
	{
		public string Name { get; set; }

		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int Depth { get; set; }

		public Vector3 Minimum { get { return new Vector3(X * Tile.FOOT, Y * Tile.FOOT, Z * Tile.FOOT); } }
		public Vector3 Maximum { get { return Minimum + 
			new Vector3(Width * Tile.FOOT, Height * Tile.FOOT, Depth * Tile.FOOT); } }

		public Vector3 RandomOnGround()
		{
			// this isn't actually random but that's not important rn  
			return (Minimum + Maximum) / 2;
		}
	}
}

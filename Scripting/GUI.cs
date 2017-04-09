using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners.Scripting
{
	public static class GUI
	{
		private static List<Tooltip> Elements;
		private static Dictionary<Rectangle, List<Tooltip>> AreaSets;

		static GUI()
		{
			Elements = new List<Tooltip>();
			AreaSets = new Dictionary<Rectangle, List<Tooltip>>();
		}

		public static void Update()
		{
			foreach (Tooltip tip in Elements)
				tip.Update();

			// check for input 
			// if tooltip added/changed, sort
		}

		public static void Add(Tooltip toAdd)
		{
			Elements.Add(toAdd);
			//Sort();
		}

		// should only recalculate when a tooltip goes on/offline 
		// or a dropdown is opened/closed
		// store offsets in individual tooltips 
		/// <summary>
		/// Doesn't work
		/// </summary>
		static private void Sort()
		{
			foreach (Tooltip tip in Elements)
			{
				bool inArea = false;
				Rectangle bounds;
				List<Tooltip> list;

				foreach (Rectangle area in AreaSets.Keys)
				{
					if (area.Intersects(tip.Bounds))
					{
						inArea = true;
						int height = area.Height + tip.Bounds.Height;
						int width = area.Width + tip.Bounds.Width;

						int count = AreaSets[area].Count();
						int x = (area.X * count) + tip.Bounds.X;
						int y = (area.Y * count) + tip.Bounds.Y;
						x = (int)Math.Floor(x / count + 1.0f);
						y = (int)Math.Floor(y / count + 1.0f);

						bounds = new Rectangle(x, y, width, height);

						list = AreaSets[area];
						list.Add(tip);
						AreaSets.Remove(area);
						AreaSets.Add(bounds, list);
					}
				}

				if (!inArea)
				{
					list = new List<Tooltip>();
					list.Add(tip);
					bounds = tip.Bounds;
					AreaSets.Add(bounds, list);
				}
			}

			for (int i = 0; i < AreaSets.Keys.Count; i++)
			{
				Rectangle area = AreaSets.Keys.ElementAt(i);
				for (int n = i + 1; n < AreaSets.Keys.Count; n++)
				{

				}
			}
		}

		static public void Draw(SpriteBatch batch)
		{
			foreach (Tooltip tip in Elements)
				tip.Draw(batch);
		}
	}
}

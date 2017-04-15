using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
    public interface IManager
    {
        void RunSim(float cycleTime);
        void DrawSprites(SpriteBatch spriteBatch);
    }

    public class Manager<T> : IManager where T : SimFrame
	{
		public List<T> Active { get; private set; }
		public List<T> Stored { get; private set; }
        private List<T> ToCull;

        public Manager()
        {
            Stored = new List<T>();
            Active = new List<T>();
            ToCull = new List<T>();
        }

		#region Update and cull
		public void RunSim(float cycleTime)
        {
            foreach (SimFrame s in Active)
                s.Update(cycleTime);
            Cull();
        }

        private void Cull()
        {
            foreach(T sim in Active)
                if (!sim.Active)
                    ToCull.Add(sim);

            foreach(T sim in ToCull)
            {
                Stored.Add(sim);
                Active.Remove(sim);
            }

            ToCull.Clear();
        }
		#endregion

		#region adding/storing
		/// <summary> Stores object. </summary>
		public void Ready(T toStore)
		{
			Stored.Add(toStore);
		}

		/// <summary> Adds object. </summary>
		public void Add(T toAdd)
		{
			Active.Add(toAdd);
			Stored.Remove(toAdd);
		}
		#endregion

		public void DrawSprites(SpriteBatch spriteBatch)
        {
            foreach (SimFrame sim in Active)
                sim.Draw(spriteBatch);
        }
    }
}
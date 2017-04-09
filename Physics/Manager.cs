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

    public class Manager<T> : IManager where T:SimFrame
    {
        public List<T> Stored { get; protected set; }
        public List<T> Active { get; protected set; }
        private List<SimFrame> ToCull;

        public Manager()
        {
            Stored = new List<T>();
            Active = new List<T>();
            ToCull = new List<SimFrame>();
        }

        public void RunSim(float cycleTime)
        {
            foreach (SimFrame s in Active)
                s.Update(cycleTime);
            Cull();
        }

        private void Cull()
        {
            foreach(SimFrame sim in Active)
                if (!sim.Active)
                    ToCull.Add(sim);

            foreach(T sim in ToCull)
            {
                Stored.Add(sim);
                Active.Remove(sim);
            }

            ToCull.Clear();
        }

        public void Add(T sim)
        {
            Active.Add(sim);
        }

        public void DrawSprites(SpriteBatch spriteBatch)
        {
            foreach (SimFrame sim in Active)
                sim.Draw(spriteBatch);
        }
    }

    public class ParticleManager : Manager<Particle>
    {
        public void Activate(Vector3 pos, Vector3 mom)
        {
            if (Stored.Count == 0)
                Ready(10);

            Stored[0].Activate(pos, mom, 100.0f);
            Active.Add(Stored[0]);
            Stored.Remove(Stored[0]);
        }
        
        public void Wreck(Vector3 pos, Vector3 mom, float face, float angVel)
        {
            Ready(1);
            Stored[0].ActWreck(face, angVel);
            Activate(pos, mom);
        }

        private void Ready(int num)
        {
            for (int i = 0; i < num; i++)
                Stored.Add(new Particle(0.99f, 0.9f));
        }
    }
}
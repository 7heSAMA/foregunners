using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Foregunners
{
    public enum FacTag
    {
        OSO,
        JKL,
        WSL,
        LYNX,
    }

    public class Faction
    {
        public FacTag Tag;
        public List<Unit> Forces { get; protected set; }

        public Faction (FacTag tag)
        {
            this.Tag = tag;
        }
    }
}

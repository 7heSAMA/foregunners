using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
    public class BoundingCylinder 
    {
        public Vector3 Positon;
        public float Radius;
        public float Depth;

        public Vector3 Bottom { get { return Positon; } }
        public Vector3 Top { get { return Positon + new Vector3(Vector2.Zero, Depth); } }

        public BoundingCylinder(Vector3 pos, float r, float depth)
        {
            Positon = pos;
            Radius = r;
            Depth = depth;
        }
    }

    public abstract class SimFrame : IVisible
    {
        #region fields
        protected bool Gravitized { private get; set; } = true;
        public bool Collides { get; protected set; } = true;
        public bool Active { get; protected set; } = false;
        protected bool OnGround { get; set; }
        protected bool OnWall { get; set; }

        private float _elas { get; set; } = 0.0f;
        protected float Elasticity
        {
            get { return _elas; }
            set { _elas = MathHelper.Clamp(value, 0.0f, 1.0f); }
        }

        private float _aero { get; set; } = 1.0f;
        public float Aero
        {
            get { return _aero; }
            protected set { _aero = MathHelper.Clamp(value, 0.0f, 1.0f); }
        }

        public Vector3 Position { get; protected set; }
        public Vector3 Velocity { get; protected set; }
        protected Vector3 LastPos;
        #endregion

        public SimFrame() { }

        public SimFrame(float aero, float elasticity, bool grav)
        {
            Aero = aero;
            Elasticity = elasticity;
            Gravitized = grav;
        }

        public void Update(float cycleTime)
        {
            if (Registry.Stage != null)
                RunPhysics(cycleTime);
            RunLogic(cycleTime);
        }

        protected abstract void RunLogic(float cycleTime);
        protected abstract void RunAxisFull(float cylceTime, string xyz);

        private void RunPhysics(float cycleTime)
        {
            LastPos = Position;
            OnGround = false;
            OnWall = false;

            if (Collides && Registry.Stage != null)
            {
                RunAxisFull(cycleTime, "Z");
                RunAxisFull(cycleTime, "Y");
                RunAxisFull(cycleTime, "X");
            }
            else
                Position += Velocity;
            
            Velocity *= Aero;
            if (Gravitized)
                Velocity += new Vector3(Vector2.Zero, Registry.Stage.Gravity);
        }
        
        public abstract void Draw(SpriteBatch spriteBatch);
    }

    // SimFrame should implement ISpatial but can't yet 
    // due to lack of interlocking bounding shapes (point, cube, cylinder, sphere)
    public abstract class SimCube : SimFrame, ISpatial 
    {
        #region fields that should be implemented by a child class but can't rn 
        /// <summary>
        /// Returns a BoundingBox with Position as the center. 
        /// </summary>
        public BoundingBox Bounds
        {
            get
            {
                Vector3 pos = new Vector3(
                    (float)Math.Floor(Position.X),
                    (float)Math.Floor(Position.Y),
                    (float)Math.Floor(Position.Z));
                return new BoundingBox(pos - Center, pos + Center);
            }
        }

        public float Facing { get; protected set; }

        private int _depth;
        public int Depth
        {
            get { return _depth; }
            private set { _depth = (value / 2) * 2; }
        }

        private int _foot;
        public int Foot
        {
            get { return _foot; }
            private set { _foot = (value / 2) * 2; }
        }

        public Vector2 Print { get; private set; }
        public Vector2 Origin { get; private set; }
        public Vector3 Size { get; private set; }
        public Vector3 Center { get; private set; }
        #endregion

        #region constructors and inits
        public SimCube(int foot, int depth, float aero, float elasticity)
            : base(aero, elasticity, true)
        { SetDimensions(foot, depth); }

        public SimCube(int foot, int depth)
            : base()
        { SetDimensions(foot, depth); }

        private void SetDimensions(int foot, int depth)
        {
            Foot = foot;
            Depth = depth;
            Print = new Vector2(Foot);
            Origin = new Vector2(Foot / 2);
            Size = new Vector3(Print, Depth);
            Center = new Vector3(Origin, Depth / 2);
        }
        #endregion

        #region physics
        protected override void RunAxisFull(float cycleTime, string xyz)
        {
            int size = Foot;
            if (xyz == "Z")
                size = Depth;

            var axis = typeof(Vector3).GetField(xyz);
            object pos = Position;

            axis.SetValue(pos,
                (float)axis.GetValue(Position) + (float)axis.GetValue(Velocity) * cycleTime);
            Position = (Vector3)pos;

            int[] min = tileRange(Bounds.Min);
            int[] max = tileRange(Bounds.Max);

            for (int z = min[2]; z <= max[2]; z++)
                for (int y = min[1]; y <= max[1]; y++)
                    for (int x = min[0]; x <= max[0]; x++)
                        if (Registry.Stage.GetCollision(x, y, z) == TileCollision.Solid)
                        {
                            BoundingBox tile = Tile.Bounds(x, y, z);
                            if (Bounds.Intersects(tile))
                            {
                                float tileMin = (float)axis.GetValue(tile.Min);
                                float tileMax = (float)axis.GetValue(tile.Max);

                                if ((float)axis.GetValue(LastPos) < (tileMin + tileMax) / 2)
                                    axis.SetValue(pos, tileMin - (size / 2) - 1);
                                else
                                    axis.SetValue(pos, tileMax + (size / 2));
                                Position = (Vector3)pos;

                                object mom = Velocity;
                                axis.SetValue(mom, (float)axis.GetValue(mom) * -Elasticity);
                                Velocity = (Vector3)mom;

                                if (xyz == "Z")
                                    OnGround = true;
                                else
                                    OnWall = true;
                            }
                        }
                        else if (xyz == "Z" &&
                            Registry.Stage.GetCollision(x, y, z) == TileCollision.Slope ||
                            Registry.Stage.GetCollision(x, y, z) == TileCollision.Landing)
                        {
                            BoundingBox tile = Tile.Bounds(x, y, z);
                            if (Bounds.Intersects(tile))
                            {
                                float zPos = Registry.Stage.GetSlope(Position, x, y, z);

                                if (Bounds.Min.Z < zPos && (
                                    Position.X > tile.Min.X && Position.X < tile.Max.X &&
                                    Position.Y > tile.Min.Y && Position.Y < tile.Max.Y))
                                {
                                    Position = new Vector3(
                                        Position.X, Position.Y, zPos + Depth / 2);
                                    Velocity = new Vector3(
                                        Velocity.X, Velocity.Y, -Velocity.Z * Elasticity);
                                    OnGround = true;
                                }
                            }
                        }
        }

        protected int[] tileRange(Vector3 pos)
        {
            return new int[3] {
                Tile.GetArrayXY(pos.X),
                Tile.GetArrayXY(pos.Y),
                Tile.GetArrayZ(pos.Z) };
        }
        #endregion

        public void Push(Vector2 dir)
        {
            Velocity += new Vector3(dir.X, dir.Y, 0);
        }
    }

    public abstract class SimBody : SimFrame
    {
        #region fields that should be implemented by a child class but can't rn 
        /// <summary>
        /// Returns a BoundingBox with Position as the center. 
        /// </summary>
        public BoundingCylinder Bounds
        {
            get
            {
                Vector3 pos = new Vector3(
                    (float)Math.Floor(Position.X),
                    (float)Math.Floor(Position.Y),
                    (float)Math.Floor(Position.Z));
                return new BoundingCylinder(pos, Radius, Depth);
            }
        }

        private int _depth;
        public int Depth
        {
            get { return _depth; }
            private set { _depth = (value / 2) * 2; }
        }

        private int _radius;
        public int Radius
        {
            get { return _radius; }
            private set { _radius = (value / 2) * 2; }
        }
        #endregion

        protected override void RunAxisFull(float cycleTime, string xyz)
        {
            if (xyz == "Z")
                RunZ(cycleTime);
            else
                RunXY(cycleTime, xyz);
        }

        private void RunXY(float cycleTime, string xy)
        {
            var axis = typeof(Vector3).GetField(xy);
            object pos = Position;

            axis.SetValue(pos,
                (float)axis.GetValue(Position) + (float)axis.GetValue(Velocity) * cycleTime);
            Position = (Vector3)pos;

            int[,] range = tileRange(Position);

            for (int z = range[2, 0]; z <= range[2, 1]; z++)
                for (int y = range[1, 0]; y <= range[1, 1]; y++)
                    for (int x = range[0, 0]; x <= range[0, 1]; x++)
                        if (Registry.Stage.GetCollision(x, y, z) == TileCollision.Solid)
                        {
                            Vector3 closest;
                            if (Velocity.X > 0.0f)
                            {
                                Vector2 p1, p2;
                            }
                            
                            BoundingBox tile = Tile.Bounds(x, y, z);
                            
                        }

        }

        private void RunZ(float cycleTime)
        {

        }

        protected int[,] tileRange(Vector3 pos)
        {
            int x = Tile.GetArrayXY(pos.X);
            int y = Tile.GetArrayXY(pos.Y);
            int z = Tile.GetArrayXY(pos.Z);

            return new int[3, 2] { 
                { x - 1, x + 1 }, 
                { y - 1, y + 1 }, 
                { z - 1, z + 1 }
            };
        }
    }
}

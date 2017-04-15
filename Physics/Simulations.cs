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
    public abstract class SimFrame : IReal
    {
		#region abstract fields for static child properties
		private float _elas { get; set; } = 0.0f;
		protected float Elasticity
		{
			get { return _elas; }
			set { _elas = MathHelper.Clamp(value, 0.0f, 1.0f); }
		}
		
		private float _aero { get; set; } = 0.9f;
		public float Aero
		{
			get { return _aero; }
			protected set { _aero = MathHelper.Clamp(value, 0.0f, 1.0f); }
		}

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

		#region fields and properties
		protected bool Gravitized { private get; set; } = true;
		public bool Active { get; protected set; } = false;
		protected bool OnGround { get; set; }
		protected bool OnWall { get; set; }

		public Vector3 Position { get; protected set; }
		public Vector3 Velocity { get; protected set; }
		protected Vector3 LastPos;

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
		#endregion

		#region constructors and inits
		protected SimFrame(int foot, int depth, float aero = 0.9f, float elas = 0.0f, bool grav = true)
		{
			SetDimensions(foot, depth);
			Aero = aero;
			Elasticity = elas;
			Gravitized = grav;
		}

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

		#region logic and drawing
		public void Update(float cycleTime)
		{
			LastPos = Position;
			OnGround = false;
			OnWall = false;

			if (Registry.Stage != null)
				RunPhysics(cycleTime);
			else
			{
				Position += Velocity * cycleTime;
				if (Position.Z < 0 + Depth / 2)
				{
					OnGround = true;
					Position = new Vector3(Position.X, Position.Y, 0 + Depth / 2);
					Velocity = new Vector3(Velocity.X, Velocity.Y, -Velocity.Z * Elasticity);
				}
			}

			if (Gravitized)
				Velocity += new Vector3(Vector2.Zero, Registry.Gravity);
			Velocity *= Aero;

			RunLogic(cycleTime);
        }

        protected abstract void RunLogic(float cycleTime);

        private void RunPhysics(float cycleTime)
        {
			RunAxisFull(cycleTime, "Z");
			RunAxisFull(cycleTime, "Y");
			RunAxisFull(cycleTime, "X");
		}
        
		public void Push(Vector2 dir)
		{
			Velocity += new Vector3(dir, 0);
		}

        public abstract void Draw(SpriteBatch batch);
		#endregion
		
		#region physics
		private void RunAxisFull(float cycleTime, string xyz)
		{
			int size = Foot;
			if (xyz == "Z")
				size = Depth;

			var axis = typeof(Vector3).GetField(xyz);
			object pos = Position;

			axis.SetValue(pos,
				(float)axis.GetValue(Position) + (float)axis.GetValue(Velocity) * cycleTime);
			Position = (Vector3)pos;

			int[] min = TileRange(Bounds.Min);
			int[] max = TileRange(Bounds.Max);

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

		protected int[] TileRange(Vector3 pos)
		{
			return new int[3] {
				Tile.GetArrayXY(pos.X),
				Tile.GetArrayXY(pos.Y),
				Tile.GetArrayZ(pos.Z) };
		}
		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Foregunners
{
    public static class Camera2D
    {
		public static float _zoom = 1.0f;
		public static float _perspective = 0.0f;
        private static float _rotation = MathHelper.Pi / 4.0f;
		private static Vector2 _pos = Vector2.Zero;
        private static Matrix _transform;

		public static float Perspective
		{
			get { return _perspective; }
			set { _perspective = MathHelper.Clamp(value, 0.0f, MathHelper.Pi / 4.0f); }
		}

		// TODO: add method to change rotation by degrees
		public static float Rotation
		{
			get { return _rotation; }
			set { _rotation = Gizmo.WrapAngle(value); }
		}

		public static float Zoom
		{
			get { return _zoom; }
			set { _zoom = MathHelper.Clamp(value, 0.25f, 1.0f); }
		}

		public static Vector3 Lens()
		{
			return new Vector3(Zoom, Zoom * (float)Math.Cos(Perspective), 1.0f);
		}
		
		public static Vector2 Pos
		{
			get { return _pos; }
			set { _pos = value; }
		}

        public static Matrix get_transformation(Viewport viewport)
        {
            _transform =       // Thanks to o KB o for this solution
              Matrix.CreateTranslation(new Vector3(-_pos.X, -_pos.Y, 0)) *
                                         Matrix.CreateRotationZ(Rotation) *
                                         Matrix.CreateScale(Lens()) * 
                                         Matrix.CreateTranslation(new Vector3(viewport.Width * 0.5f, 
                                             viewport.Height * 0.5f, 0));
            return _transform;
        }
    }
}

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
    public class Camera2D 
    {
        GraphicsDeviceManager m_GraphicsDeviceManager; 
        
        protected Vector3 _zoom;
        public Matrix _transform; // Matrix Transform
        public Vector2 _pos; // Camera Position
        protected float _rotation; // Camera Rotation

        public Camera2D(GraphicsDeviceManager graphicsDeviceManager)
        {
            m_GraphicsDeviceManager = graphicsDeviceManager;
            _zoom = Vector3.One;//1.0f;
            _rotation = 0.0f;
            _pos = Vector2.Zero;
        }
        
        // Gets and sets zoom
        public Vector3 Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = value;
                if (_zoom.X < 0.1f) _zoom.X = 0.1f;
                if (_zoom.Y < 0.1f) _zoom.Y = 0.1f;
                if (_zoom.Z < 0.1f) _zoom.Z = 0.1f;
            }
        }

        // Gets and sets rotation
        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        // Auxiliary function to move the camera
        public void Move(Vector2 amount)
        {
            _pos += amount;
        }

        // Gets and sets position
        public Vector2 Pos
        {
            get { return _pos; }
            set { _pos = value; }
        }

        public Matrix get_transformation(Viewport viewport)
        {
            //Viewport viewport = graphics.GraphicsDevice.Viewport;
            _transform =       // Thanks to o KB o for this solution
              Matrix.CreateTranslation(new Vector3(-_pos.X, -_pos.Y, 0)) *
                                         Matrix.CreateRotationZ(Rotation) *
                                         Matrix.CreateScale(_zoom) * 
                                         Matrix.CreateTranslation(new Vector3(viewport.Width * 0.5f, 
                                             viewport.Height * 0.5f, 0));
            return _transform;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Foregunners
{
    public class Player : Unit
    {
        private float OnArmMouseY, AimMouseY;
        private bool Armed;
        private Vector3 StoredTarget;
        private TurretMount Turret;

        public Player(Vector3 pos)
            : base("Avatar", 48, 32, 0.9f, 0.0f)
        {
            Active = true;
            Position = pos;
            LastPos = pos;
            Chassis = new Sprite(this);

            Turret = new TurretMount(this, MunManager.Pulse, 24, 0.1f, 0.05f);

            Tipper2();
        }

        private void Tipper2()
        {
            Tooltip vitals = new Tooltip(this, TextBox.Make("Avatar", Registry.Header));

            vitals.AddEntry(TextBox.Make(this, "Shield", Registry.Body, "Shield: "));
            vitals.AddEntry(TextBox.Make(this, "Armor", Registry.Body, "Armor: "));
            vitals.AddEntry(TextBox.Make(this, "Hull", Registry.Body, "Hull: "));

            TipSorter.Add(vitals);
        }
        
        protected override void RunLogic(float cycleTime)
        {
            Vector2 accel = Vector2.Zero;
            if (Keyboard.GetState().IsKeyDown(Keys.A))
                accel.X -= 1.0f;
            if (Keyboard.GetState().IsKeyDown(Keys.D))
                accel.X += 1.0f;
            if (Keyboard.GetState().IsKeyDown(Keys.W))
                accel.Y -= 1.0f;
            if (Keyboard.GetState().IsKeyDown(Keys.S))
                accel.Y += 1.0f;
            
            float angle = (float)Math.Atan2(accel.Y, accel.X) - Main.Cam.Rotation;
            
            if (accel != Vector2.Zero && OnGround)
            {
                Velocity += new Vector3(
                    (float)Math.Cos(angle) * 1.5f,
                    (float)Math.Sin(angle) * 1.5f,
                    0.0f);
                Facing = Gizmo.TurnToAngle((float)Math.Atan2(Velocity.Y, Velocity.X), Facing, 
					Velocity.Length() * 0.01f);
            }
            
            Vector3 target = Registry.MouseCast + new Vector3(0, 0, Depth / 2);

            Turret.Target(target);
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                Turret.Fire();

            if (Registry.LeftClick() && !Armed)
            {
                Armed = true;
                OnArmMouseY = Registry.MouseV2.Y;
                StoredTarget = target;
            }
            
            base.RunLogic(cycleTime);
        }

        public override void Draw(SpriteBatch batch)
        {
            Chassis.Draw(batch);

            Registry.Points.Add(Registry.MouseCast + new Vector3(0, 0, Depth / 2));

            Registry.DrawQuad(batch, Registry.CalcRenderPos(Registry.MouseCast +
                new Vector3(0, 0, Depth / 2)),
                Color.Black, MathHelper.Pi / 2.0f, new Vector2(Foot / 2 + 8), 0.02f, true);
            Registry.DrawQuad(batch, Registry.CalcRenderPos(Registry.MouseCast +
                new Vector3(0, 0, Depth / 2)),
                Color.MonoGameOrange, MathHelper.Pi / 2.0f, new Vector2(Foot / 2), 0.0f, true);
        }
    }
}

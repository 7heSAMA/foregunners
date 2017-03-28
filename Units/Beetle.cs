using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
    public class Spectrum
    {
        private readonly float Max;
        private float Fill;
        public float Percent { get; private set; }
        public bool Filled { get; private set; }
        public bool Empty { get; private set; }

        /// <summary></summary>
        /// <param name="timeToFill">Measured in seconds</param>
        public Spectrum (float timeToFill)
        {
            Max = timeToFill * 60.0f;
        }
        
        public void Increment(float cycleTime)
        {
            Empty = false;
            Fill += cycleTime;

            if (Fill > Max)
            {
                Fill = Max;
                Filled = true;
            }
            Percent = Fill / Max;
        }

        public void Decrement(float cycleTime)
        {
            Filled = false;
            Fill -= cycleTime;

            if (Fill < 0.0f)
            {
                Fill = 0.0f;
                Empty = true;
            }
            Percent = Fill / Max;
        }
    }
    
    public class Beetle : Unit
    {
        private State Current;
        private const float EngageDist = Tile.FOOT * 4;
        private const float Heuristic = Tile.FOOT * 3;
        
        private float Aim;

        private Spectrum Entrenchment = new Spectrum(2.0f);

        private bool InBurst;
        private const float BurstGapSpan = 75.0f;
        private const float ShotGapSpan = 8.0f;
        private float BurstCd;
        private float ShotCd;

        int BurstCount;
        
        private enum State
        {
            Travel,
            Setup,
            Engage,
            Packup,
        }

        public Beetle(Vector3 pos)
            : base("Beetle", 32, 24, 0.9f, 0.0f)
        {
            Facing = 0.0f;
            Position = pos;
            Active = true;
            //InjectTooltip();
            Chassis = new Sprite(this);
        }

        protected override void RunLogic(float cycleTime)
        {
            Vector2 pos = new Vector2(Position.X, Position.Y);
            Vector2 avaPos = 
                //Vector2.Zero; 
                new Vector2(Registry.Avatar.Position.X, Registry.Avatar.Position.Y);

            float distanceToAv = Vector2.Distance(pos, avaPos);

            float AngleToPlayer = (float)Math.Atan2(avaPos.Y - pos.Y, avaPos.X - pos.X);
            Aim = Gizmo.TurnToAngle(AngleToPlayer, Aim, 0.15f);

            Specs Wep = MunManager.Pulse;

            switch (Current)
            {
                case State.Travel:
                    Facing = Gizmo.TurnToAngle(AngleToPlayer, Facing, 0.1f);

                    float accel = 0.65f;
                    Velocity += new Vector3((float)Math.Cos(Facing) * accel,
                        (float)Math.Sin(Facing) * accel, 0.0f);

                    if (distanceToAv < EngageDist)
                        Current = State.Setup;

                    break;

                case State.Setup:
                    if (distanceToAv > EngageDist + Heuristic)
                    {
                        Current = State.Packup;
                        break;
                    }

                    Entrenchment.Increment(cycleTime);
                    if (Entrenchment.Filled)
                        Current = State.Engage;
                    break;

                case State.Engage:
                    BurstCd -= cycleTime;

                    if (!InBurst)
                    {
                        if (distanceToAv > EngageDist + Heuristic)
                            Current = State.Packup;
                        else if (distanceToAv < EngageDist + (Heuristic / 2) && BurstCd <= 0)
                        {
                            InBurst = true;
                            BurstCount = Registry.RNG.Next(4) + 4;
                        }
                    }

                    if (InBurst)
                    {
                        ShotCd -= cycleTime;

                        if (ShotCd <= 0)
                        {
                            Vector3 mv = new Vector3(
                                (float)Math.Cos(Aim),
                                (float)Math.Sin(Aim),
                                0.0f) * Wep.MuzzleVel;

                            Registry.MunMan.Activate(this, Position, mv, Wep);
                            BurstCount -= 1;

                            if (BurstCount > 0)
                                ShotCd = ShotGapSpan;
                            else
                            {
                                ShotCd = 0;
                                InBurst = false;
                                BurstCd = BurstGapSpan;
                            }
                        }
                    }
                    break;

                case State.Packup:
                    if (distanceToAv < EngageDist)
                    {
                        Current = State.Setup;
                        break;
                    }

                    Entrenchment.Decrement(cycleTime);
                    if (Entrenchment.Empty)
                        Current = State.Travel;
                    break;
            }
            base.RunLogic(cycleTime);
        }

        public override void Draw(SpriteBatch batch)
        {
            Chassis.Draw(batch);
        }
    }
}

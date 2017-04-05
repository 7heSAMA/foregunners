using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Foregunners
{
    public static class Gizmo
    {
        private static Random RNG;

        static Gizmo()
        {
            RNG = new Random();
        }

        public static char[] NewLine = { '\r', '\n' };
        public static string WrapWord(StringBuilder original, SpriteFont font, Rectangle bounds)
        {
            StringBuilder target = new StringBuilder();
            int lastWhiteSpace = 0;
            Vector2 currentTargetSize;
            for (int i = 0; i < original.Length; i++)
            {
                char character = original[i];
                if (char.IsWhiteSpace(character))
                {
                    lastWhiteSpace = target.Length;
                }
                target.Append(character);
                currentTargetSize = font.MeasureString(target);
                if (currentTargetSize.X > bounds.Width)
                {
                    target.Insert(lastWhiteSpace, NewLine);
                    target.Remove(lastWhiteSpace + NewLine.Length, 1);
                }
            }
            return target.ToString();
        }

        #region Maths
        public static Vector3 RandomVector3(float minValue, float maxValue)
        {
            return new Vector3((float)RNG.NextDouble() * (maxValue - minValue) + minValue,
                (float)RNG.NextDouble() * (maxValue - minValue) + minValue,
                (float)RNG.NextDouble() * (maxValue - minValue) + minValue);
        }

        public static int WrapInt(int val, int min, int max)
        {
            int range = max - min;
            while (val > max)
                val -= range;
            while (val < min)
                val += range;
            return val;
        }
        #endregion

        #region AlignTo
        public static Vector2 SmoothStepVec(Vector2 p1, Vector2 p2, float scale)
        {
            return new Vector2(MathHelper.SmoothStep(p1.X, p2.X, scale), MathHelper.SmoothStep(p1.Y, p2.Y, scale));
        }

        public static Vector2 PaceMomentum(Vector2 target, Vector2 position, Vector2 momentum, float maxSpeed)
        {
            float speed = maxSpeed;
            float distance = Vector2.Distance(position + momentum, target);
            if (distance < maxSpeed)
                speed = distance;

            float angle = FindAngle(position + momentum, target);
            Vector2 unit = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            return unit * speed;
        }

        public static Vector2 AlignTo(Vector2 target, Vector2 position, float maxSpeed)
        {
            float distance = Vector2.Distance(target, position);
            if (distance < maxSpeed)
                return target;

            float angle = FindAngle(position, target);
            Vector2 unit = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            return unit * maxSpeed + position;
        }

        public static float AlignTo(float target, float value, float maxSpeed)
        {
            if (maxSpeed <= 0)
                throw new NotSupportedException();
            if (Math.Abs(value - target) <= maxSpeed)
                return target;
            if (target < value)
                return target + maxSpeed;
            else
                return target - maxSpeed;
        }
        #endregion

        #region Circle to Circle Collision
        //Returns true if the circles are touching, or false if they are not
        public static bool circlesColliding(Vector2 v1, float radius1, Vector2 v2, float radius2)
        {
            //compare the distance to combined radii
            float dx = v2.X - v1.X;
            float dy = v2.Y - v1.Y;
            float radii = radius1 + radius2;

            if ((dx * dx) + (dy * dy) < radii * radii)
                return true;
            return false;
        }

        public static bool spheresColliding(Vector3 v1, float r1, Vector3 v2, float r2)
        {
            float dx = v2.X - v1.X;
            float dy = v2.Y - v1.Y;
            float dz = v2.Z - v1.Z;
            float radii = r1 + r2;

            if ((dx * dx) + (dy * dy) + (dz * dz) < radii * radii)
                return true;
            else
                return false;
        }
        #endregion

        #region Line to Circle Collision
        public static bool LineToCircle(Vector2 circlePos, float circleRadius, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 segment = new Vector2();
            segment = (lineEnd - lineStart);
            Vector2 toCircle = new Vector2();
            toCircle = (circlePos - lineStart);

            if (segment.Length() <= 0)
                throw new NotSupportedException("Invalid segment length");

            Vector2 segmentUnit = new Vector2();
            segmentUnit = (Vector2.Normalize(segment));

            float projection = Vector2.Dot(toCircle, segmentUnit);

            Vector2 proVector = new Vector2();

            if (projection <= 0)
                proVector = lineStart;
            else if (projection >= segment.Length())
                proVector = lineEnd;
            else
            {
                proVector = segmentUnit * projection;
                proVector += lineStart;
            }

            return circlesColliding(circlePos, circleRadius, proVector, 1);
        }
        #endregion

        #region TurnToFace Methods
        public static float TurnToFace(Vector2 position, Vector2 faceThis, float currentAngle, float turnSpeed)
        {
            float desiredAngle = FindAngle(position, faceThis);
            return TurnToAngle(desiredAngle, currentAngle, turnSpeed);
        }

        public static float TurnToFace(
            float x1, float y1, 
            float x2, float y2, 
            float currentAngle, float turnSpeed)
        {
            float desiredAngle = FindAngle(x1, y1, x2, y2);
            return TurnToAngle(desiredAngle, currentAngle, turnSpeed);
        }

        /// <summary>
        /// Calculates the angle that an object should face, given its position, its
        /// target's position, its current angle, and its maximum turning speed.
        /// </summary>
        public static float FindAngle(Vector2 position, Vector2 faceThis)
        {
            float x = faceThis.X - position.X;
            float y = faceThis.Y - position.Y;

            float Angle = (float)Math.Atan2(y, x);
            return Angle;
        }

        public static float FindAngle(float x1, float y1, float x2, float y2)
        {
            float x = x2 - x1;
            float y = y2 - y1;

            float Angle = (float)Math.Atan2(y, x);
            return Angle;
        }

        public static float TurnToAngle(float desiredAngle, float currentAngle, float turnSpeed)
        {
            // first, figure out how much we want to turn, using WrapAngle to get our
            // result from -Pi to Pi ( -180 degrees to 180 degrees )
            float difference = WrapAngle(desiredAngle - currentAngle);

            // clamp that between -turnSpeed and turnSpeed.
            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            // so, the closest we can get to our target is currentAngle + difference.
            // return that, using WrapAngle again.
            return WrapAngle(currentAngle + difference);
        }

        /// <summary>
        /// Returns the angle expressed in radians between -Pi and Pi.
        /// </summary>
        public static float WrapAngle(float radians)
        {
            while (radians < -MathHelper.Pi)
            {
                radians += MathHelper.TwoPi;
            }
            while (radians > MathHelper.Pi)
            {
                radians -= MathHelper.TwoPi;
            }
            return radians;
        }
        #endregion

        public static Vector2 GetClosestPointOnLineSegment(Vector2 A, Vector2 B, Vector2 P)
        {
            Vector2 AP = P - A;       //Vector from A to P   
            Vector2 AB = B - A;       //Vector from A to B  

            //Magnitude of AB vector (it's length squared)
            float magnitudeAB = AB.LengthSquared();
            //The DOT product of a_to_p and a_to_b
            float ABAPproduct = Vector2.Dot(AP, AB);
            //The normalized "distance" from a to your closest point
            float distance = ABAPproduct / magnitudeAB;

            //Check if P projection is over vectorAB
            if (distance < 0)     
                return A;
            else if (distance > 1)
                return B;
            else
                return A + AB * distance;
        }
    }
}

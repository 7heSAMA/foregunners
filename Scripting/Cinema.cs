using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Foregunners
{
	public class Cinema : ScBasic
	{
		protected float Zoom { get; set; } = 0.0f;
		protected float Rotation { get; set; } = 0.0f;
		protected float Perspective { get; set; } = 0.0f;
		protected float Time { get; set; } = 0.0f;

		protected double StartTime { get; set; }

		public class Data : DataBasic
		{
			public float Zoom { get; set; }
			public float Rotation { get; set; }
			public float Perspective { get; set; }
			public float Time { get; set; }
		}

		public Cinema(Data data)
			: base(data)
		{
			Zoom = data.Zoom;
			Rotation = data.Rotation;
			Perspective = data.Perspective;
			Time = data.Time;
		}

		public override void Setup()
		{
			StartTime = Registry.Seconds;
		}

		public override void Update()
		{
			float percent = (float)(Registry.Seconds - StartTime) / Time;
			Console.WriteLine(percent);
			
			if (Zoom != 0.0f)
				Camera2D.Zoom = MathHelper.SmoothStep(Camera2D.Zoom, Zoom, 0.075f);
			if (Rotation != 0.0f)
				Camera2D.Rotation = MathHelper.SmoothStep(Camera2D.Rotation, Rotation, 0.075f);
			if (Perspective != 0.0f)
				Camera2D.Perspective = MathHelper.SmoothStep(Camera2D.Perspective, Perspective, 0.075f);
			
			if (percent > 1.0f)
				Shutdown();
		}
	}
}

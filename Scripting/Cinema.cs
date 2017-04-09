using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Foregunners.Scripting
{
	public class Cinema : ScBasic
	{
		protected float Zoom { get; set; } = 0.0f;
		protected float Rotation { get; set; } = 0.0f;
		protected float Perspective { get; set; } = 0.0f;
		protected float Time { get; set; } = 0.0f;

		protected float StartZoom { get; set; }
		protected float StartRot { get; set; }
		protected float StartPers { get; set; }
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
			StartZoom = Camera.Zoom;
			StartRot = Camera.Rotation;
			StartPers = Camera.Perspective;
			StartTime = Registry.Seconds;
		}

		public override void Update()
		{
			float percent = (float)(Registry.Seconds - StartTime) / Time;
			
			if (Zoom != 0.0f)
				Camera.Zoom = MathHelper.SmoothStep(StartZoom, Zoom, percent);
			if (Rotation != 0.0f)
				Camera.Rotation = MathHelper.SmoothStep(StartRot, Rotation, percent);
			if (Perspective != 0.0f)
				Camera.Perspective = MathHelper.SmoothStep(StartPers, Perspective, percent);
			
			if (percent > 1.0f)
				Shutdown();
		}
	}
}

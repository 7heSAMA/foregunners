﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foregunners.Scripting
{
	public class ScriptRunner
	{
		private Dictionary<string, IScript> ScActive = new Dictionary<string, IScript>();
		private Dictionary<string, IScript> ScWaiting = new Dictionary<string, IScript>();
		private List<IScript> ScInjected = new List<IScript>();
		private List<string> SidToRemove = new List<string>();

		public int ActiveCount { get { return ScActive.Count(); } }

		// not sure this is really the right spot for this 
		public List<Zone> Zones { get; private set; } = new List<Zone>();

		public void BuildScene(Scenario scene)
		{
			if (!float.IsNaN(scene.Zoom))
				Camera.Zoom = scene.Zoom;
			if (!float.IsNaN(scene.Rotation))
				Camera.Rotation = scene.Rotation;
			if (!float.IsNaN(scene.Perspective))
				Camera.Perspective = scene.Perspective;

			ScBasic temp;

			foreach (Dialogue.Data dat in scene.Dialogue)
			{
				temp = new Dialogue(dat);
				ScWaiting.Add(temp.Sid, temp);
			}

			foreach (Tracker.Data dat in scene.Trackers)
			{
				temp = new Tracker(dat);
				ScWaiting.Add(temp.Sid, temp);
			}
			
			foreach (Cinema.Data dat in scene.Cameras)
			{
				temp = new Cinema(dat);
				ScWaiting.Add(temp.Sid, temp);
			}

			foreach (string sid in scene.Inject)
				Inject(sid);

			foreach (Zone zone in scene.Zones)
				Zones.Add(zone);

			foreach (Scenario.Light light in scene.Lights)
				Registry.Stage.PlaceLight(light.X, light.Y, light.Radius);
		}
		
		public void Update()
		{
			foreach (string sid in ScActive.Keys)
				ScActive[sid].Update();

			// remove deactivated scripts AFTER we have looped through all, 
			// as scripts may be deactivated by themselves -or- other scripts 
			foreach (string sid in ScActive.Keys)
				if (!ScActive[sid].Active)
				{
					ScActive[sid].Shutdown();
					SidToRemove.Add(sid);
				}

			// clear old scripts before adding new as we may re-inject
			foreach (string sid in SidToRemove)
				ScActive.Remove(sid);

			// activate injected scripts
			foreach (IScript script in ScInjected)
				if (!ScActive.ContainsKey(script.Sid))
					ScActive.Add(script.Sid, script);

			ScInjected.Clear();
		}

		// note: does -not- remove injected scripts from the stored database, 
		// so that they may be reactivated 
		public void Inject(string sid)
		{
			IScript toInject;
			if (ScWaiting.TryGetValue(sid, out toInject))
			{
				toInject.Setup();
				ScInjected.Add(toInject);
			}
			else
				throw new KeyNotFoundException("Script " + sid + " not in scenario");
		}
	}

	public abstract class ScBasic : IScript
	{
		public string Sid { get; private set; }
		public bool Repeat { get; protected set; } = false;
		public bool Active { get; protected set; } = false;

		public List<string> InjectOnFinish { get; private set; }

		public abstract class DataBasic
		{
			public string Sid { get; set; }
			public List<string> Inject { get; set; }
		}

		public ScBasic(DataBasic data)
		{
			if (data.Sid != null)
				Sid = data.Sid;
			else
				throw new ArgumentException("A script is missing it's SID");

			if (data.Inject != null)
				InjectOnFinish = data.Inject;

			Active = true;
			Repeat = false;
		}

		public virtual void Setup()
		{
			Active = true;
		}

		public abstract void Update();

		public virtual void Shutdown()
		{
			if (Repeat)
				Setup();
			else
				Active = false;

			if (InjectOnFinish != null)
				foreach (string sid in InjectOnFinish)
					Registry.Runner.Inject(sid);
		}
	}
}

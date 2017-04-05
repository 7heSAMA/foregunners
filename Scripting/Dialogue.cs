using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foregunners
{
	public class Dialogue : ScBasic
	{
		public string Text { get; protected set; }
		private int Index;

		public class Data : DataBasic
		{
			public string Text { get; set; }
		}

		public Dialogue(Data data)
			: base(data)
		{
			Text = data.Text;
			Index = 0;
		}

		public override void Update()
		{
			Console.Write(Text[Index]);
			Index++;
			if (Index == Text.Length)
			{
				Console.WriteLine();
				Active = false;
			}
		}
	}
}

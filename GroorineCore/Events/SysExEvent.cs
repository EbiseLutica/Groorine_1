using System;
using System.Linq;

namespace GroorineCore.Events
{

	public class SysExEvent : MidiEvent
	{
		public byte[] Data { get; set; }
		public override string ToString() => base.ToString() + string.Concat(Data?.Select(b => Convert.ToString(b, 16) + "H "));
	}


}
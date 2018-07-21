namespace Groorine.Events
{
	public class ControlEvent : MidiEvent
	{
		public byte ControlNo { get; set; }
		public byte Data { get; set; }
		public override string ToString() => base.ToString() + $"{ControlNo} {Data} ";
	}
}
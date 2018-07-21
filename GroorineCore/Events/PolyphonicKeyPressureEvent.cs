namespace Groorine.Events
{

	public class PolyphonicKeyPressureEvent : MidiEvent
	{
		public byte Pressure { get; set; }
		public byte NoteNumber { get; set; }
		public override string ToString() => base.ToString() + $"{NoteNumber} {Pressure}";
	}
}
namespace Groorine.Events
{

	public class NoteEvent : MidiEvent
	{
		public byte Note { get; set; }
		public byte Velocity { get; set; }
		public long Gate { get; set; }
		public override string ToString() => base.ToString() + $"{Note} {Velocity} ";

	}
}

namespace GroorineCore.Events
{
	public class PitchEvent : MidiEvent
	{
		public short Bend { get; set; }
		public override string ToString() => base.ToString() + $"{Bend} ";
	}
}
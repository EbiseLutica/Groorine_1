namespace GroorineCore.Events
{
	public class ProgramEvent : MidiEvent
	{
		public byte ProgramNo { get; set; }
		public override string ToString() => base.ToString() + $"{ProgramNo} ";
	}
}
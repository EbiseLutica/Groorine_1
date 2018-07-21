namespace Groorine.Events
{
	public abstract class MetaEvent : MidiEvent
	{
		public abstract string DisplayName { get; }

		public override string ToString() => $"[{Tick}] {DisplayName}: ";
	}
	

}
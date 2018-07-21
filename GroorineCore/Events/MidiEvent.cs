using Groorine.Helpers;

namespace Groorine.Events
{
	public abstract class MidiEvent : BindableBase
	{
		public long Tick { get; set; }
		public byte Channel { get; set; }
		public override string ToString() => $"[{Tick}] CH{Channel}.{GetType().Name} ";
	}
}
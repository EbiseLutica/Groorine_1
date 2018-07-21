namespace Groorine.Events
{

	public class ChannelPressureEvent : MidiEvent
	{
		public byte Pressure { get; set; }
		public override string ToString() => base.ToString() + $"{Pressure}";
	}
}
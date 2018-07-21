namespace Groorine.DataModel
{

	public interface IChannel
	{
		short BendRange { get; set; }
		byte Expression { get; set; }
		short NoteShift { get; set; }
		byte Panpot { get; set; }
		int Pitchbend { get; set; }
		short Tweak { get; set; }
		byte Volume { get; set; }
		double FreqExts { get; set; }
	}
}
namespace Groorine.Helpers
{
	internal static class MidiTimingConverter
	{
		public static int TempoToBpm(int tempo) => (int)(1.0 / tempo * 60 * 1000000);
		public static int BpmToTempo(int bpm) => (int)(60.0 * 1000000 / bpm);


		public static double GetTime(int index, int sampleRate) => index / (sampleRate * 0.001 * 2);

	}
}

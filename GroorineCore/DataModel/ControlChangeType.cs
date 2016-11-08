namespace GroorineCore
{

	public enum ControlChangeType
	{
		BankMSB,
		Modulation,
		DataMSB = 6,
		Volume,
		Panpot = 10,
		Expression,
		BankLSB = 32,
		DataLSB = 38,
		HoldPedal = 64,
		Reverb = 91,
		Chorus = 93,
		Delay,
		RPNLSB = 100,
		RPNMSB,
		AllSoundOff = 120,
		ResetAllController,
		AllNoteOff = 123,
		Mono = 126,
		Poly
	}
}
using Groorine.DataModel;

namespace Groorine.Synth
{

	public interface IInstrument
	{
		Range<byte> KeyRange { get; }
		Range<byte> VelocityRange { get; }
		IAudioSource Source { get; }
		double Pan { get; }

	}

}

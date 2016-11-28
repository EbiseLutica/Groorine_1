using GroorineCore.DataModel;

namespace GroorineCore.Synth
{

	public interface IInstrument
	{
		Range<byte> KeyRange { get; }
		Range<byte> VelocityRange { get; }
		IAudioSource Source { get; }
		double Pan { get; }

	}

}

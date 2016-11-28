using System;
namespace GroorineCore.Synth
{

	public interface IAudioSource
	{
		ValueTuple<short, short> GetSample(int index, double sampleRate);
	}


}
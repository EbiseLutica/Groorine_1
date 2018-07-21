using Groorine.DataModel;
using System;
namespace Groorine.Synth
{

	public interface IAudioSource
	{
		ValueTuple<short, short> GetSample(int index, double sampleRate, Tone tone);
	}


}
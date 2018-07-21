using System;
using Groorine.DataModel;

namespace Groorine.Synth
{

	public class Instrument : IInstrument
	{
		private double _pan;

		public Range<byte> KeyRange { get; }
		public Range<byte> VelocityRange { get; }
		public IAudioSource Source { get; }
		public double Pan
		{
			get { return _pan; }
			set
			{
				if (_pan > 100 || _pan < -100)
					throw new ArgumentOutOfRangeException(nameof(Pan));
				_pan = value;
				
			}
		}

		internal Instrument(Range<byte> key, Range<byte> vel, IAudioSource src, double pan = 0)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (vel == null)
				throw new ArgumentNullException(nameof(vel));
			if (!key.IsBetween(0, 127))
				throw new ArgumentOutOfRangeException(nameof(key));
			if (!vel.IsBetween(0, 127))
				throw new ArgumentOutOfRangeException(nameof(vel));
			if (src == null)
				throw new ArgumentNullException(nameof(src));
			KeyRange = key;
			VelocityRange = vel;
			Source = src;
			Pan = pan;
		}

		internal Instrument(IAudioSource src, double pan = 0)
			: this(new Range<byte>(0, 127), new Range<byte>(0, 127), src, pan) { }

		internal Instrument(byte noteNo, IAudioSource src, double pan = 0)
			: this(new Range<byte>(noteNo, noteNo), new Range<byte>(0, 127), src, pan) { }

		

	}

}

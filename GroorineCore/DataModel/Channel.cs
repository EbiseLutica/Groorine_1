using System;
using Groorine.Helpers;

namespace Groorine.DataModel
{
	public class Channel : BindableBase, IChannel
	{
		private byte _panpot;
		private byte _volume;
		private byte _expression;
		private int _pitchbend;
		private short _tweak;
		private short _noteShift;
		private short _bendRange;
		private double _freqExts;

		public byte Panpot
		{
			get { return _panpot; }
			set { SetProperty(ref _panpot, value); }
		}

		public byte Volume
		{
			get { return _volume; }
			set { SetProperty(ref _volume, value); }
		}

		private void CheckCc(byte ccValue)
		{
			if (ccValue > 127)
				throw new ArgumentOutOfRangeException(nameof(ccValue));
		}

		public byte Expression
		{

			get { return _expression; }
			set { SetProperty(ref _expression, value); }
		}

		public int Pitchbend
		{
			get { return _pitchbend; }
			set
			{
				SetProperty(ref _pitchbend, value);
				FreqExts = GetFreqExts();
			}
		}

		public short Tweak
		{
			get { return _tweak; }
			set
			{
				SetProperty(ref _tweak, value);
				FreqExts = GetFreqExts();
			}
		}

		public short NoteShift
		{
			get { return _noteShift; }
			set
			{
				SetProperty(ref _noteShift, value);
				FreqExts = GetFreqExts();
			}
		}

		public short BendRange
		{
			get { return _bendRange; }
			set
			{
				SetProperty(ref _bendRange, value);
				FreqExts = GetFreqExts();
			}
		}

		public double FreqExts
		{
			get { return _freqExts; }
			set { SetProperty(ref _freqExts, value); }
		}
		
		internal Channel(byte panpot, byte volume, byte expression, short tweak, short noteShift, short bendRange)
		{
			Panpot = panpot;
			Volume = volume;
			Expression = expression;
			Tweak = tweak;
			NoteShift = noteShift;
			BendRange = bendRange;
			Pitchbend = 0;
			FreqExts = GetFreqExts();
		}

		internal Channel() : this(64, 100, 100, 0, 0, 2) { }


		public double GetFreqExts() => _freqExts = Math.Pow(2, (Pitchbend / 8192d) * (BendRange / 12d)) * Math.Pow(2, (Tweak / 8192d) * (2 / 12d)) * Math.Pow(2, NoteShift / 12d);
	}
}
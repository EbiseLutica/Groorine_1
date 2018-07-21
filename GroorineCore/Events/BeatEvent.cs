using System;

namespace Groorine.Events
{

	public class BeatEvent : MetaEvent
	{
		private int _numerator = 4;
		/// <summary>
		/// 分子です．
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"/>
		public int Rhythm
		{
			get
			{
				return _numerator;
			}
			set
			{
				if (value < 1 || value > 32)
					throw new ArgumentOutOfRangeException(nameof(value));
				SetProperty(ref _numerator, value);
			}
		}
		private int _denominator = 4;

		public BeatEvent(int numerator, int denominator)
		{
			Rhythm = numerator;
			Note = denominator;
		}

		public BeatEvent()
			: this(4, 4) { }

		/// <summary>
		/// 分母です．
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"/>
		public int Note
		{
			get
			{
				return _denominator;
			}
			set
			{
				if ((value & value - 1) != 0 && value < 1 || value > 32)
					throw new ArgumentOutOfRangeException(nameof(value));
				SetProperty(ref _denominator, value);
			}
		}

		public override string DisplayName => "拍子";

		public override string ToString() => base.ToString() + $"{Rhythm}/{Note}";

	}

}
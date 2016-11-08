using System;

namespace GroorineCore
{

	public class TempoEvent : MetaEvent
	{
		public override string DisplayName => "Tempo";

		public int Tempo { get; set; }
		public override string ToString() => base.ToString() + $"{Tempo}";
	}
}
namespace GroorineCore
{
	public abstract class MetaEvent : BindableBase
	{
		public long Tick { get; set; }

		public abstract string DisplayName { get; }

		public override string ToString() => $"[{Tick}] {DisplayName}: ";
	}

	

}
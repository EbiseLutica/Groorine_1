namespace GroorineCore
{
	public abstract class MetaEvent : BindableBase
	{
		public long Tick { get; set; }

		public override string ToString() => $"[{Tick}] {nameof(MetaEvent)}.{GetType().Name} ";
	}
}
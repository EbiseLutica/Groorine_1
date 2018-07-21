namespace Groorine.Events
{

	public abstract class TextEventBase : MetaEvent
	{
		public override string DisplayName => "Text";

		public string Text { get; }

		public TextEventBase(string text)
		{
			Text = text;
		}


		public override string ToString() => base.ToString() + Text;
	}


}
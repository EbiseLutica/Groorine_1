namespace Groorine.Events
{

	public class CommentEvent : TextEventBase
	{
		public CommentEvent(string text) : base(text) { }
		public override string DisplayName => "Comment";
	}


}
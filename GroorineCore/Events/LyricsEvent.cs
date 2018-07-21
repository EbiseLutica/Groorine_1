namespace Groorine.Events
{

	public class LyricsEvent : TextEventBase
	{
		public LyricsEvent(string text) : base(text) { }
		public override string DisplayName => "Lyrics";
	}


}
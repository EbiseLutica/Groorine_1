using System;
using System.Linq;

namespace GroorineCore.Events
{

	public class EndOfTrackEvent : MetaEvent
	{
		public override string DisplayName => "EndOfTrack";
	}

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

	public class CommentEvent : TextEventBase
	{
		public CommentEvent(string text) : base(text) { }
		public override string DisplayName => "Comment";
	}

	public class LyricsEvent : TextEventBase
	{
		public LyricsEvent(string text) : base(text) { }
		public override string DisplayName => "Lyrics";
	}

	public class SysExEvent : MidiEvent
	{
		public byte[] Data { get; set; }
		public override string ToString() => base.ToString() + string.Concat(Data?.Select(b => Convert.ToString(b, 16) + "H "));
	}


}
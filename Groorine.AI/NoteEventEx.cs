using Groorine.Events;

namespace Groorine.AI
{
	/// <summary>
	/// 次のノートまでの待ち時間を持った NoteEvent です。
	/// </summary>
	internal class NoteEventEx : NoteEvent
	{
		public long Wait { get; set; }

		public override string ToString() => base.ToString() + $"{Wait} ";
	}
}

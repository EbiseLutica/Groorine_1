namespace Groorine.DataModel
{

	/// <summary>
	/// 位置情報とテンポのセットです。
	/// </summary>
	public class ScoreTempo
	{
		/// <summary>
		/// データの時刻です。
		/// </summary>
		public int MilliSeconds { get; }
		/// <summary>
		/// データの時刻です。
		/// </summary>
		public int Tick { get; }
		/// <summary>
		/// テンポの値です。
		/// </summary>
		public int Tempo { get; }
		/// <summary>
		/// ScoreTempo のインスタンスを作成します。
		/// </summary>
		internal ScoreTempo(int msec, int tick, int tempo)
		{
			MilliSeconds = msec;
			Tick = tick;
			Tempo = tempo;
		}
	}
}
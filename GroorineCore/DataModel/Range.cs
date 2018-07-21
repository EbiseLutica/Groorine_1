using System;
using Groorine.Helpers;

namespace Groorine.DataModel
{
	/// <summary>
	/// 任意の比較可能な型の特定の範囲を表します。
	/// </summary>
	/// <typeparam name="T">範囲を決める対象のデータ型。</typeparam>
	public class Range<T> : BindableBase where T : IComparable<T>
	{
		
		private T _from;
		private T _to;

		/// <summary>
		/// 範囲の始点を取得します。
		/// </summary>
		public T From
		{
			get { return _from; }
			private set { SetProperty(ref _from, value); }
		}

		/// <summary>
		/// 範囲の終点を取得します。
		/// </summary>
		public T To
		{
			get { return _to; }
			private set { SetProperty(ref _to, value); }
		}

		
		/// <summary>
		/// 値が指定された範囲内にあるかどうか比較します。
		/// </summary>
		/// <param name="from">範囲の始点。</param>
		/// <param name="me">比較する値。</param>
		/// <param name="to">範囲の終点</param>
		/// <returns>値が範囲内にあれば <see cref="true"/>、なければ<see cref="false"/>。</returns>
		public static bool Contains(T from, T me, T to) => from.CompareTo(me) <= 0 && me.CompareTo(to) <= 0;

		/// <summary>
		/// 値が現在のオブジェクトの範囲内にあるかどうか比較します。
		/// </summary>
		/// <param name="value">比較する値。</param>
		/// <returns>値が範囲内にあれば <see cref="true"/>、なければ<see cref="false"/>。</returns>
		public bool Contains(T value) => Contains(From, value, To);

		/// <summary>
		/// 指定された範囲内に現在のオブジェクトが示す範囲が含まれるかどうか比較します。
		/// </summary>
		/// <param name="from">範囲の始点。</param>
		/// <param name="to">範囲の終点。</param>
		/// <returns>指定された範囲内に現在のオブジェクトが含まれれば<see cref="true"/>。なければ<see cref="false"/>。</returns>
		public bool IsBetween(T from, T to) => from.CompareTo(From) <= 0 && To.CompareTo(to) <= 0;

		/// <summary>
		/// 指定された範囲内に現在のオブジェクトが示す範囲が含まれるかどうか比較します。
		/// </summary>
		/// <param name="target">比較する範囲を表す <see cref="Range{T}"/>。</param>
		/// <returns></returns>
		public bool IsBetween(Range<T> target) => IsBetween(target.From, target.To);


		/// <summary>
		/// 範囲の始点と終点を指定して、<see cref="Range&lt;T&gt;"/> クラスの新しいインスタンスを初期化します。
		/// </summary>
		/// <param name="from">範囲の始点。</param>
		/// <param name="to">範囲の終点。</param>
		internal Range(T from, T to)
		{
			if (from.CompareTo(to) > 0)
				throw new ArgumentException($"始点を終点よりも大きくすることはできません。");
			From = from;
			To = to;
		}


	}
}

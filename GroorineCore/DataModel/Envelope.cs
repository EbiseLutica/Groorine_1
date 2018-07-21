namespace Groorine.DataModel
{

	public struct Envelope
	{
		public int A { get; }
		public int D { get; }
		public int S { get; }
		public int R { get; }
		internal Envelope(int a, int d, int s, int r)
		{
			A = a;
			D = d;
			S = s;
			R = r;
			
		}
	}

}
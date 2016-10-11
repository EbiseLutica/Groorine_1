namespace GroorineCore
{
	public class Player : BindableBase
	{
		private MidiSynth _mySynth;
		private long _tick;
		private bool _isPlaying;

		public MidiSynth MySynth
		{
			get { return _mySynth; }
			set { SetProperty(ref _mySynth, value); }
		}

		public Player()
		{
			MySynth = new MidiSynth();
		}

		public long Tick
		{
			get { return _tick; }
			set { SetProperty(ref _tick, value); }
		}

		public bool IsPlaying
		{
			get { return _isPlaying; }
			set { SetProperty(ref _isPlaying, value); }
		}

		public void Play()
		{

		}

		public void Stop()
		{
			
		}

		public void Pause()
		{
			
		}

	}
}
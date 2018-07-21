using System.Collections.Generic;
using System.Linq;

namespace Groorine.Synth
{
	public class InstrumentList : List<IInstrument>
	{
		public IEnumerable<IInstrument> FindInstrumentsByNote(byte noteNo) => this.Where(i => i.KeyRange.Contains(noteNo));
		public IEnumerable<IInstrument> FindInstrumentsByVelocity(byte velocity) => this.Where(i => i.VelocityRange.Contains(velocity));
		public IEnumerable<IInstrument> FindInstruments(byte noteNo, byte velocity) => this.Where(i => i.KeyRange.Contains(noteNo) && i.VelocityRange.Contains(velocity));
	}

}

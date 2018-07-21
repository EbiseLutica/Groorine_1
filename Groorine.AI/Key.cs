using Groorine.Events;

namespace Groorine.AI
{
	public struct Key
	{
		public byte Note;
		public int Gate;
		public int Velocity;


		public Key(NoteEvent ne)
		{
			Note = ne.Note;
			Gate = (int)ne.Gate / 8 * 8;
			Velocity = ne.Velocity / 8 * 8;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Key))
			{
				return false;
			}

			var key = (Key)obj;
			return Note == key.Note &&
				   Gate == key.Gate &&
				   Velocity == key.Velocity;
		}

		public override int GetHashCode()
		{
			var hashCode = -916014847;
			hashCode = hashCode * -1521134295 + Note.GetHashCode();
			hashCode = hashCode * -1521134295 + Gate.GetHashCode();
			hashCode = hashCode * -1521134295 + Velocity.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(Key k1, Key k2) => k1.Gate == k2.Gate && k1.Note == k2.Note && k1.Velocity == k2.Velocity;
		public static bool operator !=(Key k1, Key k2) => !(k1 == k2);

	}

}

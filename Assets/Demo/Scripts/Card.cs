namespace Demo
{
	public enum ECardType
	{
		Invalid,
		Spade,
		Heart,
		Diamond,
		Club,
	}

	public class Card
	{
		public int m_iNumber { get; private set; }
		public ECardType m_eType { get; private set; }

		public Card(int number, ECardType type)
		{
			m_iNumber = number;
			m_eType = type;
		}

		public override string ToString()
		{
			return string.Format( "{0}{1}", m_eType, m_iNumber );
		}
	}
}
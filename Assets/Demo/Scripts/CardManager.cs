using System;
using System.Collections.Generic;
using UnityEngine;

namespace Demo
{
	public class CardManager
	{
		const int MINNUMBER = 1;
		const int MAXNUMBER = 9;

		public List<Card> m_listUnusedCards;
		public List<Card> m_listUsedCards;

		public CardManager()
		{
			m_listUnusedCards = new List<Card>();
			m_listUsedCards = new List<Card>();
		}

		~CardManager()
		{
			m_listUnusedCards.Clear();
			m_listUnusedCards = null;
		}

		public void InitCardPool()
		{
			m_listUnusedCards.Clear();

			foreach( ECardType _eType in Enum.GetValues( typeof( ECardType ) ) )
			{
				if( _eType != ECardType.Invalid )
				{
					for( int i = MINNUMBER; i <= MAXNUMBER; i++ )
						m_listUnusedCards.Add( new Card( i, _eType ) );
				}
			}
		}

		public List<Card> DrawCards(int count)
		{
			List<Card> _listCards = new List<Card>();
			if( count <= 0 )
			{
				LogMgr.Instance.Log( string.Format( "CardManager::DrawCards is failed! Count:{0} is invalid!", count ), LogType.Error );
				return _listCards;
			}

			if( count > m_listUnusedCards.Count )
				ShuffleCards();

			if( count > m_listUnusedCards.Count )
			{
				LogMgr.Instance.Log( string.Format( "CardManager::DrawCards is failed! Count:{0} is out of pool range!", count ), LogType.Error );
				return _listCards;
			}

			while( count > 0 )
			{
				int _iIndex = UnityEngine.Random.Range( 0, m_listUnusedCards.Count );
				Card _cCard = m_listUnusedCards[_iIndex];
				_listCards.Add( _cCard );
				m_listUnusedCards.Remove( _cCard );
				count--;
			}
			return _listCards;
		}

		public void DropCard(Card card)
		{
			if( card == null )
			{
				LogMgr.Instance.Log( "CardManager::DropCard is failed! Card is invalid!", LogType.Error );
				return;
			}

			m_listUsedCards.Add( card );
		}

		void ShuffleCards()
		{
			m_listUnusedCards.AddRange( m_listUsedCards );
			m_listUsedCards.Clear();
		}
	}
}

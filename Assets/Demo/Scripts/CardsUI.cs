using System;
using System.Collections.Generic;
using UnityEngine;

namespace Demo
{
	public class CardsUI : MonoBehaviour
	{
		[SerializeField]
		float m_fInterval = 50;
		[SerializeField]
		float normalHeight = 0;
		[SerializeField]
		float clickHeight = 0;
		[SerializeField]
		Sprite[] m_cArrNumberSprites = null;
		[SerializeField]
		Sprite[] m_cArrTypeSprites = null;
		[SerializeField]
		CardUI m_cCardPrefab = null;

		List<CardUI> m_listCardUIs;
		public Action<Vector2> onDraging;
		public Action<CardUI, Vector2> onDropCard;

		void Awake()
		{
			m_listCardUIs = new List<CardUI>();
		}

		public void AddCard(Card card)
		{
			CardUI cardUI = Instantiate( m_cCardPrefab, transform );
			cardUI.Initialize();
			cardUI.gameObject.SetActive( true );
			ChangeCardData( cardUI, card );
			cardUI.onBeginDrag += ChangeCardStatus;
			cardUI.onDraging += OnDraging;
			cardUI.onDragEnd += OnDropCard;
			cardUI.onClick += ChangeCardStatus;
			m_listCardUIs.Add( cardUI );
			RefreshCards();
		}

		public void ChangeCardData(CardUI cardUI, Card card)
		{
			cardUI.SetCardData( card );
			cardUI.SetCardNumberSprite( LoadCardNumber( card ) );
			cardUI.SetCardTypeSprite( LoadCardType( card ) );
		}

		public void RemoveCard(CardUI card)
		{
			m_listCardUIs.Remove( card );
			Destroy( card.gameObject );
			RefreshCards();
		}

		public void SetCradsDragable(bool canDrag)
		{
			foreach( var card in m_listCardUIs )
				card.SetDragable( canDrag );
		}

		public CardUI CreateCardUI()
		{
			var card = Instantiate( m_cCardPrefab, transform );
			card.Initialize();
			return card;
		}

		public void RefreshCards()
		{
			int _iCount = m_listCardUIs.Count;
			float fRight = 0;
			if( _iCount % 2 == 0 )
				fRight = m_fInterval * ( _iCount / 2 - 0.5f );
			else
				fRight = m_fInterval * ( ( _iCount - 1 ) / 2 );

			for( int i = 0; i < _iCount; i++ )
				m_listCardUIs[i].SetCardPosition( new Vector2( fRight - m_fInterval * i, normalHeight ) );
		}

		void OnDraging(Vector2 position)
		{
			onDraging?.Invoke( position );
		}

		void OnDropCard(CardUI card, Vector2 position)
		{
			onDropCard?.Invoke( card, position );
		}

		void ChangeCardStatus(CardUI card)
		{
			foreach( var c in m_listCardUIs )
				c.SetCardPosY( normalHeight );

			card.SetCardPosY( clickHeight );
		}

		Sprite LoadCardNumber(Card card)
		{
			return m_cArrNumberSprites[card.m_iNumber - 1];
		}

		Sprite LoadCardType(Card card)
		{
			return m_cArrTypeSprites[(int)card.m_eType - 1];
		}

		public int CardsCount
		{
			get { return m_listCardUIs.Count; }
		}
	}
}
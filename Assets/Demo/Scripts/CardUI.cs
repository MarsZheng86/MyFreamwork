using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Demo
{
	public class CardUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		public Card m_cCard { get; private set; }
		public bool m_bDraging { get; private set; }

		[SerializeField]
		float dragAlpha = 0.5f;
		[SerializeField]
		Vector3 normalScale = Vector3.zero;
		[SerializeField]
		Vector3 dragingScale = Vector3.zero;
		Camera m_sceneCamera = null;
		Camera uiCamera = null;
		BattleRole role;
		RectTransform parent;
		RectTransform rect;
		Image m_cBg;
		Image m_cNumber;
		Vector2 m_cOriginalPosition = Vector2.zero;

		public Action<CardUI> onBeginDrag;
		public Action<Vector2> onDraging;
		public Action<CardUI, Vector2> onDragEnd;
		public Action<CardUI> onClick;

		void Awake()
		{
			Initialize();
		}

		public void Initialize()
		{
			m_sceneCamera = GameObject.Find( "Camera" ).GetComponent<Camera>();
			uiCamera = GameObject.Find( "Camera_UI" ).GetComponent<Camera>();
			rect = gameObject.GetComponent<RectTransform>();
			parent = rect.parent.GetComponent<RectTransform>();
			m_cBg = rect.Find( "Bg" ).GetComponent<Image>();
			m_cNumber = m_cBg.transform.Find( "Number" ).GetComponent<Image>();
			rect.localScale = normalScale;
		}

		public void SetCardPosition(Vector2 pos)
		{
			rect.anchoredPosition = pos;
			m_cOriginalPosition = pos;
		}

		public void SetCardPosY(float y)
		{
			rect.anchoredPosition = new Vector2( rect.anchoredPosition.x, y );
		}

		public void SetCardOffset(Vector2 offset)
		{
			m_cBg.rectTransform.anchoredPosition = offset;
		}

		public void SetCardData(Card card)
		{
			m_cCard = card;
		}

		public void SetCardTypeSprite(Sprite sp)
		{
			m_cBg.sprite = sp;
		}

		public void SetCardNumberSprite(Sprite sp)
		{
			m_cNumber.sprite = sp;
		}

		public void RevertPosition()
		{
			rect.anchoredPosition = m_cOriginalPosition;
		}

		public void SetWorldPosition(Vector3 worldPosition)
		{
			var pos = m_sceneCamera.WorldToScreenPoint( worldPosition );
			Vector2 localPos = Vector2.zero;
			RectTransformUtility.ScreenPointToLocalPointInRectangle( parent, pos, uiCamera, out localPos );
			rect.anchoredPosition = localPos;
		}

		public void SetDragable(bool canDrag)
		{
			m_cBg.raycastTarget = canDrag;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			m_cBg.color = new Color( m_cBg.color.r, m_cBg.color.g, m_cBg.color.b, dragAlpha );
			m_cNumber.color = new Color( m_cNumber.color.r, m_cNumber.color.g, m_cNumber.color.b, dragAlpha );
			m_bDraging = true;
			onBeginDrag?.Invoke( this );
			rect.localScale = dragingScale;
		}

		public void OnDrag(PointerEventData eventData)
		{
			rect.anchoredPosition += eventData.delta;
			onDraging?.Invoke( eventData.position );
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			m_cBg.color = new Color( m_cBg.color.r, m_cBg.color.g, m_cBg.color.b, 1 );
			m_cNumber.color = new Color( m_cNumber.color.r, m_cNumber.color.g, m_cNumber.color.b, 1 );
			m_bDraging = false;
			onDragEnd?.Invoke( this, eventData.position );
			rect.localScale = normalScale;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if( !m_bDraging )
				onClick?.Invoke( this );
		}
	}
}
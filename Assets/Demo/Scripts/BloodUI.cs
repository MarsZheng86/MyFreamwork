using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
	[Serializable]
	public class Blood
	{
		public int Count;
		public Image UI;
	}

	public class BloodUI : MonoBehaviour
	{
		[SerializeField]
		Vector2 offset = Vector2.zero;
		[SerializeField]
		Blood[] bloods = null;
		[SerializeField]
		Sprite[] changeSprites = null;

		Camera m_sceneCamera = null;
		Camera uiCamera = null;
		BattleRole role;
		RectTransform parent;
		RectTransform rect;
		DOTweenAnimation tween;
		Image change;
		int currentHP;

		public void Init()
		{
			m_sceneCamera = GameObject.Find( "Camera" ).GetComponent<Camera>();
			uiCamera = GameObject.Find( "Camera_UI" ).GetComponent<Camera>();
			rect = GetComponent<RectTransform>();
			parent = rect.parent.GetComponent<RectTransform>();
			tween = transform.Find( "Change" ).GetComponent<DOTweenAnimation>();
			change = tween.GetComponent<Image>();

			foreach( var blood in bloods )
				blood.UI.material = Instantiate( blood.UI.material );

			change.material = Instantiate( change.material );
		}

		public void SetBlood(int hp)
		{
			currentHP = hp;

			foreach( var blood in bloods )
			{
				if( blood.Count <= hp )
				{
					blood.UI.gameObject.SetActive( true );
					hp -= blood.Count;
				}
				else
					blood.UI.gameObject.SetActive( false );
			}
		}

		public void SetPosition(Vector3 worldPosition)
		{
			var pos = m_sceneCamera.WorldToScreenPoint( worldPosition );
			Vector2 localPos = Vector2.zero;
			RectTransformUtility.ScreenPointToLocalPointInRectangle( parent, pos, m_sceneCamera, out localPos );
			rect.anchoredPosition = localPos + offset;
		}

		public void PlayAnimation(int count)
		{
			change.sprite = changeSprites[count];
			tween.gameObject.SetActive( true );
			tween.DORestart();
		}

		public void AnimationCallback()
		{
			tween.gameObject.SetActive( false );
		}

		public void SetRenderQueue(int queue)
		{
			foreach( var blood in bloods )
				blood.UI.material.renderQueue = queue;

			change.material.renderQueue = queue;
		}
	}
}

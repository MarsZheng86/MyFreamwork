using DG.Tweening;
using System;
using UnityEngine;

namespace Demo
{
	[Serializable]
	public class SceneBlood
	{
		public int Count;
		public SpriteRenderer UI;
	}

	public class SceneBloodUI : MonoBehaviour
	{
		[SerializeField]
		Vector3 offset = Vector3.zero;
		[SerializeField]
		Vector3 scale = Vector3.zero;
		[SerializeField]
		Vector3 criticalScale = Vector3.zero;
		[SerializeField]
		SceneBlood[] bloods = null;
		[SerializeField]
		Sprite[] changeSprites = null;

		Camera m_sceneCamera = null;
		Camera uiCamera = null;
		DOTweenAnimation tween;
		SpriteRenderer change;
		int currentHP;

		public void Init()
		{
			m_sceneCamera = GameObject.Find( "Camera" ).GetComponent<Camera>();
			uiCamera = GameObject.Find( "Camera_UI" ).GetComponent<Camera>();
			tween = transform.Find( "Change" ).GetComponent<DOTweenAnimation>();
			change = tween.transform.GetChild( 0 ).GetComponent<SpriteRenderer>();
			transform.localPosition = offset;
			transform.localScale = scale;
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

		public void PlayAnimation(int count, bool critical = false)
		{
			change.transform.localScale = critical ? criticalScale : Vector3.one;
			change.sprite = changeSprites[count];
			tween.gameObject.SetActive( true );
			tween.DORestart();
			DOTween.Restart( change.gameObject );
		}

		public void AnimationCallback()
		{
			change.transform.localScale = Vector3.one;
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
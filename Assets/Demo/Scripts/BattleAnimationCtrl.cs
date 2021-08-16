using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

namespace Demo
{
	public class BattleAnimationCtrl
	{
		SkeletonAnimation animation;

		public BattleAnimationCtrl(GameObject go)
		{
			animation = go.GetComponent<SkeletonAnimation>();
		}

		public void ChangeAnimationTimeScale(float scale)
		{
			animation.state.TimeScale = scale;
		}

		public void PlayAnimation(string name, bool loop, bool flip)
		{
			if( animation.AnimationName == name )
				return;

			animation.skeleton.ScaleX = flip ? -1 : 1;
			if( !string.IsNullOrEmpty( animation.AnimationName ) )
				animation.state.Data.SetMix( animation.AnimationName, name, 0 );

			animation.state.SetAnimation( 0, name, loop );
		}
	}
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Demo
{
	public enum BattleRoleState
	{
		Invalid,
		Idle,
		Move,
		UseSkill,
		MoveBack,
		BeHit,
	}

	public class BattleRole
	{
		const float ARRIVEDISTANCE = 0.01f;

		RoleConfig configData;
		BattleRoleState nextState;
		List<Grid> gridPath;
		BloodUI blood;
		BloodUI energy;
		SceneBloodUI sceneBlood;
		SceneBloodUI sceneEnergy;
		BattleAnimationCtrl animationCtrl;
		MeshRenderer roleRender;

		public int HP { get; private set; }
		public int EN { get; private set; }
		public int Attack { get; private set; }
		public int Defence { get; private set; }
		public int ActionSpeed { get; private set; }
		public int Damage { get; private set; }
		public int MoveSpeed { get; private set; }
		public GameObject GameObject { get; private set; }
		public AudioSource AudioSource { get; private set; }
		public BattleRoleState CurrentState { get; private set; }
		public Grid CurrentGrid { get; private set; }
		public CardUI CardUI { get; private set; }
		bool flip;

		public Action m_actOnRoleDead;
		public Action<int> m_actOnRoleHPChange;
		public Action<int> m_actOnRoleENChange;

		#region Constructor

		public BattleRole(RoleConfig data)
		{
			configData = data;
			CurrentState = BattleRoleState.Invalid;
			gridPath = new List<Grid>();
			LoadRoleConfig();
		}

		~BattleRole()
		{
			gridPath.Clear();
			m_actOnRoleDead = null;
			m_actOnRoleHPChange = null;
			m_actOnRoleENChange = null;
		}

		#endregion

		#region Life

		public void Update()
		{
			switch( nextState )
			{
				case BattleRoleState.Idle:
					EnterIdleState();
					break;
				case BattleRoleState.Move:
					EnterMoveState();
					break;
				case BattleRoleState.UseSkill:
					EnterUseSkillState();
					break;
				case BattleRoleState.MoveBack:
					EnterMoveBackState();
					break;
				case BattleRoleState.BeHit:
					EnterBeHitState();
					break;
			}

			if( blood )
				blood.SetPosition( GameObject.transform.position );

			if( energy )
				energy.SetPosition( GameObject.transform.position );

			if( CardUI )
				CardUI.SetWorldPosition( GameObject.transform.position );
		}

		#endregion

		#region Public Functions

		public void ChangeGameObject(GameObject go)
		{
			GameObject = go;
			AudioSource = go.GetComponent<AudioSource>();
			animationCtrl = new BattleAnimationCtrl( go.transform.GetChild( 0 ).gameObject );
			roleRender = go.GetComponent<MeshRenderer>();
		}

		public void SetFlip(bool needFlip)
		{
			flip = needFlip;
		}

		public void SetBloodUI(BloodUI ui)
		{
			blood = ui;
			blood.Init();
			blood.SetBlood( HP );
		}

		public void SetBloodUI(SceneBloodUI ui)
		{
			sceneBlood = ui;
			sceneBlood.Init();
			sceneBlood.SetBlood( HP );
		}

		public void SetEnergyUI(BloodUI ui)
		{
			energy = ui;
			energy.Init();
			energy.SetBlood( EN );
		}

		public void SetEnergyUI(SceneBloodUI ui)
		{
			sceneEnergy = ui;
			sceneEnergy.Init();
			sceneEnergy.SetBlood( EN );
		}

		public void SetCardUI(CardUI card)
		{
			CardUI = card;
		}

		public void ChangeHP(int point, bool critical)
		{
			HP = Mathf.Clamp( HP + point, 0, configData.HP );
			if( blood )
			{
				blood.SetBlood( HP );
				blood.PlayAnimation( Mathf.Abs( point ) );
			}
			if( sceneBlood )
			{
				sceneBlood.SetBlood( HP );
				sceneBlood.PlayAnimation( Mathf.Abs( point ), critical );
			}
			m_actOnRoleHPChange?.Invoke( point );

			if( HP == 0 )
				m_actOnRoleDead?.Invoke();
		}

		public void ChangeEN(int point)
		{
			EN = Mathf.Clamp( EN + point, 0, configData.EN );
			if( energy )
			{
				energy.SetBlood( EN );
				energy.PlayAnimation( Mathf.Abs( point ) );
			}
			if( sceneEnergy )
			{
				sceneEnergy.SetBlood( EN );
				sceneEnergy.PlayAnimation( Mathf.Abs( point ) );
			}
			m_actOnRoleENChange?.Invoke( point );
		}

		public void ChangeNextState(BattleRoleState state)
		{
			if( nextState == state )
			{
				Debug.LogErrorFormat( "BattleRole::ChangeState is failed! Role state is same {0}!", state );
				return;
			}

			nextState = state;
		}

		public void ChangeCurrentGrid(Grid grid)
		{
			if( grid == null )
			{
				Debug.LogError( "BattleRole::ChangeCurrentGrid is failed! Grid is invalid!" );
				return;
			}

			CurrentGrid = grid;
			RefreshRenderQueue();
		}

		//Temp
		public void ChangeMoveTargetGrid(AStar aStar, Grid targetGrid)
		{
			if( targetGrid == null )
			{
				Debug.LogError( "BattleRole::ChangeMoveTargetGrid is failed! Grid is invalid!" );
				return;
			}

			//if( m_eCurrentState == BattleRoleState.Move )
			//{
			//	Debug.LogError( "BattleRole::ChangeMoveTargetGrid is failed! Last move is not finished!" );
			//	return;
			//}

			if( aStar.FindPath( CurrentGrid, targetGrid, gridPath ) )
			{
				if( gridPath[0] == CurrentGrid )
				{
					gridPath.RemoveAt( 0 );
					Debug.LogFormat( "BattleRole::ChangeMoveTargetGrid remove current grid:{0},{1}!", CurrentGrid.m_cPoint.m_iX, CurrentGrid.m_cPoint.m_iY );
				}

				if( gridPath[gridPath.Count - 1] != targetGrid )
				{
					Debug.LogFormat( "BattleRole::ChangeMoveTargetGrid add target grid:{0},{1}!", targetGrid.m_cPoint.m_iX, targetGrid.m_cPoint.m_iY );
					gridPath.Add( targetGrid );
				}
			}
		}

		public bool IsDead()
		{
			return HP > 0;
		}

		public void RefreshRenderQueue()
		{
			if( blood )
				blood.SetRenderQueue( roleRender.material.renderQueue + 1 );

			if( energy )
				energy.SetRenderQueue( roleRender.material.renderQueue + 1 );
		}

		#endregion

		#region Private Functions

		void LoadRoleConfig()
		{
			//Todo
			HP = configData.HP;
			EN = configData.EN;
			Attack = configData.Attack;
			Defence = configData.Defence;
			ActionSpeed = configData.ActionSpeed;
			Damage = configData.Damage;
			MoveSpeed = configData.MoveSpeed;
		}

		#region Role State Machine

		void EnterIdleState()
		{
			if( CurrentState == BattleRoleState.Idle )
				return;

			//Todo
			SetCurrentState( BattleRoleState.Idle );
			PlayAnimation( "idle", true );
		}

		void EnterMoveState()
		{
			if( CurrentState == BattleRoleState.Move )
				RoleMove();
			else
				SetCurrentState( BattleRoleState.Move );
		}

		void EnterUseSkillState()
		{
			if( CurrentState == BattleRoleState.UseSkill )
				return;

			//Todo
			SetCurrentState( BattleRoleState.UseSkill );
			PlayAnimation( "attack", true );
		}

		void EnterMoveBackState()
		{
			if( CurrentState == BattleRoleState.MoveBack )
				MoveBack();
			else
				SetCurrentState( BattleRoleState.MoveBack );
		}

		void EnterBeHitState()
		{
			if( CurrentState == BattleRoleState.BeHit )
				return;

			//Todo
			SetCurrentState( BattleRoleState.BeHit );
			PlayAnimation( "behit", true );
		}

		void SetCurrentState(BattleRoleState state)
		{
			if( CurrentState == state )
			{
				Debug.LogErrorFormat( "BattleRole::SetCurrentState is failed! Current state is {0}!", state );
				return;
			}

			CurrentState = state;
		}

		#endregion

		void RoleMove()
		{
			if( gridPath.Count == 0 )
				return;

			if( !CanMove() )
				return;

			if( gridPath.Count == 1 && gridPath[0].m_cRole != null )
			{
				float _fDistance = Vector3.Distance( GameObject.transform.position, ( CurrentGrid.m_cCenter + gridPath[0].m_cCenter ) / 2 );
				if( _fDistance <= ARRIVEDISTANCE )
				{
					ChangeRolePosition( ( CurrentGrid.m_cCenter + gridPath[0].m_cCenter ) / 2 );
					gridPath.RemoveAt( 0 );
					ChangeNextState( BattleRoleState.Idle );
					return;
				}
				ChangeRolePosition( Vector3.MoveTowards( GameObject.transform.position, ( CurrentGrid.m_cCenter + gridPath[0].m_cCenter ) / 2, MoveSpeed * Time.deltaTime ) );
			}
			else
			{
				float _fDistance = Vector3.Distance( GameObject.transform.position, gridPath[0].m_cCenter );
				if( _fDistance <= ARRIVEDISTANCE )
				{
					ChangeCurrentGrid( gridPath[0] );
					gridPath.RemoveAt( 0 );
				}
				ChangeRolePosition( Vector3.MoveTowards( GameObject.transform.position, gridPath[0].m_cCenter, MoveSpeed * Time.deltaTime ) );
			}

			PlayAnimation( "run", true );
		}

		void MoveBack()
		{
			float _fDistance = Vector3.Distance( GameObject.transform.position, CurrentGrid.m_cCenter );
			if( _fDistance <= ARRIVEDISTANCE )
			{
				ChangeRolePosition( CurrentGrid.m_cCenter );
				ChangeNextState( BattleRoleState.Idle );
				return;
			}

			ChangeRolePosition( Vector3.MoveTowards( GameObject.transform.position, CurrentGrid.m_cCenter, MoveSpeed * Time.deltaTime ) );
			PlayAnimation( "run", false );
		}

		bool CanMove()
		{
			return true;
		}

		void ChangeRolePosition(Vector3 pos)
		{
			if( !GameObject )
			{
				Debug.LogError( "BattleRole::ChangeRolePosition is failed! GameObject is invalid!" );
				return;
			}

			GameObject.transform.position = pos;
		}

		void PlayAnimation(string animationName, bool f)
		{
			animationCtrl.PlayAnimation( animationName, true, flip != f ? true : false );
		}

		#endregion
	}
}
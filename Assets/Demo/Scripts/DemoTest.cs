using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Demo
{
	[Serializable]
	public class GridPrefabConfig
	{
		public EGridType m_eType;
		public GameObject[] m_goArrPrefabs;
	}

	[Serializable]
	public class RoleConfig
	{
		public int HP;
		public int EN;
		//public int MP;
		public int Attack;
		public int Defence;
		public int ActionSpeed;
		public int Damage;
		public int MoveSpeed;
	}

	class BattleOrder
	{
		public BattleRole Attacker { get; private set; }
		public BattleRole Target { get; private set; }
		public Card Card { get; private set; }
		public int OrderIndex
		{
			get
			{
				return Card.m_iNumber * 100 + Attacker.ActionSpeed;
			}
		}

		public BattleOrder(BattleRole attacker, BattleRole target, Card card)
		{
			Attacker = attacker;
			Target = target;
			Card = card;
		}

		public void ChangeCard(Card card)
		{
			Card = card;
		}
	}

	public class DemoTest : MonoBehaviour
	{
		const float RADIUS = 1.5f;
		const int MAXCARDCOUNT = 5;

		[SerializeField]
		AudioSource audioSource = null;
		[SerializeField]
		AudioClip[] clips = null;
		[SerializeField]
		Vector3 sceneOffset = Vector3.zero;
		[SerializeField]
		TextAsset m_cCurrentFileConfig = null;
		[SerializeField]
		CardsUI m_cCardsUIPrefab = null;
		[SerializeField]
		Camera m_sceneCamera = null;
		//[SerializeField]
		//BloodUI bloodPrefab = null;
		//[SerializeField]
		//BloodUI energyPrefab = null;
		[SerializeField]
		SceneBloodUI sceneBloodPrefab = null;
		[SerializeField]
		SceneBloodUI sceneEnergyPrefab = null;
		[SerializeField]
		Vector2 cardScale = Vector2.one;
		[SerializeField]
		Vector2 selfCardOffset = Vector2.zero;
		[SerializeField]
		Vector2 enemyCardOffset = Vector2.zero;
		[SerializeField]
		GridPrefabConfig[] m_cConfig = null;
		[SerializeField]
		Point selfPoint = null;
		[SerializeField]
		Point enemyPoint = null;
		[SerializeField]
		Button buttonGo = null;
		[SerializeField]
		GameObject selfRolePrefab = null;
		[SerializeField]
		GameObject enemyRolePrefab = null;
		[SerializeField]
		Vector3 roleQuaternion = Vector3.zero;
		[SerializeField]
		RoleConfig selfConfig = null;
		[SerializeField]
		RoleConfig enemyConfig = null;

		GridManager m_cGridManager;
		AStar m_cAStar;
		Transform m_cGridSceneRoot;
		Grid currentGrid;
		int m_iRow;
		int m_iCol;
		Dictionary<string, GameObject[]> m_dictGridPrefabs;

		int m_iDrawCardCount;
		bool battleFinish;
		bool animating;
		BattleRole m_cSelfRole;
		BattleRole m_cEnemyRole;
		CardManager m_cCardManager;
		CardsUI m_cCardsUI;
		BloodUI selfBlood;
		BloodUI enemyBlood;
		List<Card> m_listEnemyCards;
		List<BattleOrder> orders;

		void Start()
		{
			m_cGridManager = new GridManager();
			m_cGridManager.SetGridRadius( RADIUS );
			m_cAStar = new AStar( m_cGridManager );
			m_dictGridPrefabs = new Dictionary<string, GameObject[]>();
			foreach( GridPrefabConfig _cConfig in m_cConfig )
				m_dictGridPrefabs[_cConfig.m_eType.ToString()] = _cConfig.m_goArrPrefabs;

			LoadScene();
			m_cCardManager = new CardManager();
			m_cCardManager.InitCardPool();
			//Transform sceneUIRoot = transform.Find( "SCENE_ROOT/SCENE_CANVAS" );
			Transform _cUIRoot = transform.Find( "UI_ROOT/Canvas_Normal" );
			m_listEnemyCards = new List<Card>();
			orders = new List<BattleOrder>();
			m_cSelfRole = new BattleRole( selfConfig );
			m_cEnemyRole = new BattleRole( enemyConfig );

			CreateRole( selfPoint, m_cSelfRole, selfRolePrefab );
			m_cSelfRole.SetFlip( false );
			//m_cSelfRole.SetBloodUI( Instantiate( bloodPrefab, sceneUIRoot ) );
			//m_cSelfRole.SetEnergyUI( Instantiate( energyPrefab, sceneUIRoot ) );
			m_cSelfRole.SetBloodUI( Instantiate( sceneBloodPrefab, m_cSelfRole.GameObject.transform.GetChild( 0 ) ) );
			m_cSelfRole.SetEnergyUI( Instantiate( sceneEnergyPrefab, m_cSelfRole.GameObject.transform.GetChild( 0 ) ) );
			m_cSelfRole.ChangeNextState( BattleRoleState.Idle );
			CreateRole( enemyPoint, m_cEnemyRole, enemyRolePrefab );
			m_cEnemyRole.SetFlip( true );
			//m_cEnemyRole.SetBloodUI( Instantiate( bloodPrefab, sceneUIRoot ) );
			//m_cEnemyRole.SetEnergyUI( Instantiate( energyPrefab, sceneUIRoot ) );
			m_cEnemyRole.SetBloodUI( Instantiate( sceneBloodPrefab, m_cEnemyRole.GameObject.transform.GetChild( 0 ) ) );
			m_cEnemyRole.SetEnergyUI( Instantiate( sceneEnergyPrefab, m_cEnemyRole.GameObject.transform.GetChild( 0 ) ) );
			m_cEnemyRole.ChangeNextState( BattleRoleState.Idle );

			m_cCardsUI = Instantiate( m_cCardsUIPrefab, _cUIRoot );
			m_cCardsUI.onDraging += OnDraging;
			m_cCardsUI.onDropCard += OnDropCard;
			SupplyCards();
			buttonGo.gameObject.SetActive( false );
			m_cSelfRole.SetCardUI( m_cCardsUI.CreateCardUI() );
			m_cSelfRole.CardUI.SetDragable( false );
			m_cSelfRole.CardUI.SetCardOffset( selfCardOffset );
			m_cSelfRole.CardUI.transform.localScale = cardScale;
			m_cSelfRole.CardUI.gameObject.SetActive( false );
			m_cSelfRole.RefreshRenderQueue();
			m_cEnemyRole.SetCardUI( m_cCardsUI.CreateCardUI() );
			m_cEnemyRole.CardUI.SetDragable( false );
			m_cEnemyRole.CardUI.SetCardOffset( enemyCardOffset );
			m_cEnemyRole.CardUI.transform.localScale = cardScale;
			m_cEnemyRole.CardUI.gameObject.SetActive( false );
			m_cEnemyRole.RefreshRenderQueue();
		}

		void Update()
		{
			if( !battleFinish )
			{
				m_cSelfRole.Update();
				m_cEnemyRole.Update();
			}
		}

		void LoadScene()
		{
			GridManagerData m_cConfigData = JsonConvert.DeserializeObject<GridManagerData>( m_cCurrentFileConfig.text );
			m_iRow = m_cConfigData.m_iRow;
			m_iCol = m_cConfigData.m_iCol;
			m_cGridSceneRoot = new GameObject( "GridSceneRoot" ).transform;
			m_cGridSceneRoot.SetParent( transform.Find( "SCENE_ROOT/ROOT" ) );
			m_cGridSceneRoot.localPosition = Vector3.zero;
			m_cGridManager.CreateGrids( m_cConfigData, m_dictGridPrefabs, m_cGridSceneRoot );
			m_cGridSceneRoot.localPosition = sceneOffset;
			Debug.Log( "GridEditorWindow::LoadScene success!" );
		}

		void CreateRole(Point p, BattleRole role, GameObject prefab)
		{
			var grid = m_cGridManager.GetGrid( p );
			if( grid != null )
			{
				var go = Instantiate( prefab, m_cGridSceneRoot.parent );
				go.transform.position = grid.m_cCenter;
				go.name = role == m_cSelfRole ? "Self" : "Enemy";
				go.transform.localRotation = Quaternion.Euler( roleQuaternion );//m_sceneCamera.transform.localRotation;
				role.ChangeGameObject( go );
				role.ChangeCurrentGrid( grid );
				grid.ChangeRole( role );
			}
		}

		void SupplyCards()
		{
			int selfSupplyCount = MAXCARDCOUNT - m_cCardsUI.CardsCount;
			if( selfSupplyCount > 0 )
			{
				List<Card> _listCards = m_cCardManager.DrawCards( selfSupplyCount );
				foreach( Card _cCard in _listCards )
					m_cCardsUI.AddCard( _cCard );
			}
			int enemySupplyCount = MAXCARDCOUNT - m_listEnemyCards.Count;
			if( enemySupplyCount > 0 )
				m_listEnemyCards.AddRange( m_cCardManager.DrawCards( enemySupplyCount ) );
		}

		void DropCard(CardUI cardUI)
		{
			if( orders.Count > 0 )
			{
				var data = cardUI.m_cCard;
				m_cCardsUI.ChangeCardData( cardUI, m_cSelfRole.CardUI.m_cCard );
				cardUI.RevertPosition();
				m_cSelfRole.CardUI.SetCardData( data );
				m_cCardsUI.ChangeCardData( m_cSelfRole.CardUI, data );
				orders[0].ChangeCard( data );
				return;
			}
			m_cCardsUI.ChangeCardData( m_cSelfRole.CardUI, cardUI.m_cCard );
			m_cSelfRole.CardUI.gameObject.SetActive( true );
			orders.Add( new BattleOrder( m_cSelfRole, m_cEnemyRole, cardUI.m_cCard ) );
			m_cCardManager.DropCard( cardUI.m_cCard );
			m_cCardsUI.RemoveCard( cardUI );
			int index = UnityEngine.Random.Range( 0, m_listEnemyCards.Count );
			Card card = m_listEnemyCards[index];
			orders.Add( new BattleOrder( m_cEnemyRole, m_cSelfRole, card ) );
			m_listEnemyCards.RemoveAt( index );
			buttonGo.gameObject.SetActive( true );
		}

		void OnDraging(Vector2 pos)
		{
			Grid _cGrid = m_cGridManager.CalculatePositionGrid( ScreenToWorldPosition( pos ) );
			if( _cGrid != currentGrid )
			{
				if( currentGrid != null )
					currentGrid.HighLight( false );

				currentGrid = _cGrid;
				if( currentGrid != null )
				{
					currentGrid.HighLight( true );
					currentGrid.m_cAudioSource.PlayOneShot( clips[4] );
				}
			}
		}

		void OnDropCard(CardUI card, Vector2 pos)
		{
			if( currentGrid != null )
				currentGrid.HighLight( false );

			Grid _cGrid = m_cGridManager.CalculatePositionGrid( ScreenToWorldPosition( pos ) );
			if( _cGrid != null && _cGrid.m_cRole == m_cEnemyRole )
				DropCard( card );
			else
				card.RevertPosition();
		}

		Vector3 ScreenToWorldPosition(Vector2 pos)
		{
			Vector3 worldPos = m_sceneCamera.ScreenToWorldPoint( new Vector3( pos.x, pos.y, m_sceneCamera.transform.position.z ) );
			var dir = worldPos - m_sceneCamera.transform.position;
			var dis = -worldPos.y / dir.y;
			return worldPos + dir * dis;
		}

		public void OnClickGo()
		{
			if( !animating && orders.Count > 0 )
			{
				audioSource.PlayOneShot( clips[0] );
				buttonGo.gameObject.SetActive( false );
				StartCoroutine( PlayOrders() );
			}
		}

		public void OnClickBg()
		{
			m_cCardsUI.RefreshCards();
		}

		IEnumerator PlayOrders()
		{
			animating = true;
			m_cCardsUI.SetCradsDragable( false );
			var newOrders = orders.OrderBy( o => o.OrderIndex );
			foreach( var order in newOrders )
			{
				order.Attacker.CardUI.gameObject.SetActive( true );
				m_cCardsUI.ChangeCardData( order.Attacker.CardUI, order.Card );

				order.Attacker.ChangeEN( -1 );
				order.Attacker.ChangeMoveTargetGrid( m_cAStar, order.Target.CurrentGrid );
				order.Attacker.ChangeNextState( BattleRoleState.Move );
				yield return null;
				yield return null;
				while( order.Attacker.CurrentState == BattleRoleState.Move )
					yield return null;

				order.Attacker.CardUI.gameObject.SetActive( false );
				order.Attacker.ChangeNextState( BattleRoleState.UseSkill );
				yield return new WaitForSeconds( 0.3f );
				order.Attacker.AudioSource.PlayOneShot( clips[1] );
				yield return new WaitForSeconds( 0.2f );
				order.Attacker.AudioSource.PlayOneShot( clips[2] );
				yield return new WaitForSeconds( 0.2f );
				order.Attacker.AudioSource.PlayOneShot( clips[3] );
				order.Target.ChangeNextState( BattleRoleState.BeHit );

				var totalAttack = order.Attacker.Attack + order.Card.m_iNumber;
				var damage = 0;
				if( totalAttack < order.Target.Defence )
					damage = 1;
				else if( totalAttack > order.Target.Defence * 2 )
					damage = order.Attacker.Damage * 2;
				else
					damage = order.Attacker.Damage;

				order.Target.ChangeHP( -damage, damage == order.Attacker.Damage * 2 );
				Debug.LogFormat( "Attack:{0} Defence:{1} Damage:{2} Number:{3} FinalDamage:{4} RemainHP:{5}", order.Attacker.Attack, order.Target.Defence, order.Attacker.Damage, order.Card.m_iNumber, damage, order.Target.HP );

				yield return new WaitForSeconds( 0.55f );
				order.Target.ChangeNextState( BattleRoleState.Idle );
				yield return new WaitForSeconds( 0.25f );
				order.Attacker.ChangeNextState( BattleRoleState.MoveBack );
				yield return null;
				yield return null;
				while( order.Attacker.CurrentState == BattleRoleState.MoveBack )
					yield return null;

				if( order.Target.HP <= 0 )
				{
					Debug.Log( "Battle Finish!" );
					battleFinish = true;
					orders.Clear();
					m_cCardsUI.SetCradsDragable( false );
					yield break;
				}
				//Debug.LogFormat( "Order attacker:{0} target:{1} card:{2} finish", order.Attacker.GameObject, order.Target.GameObject, order.Card.ToString() );
				yield return new WaitForSeconds( 1 );
			}

			animating = false;
			m_cCardsUI.SetCradsDragable( !battleFinish );
			orders.Clear();
			if( !battleFinish )
				SupplyCards();
		}
	}
}
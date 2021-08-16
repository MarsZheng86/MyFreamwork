using Newtonsoft.Json;
using UnityEngine;

namespace Demo
{
	public enum EGridSideType
	{
		Invalid = 0,
		Normal,
		Border,
		Barrier,
	}

	public enum EGridType
	{
		Invalid = 0,
		Grass,
		Desert,
		Snow,
		Flower,
	}

	public class GridData
	{
		public int m_iXIndex;
		public int m_iYIndex;
		public EGridType m_eGridType;
		public string m_strPrefabName;
		public EGridSideType[] m_eArrGridSidesType;
	}

	public class Grid
	{
		const int SIDECOUNT = 6;
		public Point m_cPoint { get; private set; }
		public Vector3 m_cCenter { get; private set; }

		// Up Down UpRight DownRight DownLeft UpLeft
		public Point[] m_cArrArroundPoints { get; private set; }

		public EGridType m_eType { get; private set; }
		public GameObject m_goPrefab { get; private set; }
		public BattleRole m_cRole { get; private set; }
		public AudioSource m_cAudioSource { get; private set; }

		// UpRight DownRight Down DownLeft UpLeft Up
		EGridSideType[] sides;

		MeshRenderer meshRenderer;
		Material material;
		Material highlightMaterial;

		public Grid(Point p, Vector3 center, EGridType type = EGridType.Invalid)
		{
			m_cPoint = p;
			m_cCenter = center;
			m_eType = type;

			InitArroundPoints();
			sides = new EGridSideType[SIDECOUNT] { EGridSideType.Invalid, EGridSideType.Invalid, EGridSideType.Invalid, EGridSideType.Invalid, EGridSideType.Invalid, EGridSideType.Invalid };
		}

		void InitArroundPoints()
		{
			//bool isDoubleRaw = Point.Y % 2 == 0;
			//ArroundPoints = new Point[]
			//{
			//	new Point( Point.X + ( isDoubleRaw ? 1 : 0 ), Point.Y + 1 ),
			//	new Point( Point.X + ( isDoubleRaw ? 1 : 0 ), Point.Y - 1 ),
			//	new Point( Point.X, Point.Y - 2 ),
			//	new Point( Point.X - ( isDoubleRaw ? 0 : 1 ), Point.Y - 1 ),
			//	new Point( Point.X - ( isDoubleRaw ? 0 : 1 ), Point.Y + 1 ),
			//	new Point( Point.X, Point.Y + 2 ),
			//};
			bool _bIsDoubleCol = m_cPoint.m_iX % 2 == 0;
			m_cArrArroundPoints = new Point[]
			{
				new Point( m_cPoint.m_iX, m_cPoint.m_iY + 1 ),
				new Point( m_cPoint.m_iX, m_cPoint.m_iY - 1 ),
				new Point( m_cPoint.m_iX + 1, m_cPoint.m_iY + ( _bIsDoubleCol ? 1 : 0 ) ),
				new Point( m_cPoint.m_iX + 1, m_cPoint.m_iY - ( _bIsDoubleCol ? 0 : 1 ) ),
				new Point( m_cPoint.m_iX - 1, m_cPoint.m_iY - ( _bIsDoubleCol ? 0 : 1 ) ),
				new Point( m_cPoint.m_iX - 1, m_cPoint.m_iY + ( _bIsDoubleCol ? 1 : 0 ) ),
			};
		}

		~Grid()
		{
		}

		void ResetGridSides()
		{
			for( int i = 0; i < SIDECOUNT; i++ )
				ChangeSide( i, EGridSideType.Invalid );
		}

		public void ChangeSide(int side, EGridSideType type)
		{
			if( side < 0 || side >= SIDECOUNT )
			{
				LogMgr.Instance.Log( string.Format( "Side index:{0} is out of range!", side ), LogType.Error );
				return;
			}

			sides[side] = type;
		}

		public void ChangeSides(EGridSideType[] sides)
		{
			if( sides.Length != SIDECOUNT )
			{
				LogMgr.Instance.Log( string.Format( "Side count:{0} is invalid!", sides.Length ), LogType.Error );
				return;
			}

			this.sides = sides;
		}

		public bool CanThrough(int side)
		{
			if( m_eType == EGridType.Invalid )
				return false;

			if( side < 0 || side >= SIDECOUNT )
			{
				LogMgr.Instance.Log( string.Format( "Side index:{0} is out of range!", side ), LogType.Error );
				return false;
			}

			return sides[side] == EGridSideType.Normal;
		}

		public void SetGridType(EGridType type)
		{
			m_eType = type;
		}

		public void SetGridPrefab(GameObject prefab)
		{
			m_goPrefab = prefab;
			if( Application.isPlaying )
			{
				meshRenderer = prefab.transform.GetChild( 0 ).GetComponent<MeshRenderer>();
				material = meshRenderer.material;
				m_cAudioSource = prefab.GetComponent<AudioSource>();
			}
		}

		public string SerializeObjectToJson()
		{
			GridData _cData = new GridData()
			{
				m_iXIndex = m_cPoint.m_iX,
				m_iYIndex = m_cPoint.m_iY,
				m_eGridType = m_eType,
				m_strPrefabName = m_goPrefab != null ? m_goPrefab.name : "",
				m_eArrGridSidesType = sides,
			};

			return JsonConvert.SerializeObject( _cData );
		}

		public void ChangeRole(BattleRole role)
		{
			m_cRole = role;
		}

		public void HighLight(bool hightLight)
		{
			if( !hightLight )
				meshRenderer.material = material;
			else
			{
				if( !highlightMaterial )
				{
					highlightMaterial = UnityEngine.Object.Instantiate( material );
					highlightMaterial.shader = Shader.Find( "FX/Flare" );
				}
				meshRenderer.material = highlightMaterial;
			}
		}

#if UNITY_EDITOR

		public void RemoveGridPrefab()
		{
			if( m_goPrefab != null )
				Object.DestroyImmediate( m_goPrefab );

			m_goPrefab = null;
			m_eType = EGridType.Invalid;
			ResetGridSides();
		}

#endif
	}
}
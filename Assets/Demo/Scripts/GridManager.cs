using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Demo
{
	[Serializable]
	public class Point : IEquatable<Point>
	{
		public int m_iX;
		public int m_iY;

		public Point(int x, int y)
		{
			m_iX = x;
			m_iY = y;
		}

		public override bool Equals(object obj)
		{
			if( obj == null )
				return false;

			Point _cP = obj as Point;

			if( (object)_cP == null )
				return false;

			return m_iX == _cP.m_iX && m_iY == _cP.m_iY;
		}

		public bool Equals(Point other)
		{
			if( (object)other == null )
				return false;

			return m_iX == other.m_iX && m_iY == other.m_iY;
		}

		public override int GetHashCode()
		{
			return m_iX.GetHashCode() ^ m_iY.GetHashCode();
		}

		public static bool operator ==(Point a, Point b)
		{
			if( ReferenceEquals( a, b ) )
				return true;

			if( (object)a == null || (object)b == null )
				return false;

			return a.Equals( b );
		}

		public static bool operator !=(Point a, Point b)
		{
			return !( a == b );
		}
	}

	public class GridManagerData
	{
		public int m_iRow;
		public int m_iCol;
		public float m_fRadius;
		public string[] m_strArrGridDatas;
	}

	public class GridManager
	{
		public float m_fWidthRadius { get; private set; }
		public float m_fHeightRadius { get; private set; }
		public float m_fRatio { get; private set; }
		public int m_iRow { get; private set; }
		public int m_iCol { get; private set; }

		Dictionary<string, Grid> m_dictGrids;
		Vector3[] m_cArrGridVertexOffset;

		public GridManager()
		{
			m_fRatio = Mathf.Sqrt( 3 );
			m_dictGrids = new Dictionary<string, Grid>();
		}

		~GridManager()
		{
			m_dictGrids.Clear();
		}

		#region Public Functions

		public void SetGridRadius(float gridRadius)
		{
			if( m_fWidthRadius != gridRadius )
			{
				m_fWidthRadius = gridRadius;
				m_fHeightRadius = gridRadius * m_fRatio / 2f;

				m_cArrGridVertexOffset = new Vector3[]
				{
					new Vector3( m_fWidthRadius * 0.5f, 0, m_fHeightRadius ),
					new Vector3( m_fWidthRadius, 0, 0 ),
					new Vector3( m_fWidthRadius * 0.5f, 0 , -m_fHeightRadius),
					new Vector3( -m_fWidthRadius * 0.5f, 0, -m_fHeightRadius ),
					new Vector3( -m_fWidthRadius, 0, 0 ),
					new Vector3( -m_fWidthRadius * 0.5f, 0 , m_fHeightRadius),
				};
			}
		}

		public void CreateGrids(int row, int col)
		{
			m_dictGrids.Clear();
			m_iRow = row;
			m_iCol = col;

			for( int i = 0; i < row; i++ )
			{
				for( int j = 0; j < col; j++ )
				{
					Point _cPoint = new Point( i + 1, j + 1 );
					Vector3 _cCenter = CalculateGridPosition( _cPoint );
					Grid _cGrid = new Grid( _cPoint, _cCenter );
					m_dictGrids[GetGridKey( _cPoint.m_iX, _cPoint.m_iY )] = _cGrid;
				}
			}
		}

		public void CreateGrids(GridManagerData configData, Dictionary<string, GameObject[]> gridPrefabs, Transform root)
		{
			m_dictGrids.Clear();
			m_iRow = configData.m_iRow;
			m_iCol = configData.m_iCol;
			SetGridRadius( configData.m_fRadius );
			foreach( string _strData in configData.m_strArrGridDatas )
			{
				GridData _cGridData = JsonConvert.DeserializeObject<GridData>( _strData );
				string _strType = _cGridData.m_eGridType.ToString();
				GameObject _goPrefab = null;
				if( gridPrefabs.ContainsKey( _strType ) )
					_goPrefab = gridPrefabs[_strType].FirstOrDefault( go => go.name == _cGridData.m_strPrefabName );

				CreateGrid( _cGridData, _goPrefab, root );
			}
		}

		public Grid GetGrid(Point p)
		{
			Grid _cGrid = null;
			if( m_dictGrids.TryGetValue( GetGridKey( p.m_iX, p.m_iY ), out _cGrid ) )
				return _cGrid;

			return _cGrid;
		}

		//public Grid CalculatePositionGrid(Vector3 worldPos)
		//{
		//	Vector3 offset = worldPos - Origin;
		//	int yIndex = Mathf.FloorToInt( offset.y / HeightRadius );
		//	int xIndex = CalculatePositionIndexX( offset.x, yIndex );

		//	string key = GetGridKey( xIndex, yIndex );
		//	if( PositionInGrid( key, worldPos ) )
		//		return grids[key];

		//	key = GetGridKey( xIndex + 1, yIndex );
		//	if( PositionInGrid( key, worldPos ) )
		//		return grids[key];

		//	yIndex++;
		//	xIndex = CalculatePositionIndexX( offset.x, yIndex );

		//	key = GetGridKey( xIndex, yIndex );
		//	if( PositionInGrid( key, worldPos ) )
		//		return grids[key];

		//	key = GetGridKey( xIndex + 1, yIndex );
		//	if( PositionInGrid( key, worldPos ) )
		//		return grids[key];

		//	return null;
		//}

		public Grid CalculatePositionGrid(Vector3 worldPos)
		{
			int _iXIndex = Mathf.FloorToInt( ( worldPos.x - m_fWidthRadius ) / m_fWidthRadius / 1.5f ) + 1;
			int _iYIndex = CalculatePositionIndexY( worldPos.z, _iXIndex );
			string _strYey = GetGridKey( _iXIndex, _iYIndex );
			if( PositionInGrid( _strYey, worldPos ) )
				return m_dictGrids[_strYey];

			_strYey = GetGridKey( _iXIndex, _iYIndex + 1 );
			if( PositionInGrid( _strYey, worldPos ) )
				return m_dictGrids[_strYey];

			_iXIndex++;
			_iYIndex = CalculatePositionIndexY( worldPos.z, _iXIndex );

			_strYey = GetGridKey( _iXIndex, _iYIndex );
			if( PositionInGrid( _strYey, worldPos ) )
				return m_dictGrids[_strYey];

			_strYey = GetGridKey( _iXIndex, _iYIndex + 1 );
			if( PositionInGrid( _strYey, worldPos ) )
				return m_dictGrids[_strYey];

			return null;
		}

		public int CalculateGridsH(Grid start, Grid target)
		{
			Vector3 _cOffset = target.m_cCenter - start.m_cCenter;
			return Mathf.Abs( Mathf.RoundToInt( _cOffset.x / m_fWidthRadius ) ) + Mathf.Abs( Mathf.RoundToInt( _cOffset.z / m_fHeightRadius ) );
		}

		public string SaveGrids()
		{
			GridManagerData _cData = new GridManagerData()
			{
				m_iRow = m_iRow,
				m_iCol = m_iCol,
				m_fRadius = m_fWidthRadius,
				m_strArrGridDatas = ( from grid in m_dictGrids.Values
									  select grid.SerializeObjectToJson() ).ToArray(),
			};

			return JsonConvert.SerializeObject( _cData );
		}

		public void DrawGridsSide(LineRenderer prefab, Transform root)
		{
			foreach( Grid _cGrid in m_dictGrids.Values )
			{
				LineRenderer _cLine = UnityEngine.Object.Instantiate( prefab, root );
				_cLine.name = GetGridKey( _cGrid.m_cPoint.m_iX, _cGrid.m_cPoint.m_iY );
				_cLine.positionCount = m_cArrGridVertexOffset.Length + 1;
				for( int i = 0, j = m_cArrGridVertexOffset.Length; i < j; i++ )
					_cLine.SetPosition( i, _cGrid.m_cCenter + m_cArrGridVertexOffset[i] );

				_cLine.SetPosition( m_cArrGridVertexOffset.Length, _cGrid.m_cCenter + m_cArrGridVertexOffset[0] );
			}
		}

		//public static Vector2 CalculateGridSize(int x, int y, float radius)
		//{
		//	bool isDoubleRow = y % 2 == 0;
		//	float width = ( isDoubleRow ? 3.5f : 2f ) * radius + radius * 3 * ( x - 1 );
		//	float height = ( radius * Mathf.Sqrt( 3f ) / 2f ) * ( y + 1 );
		//	return new Vector2( width, height );
		//}

		public static Vector2 CalculateGridSize(int x, int y, float radius)
		{
			bool _bIsDoubleCol = x >= 2;
			float _fWidth = radius * 2 + 1.5f * radius * ( x - 1 );
			float _fHeight = ( radius * Mathf.Sqrt( 3f ) / 2f ) * ( 2 * y + ( _bIsDoubleCol ? 1 : 0 ) );
			return new Vector2( _fWidth, _fHeight );
		}

		#endregion

		#region Private Functions

		void CreateGrid(GridData data, GameObject prefab, Transform root)
		{
			Point _cPoint = new Point( data.m_iXIndex, data.m_iYIndex );
			Vector3 _cCenter = CalculateGridPosition( _cPoint );
			Grid _cGrid = new Grid( _cPoint, _cCenter );
			m_dictGrids[GetGridKey( _cPoint.m_iX, _cPoint.m_iY )] = _cGrid;

			if( prefab )
			{
				GameObject _goGo = UnityEngine.Object.Instantiate( prefab, root );
				_goGo.name = prefab.name;
				_goGo.transform.position = _cGrid.m_cCenter;

				_cGrid.SetGridType( data.m_eGridType );
				_cGrid.SetGridPrefab( _goGo );
				_cGrid.ChangeSides( data.m_eArrGridSidesType );
			}
		}

		Vector3 CalculateGridPosition(Point p)
		{
			//bool isDoubleRow = p.Y % 2 == 0;
			//float posX = ( isDoubleRow ? 2.5f : 1 ) * WidthRadius + WidthRadius * 3 * ( p.X - 1 );
			//float posY = HeightRadius * p.Y;
			//return Origin + new Vector3( posX, posY, 0 );

			bool _bIsDoubleCol = p.m_iX % 2 == 0;
			float _fPosX = m_fWidthRadius + 1.5f * m_fWidthRadius * ( p.m_iX - 1 );
			float _fPosY = ( _bIsDoubleCol ? 2 : 1 ) * m_fHeightRadius + m_fHeightRadius * 2 * ( p.m_iY - 1 );
			return new Vector3( _fPosX, 0, _fPosY );
		}

		//int CalculatePositionIndexX(float offsetX, int y)
		//{
		//	bool isDoubleRow = y % 2 == 0;
		//	return Mathf.FloorToInt( ( offsetX - ( isDoubleRow ? 2.5f : 1 ) * WidthRadius ) / WidthRadius / 3 + 1 );
		//}

		int CalculatePositionIndexY(float offsetY, int x)
		{
			bool _bIsDoubleCol = x % 2 == 0;
			return Mathf.FloorToInt( ( offsetY - ( _bIsDoubleCol ? 1 : 0 ) * m_fHeightRadius ) / m_fHeightRadius / 2 + 1 );
		}

		bool PositionInGrid(string key, Vector3 worldPos)
		{
			Grid _cGrid = null;
			if( m_dictGrids.TryGetValue( key, out _cGrid ) )
			{
				Vector3 _cOffset = worldPos - _cGrid.m_cCenter;
				return Mathf.Abs( _cOffset.x ) <= m_fWidthRadius - Mathf.Abs( _cOffset.z ) / m_fRatio;
				//return Mathf.Abs( offset.y ) <= Radius - Mathf.Abs( offset.x ) / ratio;
			}

			return false;
		}

		string GetGridKey(int x, int y)
		{
			return string.Format( "({0},{1})", x, y );
		}

		#endregion
	}
}
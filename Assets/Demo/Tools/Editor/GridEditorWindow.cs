using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Demo
{
	class GridEditorWindow : EditorWindow
	{
		const string PREFABDIRECTORYPATH = "Assets/Demo/Prefabs/{0}";
		const string PREFABPATH = "Assets/Demo/Prefabs/{0}/{1}";
		const string GRIDBGPATH = "Assets/Demo/Prefabs/GridBg.prefab";
		const string CONFIGPATH = "Assets/Demo/Config/{0}.txt";
		const string ROOTGAMEOBJECTNAME = "ROOT";

		#region Window Show Properties

		float radius = 1.5f;
		int m_iRow;
		int m_iCol;
		string[] m_strArrGridTypes;
		string[] m_strArrGridSideTypes;
		int m_iGridTypeIndex;
		int m_iGridPrefabIndex;
		bool m_bShowGridSide;
		GameObject m_goBrush;
		bool m_bCreateGrid;
		bool m_bClearGrid;
		bool m_bFindPath;
		int m_iUpRightSide = 1;
		int m_iDownRightSide = 1;
		int m_iDownSide = 1;
		int m_iDownLeftSide = 1;
		int m_iUpLeftSide = 1;
		int m_iUpSide = 1;
		string m_strFileConfigName;
		TextAsset m_cCurrentFileConfig;

		#endregion

		GameObject m_goGridPrefab;
		Transform m_cGridSceneRoot;
		Transform m_gridRoot;
		GridManager m_cGridManager;
		AStar m_cAStar;
		Grid m_cStartGrid;
		Grid m_cTargetGrid;

		LineRenderer m_cPathLine;
		Transform m_cGridSideLineRoot;
		GameObject m_goGridBg;
		bool m_bSceneCreated;
		string m_strCurrentFileConfigName;
		Dictionary<string, string[]> m_dictGridNames;
		Dictionary<string, GameObject[]> m_dictGridPrefabs;
		List<Grid> m_listPath;

		#region Window Life

		[MenuItem( "MyTools/Grid Editor" )]
		static void Create()
		{
			GridEditorWindow _cWindow = (GridEditorWindow)GetWindow( typeof( GridEditorWindow ) );
			_cWindow.Initialize();

			_cWindow.titleContent.text = "Grid Editor";
			_cWindow.Show();
		}

		void Initialize()
		{
			m_dictGridPrefabs = new Dictionary<string, GameObject[]>();
			m_dictGridNames = new Dictionary<string, string[]>();
			m_listPath = new List<Grid>();
			m_cGridManager = new GridManager();
			m_cGridManager.SetGridRadius( radius );
			m_cAStar = new AStar( m_cGridManager );

			Array _arrTypes = Enum.GetValues( typeof( EGridType ) );
			foreach( EGridType _eType in _arrTypes )
			{
				if( _eType != EGridType.Invalid )
				{
					GameObject[] _goArrPrefabs = LoadGridPrefabs( _eType );
					if( _goArrPrefabs != null )
						m_dictGridPrefabs[_eType.ToString()] = _goArrPrefabs;
				}
			}
			m_strArrGridTypes = m_dictGridPrefabs.Keys.ToArray();

			List<string> _strArrSideTypes = new List<string>();
			foreach( EGridSideType _eType in Enum.GetValues( typeof( EGridSideType ) ) )
				_strArrSideTypes.Add( _eType.ToString() );

			m_strArrGridSideTypes = _strArrSideTypes.ToArray();
			SceneView.onSceneGUIDelegate += OnSceneGUI;
		}

		void OnEnable()
		{
			if( m_dictGridPrefabs == null )
			{
				Initialize();
				ClearGridScene();
			}
		}

		void OnDestroy()
		{
			m_dictGridNames.Clear();
			m_dictGridPrefabs.Clear();
			m_listPath.Clear();

			ClearGridScene();
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
		}

		void OnGUI()
		{
			radius = EditorGUILayout.FloatField( "Radius", radius );
			EditorGUILayout.BeginHorizontal();
			{
				m_iRow = EditorGUILayout.IntField( "Grid Row", m_iRow );
				m_iCol = EditorGUILayout.IntField( "Grid Col", m_iCol );

				if( m_iRow > 0 && m_iCol > 0 )
				{
					if( GUILayout.Button( "Create Grid Scene" ) )
						CreateGridScene();
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			m_strFileConfigName = EditorGUILayout.TextField( "Scene Config Name", m_strFileConfigName );
			if( !string.IsNullOrEmpty( m_strFileConfigName ) && m_strFileConfigName != m_strCurrentFileConfigName )
			{
				string _strPath = string.Format( CONFIGPATH, m_strFileConfigName );
				if( File.Exists( _strPath ) )
				{
					m_strCurrentFileConfigName = m_strFileConfigName;
					m_cCurrentFileConfig = AssetDatabase.LoadAssetAtPath<TextAsset>( _strPath );
				}
			}
			m_cCurrentFileConfig = (TextAsset)EditorGUILayout.ObjectField( "Scene Config Asset", m_cCurrentFileConfig, typeof( TextAsset ), false );
			if( m_cCurrentFileConfig != null )
			{
				if( GUILayout.Button( "Load Grid Scene" ) )
					LoadScene();
			}
			EditorGUILayout.Space();

			if( m_bSceneCreated )
			{
				m_iGridTypeIndex = EditorGUILayout.Popup( "Type", m_iGridTypeIndex, m_strArrGridTypes );

				m_iGridPrefabIndex = Mathf.Clamp( m_iGridPrefabIndex, m_iGridPrefabIndex, m_dictGridNames[m_strArrGridTypes[m_iGridTypeIndex]].Length - 1 );
				m_iGridPrefabIndex = EditorGUILayout.Popup( "Prefab", m_iGridPrefabIndex, m_dictGridNames[m_strArrGridTypes[m_iGridTypeIndex]] );

				m_goGridPrefab = m_dictGridPrefabs[m_strArrGridTypes[m_iGridTypeIndex]][m_iGridPrefabIndex];
				m_goGridPrefab = (GameObject)EditorGUILayout.ObjectField( "Grid Prefab", m_goGridPrefab, typeof( GameObject ), false );
				CreateBrush( m_goGridPrefab );
				EditorGUILayout.Space();

				m_bShowGridSide = EditorGUILayout.Toggle( "Show Grid Side", m_bShowGridSide );
				ShowGridSide( m_bShowGridSide );

				m_goBrush = (GameObject)EditorGUILayout.ObjectField( "Grid Brush", m_goBrush, typeof( GameObject ), false );
				EditorGUILayout.Space();

				m_bCreateGrid = EditorGUILayout.Toggle( "Create Grid", m_bCreateGrid );
				if( m_bCreateGrid )
				{
					m_bClearGrid = false;
					m_bFindPath = false;
				}

				m_bClearGrid = EditorGUILayout.Toggle( "Clear Grid", m_bClearGrid );
				if( m_bClearGrid )
				{
					m_bCreateGrid = false;
					m_bFindPath = false;
				}

				m_bFindPath = EditorGUILayout.Toggle( "Find Path", m_bFindPath );
				if( m_bFindPath )
				{
					m_bCreateGrid = false;
					m_bClearGrid = false;
				}

				EditorGUILayout.Space();

				m_iUpRightSide = EditorGUILayout.Popup( "Up Right Side Type", m_iUpRightSide, m_strArrGridSideTypes );
				m_iDownRightSide = EditorGUILayout.Popup( "Down Right Side Type", m_iDownRightSide, m_strArrGridSideTypes );
				m_iDownSide = EditorGUILayout.Popup( "Down Side Type", m_iDownSide, m_strArrGridSideTypes );
				m_iDownLeftSide = EditorGUILayout.Popup( "Down Left Side Type", m_iDownLeftSide, m_strArrGridSideTypes );
				m_iUpLeftSide = EditorGUILayout.Popup( "Up Left Side Type", m_iUpLeftSide, m_strArrGridSideTypes );
				m_iUpSide = EditorGUILayout.Popup( "Up Side Type", m_iUpSide, m_strArrGridSideTypes );
				EditorGUILayout.Space();

				if( !string.IsNullOrEmpty( m_strFileConfigName ) )
				{
					if( GUILayout.Button( "Save Grid Scene" ) )
						SaveScene();

					EditorGUILayout.Space();
				}

				if( GUILayout.Button( "Clear Grid Scene" ) )
					ClearGridScene();
			}
		}

		void OnSceneGUI(SceneView sceneView)
		{
			if( m_bSceneCreated && ( Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag ) )
			{
				Vector3 _cMousePosition = Event.current.mousePosition;
				_cMousePosition.y = sceneView.camera.pixelHeight - _cMousePosition.y;
				_cMousePosition = sceneView.camera.ScreenToWorldPoint( new Vector3( _cMousePosition.x, _cMousePosition.y, _cMousePosition.z ) );
				Grid _cGrid = m_cGridManager.CalculatePositionGrid( _cMousePosition );
				if( _cGrid != null )
				{
					if( m_bCreateGrid )
					{
						EGridType _eType = (EGridType)Enum.Parse( typeof( EGridType ), m_strArrGridTypes[m_iGridTypeIndex] );
						if( _cGrid.m_eType == _eType && _cGrid.m_goPrefab.name == m_goGridPrefab.name )
						{
							ChangeGridSide( _cGrid );
							return;
						}

						_cGrid.RemoveGridPrefab();
						GameObject _goGo = Instantiate( m_goGridPrefab, m_gridRoot.transform );
						_goGo.name = m_goGridPrefab.name;
						_goGo.transform.position = _cGrid.m_cCenter;

						_cGrid.SetGridType( _eType );
						_cGrid.SetGridPrefab( _goGo );
						ChangeGridSide( _cGrid );
					}
					else if( m_bClearGrid )
						_cGrid.RemoveGridPrefab();
					else if( m_bFindPath )
					{
						if( m_cStartGrid == null )
							m_cStartGrid = _cGrid;
						else if( m_cTargetGrid == null && _cGrid != m_cStartGrid )
						{
							m_cTargetGrid = _cGrid;
							CalculateShortestPath();
						}
					}
				}
			}
		}

		#endregion

		#region Utils

		GameObject[] LoadGridPrefabs(EGridType type)
		{
			string _strDirectoryPath = string.Format( PREFABDIRECTORYPATH, type );
			if( !Directory.Exists( _strDirectoryPath ) )
			{
				Debug.LogErrorFormat( "GridEditorWindow::LoadGridPrefabs is failed! Grid type {0} 's prefab floder is not exist!", type );
				return null;
			}

			///??????
			IEnumerable<string> _arrFileNames = from _strFilePath in Directory.GetFiles( _strDirectoryPath )
												let _strFileName = Path.GetFileName( _strFilePath )
												where !_strFileName.Contains( ".meta" )
												select _strFileName;

			m_dictGridNames[type.ToString()] = ( from _strFileName in _arrFileNames
												 select _strFileName ).ToArray();

			return ( from _strFileName in m_dictGridNames[type.ToString()]
					 select AssetDatabase.LoadAssetAtPath<GameObject>( string.Format( PREFABPATH, type, _strFileName ) ) ).ToArray();
		}

		void CreateSceneRoot()
		{
			m_cGridSceneRoot = new GameObject( "GridEditorRoot" ).transform;
			m_cGridSceneRoot.SetParent( GameObject.Find( ROOTGAMEOBJECTNAME ).transform );
			m_cGridSceneRoot.localPosition = Vector3.zero;
			m_gridRoot = new GameObject( "GridRoot" ).transform;
			m_gridRoot.SetParent( m_cGridSceneRoot );
			m_gridRoot.localPosition = Vector3.zero;
		}

		void CreatePathLine()
		{
			GameObject m_goGo = new GameObject( "PathLine" );
			m_goGo.transform.SetParent( m_cGridSceneRoot );
			m_cPathLine = m_goGo.AddComponent<LineRenderer>();
			m_cPathLine.startWidth = 0.05f;
			m_cPathLine.endWidth = 0.05f;
			m_cPathLine.startColor = Color.cyan;
			m_cPathLine.endColor = Color.cyan;
			m_cPathLine.sortingOrder = 1;
			m_cPathLine.positionCount = 0;
		}

		void CreateGirdBg()
		{
			Vector2 _cSize = GridManager.CalculateGridSize( m_iRow, m_iCol, radius );
			m_goGridBg = Instantiate( AssetDatabase.LoadAssetAtPath<GameObject>( GRIDBGPATH ), m_cGridSceneRoot );
			m_goGridBg.transform.localScale = new Vector3( _cSize.x, 0, _cSize.y );
			m_goGridBg.transform.localPosition = new Vector3( _cSize.x / 2, 0, _cSize.y / 2 );
		}

		void CreateBrush(GameObject prefab)
		{
			if( prefab == null || ( m_goBrush != null && prefab.name == m_goBrush.name ) )
				return;

			if( m_goBrush )
				DestroyImmediate( m_goBrush );

			if( m_bSceneCreated )
			{
				m_goBrush = Instantiate( prefab, m_cGridSceneRoot );
				m_goBrush.transform.position = new Vector3( -radius - 1, 0, m_goGridBg.transform.position.z );
				m_goBrush.name = prefab.name;
			}
		}

		#endregion

		#region Scene

		void ClearGridScene()
		{
			if( m_cGridSceneRoot )
				DestroyImmediate( m_cGridSceneRoot.gameObject );

			m_bSceneCreated = false;
			m_bCreateGrid = false;
			m_bClearGrid = false;
			m_bFindPath = false;
		}

		void CreateGridScene()
		{
			ClearGridScene();
			CreateSceneRoot();
			CreatePathLine();
			CreateGirdBg();
			CreateBrush( m_goGridPrefab );

			m_cGridManager.CreateGrids( m_iRow, m_iCol );
			m_bSceneCreated = true;
		}

		void LoadScene()
		{
			GridManagerData m_cConfigData = JsonConvert.DeserializeObject<GridManagerData>( m_cCurrentFileConfig.text );
			radius = m_cConfigData.m_fRadius;
			m_iRow = m_cConfigData.m_iRow;
			m_iCol = m_cConfigData.m_iCol;
			CreateGridScene();
			m_cGridManager.CreateGrids( m_cConfigData, m_dictGridPrefabs, m_gridRoot.transform );
			Debug.Log( "GridEditorWindow::LoadScene success!" );
		}

		void SaveScene()
		{
			string _strGridsJson = m_cGridManager.SaveGrids();
			string _strSavePath = string.Format( CONFIGPATH, m_strFileConfigName );
			///???
			byte[] _btBytes = System.Text.Encoding.UTF8.GetBytes( _strGridsJson );
			if( !File.Exists( _strSavePath ) )
			{
				FileStream _cFileStream = new FileStream( _strSavePath, FileMode.Create );
				_cFileStream.Write( _btBytes, 0, _btBytes.Length );
				if( _cFileStream != null )
				{
					_cFileStream.Flush();
					_cFileStream.Close();
					_cFileStream.Dispose();
					Debug.Log( "GridEditorWindow::SaveScene success!" );
				}
			}
			else
			{
				File.WriteAllBytes( _strSavePath, _btBytes );
				Debug.Log( "GridEditorWindow::SaveScene success!" );
			}
			AssetDatabase.Refresh();
		}

		#endregion

		#region Grid

		void ChangeGridSide(Grid grid)
		{
			grid.ChangeSide( 0, (EGridSideType)Enum.Parse( typeof( EGridSideType ), m_strArrGridSideTypes[m_iUpRightSide] ) );
			grid.ChangeSide( 1, (EGridSideType)Enum.Parse( typeof( EGridSideType ), m_strArrGridSideTypes[m_iDownRightSide] ) );
			grid.ChangeSide( 2, (EGridSideType)Enum.Parse( typeof( EGridSideType ), m_strArrGridSideTypes[m_iDownSide] ) );
			grid.ChangeSide( 3, (EGridSideType)Enum.Parse( typeof( EGridSideType ), m_strArrGridSideTypes[m_iDownLeftSide] ) );
			grid.ChangeSide( 4, (EGridSideType)Enum.Parse( typeof( EGridSideType ), m_strArrGridSideTypes[m_iUpLeftSide] ) );
			grid.ChangeSide( 5, (EGridSideType)Enum.Parse( typeof( EGridSideType ), m_strArrGridSideTypes[m_iUpSide] ) );
		}

		void ShowGridSide(bool show)
		{
			if( show )
			{
				if( !m_cGridSideLineRoot )
				{
					m_cGridSideLineRoot = new GameObject( "GridLineRoot" ).transform;
					m_cGridSideLineRoot.SetParent( m_cGridSceneRoot );
				}

				if( m_cGridSideLineRoot.childCount > 0 )
					return;

				m_cGridManager.DrawGridsSide( m_cPathLine, m_cGridSideLineRoot );
			}
			else if( m_cGridSideLineRoot )
				DestroyImmediate( m_cGridSideLineRoot.gameObject );
		}

		#endregion

		#region AStar

		void CalculateShortestPath()
		{
			m_listPath.Clear();
			if( m_cAStar.FindPath( m_cStartGrid, m_cTargetGrid, m_listPath ) )
			{
				m_cPathLine.positionCount = m_listPath.Count + 1;
				m_cPathLine.SetPosition( 0, m_cStartGrid.m_cCenter );
				for( int i = 0, j = m_listPath.Count; i < j; i++ )
					m_cPathLine.SetPosition( i + 1, m_listPath[i].m_cCenter );
			}
			else
			{
				m_cPathLine.positionCount = 0;
				Debug.LogErrorFormat( "GridEditorWindow::CalculateShortestPath is failed! Path is invalid between grid:{0},{1} and grid:{2},{3} !", m_cStartGrid.m_cPoint.m_iX, m_cStartGrid.m_cPoint.m_iY, m_cTargetGrid.m_cPoint.m_iX, m_cTargetGrid.m_cPoint.m_iY );
			}

			m_cStartGrid = null;
			m_cTargetGrid = null;
		}

		#endregion
	}
}
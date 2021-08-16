using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Demo
{
	class AStarNode
	{
		public int m_iG;//与初始值的优先值
		public int m_iH;//与目的地的优先值
		public int m_iF { get { return m_iG + m_iH; } }

		public Grid m_cGrid;
		public AStarNode m_cLastNode;

		public AStarNode(int g, int h, Grid grid)
		{
			m_iG = g;
			m_iH = h;
			m_cGrid = grid;
		}

		~AStarNode()
		{
			m_cLastNode = null;
		}
	}

	public class AStar
	{
		GridManager m_cGridManager;
		List<AStarNode> m_listOpenNodes;
		List<AStarNode> m_listCloseNodes;


		public AStar(GridManager manager)
		{
			m_cGridManager = manager;
			m_listOpenNodes = new List<AStarNode>();
			m_listCloseNodes = new List<AStarNode>();
		}

		public bool FindPath(Grid start, Grid target, List<Grid> path)
		{
			m_listOpenNodes.Clear();
			m_listCloseNodes.Clear();
			AStarNode _cCurrentNode = new AStarNode( 0, m_cGridManager.CalculateGridsH( start, target ), start );
			AddOpenNode( _cCurrentNode );

			while( m_listOpenNodes.Count > 0 )
			{
				_cCurrentNode = m_listOpenNodes[0];
				m_listOpenNodes.Remove( _cCurrentNode );
				m_listCloseNodes.Add( _cCurrentNode );

				for( int i = 0, j = _cCurrentNode.m_cGrid.m_cArrArroundPoints.Length; i < j; i++ )
				{
					Grid _cGrid = m_cGridManager.GetGrid( _cCurrentNode.m_cGrid.m_cArrArroundPoints[i] );
					if( _cGrid != null && !m_listCloseNodes.Any( node => node.m_cGrid == _cGrid ) && _cCurrentNode.m_cGrid.CanThrough( i ) )
					{
						int _iG = _cCurrentNode.m_iG + 1;
						int _iH = m_cGridManager.CalculateGridsH( _cCurrentNode.m_cGrid, target );

						AStarNode _cNode = m_listOpenNodes.FirstOrDefault( n => n.m_cGrid == _cGrid );
						if( _cNode == null )
						{
							_cNode = new AStarNode( _iG, _iH, _cGrid );
							_cNode.m_cLastNode = _cCurrentNode;
							AddOpenNode( _cNode );

							if( _cNode.m_cGrid == target )
							{
								CalculatePath( _cNode, path );
								return true;
							}
						}
						else if( _cNode.m_iF > _iG + _iH )
						{
							_cNode.m_iG = _iG;
							_cNode.m_iH = _iH;
							_cNode.m_cLastNode = _cCurrentNode;
							m_listOpenNodes.Remove( _cNode );
							AddOpenNode( _cNode );
						}
					}
				}
			}

			return false;
		}

		void CalculatePath(AStarNode node, List<Grid> path)
		{
			path.Clear();
			while( node != null )
			{
				path.Insert( 0, node.m_cGrid );
				node = node.m_cLastNode;
			}
		}

		void AddOpenNode(AStarNode node)
		{
			int _iStart = 0;
			int _iEnd = m_listOpenNodes.Count - 1;

			while( _iStart <= _iEnd )
			{
				int _iMiddle = Mathf.FloorToInt( ( _iStart + _iEnd ) / 2f );
				if( m_listOpenNodes[_iMiddle].m_iF > node.m_iF )
					_iEnd = _iMiddle - 1;
				else //F相同，新Node排在后面
					_iStart = _iMiddle + 1;
			}

			m_listOpenNodes.Insert( _iStart, node );
		}
	}
}
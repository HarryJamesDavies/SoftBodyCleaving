using System.Collections.Generic;

namespace CSG
{
    public class CSGNode
	{
		public List<CSGPolygon> m_polygons;

		public CSGNode m_frontNode;	
		public CSGNode m_backNode;	

		public CSGPlane m_plane;

		public CSGNode()
		{
			m_frontNode = null;
			m_backNode = null;
		}

		public CSGNode(List<CSGPolygon> _polygons)
		{
			Build(_polygons);
		}

		public CSGNode(List<CSGPolygon> _polygons, CSGPlane _plane, CSGNode _frontNode, CSGNode _backNode)
		{
			m_polygons = _polygons;
			m_plane = _plane;
			m_frontNode = _frontNode;
			m_backNode = _backNode;
		}

		public CSGNode Clone()
		{
			CSGNode clone = new CSGNode(m_polygons, m_plane, m_frontNode, m_backNode);

			return clone;
		}

		// Remove all polygons in this BSP tree that are inside the other BSP tree
		// `bsp`.
		public void ClipTo(CSGNode _otherNode)
		{
			m_polygons = _otherNode.ClipPolygons(m_polygons);

			if (m_frontNode != null)
			{
				m_frontNode.ClipTo(_otherNode);
			}

			if (m_backNode != null)
			{
				m_backNode.ClipTo(_otherNode);
			}
		}

		// Convert solid space to empty space and empty space to solid space.
		public void Invert()
		{	
			for (int i = 0; i < m_polygons.Count; i++)
				m_polygons[i].Flip();

			m_plane.Flip();

			if (m_frontNode != null)
			{
				m_frontNode.Invert();
			}

			if (m_backNode != null)
			{
				m_backNode.Invert();
			}

			CSGNode tempNode = m_frontNode;
			m_frontNode = m_backNode;
			m_backNode = tempNode;
		}

        // Build a BSP tree out of `polygons`. When called on an existing tree, the
        // new polygons are filtered down to the bottom of the tree and become new
        // nodes there. Each set of polygons is partitioned using the first polygon
        // (no heuristic is used to pick a good split).
        public void Build(List<CSGPolygon> _polygons)
        {
            if (_polygons.Count < 1)
                return;

            if (m_plane == null || !m_plane.Valid())
            {
                m_plane = new CSGPlane();
                m_plane.m_normal = _polygons[0].m_plane.m_normal;
                m_plane.m_planeWidth = _polygons[0].m_plane.m_planeWidth;
            }


            if (m_polygons == null)
                m_polygons = new List<CSGPolygon>();

            List<CSGPolygon> frontPolygons = new List<CSGPolygon>();
            List<CSGPolygon> backPolygons = new List<CSGPolygon>();

            for (int i = 0; i < _polygons.Count; i++)
            {
                m_plane.SplitPolygon(_polygons[i], m_polygons, m_polygons, frontPolygons, backPolygons);
            }

            if (frontPolygons.Count > 0)
            {
                if (m_frontNode == null)
                    m_frontNode = new CSGNode();

                m_frontNode.Build(frontPolygons);
            }

            if (backPolygons.Count > 0)
            {
                if (m_backNode == null)
                    m_backNode = new CSGNode();

                m_backNode.Build(backPolygons);
            }
        }

		// Recursively remove all polygons in `polygons` that are inside this BSP
		// tree.
		public List<CSGPolygon> ClipPolygons(List<CSGPolygon> _polygons)
		{
			if (!m_plane.Valid())
			{
				return _polygons;
			}

			List<CSGPolygon> frontPolygons = new List<CSGPolygon>();
			List<CSGPolygon> backPolygons = new List<CSGPolygon>();	

			for (int polygonIter = 0; polygonIter < _polygons.Count; polygonIter++)
			{
				m_plane.SplitPolygon(_polygons[polygonIter], frontPolygons, backPolygons, frontPolygons, backPolygons);
			}

			if (m_frontNode != null)
			{
				frontPolygons = m_frontNode.ClipPolygons(frontPolygons);
			}

			if (m_backNode != null)
			{
				backPolygons = m_backNode.ClipPolygons(backPolygons);
			}
			else
			{
				backPolygons.Clear();
			}
			
			frontPolygons.AddRange(backPolygons);

			return frontPolygons;
		}
        
		public List<CSGPolygon> AllPolygons()
		{
			List<CSGPolygon> polygons = this.m_polygons;
            List<CSGPolygon> frontPolygons = new List<CSGPolygon>();
            List<CSGPolygon> backPolygons = new List<CSGPolygon>();

			if (m_frontNode != null)
			{
				frontPolygons = m_frontNode.AllPolygons();
			}

			if (m_backNode != null)
			{
				backPolygons = m_backNode.AllPolygons();
			}

			polygons.AddRange(frontPolygons);
			polygons.AddRange(backPolygons);

			return polygons;
		}

#region STATIC OPERATIONS

		// Return a new CSG solid representing space in either this solid or in the
		// solid `csg`. Neither this solid nor the solid `csg` are modified.
		public static CSGNode Union(CSGNode nodeAIn, CSGNode nodeBIn)
		{
			CSGNode nodeA = nodeAIn.Clone();
			CSGNode nodeB = nodeBIn.Clone();

			nodeA.ClipTo(nodeB);
			nodeB.ClipTo(nodeA);
			nodeB.Invert();
			nodeB.ClipTo(nodeA);
			nodeB.Invert();

			nodeA.Build(nodeB.AllPolygons());

			CSGNode result = new CSGNode(nodeA.AllPolygons());

			return result;
		}

		// Return a new CSG solid representing space in this solid but not in the
		// solid `csg`. Neither this solid nor the solid `csg` are modified.
		public static CSGNode Subtract(CSGNode nodeAIn, CSGNode nodeBIn)
		{
			CSGNode nodeA = nodeAIn.Clone();
			CSGNode nodeB = nodeBIn.Clone();

			nodeA.Invert();
			nodeA.ClipTo(nodeB);
			nodeB.ClipTo(nodeA);
			nodeB.Invert();
			nodeB.ClipTo(nodeA);
			nodeB.Invert();
			nodeA.Build(nodeB.AllPolygons());
			nodeA.Invert();

			CSGNode result = new CSGNode(nodeA.AllPolygons());

			return result;
		}

		// Return a new CSG solid representing space both this solid and in the
		// solid `csg`. Neither this solid nor the solid `csg` are modified.
		public static CSGNode Intersect(CSGNode nodeAIn, CSGNode nodeBIn)
		{
			CSGNode nodeA = nodeAIn.Clone();
			CSGNode nodeB = nodeBIn.Clone();

			nodeA.Invert();
			nodeB.ClipTo(nodeA);
			nodeB.Invert();
			nodeA.ClipTo(nodeB);
			nodeB.ClipTo(nodeA);
			nodeA.Build(nodeB.AllPolygons());
			nodeA.Invert();

			CSGNode result = new CSGNode(nodeA.AllPolygons());

			return result;
		}
#endregion
	}
}
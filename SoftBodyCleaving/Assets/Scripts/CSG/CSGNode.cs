// Original CSG.JS library by Evan Wallace (http://madebyevan.com), under the MIT license.
// GitHub: https://github.com/evanw/csg.js/

using System.Collections.Generic;
using UnityEngine;

namespace CSG
{
    public class CSGNode
	{
		public List<CSGPolygon> m_polygons;

		public CSGNode m_frontNode;	
		public CSGNode m_backNode;	

		public CSGPlane m_plane;

        private const int C_MaxRecursions = 100000;


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

        public CSGNode DeepClone()
        {
            List<CSGPolygon> polygons = new List<CSGPolygon>();
            for (int polygonIter = 0; polygonIter < m_polygons.Count; polygonIter++)
            {
                polygons.Add(m_polygons[polygonIter].DeepClone());
            }
            CSGNode clone = new CSGNode(m_polygons, m_plane.DeepClone(), m_frontNode, m_backNode);

            return clone;
        }

        // Remove all polygons in this BSP tree that are inside the other BSP tree
        // `bsp`.
        public void ClipTo(CSGNode _otherNode, int _recursionCount = 0)
        {
            if (_recursionCount > C_MaxRecursions)
            {
                Debug.LogWarning("Hit RecursionCap");
                return;
            }

            m_polygons = _otherNode.ClipPolygons(m_polygons, 0);

            if (m_frontNode != null)
            {
                m_frontNode.ClipTo(_otherNode, _recursionCount + 1);
            }

            if (m_backNode != null)
            {
                m_backNode.ClipTo(_otherNode, _recursionCount + 1);
            }
        }

        // Convert solid space to empty space and empty space to solid space.
        public void Invert()
		{
            for (int i = 0; i < m_polygons.Count; i++)
            {
                m_polygons[i].Flip();
            }

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
        public void Build(List<CSGPolygon> _polygons, int _recursionCount = 0)
        {
            if (_recursionCount > C_MaxRecursions)
            {
                Debug.LogWarning("Hit RecursionCap");
                return;
            }

            if (_polygons.Count < 1)
            {
                return;
            }

            if (m_plane == null || !m_plane.Valid())
            {
                m_plane = new CSGPlane();
                m_plane.m_normal = _polygons[0].m_plane.m_normal;
                m_plane.m_planeWidth = _polygons[0].m_plane.m_planeWidth;
            }


            if (m_polygons == null)
            {
                m_polygons = new List<CSGPolygon>();
            }

            List<CSGPolygon> frontPolygons = new List<CSGPolygon>();
            List<CSGPolygon> backPolygons = new List<CSGPolygon>();

            for (int i = 0; i < _polygons.Count; i++)
            {
                m_plane.SplitPolygon(_polygons[i], m_polygons, m_polygons, frontPolygons, backPolygons);
            }

            if (frontPolygons.Count > 0)
            {
                if (m_frontNode == null)
                {
                    m_frontNode = new CSGNode();
                }

                m_frontNode.Build(frontPolygons, _recursionCount + 1);
            }

            if (backPolygons.Count > 0)
            {
                if (m_backNode == null)
                {
                    m_backNode = new CSGNode();
                }

                m_backNode.Build(backPolygons, _recursionCount + 1);
            }
        }

        // Recursively remove all polygons in `polygons` that are inside this BSP
        // tree.
        public List<CSGPolygon> ClipPolygons(List<CSGPolygon> _polygons, int _recursionCount = 0)
        {
            if (_recursionCount > C_MaxRecursions)
            {
                Debug.LogWarning("Hit RecursionCap");
                return _polygons;
            }

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
                frontPolygons = m_frontNode.ClipPolygons(frontPolygons, _recursionCount + 1);
            }

            if (m_backNode != null)
            {
                backPolygons = m_backNode.ClipPolygons(backPolygons, _recursionCount + 1);
            }
            else
            {
                backPolygons.Clear();
            }

            frontPolygons.AddRange(backPolygons);

            return frontPolygons;
        }

        public List<CSGPolygon> AllPolygons(int _recursionCount = 0)
        {
            List<CSGPolygon> polygons = m_polygons;
            List<CSGPolygon> frontPolygons = new List<CSGPolygon>();
            List<CSGPolygon> backPolygons = new List<CSGPolygon>();

            if (_recursionCount > C_MaxRecursions)
            {
                Debug.LogWarning("Hit RecursionCap");
                return m_polygons;
            }

            if (m_frontNode != null)
			{
				frontPolygons = m_frontNode.AllPolygons(_recursionCount + 1);
			}

			if (m_backNode != null)
			{
				backPolygons = m_backNode.AllPolygons(_recursionCount + 1);
			}

			polygons.AddRange(frontPolygons);
			polygons.AddRange(backPolygons);

			return polygons;
		}

        #region STATIC OPERATIONS

        // Return a new CSG solid representing space in either this solid or in the
        // solid `csg`. Neither this solid nor the solid `csg` are modified.
        public static List<Mesh> Union(CSGNode _nodeAIn, CSGNode _nodeBIn, CSGMeshingSettings _meshSettings)
        {
            List<Mesh> resultantMeshes = new List<Mesh>();

            CSGNode nodeA = _nodeAIn.Clone();
            CSGNode nodeB = _nodeBIn.Clone();

            nodeA.ClipTo(nodeB);
            nodeB.ClipTo(nodeA);
            nodeB.Invert();
            nodeB.ClipTo(nodeA);
            nodeB.Invert();

            nodeA.Build(nodeB.AllPolygons());

            CSGModel resultModel = new CSGModel(nodeA.AllPolygons());
            if (resultModel.SubDivideMesh(_meshSettings))
            {
                for (int subMeshIter = 0; subMeshIter < resultModel.m_subModels.Count; subMeshIter++)
                {
                    resultantMeshes.Add(resultModel.m_subModels[subMeshIter].ToMesh());
                }
            }
            else
            {
                resultantMeshes.Add(resultModel.ToMesh());
            }

            return resultantMeshes;
        }

        // Return a new CSG solid representing space in either this solid or in the
        // solid `csg`. Neither this solid nor the solid `csg` are modified.
        public static List<Mesh> NVUnion(CSGNode _nodeAIn, CSGNode _nodeBIn, CSGMeshingSettings _meshSettings)
        {
            List<Mesh> resultantMeshes = new List<Mesh>();

            CSGNode nodeA = _nodeAIn.Clone();
            CSGNode nodeB = _nodeBIn.Clone();
            nodeA.Build(nodeB.AllPolygons());

            CSGModel resultantModel = new CSGModel(nodeA.AllPolygons());
            resultantModel.SubDivideMesh(_meshSettings);
            resultantMeshes.Add(resultantModel.ToMesh());

            return resultantMeshes;
        }

        // Return a new CSG solid representing space in either this solid or in the
        // solid `csg`. Neither this solid nor the solid `csg` are modified.
        public static List<Mesh> HVUnion(CSGNode _nodeAIn, CSGNode _nodeBIn, CSGMeshingSettings _meshSettings)
        {
            List<Mesh> resultantMeshes = new List<Mesh>();

            CSGNode nodeA = _nodeAIn.Clone();
            CSGNode nodeB = _nodeBIn.Clone();
            nodeA.Build(nodeB.AllPolygons());

            CSGModel resultantModel = new CSGModel(nodeA.AllPolygons());
            resultantModel.SubDivideMesh(_meshSettings);
            resultantMeshes.Add(resultantModel.ToMesh());

            return resultantMeshes;
        }

        // Return a new CSG solid representing space in this solid but not in the
        // solid `csg`. Neither this solid nor the solid `csg` are modified.
        public static List<Mesh> Subtract(CSGNode _nodeAIn, CSGNode _nodeBIn, CSGMeshingSettings _meshSettings)
        {
            List<Mesh> resultantMeshes = new List<Mesh>();

            CSGNode nodeA = _nodeAIn.Clone();
			CSGNode nodeB = _nodeBIn.Clone();

            nodeA.Invert();
            nodeA.ClipTo(nodeB);
            nodeB.ClipTo(nodeA);
            nodeB.Invert();
            nodeB.ClipTo(nodeA);
            nodeB.Invert();
            nodeA.Build(nodeB.AllPolygons());
            nodeA.Invert();

            CSGModel resultModel = new CSGModel(nodeA.AllPolygons());
            if (resultModel.SubDivideMesh(_meshSettings))
            {
                for (int subMeshIter = 0; subMeshIter < resultModel.m_subModels.Count; subMeshIter++)
                {
                    resultantMeshes.Add(resultModel.m_subModels[subMeshIter].ToMesh());
                }
            }
            else
            {
                resultantMeshes.Add(resultModel.ToMesh());
            }

            return resultantMeshes;
		}

        // Return a new CSG solid representing space in this solid but not in the
        // solid `csg`. Neither this solid nor the solid `csg` are modified.
        public static List<Mesh> NVSubtract(CSGNode _nodeAIn, CSGNode _nodeBIn, CSGNode _nodeCIn, CSGNode _nodeDIn, CSGMeshingSettings _meshSettings)
        {
            List<Mesh> resultantMeshes = new List<Mesh>();

            CSGNode nodeA = _nodeAIn.Clone();
            CSGNode nodeB = _nodeBIn.Clone();
            nodeA.ClipTo(nodeB);

            CSGNode nodeC = _nodeCIn.Clone();
            CSGNode nodeD = _nodeDIn.Clone();
            nodeD.Invert();
            nodeC.ClipTo(nodeD);
            
            CSGModel resultantModelA = new CSGModel(nodeA.AllPolygons());
            CSGModel resultantModelB = new CSGModel(nodeC.AllPolygons());
            resultantMeshes.Add(resultantModelA.ToMesh());
            resultantMeshes.Add(resultantModelB.ToMesh());
            return resultantMeshes;
        }

        // Return a new CSG solid representing space in this solid but not in the
        // solid `csg`. Neither this solid nor the solid `csg` are modified.
        public static List<Mesh> HVSubtract(CSGNode _nodeAIn, CSGNode _nodeBIn, CSGMeshingSettings _meshSettings)
        {
            List<Mesh> resultantMeshes = new List<Mesh>();

            CSGNode nodeA = _nodeAIn.Clone();
            CSGNode nodeB = _nodeBIn.Clone();
            nodeA.ClipTo(nodeB);

            CSGModel resultantModels = new CSGModel(nodeA.AllPolygons());
            resultantModels.SubDivideMesh(_meshSettings);
            resultantMeshes.Add(resultantModels.ToMesh());
            return resultantMeshes;
        }

        // Return a new CSG solid representing space both this solid and in the
        // solid `csg`. Neither this solid nor the solid `csg` are modified.
        public static List<Mesh> Intersect(CSGNode _nodeAIn, CSGNode _nodeBIn, CSGMeshingSettings _meshSettings)
        {
            List<Mesh> resultantMeshes = new List<Mesh>();

            CSGNode nodeA = _nodeAIn.Clone();
			CSGNode nodeB = _nodeBIn.Clone();

			nodeA.Invert();
			nodeB.ClipTo(nodeA);
			nodeB.Invert();
			nodeA.ClipTo(nodeB);
			nodeB.ClipTo(nodeA);
			nodeA.Build(nodeB.AllPolygons());
			nodeA.Invert();

            CSGModel resultModel = new CSGModel(nodeA.AllPolygons());
            if (resultModel.SubDivideMesh(_meshSettings))
            {
                for (int subMeshIter = 0; subMeshIter < resultModel.m_subModels.Count; subMeshIter++)
                {
                    resultantMeshes.Add(resultModel.m_subModels[subMeshIter].ToMesh());
                }
            }
            else
            {
                resultantMeshes.Add(resultModel.ToMesh());
            }

            return resultantMeshes;
        }

        // Return a new CSG solid representing space both this solid and in the
        // solid `csg`. Neither this solid nor the solid `csg` are modified.
        public static List<Mesh> HVIntersect(CSGNode _nodeAIn, CSGNode _nodeBIn, CSGMeshingSettings _meshSettings)
        {
            List<Mesh> resultantMeshes = new List<Mesh>();

            CSGNode nodeA = _nodeAIn.Clone();
            CSGNode nodeB = _nodeBIn.Clone();
            nodeB.Invert();
            nodeA.ClipTo(nodeB);

            CSGModel resultantModels = new CSGModel(nodeA.AllPolygons());
            resultantModels.SubDivideMesh(_meshSettings);
            resultantMeshes.Add(resultantModels.ToMesh());
            return resultantMeshes;
        }
        #endregion
    }
}
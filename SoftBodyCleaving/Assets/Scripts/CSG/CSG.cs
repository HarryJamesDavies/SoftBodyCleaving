// Original CSG.JS library by Evan Wallace (http://madebyevan.com), under the MIT license.
// GitHub: https://github.com/evanw/csg.js/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CSG
{
	public class CSG
	{
		public static Mesh Union(GameObject _objectA, GameObject _objectB)
		{
			CSGModel modelA = new CSGModel(_objectA);
			CSGModel modelB = new CSGModel(_objectB);

			CSGNode nodeA = new CSGNode(modelA.ToPolygons() );
			CSGNode nodeB = new CSGNode(modelB.ToPolygons() );

			List<CSGPolygon> polygons = CSGNode.Union(nodeA, nodeB).AllPolygons();

			CSGModel result = new CSGModel(polygons);

			return result.ToMesh();
		}
        
		public static List<Mesh> Subtract(GameObject _objectA, GameObject _objectB)
		{
			CSGModel modelA = new CSGModel(_objectA);
			CSGModel modelB = new CSGModel(_objectB);

			CSGNode nodeA = new CSGNode( modelA.ToPolygons() );
			CSGNode nodeB = new CSGNode( modelB.ToPolygons() );

			List<CSGPolygon> polygons = CSGNode.Subtract(nodeA, nodeB).AllPolygons();

            List<Mesh> resultantMeshes = new List<Mesh>();
			CSGModel result = new CSGModel(polygons);
            if (result.SubDivideMesh())
            {
                for (int subMeshIter = 0; subMeshIter < result.m_subModels.Count; subMeshIter++)
                {
                    resultantMeshes.Add(result.m_subModels[subMeshIter].ToMesh());
                }
            }
            else
            {
                resultantMeshes.Add(result.ToMesh());
            }

            return resultantMeshes;
		}
        
		public static Mesh Intersect(GameObject _objectA, GameObject _objectB)
		{
			CSGModel modelA = new CSGModel(_objectA);
			CSGModel modelB = new CSGModel(_objectB);

			CSGNode nodeA = new CSGNode( modelA.ToPolygons() );
			CSGNode nodeB = new CSGNode( modelB.ToPolygons() );

			List<CSGPolygon> polygons = CSGNode.Intersect(nodeA, nodeB).AllPolygons();

			CSGModel result = new CSGModel(polygons);

			return result.ToMesh();
		}

        public static void RealignMeshToAveragePosition(GameObject _object)
        {
            MeshFilter filter = _object.GetComponent<MeshFilter>();

            Vector3 averagePosition = Vector3.zero;
            for (int vertexIter = 0; vertexIter < filter.sharedMesh.vertexCount; vertexIter++)
            {
                averagePosition += filter.sharedMesh.vertices[vertexIter];
            }
            averagePosition /= filter.sharedMesh.vertexCount;

            //Vector3 transformDelta = averagePosition - _object.transform.position;

            //for (int vertexIter = 0; vertexIter < filter.sharedMesh.vertexCount; vertexIter++)
            //{
            //    filter.sharedMesh.vertices[vertexIter] += transformDelta;
            //}

            _object.transform.position = averagePosition;
        }
	}
}

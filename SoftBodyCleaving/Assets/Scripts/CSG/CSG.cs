// Original CSG.JS library by Evan Wallace (http://madebyevan.com), under the MIT license.
// GitHub: https://github.com/evanw/csg.js/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CSG
{
    public enum BooleanOperations
    {
        None,
        Union,
        Subtract,
        Intersect,
        NVUnion,
        NVSubtract,
        NVIntersect,
        HVUnion,
        HVSubtract,
        HVIntersect
    }

    public class CSG
    {
        public const float EPSILON = 0.00001f;

        public static List<Mesh> Union(GameObject _objectA, GameObject _objectB, CSGMeshingSettings _meshSettings)
        {
            CSGModel modelA = new CSGModel(_objectA);
            CSGModel modelB = new CSGModel(_objectB);

            CSGNode nodeA = new CSGNode(modelA.ToPolygons());
            CSGNode nodeB = new CSGNode(modelB.ToPolygons());

            return CSGNode.Union(nodeA, nodeB, _meshSettings);
        }

        public static List<Mesh> NVUnion(GameObject _objectA, GameObject _objectB, CSGMeshingSettings _meshSettings)
        {
            CSGModel modelA = new CSGModel(_objectA);
            CSGModel modelB = new CSGModel(_objectB);

            CSGNode nodeA = new CSGNode(modelA.ToPolygons());
            CSGNode nodeB = new CSGNode(modelB.ToPolygons());

            return CSGNode.NVUnion(nodeA, nodeB, _meshSettings);
        }

        public static List<Mesh> HVUnion(GameObject _objectA, GameObject _objectB, CSGMeshingSettings _meshSettings)
        {
            CSGModel modelA = new CSGModel(_objectA);
            CSGModel modelB = new CSGModel(_objectB);

            CSGNode nodeA = new CSGNode(modelA.ToPolygons());
            CSGNode nodeB = new CSGNode(modelB.ToPolygons());

            return CSGNode.HVUnion(nodeA, nodeB, _meshSettings);
        }

        public static List<Mesh> Subtract(GameObject _objectA, GameObject _objectB, CSGMeshingSettings _meshSettings)
		{
			CSGModel modelA = new CSGModel(_objectA);
			CSGModel modelB = new CSGModel(_objectB);

			CSGNode nodeA = new CSGNode( modelA.ToPolygons() );
			CSGNode nodeB = new CSGNode( modelB.ToPolygons() );     

            return CSGNode.Subtract(nodeA, nodeB, _meshSettings);
        }

        public static List<Mesh> NVSubtract(GameObject _objectA, GameObject _objectB, CSGMeshingSettings _meshSettings)
        {
            CSGModel modelA = new CSGModel(_objectA);
            CSGModel modelB = new CSGModel(_objectB);
            CSGModel modelC = new CSGModel(_objectA);
            CSGModel modelD = new CSGModel(_objectB);

            CSGNode nodeA = new CSGNode(modelA.ToPolygons());
            CSGNode nodeB = new CSGNode(modelB.ToPolygons());
            CSGNode nodeC = new CSGNode(modelC.ToPolygons());
            CSGNode nodeD = new CSGNode(modelD.ToPolygons());

            return CSGNode.NVSubtract(nodeA, nodeB, nodeC, nodeD, _meshSettings);
        }

        public static List<Mesh> HVSubtract(GameObject _objectA, GameObject _objectB, CSGMeshingSettings _meshSettings)
        {
            CSGModel modelA = new CSGModel(_objectA);
            CSGModel modelB = new CSGModel(_objectB);

            CSGNode nodeA = new CSGNode(modelA.ToPolygons());
            CSGNode nodeB = new CSGNode(modelB.ToPolygons());

            return CSGNode.HVSubtract(nodeA, nodeB, _meshSettings);
        }

        public static List<Mesh> Intersect(GameObject _objectA, GameObject _objectB, CSGMeshingSettings _meshSettings)
		{
			CSGModel modelA = new CSGModel(_objectA);
			CSGModel modelB = new CSGModel(_objectB);

			CSGNode nodeA = new CSGNode( modelA.ToPolygons() );
			CSGNode nodeB = new CSGNode( modelB.ToPolygons() );

			return CSGNode.Intersect(nodeA, nodeB, _meshSettings);
        }

        public static List<Mesh> HVIntersect(GameObject _objectA, GameObject _objectB, CSGMeshingSettings _meshSettings)
        {
            CSGModel modelA = new CSGModel(_objectA);
            CSGModel modelB = new CSGModel(_objectB);

            CSGNode nodeA = new CSGNode(modelA.ToPolygons());
            CSGNode nodeB = new CSGNode(modelB.ToPolygons());

            return CSGNode.HVIntersect(nodeA, nodeB, _meshSettings);
        }

        public static GameObject CreateObjectFromMesh(Mesh _mesh, Material _material)
        {
            GameObject newObject = new GameObject();
            MeshFilter filter = newObject.AddComponent<MeshFilter>();
            filter.sharedMesh = _mesh;
            newObject.AddComponent<MeshRenderer>().sharedMaterial = _material;
            return newObject;
        }
    }
}

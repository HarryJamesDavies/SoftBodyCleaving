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
        Intersect
    }

    public class CSG
    {
        public const float EPSILON = 0.00001f;

        public static List<Mesh> Union(GameObject _objectA, GameObject _objectB)
        {
            CSGModel modelA = new CSGModel(_objectA);
            CSGModel modelB = new CSGModel(_objectB);

            CSGNode nodeA = new CSGNode(modelA.ToPolygons());
            CSGNode nodeB = new CSGNode(modelB.ToPolygons());

            return CSGNode.Union(nodeA, nodeB);
        }

        public static List<Mesh> Union2D(GameObject _objectA, GameObject _objectB)
        {
            CSGModel modelA = new CSGModel(_objectA);
            CSGModel modelB = new CSGModel(_objectB);

            CSGNode nodeA = new CSGNode(modelA.ToPolygons());
            CSGNode nodeB = new CSGNode(modelB.ToPolygons());

            return CSGNode.Union2D(nodeA, nodeB);
        }

        public static List<Mesh> Subtract(GameObject _objectA, GameObject _objectB)
		{
			CSGModel modelA = new CSGModel(_objectA);
			CSGModel modelB = new CSGModel(_objectB);

			CSGNode nodeA = new CSGNode( modelA.ToPolygons() );
			CSGNode nodeB = new CSGNode( modelB.ToPolygons() );     

            return CSGNode.Subtract(nodeA, nodeB);
        }

        public static List<Mesh> Subtract2D(GameObject _objectA, GameObject _objectB)
        {
            CSGModel modelA = new CSGModel(_objectA);
            CSGModel modelB = new CSGModel(_objectB);

            CSGNode nodeA = new CSGNode(modelA.ToPolygons());
            CSGNode nodeB = new CSGNode(modelB.ToPolygons());

            return CSGNode.Subtract2D(nodeA, nodeB);
        }

        public static List<Mesh> Intersect(GameObject _objectA, GameObject _objectB)
		{
			CSGModel modelA = new CSGModel(_objectA);
			CSGModel modelB = new CSGModel(_objectB);

			CSGNode nodeA = new CSGNode( modelA.ToPolygons() );
			CSGNode nodeB = new CSGNode( modelB.ToPolygons() );

			return CSGNode.Intersect(nodeA, nodeB);
        }

        public static List<Mesh> Intersect2D(GameObject _objectA, GameObject _objectB)
        {
            Debug.Log("Not valid operation for 2D");
            return new List<Mesh>();
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

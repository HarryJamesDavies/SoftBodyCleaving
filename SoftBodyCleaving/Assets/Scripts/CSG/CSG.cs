// Original CSG.JS library by Evan Wallace (http://madebyevan.com), under the MIT license.
// GitHub: https://github.com/evanw/csg.js/

using UnityEngine;
using System;
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

        public static List<Mesh> NVIntersect(GameObject _objectA, GameObject _objectB, CSGMeshingSettings _meshSettings)
        {
            Mesh mesh = _objectB.GetComponent<MeshFilter>().mesh;
            GameObject planeRight = GameObject.Instantiate(_objectB);
            planeRight.AddComponent<DestroyOnLateUpdate>();
            GameObject planeLeft = GameObject.Instantiate(_objectB);
            planeLeft.AddComponent<DestroyOnLateUpdate>();
            GameObject planeUP = GameObject.Instantiate(_objectB);
            planeUP.AddComponent<DestroyOnLateUpdate>();
            GameObject planeBack = GameObject.Instantiate(_objectB);
            planeBack.AddComponent<DestroyOnLateUpdate>();
            GameObject planeFront = GameObject.Instantiate(_objectB);
            planeFront.AddComponent<DestroyOnLateUpdate>();

            planeRight.transform.position = _objectB.transform.position + new Vector3(mesh.bounds.max.x * planeRight.transform.localScale.x, 0.0f, 0.0f);
            planeRight.transform.Rotate(new Vector3(0.0f, 0.0f, 90.0f));
            planeLeft.transform.position = _objectB.transform.position + new Vector3(mesh.bounds.min.x * planeLeft.transform.localScale.x, 0.0f, 0.0f);
            planeLeft.transform.Rotate(new Vector3(0.0f, 0.0f, -90.0f));
            planeUP.transform.position = _objectB.transform.position;
            planeUP.transform.Rotate(new Vector3(180.0f, 0.0f, 0.0f));
            planeBack.transform.position = _objectB.transform.position + new Vector3(0.0f, 0.0f, mesh.bounds.max.z * planeRight.transform.localScale.z);
            planeBack.transform.Rotate(new Vector3(-90.0f, 0.0f, 0.0f));
            planeFront.transform.position = _objectB.transform.position + new Vector3(0.0f, 0.0f, mesh.bounds.min.z * planeRight.transform.localScale.z);
            planeFront.transform.Rotate(new Vector3(90.0f, 0.0f, 0.0f));

            CSGModel modelA = new CSGModel(_objectA);
            CSGModel modelB = new CSGModel(planeRight);
            CSGModel modelC = new CSGModel(planeLeft);
            CSGModel modelD = new CSGModel(planeUP);
            CSGModel modelE = new CSGModel(planeBack);
            CSGModel modelF = new CSGModel(planeFront);

            CSGNode nodeA = new CSGNode(modelA.ToPolygons());
            CSGNode nodeB = new CSGNode(modelB.ToPolygons());
            CSGNode nodeC = new CSGNode(modelC.ToPolygons());
            CSGNode nodeD = new CSGNode(modelD.ToPolygons());
            CSGNode nodeE = new CSGNode(modelE.ToPolygons());
            CSGNode nodeF = new CSGNode(modelF.ToPolygons());

            return CSGNode.NVIntersect(nodeA, nodeB, nodeC, nodeD, nodeE, nodeF, _meshSettings);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSGTest : MonoBehaviour
{
    public GameObject objectA;
    public GameObject objectB;
    public GameObject composite;

    void Start ()
    {
        composite = new GameObject();
        composite.AddComponent<MeshFilter>().sharedMesh = Parabox.CSG.CSG.Subtract(objectA, objectB);
        composite.AddComponent<MeshRenderer>().sharedMaterial = objectA.GetComponent<MeshRenderer>().sharedMaterial;
        
        GameObject.Destroy(objectA);
        GameObject.Destroy(objectB);
    }
}

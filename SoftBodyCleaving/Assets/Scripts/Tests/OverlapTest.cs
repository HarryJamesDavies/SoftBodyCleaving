using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSG;

public class OverlapTest : MonoBehaviour
{
    [SerializeField] private MeshFilter filter;
    private CSGModel model;

    void Start ()
    {
        model = new CSGModel(gameObject);
        model.RemoveOverlappingVertices();
        model.TransfromVertexToLocal(transform);
        filter.sharedMesh = model.ToMesh();
    }
}

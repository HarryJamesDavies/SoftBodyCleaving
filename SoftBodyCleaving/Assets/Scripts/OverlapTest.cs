using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlapTest : MonoBehaviour
{
    [SerializeField] private MeshFilter filter;
    Parabox.CSG.CSG_Model model;

    void Start ()
    {
        model = new Parabox.CSG.CSG_Model(gameObject);
        model.RemoveOverlappingVertices();
        model.TransfromVertexToLocal(transform);
        filter.sharedMesh = model.ToMesh();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSG;

public class OverlapTest : MonoBehaviour
{
    [SerializeField] private MeshFilter filter;
    private CSGModel model;
    [SerializeField] private CSGMeshingSettings m_csgMeshSettings = new CSGMeshingSettings();

    void Start ()
    {
        model = new CSGModel(gameObject);
        model.RemoveOverlappingVertices(m_csgMeshSettings);
        model.TransfromVertexToLocal(transform);
        filter.sharedMesh = model.ToMesh();
    }
}

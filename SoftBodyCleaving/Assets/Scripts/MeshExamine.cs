using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshExamine : MonoBehaviour {
    public MeshFilter m_mesh;
    public Vector3[] m_vertices;
    public int[] m_indices;

    private void Start()
    {
        m_vertices = m_mesh.sharedMesh.vertices;
        m_indices = m_mesh.sharedMesh.triangles;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshExamine : MonoBehaviour {
    public MeshFilter m_mesh;
    public Vector3[] m_vertices;
    public int[] m_indices;
    public Vector3[] m_normals;

    private void Update()
    {
        m_vertices = m_mesh.sharedMesh.vertices;
        m_indices = m_mesh.sharedMesh.triangles;
        m_mesh.sharedMesh.RecalculateNormals();
        m_normals = m_mesh.sharedMesh.normals;
    }
}

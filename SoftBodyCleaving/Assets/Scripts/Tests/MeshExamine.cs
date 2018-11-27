using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshExamine : MonoBehaviour {
    public MeshFilter m_mesh;
    public Vector3[] m_vertices;
    public int[] m_indices;
    public Vector3[] m_normals;
    public Vector2[] m_uvs;

    private void Update()
    {
        if (m_mesh)
        {
            m_vertices = m_mesh.sharedMesh.vertices;
            m_indices = m_mesh.sharedMesh.triangles;
            m_normals = m_mesh.sharedMesh.normals;
            m_uvs = m_mesh.sharedMesh.uv;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSGSubModel
{
    public Parabox.CSG.CSG_Model m_parent;
    public List<int> m_vertices = new List<int>();
    public List<int> m_indices = new List<int>();

    public CSGSubModel(Parabox.CSG.CSG_Model _parent)
    {
        m_parent = _parent;
    }

    public CSGSubModel MergeModels(CSGSubModel _in)
    {
        CSGSubModel result = new CSGSubModel(m_parent);

        if (m_vertices.Count > _in.m_vertices.Count)
        {
            result.m_vertices.AddRange(m_vertices);

            for (int vertexCount = 0; vertexCount < _in.m_vertices.Count; vertexCount++)
            {
                if (!result.m_vertices.Contains(_in.m_vertices[vertexCount]))
                {
                    result.m_vertices.Add(_in.m_vertices[vertexCount]);
                }
            }
        }
        else
        {
            result.m_vertices.AddRange(_in.m_vertices);

            for (int vertexCount = 0; vertexCount < m_vertices.Count; vertexCount++)
            {
                if (!result.m_vertices.Contains(m_vertices[vertexCount]))
                {
                    result.m_vertices.Add(m_vertices[vertexCount]);
                }
            }
        }

        result.m_indices.AddRange(m_indices);
        result.m_indices.AddRange(_in.m_indices);

        return result;
    }

    public Mesh ToMesh()
    {
        Mesh m = new Mesh();

        int vc = m_vertices.Count;

        Vector3[] v = new Vector3[vc];
        Vector3[] n = new Vector3[vc];
        Vector2[] u = new Vector2[vc];
        Color[] c = new Color[vc];

        for (int i = 0; i < vc; i++)
        {
            v[i] = m_parent.m_vertices[m_vertices[i]].m_position;
            n[i] = m_parent.m_vertices[m_vertices[i]].m_normal;
            u[i] = m_parent.m_vertices[m_vertices[i]].m_uv;
            c[i] = m_parent.m_vertices[m_vertices[i]].m_color;
        }

        m.vertices = v;
        m.normals = n;
        m.colors = c;
        m.uv = u;

        for (int indexIter = 0; indexIter < m_indices.Count; indexIter++)
        {
            m_indices[indexIter] = m_vertices.FindIndex(x => x == m_indices[indexIter]);
        }
        m.triangles = m_indices.ToArray();

        return m;
    }
}

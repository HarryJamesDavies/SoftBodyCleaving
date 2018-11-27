using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VertexGroup
{
    public List<int> m_vertices = new List<int>();
    public Vector3 m_normal;
    public Vector3 m_sharedNormal;
    public bool m_useAveragePosition = false;
    public Vector3 m_averagePosition;

    public VertexGroup()
    {

    }

    public VertexGroup(int _vertex, Vector3 _normal)
    {
        m_vertices.Add(_vertex);
        AddToSharedNormal(_normal);
    }

    public void AddToSharedNormal(Vector3 _normal)
    {
        m_sharedNormal += _normal;
        m_normal = m_sharedNormal.normalized;
    }

    public void SetAveragePosition(Vector3 _averagePosition)
    {
        m_averagePosition = _averagePosition;
        m_useAveragePosition = true;
    }
}

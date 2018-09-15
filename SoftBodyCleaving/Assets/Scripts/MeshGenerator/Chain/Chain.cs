using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chain : MonoBehaviour
{
    public List<Vector3> m_corePoints = new List<Vector3>();
    public List<VertexGroup> m_vertexGroups = new List<VertexGroup>();
    public Mesh m_mesh;

    public void Initialise(List<Vector3> _corePoints, List<VertexGroup> _vertexGroups, Mesh _mesh)
    {
        m_corePoints.AddRange(_corePoints);
        m_vertexGroups.AddRange(_vertexGroups);
        m_mesh = _mesh;
    }
}

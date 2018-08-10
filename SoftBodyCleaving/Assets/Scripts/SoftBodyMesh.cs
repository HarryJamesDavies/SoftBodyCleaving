using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VertexGroup
{
   public List<int> m_vertices = new List<int>();
}

[Serializable]
public class Mass
{
    public VertexGroup vertexGroup;
    public int index;
    public SoftBodyMesh mesh;
    public List<int> springs;
    public List<int> neighbours;
    public Vector3 force;
    public Vector3 velocity;
    public bool m_fixed;

    public Mass(SoftBodyMesh _mesh, VertexGroup _group, int _index, bool _fixed)
    {
        mesh = _mesh;
        vertexGroup = _group;
        index = _index;
        springs = new List<int>();
        neighbours = new List<int>();
        force = Vector3.zero;
        velocity = Vector3.zero;
        m_fixed = _fixed;
    }

    public bool CheckDuplicateSpring(int _otherMass)
    {
        for (int springIter = 0; springIter < springs.Count; springIter++)
        {
            if(mesh.m_springs[springs[springIter]].ContainsMasses(index, _otherMass))
            {
                return true;
            }
        }
        return false;
    }

    public void AddSpring(int _spring)
    {
        if (!springs.Contains(_spring))
        {
            springs.Add(_spring);
        }
    }

    public void AddNeighbour(int _mass)
    {
        if (!neighbours.Contains(_mass))
        {
            neighbours.Add(_mass);
        }
    }
}

[Serializable]
public struct Spring
{
    public int massA;
    public int massB;
    public float equilibriumDistance;

    public Spring(int _massA, int _massB, float _equilibriumDistance)
    {
        massA = _massA;
        massB = _massB;
        equilibriumDistance = _equilibriumDistance;
    }

    public bool ContainsMasses(int _massA, int _massB)
    {
        return ((massA == _massA) && (massB == _massB))
            || ((massA == _massB) && (massB == _massA));
    }
}

public class SoftBodyMesh : MonoBehaviour
{
    public SoftBodyCore m_core = null;
    public MeshFilter m_meshFilter;

    public List<VertexGroup> m_vertexGroups = new List<VertexGroup>();
    public List<Mass> m_masses = new List<Mass>();
    public List<Spring> m_springs = new List<Spring>();

    public bool m_highlightMasses = false;
    public GameObject m_highlightObject;

    public float m_neighbourDistance = 10.0f;
    public float m_springCoefficient = 10.0f;
    public float m_dragCoefficient = 0.1f;
    public float m_fixMassHeight = 5.0f;

    private List<Vector3> m_vertices = new List<Vector3>();
    private List<Vector2> m_uvs = new List<Vector2>();
    private List<Color> m_colours = new List<Color>();
    private List<int> m_triangles = new List<int>();

    public void Start()
    {
        CreateSoftBodyFromMesh();
    }

    private void CreateSoftBodyFromMesh()
    {
        if (m_meshFilter.sharedMesh != null)
        {
            GetData();
            GroupVertices();
            GenerateMasses();
            GenerateNeighbours();
            GenerateSprings();
            
            if (m_core)
            {
                m_core.OnMeshGenerated(this);
            }
        }
    }

    private void GetData()
    {
        m_vertices = m_meshFilter.sharedMesh.vertices.ToList();
        m_uvs = m_meshFilter.sharedMesh.uv.ToList();
        m_colours = m_meshFilter.sharedMesh.colors.ToList();
        m_triangles = m_meshFilter.sharedMesh.triangles.ToList();
    }

    private void GenerateMasses()
    {
        for (int vertexIter = 0; vertexIter < m_vertexGroups.Count; vertexIter++)
        {
            CreateMass(m_vertexGroups[vertexIter]);
        }
    }

    private void GroupVertices()
    {
        List<int> groupedVertices = new List<int>();

        for (int vertexIter = 0; vertexIter < m_vertices.Count; vertexIter++)
        {
            if (!groupedVertices.Contains(vertexIter))
            {
                m_vertexGroups.Add(new VertexGroup());
                m_vertexGroups.Last().m_vertices.Add(vertexIter);
                groupedVertices.Add(vertexIter);

                for (int checkIter = vertexIter + 1; checkIter < m_vertices.Count; checkIter++)
                {
                    if (!groupedVertices.Contains(checkIter))
                    {
                        if(m_vertices[vertexIter] == m_vertices[checkIter])
                        {
                            m_vertexGroups.Last().m_vertices.Add(checkIter);
                            groupedVertices.Add(checkIter);
                        }
                    }
                }
            }
        }
    }

    private void GenerateNeighbours()
    {
        List<Mass> checkMasses = new List<Mass>();
        checkMasses.AddRange(m_masses);

        int checkStart = 1;
        for (int massIter = 0; massIter < m_masses.Count; massIter++)
        {
            for (int checkIter = checkStart; checkIter < checkMasses.Count; checkIter++)
            {
                if(Vector3.Distance(m_vertices[m_masses[massIter].vertexGroup.m_vertices[0]],
                    m_vertices[checkMasses[checkIter].vertexGroup.m_vertices[0]]) <= m_neighbourDistance)
                {
                    m_masses[massIter].AddNeighbour(checkMasses[checkIter].index);
                    checkMasses[checkIter].AddNeighbour(m_masses[massIter].index);
                }
            }
            checkStart++;
        }
        checkMasses.Clear();
    }

    private void GenerateSprings()
    {
        for (int massIter = 0; massIter < m_masses.Count; massIter++)
        {
            Mass currentMass = m_masses[massIter];
            for (int neighbourIter = 0; neighbourIter < currentMass.neighbours.Count; neighbourIter++)
            {
                CreateSpring(currentMass.index, m_masses[currentMass.neighbours[neighbourIter]].index);
            }
        }
    }

    private void CreateMass(VertexGroup _group)
    {
        m_masses.Add(new Mass(this, _group, m_masses.Count, ((transform.TransformPoint(m_vertices[_group.m_vertices[0]]).y) > m_fixMassHeight)));

        if(m_highlightMasses)
        {
            Instantiate(m_highlightObject, m_vertices[_group.m_vertices[0]] + transform.position, transform.rotation, transform);
        }
    }

    private void CreateSpring(int _massA, int _massB)
    {
        if(!m_masses[_massA].CheckDuplicateSpring(_massB))
        {
            int springIndex = m_springs.Count;
            m_springs.Add(new Spring(_massA, _massB, GetMassDistance(_massA, _massB)));
            m_masses[_massA].AddSpring(springIndex);
            m_masses[_massB].AddSpring(springIndex);
        }
    }

    private float GetMassDistance(int _massA, int _massB)
    {
        return Vector3.Distance(m_vertices[m_masses[_massA].vertexGroup.m_vertices[0]],
            m_vertices[m_masses[_massB].vertexGroup.m_vertices[0]]);
    }

    public Vector3 GetVertex(int _vertex)
    {
        return m_vertices[_vertex];
    }

    public void SetVertex(int _vertex, Vector3 _position)
    {
        m_vertices[_vertex] = _position;
    }

    public void DisplaceVertex(VertexGroup _group, Vector3 _displacement)
    {
        for (int vertexIter = 0; vertexIter < _group.m_vertices.Count; vertexIter++)
        {
            m_vertices[_group.m_vertices[vertexIter]] += _displacement;
        }
    }

    public void UpdateMesh()
    {
        m_meshFilter.sharedMesh.vertices = m_vertices.ToArray();
        m_meshFilter.sharedMesh.uv = m_uvs.ToArray();
        m_meshFilter.sharedMesh.triangles = m_triangles.ToArray();
        m_meshFilter.sharedMesh.colors = m_colours.ToArray();
        m_meshFilter.sharedMesh.RecalculateNormals();
    }
}

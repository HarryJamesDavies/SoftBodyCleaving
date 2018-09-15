using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public enum FixDirection
{
    Top,
    Bottom,
    Right,
    Left,
    Front,
    Back
}

public class SoftBodyMesh : MonoBehaviour
{
    public MeshFilter m_meshFilter;

    public List<VertexGroup> m_vertexGroups = new List<VertexGroup>();
    public List<Mass> m_masses = new List<Mass>();
    public List<Spring> m_springs = new List<Spring>();

    public float m_neighbourDistance = 10.0f;

    public float m_capSpringForce = 10.0f;
    public float m_springCoefficient = 10.0f;
    public float m_dragCoefficient = 0.1f;
    public float m_pressureCoefficient = 5.0f;

    public bool m_usePressure = true;

    public FixDirection m_fixedDirection;
    public float m_fixedOffset = 5.0f;
    private Vector3 m_furthestFixedPosition;

    private List<Vector3> m_vertices = new List<Vector3>();
    private List<Vector3> m_normals = new List<Vector3>();
    private List<Vector2> m_uvs = new List<Vector2>();
    private List<Color> m_colours = new List<Color>();
    private List<int> m_triangles = new List<int>();

    public bool m_generateOnStart = false;
    public bool m_useMeshInstance = true;

    private void Start()
    {
        if (m_generateOnStart)
        {
            Initialise();

            SoftBodyCore core = GetComponent<SoftBodyCore>();
            if (core)
            {
                core.Initialise(this);
            }
        }
    }

    public void Initialise(MeshFilter _meshFilter = null)
    {
        if (_meshFilter)
        {
            m_meshFilter = _meshFilter;
        }

        if (m_useMeshInstance)
        {
            m_meshFilter.sharedMesh = Mesh.Instantiate(m_meshFilter.sharedMesh);
        }
    }

    public void Create1DSoftBodyFromGroups(Chain _chain)
    {
        if (m_meshFilter.sharedMesh != null)
        {
            m_vertexGroups.AddRange(_chain.m_vertexGroups);
            m_usePressure = false;

            GetData();
            CheckGroupsMaxPosition();
            GenerateMasses1D();
            GenerateNeighbours1D();
            GenerateSprings();
        }
    }

    private void CheckGroupsMaxPosition()
    {
        for (int groupIter = 0; groupIter < m_vertexGroups.Count; groupIter++)
        {
            CheckMaxFixedPosition((m_vertexGroups[groupIter].m_useAveragePosition) ?
               m_vertexGroups[groupIter].m_averagePosition :
               m_vertices[m_vertexGroups[groupIter].m_vertices[0]]);
        }
    }
    
    private void GenerateMasses1D()
    {
        for (int groupIter = 0; groupIter < m_vertexGroups.Count; groupIter++)
        {
            CreateMass(m_vertexGroups[groupIter]);
        }
    }

    private void GenerateNeighbours1D()
    {
        for (int massIter = 0; massIter < m_masses.Count; massIter++)
        {
            if(massIter - 1 > -1)
            {
                m_masses[massIter].AddNeighbour(massIter - 1);
            }

            if (massIter + 1 < m_masses.Count)
            {
                m_masses[massIter].AddNeighbour(massIter + 1);
            }

            if (massIter - 2 > -1)
            {
                m_masses[massIter].AddNeighbour(massIter - 2);
            }

            if (massIter + 2 < m_masses.Count)
            {
                m_masses[massIter].AddNeighbour(massIter + 2);
            }
        }
    }

    public void Create3DSoftBodyFromMesh()
    {
        if (m_meshFilter.sharedMesh != null)
        {
            GetData();
            GroupVertices();
            GenerateMassesVertexGroups();
            GenerateNeighbours();
            GenerateSprings();
        }
    }

    private void GetData()
    {
        m_vertices = m_meshFilter.sharedMesh.vertices.ToList();
        m_uvs = m_meshFilter.sharedMesh.uv.ToList();
        m_colours = m_meshFilter.sharedMesh.colors.ToList();
        m_triangles = m_meshFilter.sharedMesh.triangles.ToList();
        m_normals = m_meshFilter.sharedMesh.normals.ToList();
    }

    private void GenerateMassesVertexGroups()
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
                m_vertexGroups.Last().AddToSharedNormal(m_normals[vertexIter]);
                groupedVertices.Add(vertexIter);
                CheckMaxFixedPosition(m_vertices[vertexIter]);

                for (int checkIter = vertexIter + 1; checkIter < m_vertices.Count; checkIter++)
                {
                    if (!groupedVertices.Contains(checkIter))
                    {
                        if(m_vertices[vertexIter] == m_vertices[checkIter])
                        {
                            m_vertexGroups.Last().m_vertices.Add(checkIter);
                            m_vertexGroups.Last().AddToSharedNormal(m_normals[checkIter]);
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
        m_masses.Add(new Mass(this, _group, m_masses.Count, CheckMassFixed((_group.m_useAveragePosition)
            ? _group.m_averagePosition : m_vertices[_group.m_vertices[0]])));
    }

    private void CheckMaxFixedPosition(Vector3 _position)
    {
        switch (m_fixedDirection)
        {
            case FixDirection.Top:
                {
                    if(_position.y > m_furthestFixedPosition.y)
                    {
                        m_furthestFixedPosition = _position;
                    }
                    break;
                }
            case FixDirection.Bottom:
                {
                    if (_position.y < m_furthestFixedPosition.y)
                    {
                        m_furthestFixedPosition = _position;
                    }
                    break;
                }
            case FixDirection.Right:
                {
                    if (_position.x > m_furthestFixedPosition.x)
                    {
                        m_furthestFixedPosition = _position;
                    }
                    break;
                }
            case FixDirection.Left:
                {
                    if (_position.x < m_furthestFixedPosition.x)
                    {
                        m_furthestFixedPosition = _position;
                    }
                    break;
                }
            case FixDirection.Front:
                {
                    if (_position.z > m_furthestFixedPosition.z)
                    {
                        m_furthestFixedPosition = _position;
                    }
                    break;
                }
            case FixDirection.Back:
                {
                    if (_position.z < m_furthestFixedPosition.z)
                    {
                        m_furthestFixedPosition = _position;
                    }
                    break;
                }
        }
    }

    private bool CheckMassFixed(Vector3 _position)
    {
        switch (m_fixedDirection)
        {
            case FixDirection.Top:
                {
                    if (_position.y > m_furthestFixedPosition.y - m_fixedOffset)
                    {
                        return true;
                    }
                    break;
                }
            case FixDirection.Bottom:
                {
                    if (_position.y < m_furthestFixedPosition.y + m_fixedOffset)
                    {
                        return true;
                    }
                    break;
                }
            case FixDirection.Right:
                {
                    if (_position.x > m_furthestFixedPosition.x - m_fixedOffset)
                    {
                        return true;
                    }
                    break;
                }
            case FixDirection.Left:
                {
                    if (_position.x < m_furthestFixedPosition.x + m_fixedOffset)
                    {
                        return true;
                    }
                    break;
                }
            case FixDirection.Front:
                {
                    if (_position.z > m_furthestFixedPosition.z - m_fixedOffset)
                    {
                        return true;
                    }
                    break;
                }
            case FixDirection.Back:
                {
                    if (_position.z < m_furthestFixedPosition.z + m_fixedOffset)
                    {
                        return true;
                    }
                    break;
                }
        }
        return false;
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
            try
            {
                m_vertices[_group.m_vertices[vertexIter]] += _displacement;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.Log("Warning");
            }
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

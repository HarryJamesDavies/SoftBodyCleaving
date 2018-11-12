using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace MSM
{
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

        public SoftBodySettings m_settings;

        private bool m_initialised = false;
        private Vector3 m_furthestFixedPosition;

        private List<Vector3> m_vertices = new List<Vector3>();
        private List<Vector3> m_normals = new List<Vector3>();
        private List<Vector2> m_uvs = new List<Vector2>();
        private List<Color> m_colours = new List<Color>();
        private List<int> m_triangles = new List<int>();

        private void Start()
        {
            //Initalises SBmesh and SBCore on Start
            if (m_settings != null)
            {
                if (m_settings.m_initialiseOnStart)
                {
                    Initialise();
                    CreateSoftBodyFromMesh();

                    SoftBodyCore core = GetComponent<SoftBodyCore>();
                    if (core)
                    {
                        core.Initialise(this);
                    }
                }
            }
        }

        /// <summary>
        /// Sets target MeshFilter and generates Mesh Instance
        /// </summary>
        /// <param name="_meshFilter"></param>
        public void Initialise(MeshFilter _meshFilter = null, SoftBodySettings _settings = null)
        {
            if (_meshFilter)
            {
                m_meshFilter = _meshFilter;
            }
            else if (m_meshFilter == null)
            {
                m_meshFilter = GetComponent<MeshFilter>();
            }

            if (_settings != null)
            {
                m_settings = new SoftBodySettings(_settings);
            }
            else if (m_settings == null)
            {
                m_settings = new SoftBodySettings();
            }

            if (m_settings.m_useMeshInstance && m_meshFilter)
            {
                m_meshFilter.sharedMesh = Instantiate(m_meshFilter.sharedMesh);
            }

            m_initialised = m_meshFilter.sharedMesh != null;
        }

        /// <summary>
        /// Creates 1D SBMesh from Chain Vertex Groups
        /// </summary>
        /// <param name="_chain"></param>
        public void CreateSoftBodyFromChain(Chain _chain)
        {
            if (m_initialised)
            {
                m_vertexGroups.AddRange(_chain.m_vertexGroups);
                m_settings.m_usePressure = false;

                GetData();
                CheckGroupsMaxPosition();
                GenerateChainMasses();
                GenerateChainNeighbours();
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

        private void GenerateChainMasses()
        {
            for (int groupIter = 0; groupIter < m_vertexGroups.Count; groupIter++)
            {
                CreateMass(m_vertexGroups[groupIter]);
            }
        }

        private void GenerateChainNeighbours()
        {
            for (int massIter = 0; massIter < m_masses.Count; massIter++)
            {
                if (massIter - 1 > -1)
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

        public void CreateSoftBodyFromMesh()
        {
            if (m_initialised)
            {
                GetData();
                GroupVertices();
                GenerateMassesVertexGroups();
                GenerateNeighbours();
                GenerateSprings();

                if (m_settings.m_useInternals)
                {
                    GenerateExternals();
                }
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
                            if (m_vertices[vertexIter] == m_vertices[checkIter])
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
                    if (Vector3.Distance(m_vertices[m_masses[massIter].vertexGroup.m_vertices[0]],
                        m_vertices[checkMasses[checkIter].vertexGroup.m_vertices[0]]) <= m_settings.m_neighbourDistance)
                    {
                        m_masses[massIter].AddNeighbour(checkMasses[checkIter].index);
                        checkMasses[checkIter].AddNeighbour(m_masses[massIter].index);
                    }
                }
                checkStart++;
            }
            checkMasses.Clear();
        }

        private void GenerateNeighboursAlt()
        {
            Vector3 minimum = Vector3.zero;
            Vector3 maximum = Vector3.zero;

            //Find center of mesh through min/max difference/2
            for (int massIter = 0; massIter < m_masses.Count; massIter++)
            {
                if (m_masses[massIter].GetPostion().x < minimum.x)
                {
                    minimum.x = m_masses[massIter].GetPostion().x;
                }
                else if (m_masses[massIter].GetPostion().x > maximum.x)
                {
                    maximum.x = m_masses[massIter].GetPostion().x;
                }

                if (m_masses[massIter].GetPostion().y < minimum.y)
                {
                    minimum.y = m_masses[massIter].GetPostion().y;
                }
                else if (m_masses[massIter].GetPostion().y > maximum.y)
                {
                    maximum.y = m_masses[massIter].GetPostion().y;
                }

                if (m_masses[massIter].GetPostion().z < minimum.z)
                {
                    minimum.z = m_masses[massIter].GetPostion().z;
                }
                else if (m_masses[massIter].GetPostion().z > maximum.z)
                {
                    maximum.z = m_masses[massIter].GetPostion().z;
                }
            }

            Vector3 difference = maximum - minimum;
            Vector3 center = (difference / 2.0f) + minimum;

            //2(A^2)=C^2 ==> A = 2¬((C^2)/2)
            float sectionSideLength = Mathf.Sqrt((m_settings.m_neighbourDistance * m_settings.m_neighbourDistance) / 2.0f);
            Vector3 sideSpace = (difference - (Vector3.one * sectionSideLength)) / 2.0f;
            Vector3 subSectionsCount = new Vector3(
                Mathf.Ceil(sideSpace.x / sectionSideLength),
                Mathf.Ceil(sideSpace.y / sectionSideLength),
                Mathf.Ceil(sideSpace.z / sectionSideLength));
            subSectionsCount *= 2.0f;
            subSectionsCount += Vector3.one;

            Vector3 subSectionMinimum = center - ((subSectionsCount / 2.0f) * sectionSideLength);

            List<MeshSubSection> subSections = new List<MeshSubSection>();
            for (int x = 0; x < subSectionsCount.x; x++)
            {
                float xOffset = x * sectionSideLength;
                for (int y = 0; y < subSectionsCount.y; y++)
                {
                    float yOffset = y * sectionSideLength;
                    for (int z = 0; z < subSectionsCount.z; z++)
                    {
                        float zOffset = z * sectionSideLength;
                        subSections.Add(new MeshSubSection(new Vector3(subSectionMinimum.x + xOffset,
                            subSectionMinimum.y + yOffset, subSectionMinimum.z + zOffset), sectionSideLength));
                        subSections.Last().m_neighbours.AddRange(GenerateNeighbourIndices(
                            new Vector3Int(x, y, z), subSectionsCount));
                    }
                }
            }

            for (int massIter = 0; massIter < m_masses.Count; massIter++)
            {
                for (int sectionIter = 0; sectionIter < subSections.Count; sectionIter++)
                {
                    if (subSections[sectionIter].WithinSubSection(m_masses[massIter].GetPostion()))
                    {
                        subSections[sectionIter].m_masses.Add(massIter);
                    }
                }
            }

            for (int sectionIter = 0; sectionIter < subSections.Count; sectionIter++)
            {
                for (int massIter = 0; massIter < subSections[sectionIter].m_masses.Count; massIter++)
                {
                    subSections[sectionIter].m_masses.AddRange(subSections[sectionIter].m_masses);

                    for (int neighbourIter = 0; neighbourIter < subSections[sectionIter].m_neighbours.Count; neighbourIter++)
                    {
                        int neighbourIndex = subSections[sectionIter].m_neighbours[neighbourIter];

                        for (int neighbourMassIter = 0; neighbourMassIter < subSections[neighbourIndex].m_masses.Count; neighbourMassIter++)
                        {
                            int massA = subSections[sectionIter].m_masses[massIter];
                            int massB = subSections[neighbourIndex].m_masses[neighbourMassIter];

                            if (!m_masses[massA].neighbours.Contains(massB))
                            {
                                if (Vector3.Distance(m_masses[massA].GetPostion(),
                                    m_masses[massB].GetPostion()) <= m_settings.m_neighbourDistance)
                                {
                                    m_masses[massA].AddNeighbour(massA);
                                    m_masses[massB].AddNeighbour(massA);
                                }
                            }
                        }
                    }
                }
            }
        }

        private List<int> GenerateNeighbourIndices(Vector3Int _center, Vector3 _sectionsCount)
        {
            List<int> neighbours = new List<int>();

            for (int x = -1; x < 2; x++)
            {
                int sectionX = _center.x + x;
                if ((sectionX >= 0) && (sectionX < _sectionsCount.x))
                {
                    for (int y = -1; y < 2; y++)
                    {
                        int sectionY = _center.y + y;
                        if ((sectionY >= 0) && (sectionY < _sectionsCount.y))
                        {
                            for (int z = -1; z < 2; z++)
                            {
                                int sectionZ = _center.z + z;
                                if ((sectionZ >= 0) && (sectionZ < _sectionsCount.z))
                                {
                                    int stackLength = Mathf.CeilToInt(_sectionsCount.y * _sectionsCount.z);
                                    neighbours.Add((sectionX * stackLength) + Mathf.CeilToInt(sectionY * _sectionsCount.z) + sectionZ);
                                }
                            }
                        }
                    }
                }
            }

            return neighbours;
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

        private void GenerateExternals()
        {
            int startingMass = m_masses.Count;
            List<Mass> externalMasses = new List<Mass>();
            for (int massIter = 0; massIter < m_masses.Count; massIter++)
            {
                Mass mass = new Mass(this, m_masses[massIter].GetPostion() + (Vector3.one * m_settings.m_internalOffset),
                    Vector3.one, massIter + m_masses.Count, m_masses[massIter].m_fixed);
                mass.neighbours.AddRange(m_masses[massIter].neighbours);
                m_masses[massIter].neighbours.ForEach(neighbour => mass.neighbours.Add(neighbour + m_masses.Count));
                externalMasses.Add(mass);
            }
            m_masses.AddRange(externalMasses);

            for (int massIter = startingMass; massIter < m_masses.Count; massIter++)
            {
                for (int neighbourIter = 0; neighbourIter < m_masses[massIter].neighbours.Count; neighbourIter++)
                {
                    CreateSpring(massIter, m_masses[massIter].neighbours[neighbourIter]);
                }
            }
        }

        private void CheckMaxFixedPosition(Vector3 _position)
        {
            switch (m_settings.m_fixedDirection)
            {
                case FixDirection.Top:
                    {
                        if (_position.y > m_furthestFixedPosition.y)
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
            switch (m_settings.m_fixedDirection)
            {
                case FixDirection.Top:
                    {
                        if (_position.y > m_furthestFixedPosition.y
                            - (1.0f / m_settings.m_fixedOffset))
                        {
                            return true;
                        }
                        break;
                    }
                case FixDirection.Bottom:
                    {
                        if (_position.y < m_furthestFixedPosition.y
                            + (1.0f / m_settings.m_fixedOffset))
                        {
                            return true;
                        }
                        break;
                    }
                case FixDirection.Right:
                    {
                        if (_position.x > m_furthestFixedPosition.x
                            - (1.0f / m_settings.m_fixedOffset))
                        {
                            return true;
                        }
                        break;
                    }
                case FixDirection.Left:
                    {
                        if (_position.x < m_furthestFixedPosition.x
                            + (1.0f / m_settings.m_fixedOffset))
                        {
                            return true;
                        }
                        break;
                    }
                case FixDirection.Front:
                    {
                        if (_position.z > m_furthestFixedPosition.z
                            - (1.0f / m_settings.m_fixedOffset))
                        {
                            return true;
                        }
                        break;
                    }
                case FixDirection.Back:
                    {
                        if (_position.z < m_furthestFixedPosition.z
                            + (1.0f / m_settings.m_fixedOffset))
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
            if ((!m_masses[_massA].CheckDuplicateSpring(_massB)) && (_massA != _massB))
            {
                int springIndex = m_springs.Count;
                m_springs.Add(new Spring(_massA, _massB, GetMassDistance(_massA, _massB)));
                m_masses[_massA].AddSpring(springIndex);
                m_masses[_massB].AddSpring(springIndex);
            }
        }

        private float GetMassDistance(int _massA, int _massB)
        {
            return Vector3.Distance(m_masses[_massA].GetPostion(),
                   m_masses[_massB].GetPostion());
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
}

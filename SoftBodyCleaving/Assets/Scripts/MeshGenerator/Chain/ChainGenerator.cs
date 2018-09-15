using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChainGenerator : MonoBehaviour
{
    [SerializeField] private bool m_generateOnStart = false;
    [SerializeField] private int m_sectionCount;
    [SerializeField] private float m_lineLength;
    [SerializeField] private float m_lineWidth;
    [SerializeField] private Vector3 m_lineDirection;
    [SerializeField] private Vector3 m_lineNormal;
    [SerializeField] private float m_faceAngle;

    [SerializeField] private MeshFilter m_meshFilter;
    private Chain m_chain;

    private List<Vector3> m_corePoints = new List<Vector3>();
    private List<VertexGroup> m_vertexGroups = new List<VertexGroup>();

    private List<Vector3> m_vertices = new List<Vector3>();
    private List<Vector3> m_normals = new List<Vector3>();
    private List<int> m_triangles = new List<int>();

    [SerializeField] private bool m_backfaceCulling = false;
    [SerializeField] private bool m_generateSoftBody = false;

    void Start()
    {
        if (m_generateOnStart)
        {
            transform.position += Vector3.one;
            GenerateCorePoints();
            GenerateMesh();
            transform.position -= Vector3.one;

            GenerateChain();

            if (m_generateSoftBody)
            {
                MSM.MSM.MakeObjectSoftbody1D(gameObject, m_chain);
            }
        }
    }

    private void GenerateCorePoints()
    {
        Vector3 pointDisplacement = m_lineDirection.normalized * (m_lineLength / m_sectionCount);

        for (int sectionIter = 0; sectionIter < m_sectionCount + 1; sectionIter++)
        {
            m_corePoints.Add(pointDisplacement * sectionIter);
        }
    }

    private void GenerateMesh()
    {
        if (m_corePoints.Count > 1)
        {
            Vector3 outDir = Vector3.Cross(m_lineNormal, m_lineDirection);
            outDir.Normalize();

            Mesh mesh = new Mesh();

            for (int coreIter = 0; coreIter < m_corePoints.Count; coreIter++)
            {
                float halfWidth = m_lineWidth / 2;

                m_vertexGroups.Add(new VertexGroup());
                m_vertexGroups[coreIter].SetAveragePosition(m_corePoints[coreIter]);

                m_vertices.Add(m_corePoints[coreIter] + (outDir * halfWidth));
                m_vertexGroups[coreIter].m_vertices.Add(m_vertices.Count - 1);
                m_normals.Add(Vector3.one);

                m_vertices.Add(m_corePoints[coreIter] + (outDir * -halfWidth));
                m_vertexGroups[coreIter].m_vertices.Add(m_vertices.Count - 1);
                m_normals.Add(Vector3.one);

            }

            for (int coreIter = 0; coreIter < m_corePoints.Count - 1; coreIter++)
            {
                int vertexStart = coreIter * 2;

                m_triangles.Add(vertexStart + 1);
                m_triangles.Add(vertexStart);
                m_triangles.Add(vertexStart + 2);

                //m_normals.Add(Vector3.Cross((m_vertices[vertexStart] - m_vertices[vertexStart + 1]),
                //    (m_vertices[vertexStart] - m_vertices[vertexStart + 2])));
                //m_vertexGroups[coreIter].AddToSharedNormal(m_normals.Last());

                m_triangles.Add(vertexStart + 1);
                m_triangles.Add(vertexStart + 2);
                m_triangles.Add(vertexStart + 3);

                //m_normals.Add(Vector3.Cross((m_vertices[vertexStart + 1] - m_vertices[vertexStart]),
                //    (m_vertices[vertexStart + 1] - m_vertices[vertexStart + 3])));
                //m_vertexGroups[coreIter + 1].AddToSharedNormal(m_normals.Last());
            }

            if (!m_backfaceCulling)
            {
                GenerateBackFace();
            }

            mesh.vertices = m_vertices.ToArray();
            mesh.triangles = m_triangles.ToArray();
            mesh.normals = m_normals.ToArray();
            mesh.RecalculateNormals();

            m_meshFilter.sharedMesh = mesh;
        }
    }

    void GenerateBackFace()
    {
        int firstVertex = m_vertices.Count;

        for (int coreIter = 0; coreIter < m_corePoints.Count; coreIter++)
        {
            m_vertices.Add(m_vertices[(coreIter * 2)]);
            m_vertexGroups[coreIter].m_vertices.Add(m_vertices.Count - 1);
            m_normals.Add(Vector3.one);

            m_vertices.Add(m_vertices[(coreIter * 2) + 1]);
            m_vertexGroups[coreIter].m_vertices.Add(m_vertices.Count - 1);
            m_normals.Add(Vector3.one);

        }

        GenerateAntiClockWiseTriangles(firstVertex);
    }

    void GenerateAntiClockWiseTriangles(int _firstVertex)
    {
        for (int coreIter = 0; coreIter < m_corePoints.Count - 1; coreIter++)
        {
            int vertexStart = (coreIter * 2) + _firstVertex;

            m_triangles.Add(vertexStart);
            m_triangles.Add(vertexStart + 1);
            m_triangles.Add(vertexStart + 2);
            
            m_triangles.Add(vertexStart + 2);
            m_triangles.Add(vertexStart + 1);
            m_triangles.Add(vertexStart + 3);
        }
    }

    public Vector3 GetEndPoint()
    {
        return transform.position + (m_lineDirection.normalized * m_lineLength);
    }

    private void GenerateChain()
    {
        m_chain = gameObject.AddComponent<Chain>();
        m_chain.Initialise(m_corePoints, m_vertexGroups, m_meshFilter.sharedMesh);
        ChainPool.s_instance.AddToPool(m_chain);
    }
}
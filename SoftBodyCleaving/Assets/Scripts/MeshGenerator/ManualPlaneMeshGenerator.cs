using MSM;
using System.Collections.Generic;
using UnityEngine;

public class ManualPlaneMeshGenerator : MonoBehaviour
{
    private Mesh m_mesh;
    public MeshFilter m_meshFilter;

    public bool m_highlightVertices = false;
    public GameObject m_highlightObject;

    private List<Vector3> m_vertices = new List<Vector3>();
    private List<Vector2> m_uvs = new List<Vector2>();
    private List<Color> m_colours = new List<Color>();
    private List<int> m_triangles = new List<int>();

    public Color m_colour;
    public Vector3Int m_sections;
    public Vector3Int m_dimensions;
    private Vector2 m_UVDelta;

    public bool m_useWorldSpace = true;
    public bool m_makeSoftBody = false;
    public bool m_backfaceCulling = false;
    public float m_backFaceOffset = 0.01f;

    [SerializeField] private SoftBodySettings m_settings = new SoftBodySettings();

    private void Awake()
    {
        CreateMesh();
    }

    private void CreateMesh()
    {
        m_mesh = new Mesh();
        m_mesh.MarkDynamic();

        m_meshFilter.mesh = m_mesh;

        GenerateVertices();
        UpdateMesh();

        if(m_makeSoftBody)
        {
            MSM.MSM.MakeObjectSoftbodyObject(gameObject, m_settings);
        }
    }

    void GenerateVertices()
    {
        m_sections.z = m_sections.x * m_sections.y;
        m_UVDelta = new Vector2(1.0f / m_sections.x, 1.0f / m_sections.y);

        for (int x = 0; x <= m_sections.x; x++)
        {
            GenerateColumn(x);
        }

        GenerateClockWiseTriangles();

        if (!m_backfaceCulling)
        {
            GenerateBackFace();
        }

        if (m_useWorldSpace)
        {
            AdjustToWorldSpace();
        }

        if (m_highlightVertices)
        {
            HighlightVertex();
        }
    }

    private void GenerateColumn(float _X)
    {
        float startX = _X * m_dimensions.x;
        float startY = 0.0f;

        Vector3 position = new Vector3(startX, startY, 0.0f);
        for (int yIter = 0; yIter <= m_sections.y; yIter++)
        {
            position.y = yIter * m_dimensions.y;
            AddVertex(position);
            m_uvs.Add(new Vector2(m_UVDelta.x * _X, m_UVDelta.y * yIter));
        }
    }

    void AddVertex(Vector3 _position)
    {
        if (!m_vertices.Contains(_position))
        {
            m_vertices.Add(_position);
            m_colours.Add(m_colour);
        }
    }

    void GenerateClockWiseTriangles()
    {
        int startIndex, vertexA, vertexB, vertexC, vertexD = 0;

        for (int xIter = 0; xIter < m_sections.x; xIter++)
        {
            startIndex = (xIter * m_sections.y) + xIter;
            vertexA = startIndex;
            vertexB = startIndex + 1;
            vertexC = startIndex + (m_sections.y + 1);
            vertexD = startIndex + (m_sections.y + 2);

            for (int yIter = 0; yIter < m_sections.y; yIter++)
            {
                m_triangles.Add(vertexA);
                m_triangles.Add(vertexB);
                m_triangles.Add(vertexC);
                m_triangles.Add(vertexC);
                m_triangles.Add(vertexB);
                m_triangles.Add(vertexD);

                vertexA++;
                vertexB++;
                vertexC++;
                vertexD++;
            }
        }
    }

    void GenerateBackFace()
    {
        int firstVertex = m_vertices.Count;

        Vector3 U = m_vertices[1] - m_vertices[0];
        Vector3 V = m_vertices[2] - m_vertices[0];
        Vector3 planeNormal = Vector3.Cross(U, V);
        Vector3 backFaceOffset = planeNormal * m_backFaceOffset;

        List<Vector3> backVertices = new List<Vector3>();
        for (int vertexIter = 0; vertexIter < m_vertices.Count; vertexIter++)
        {
            backVertices.Add(m_vertices[vertexIter] - backFaceOffset);
            m_colours.Add(m_colour);
        }
        m_vertices.AddRange(backVertices);

        for (int X = 0; X <= m_sections.x; X++)
        {
            for (int Y = 0; Y <= m_sections.y; Y++)
            {
                m_uvs.Add(new Vector2(m_UVDelta.x * X, m_UVDelta.y * Y));
            }
        }

        GenerateBackFaceTriangles(firstVertex);
    }

    void GenerateBackFaceTriangles(int _firstVertex)
    {
        int startIndex, vertexA, vertexB, vertexC, vertexD = 0;

        for (int xIter = 0; xIter < m_sections.x; xIter++)
        {
            startIndex = (xIter * m_sections.y) + xIter + _firstVertex;
            vertexA = startIndex;
            vertexB = startIndex + 1;
            vertexC = startIndex + (m_sections.y + 1);
            vertexD = startIndex + (m_sections.y + 2);

            for (int yIter = 0; yIter < m_sections.y; yIter++)
            {
                m_triangles.Add(vertexD);
                m_triangles.Add(vertexB);
                m_triangles.Add(vertexC);
                m_triangles.Add(vertexC);
                m_triangles.Add(vertexB);
                m_triangles.Add(vertexA);

                vertexA++;
                vertexB++;
                vertexC++;
                vertexD++;
            }
        }
    }

    void AdjustToWorldSpace()
    {
        Vector3 objectPosition = transform.position;

        for (int iter = 0; iter <= m_vertices.Count - 1; iter++)
        {
            m_vertices[iter] = m_vertices[iter] - objectPosition;
        }
    }

    void UpdateMesh()
    {
        m_mesh.Clear();
        m_mesh.vertices = m_vertices.ToArray();
        m_mesh.triangles = m_triangles.ToArray();
        m_mesh.uv = m_uvs.ToArray();
        m_mesh.colors = m_colours.ToArray();
        m_mesh.RecalculateNormals();
    }

    private void HighlightVertex()
    {
        for (int vertexIter = 0; vertexIter < m_vertices.Count; vertexIter++)
        {
            Instantiate(m_highlightObject, m_vertices[vertexIter], Quaternion.identity, transform);
        }
    }
}

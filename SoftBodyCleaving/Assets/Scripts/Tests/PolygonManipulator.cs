using System.Linq;
using System.Collections;
using UnityEngine;

public class PolygonManipulator : MonoBehaviour
{
    public bool m_reset = true;
    [SerializeField] private MeshFilter m_mesh;
    public int m_currentPolygonCount;
    private int[] m_indices;
    private int m_maxPolygonCount;

    void Update ()
    {
        if (m_mesh)
        {
            if (m_reset)
            {
                m_indices = m_mesh.sharedMesh.GetIndices(0);
                m_maxPolygonCount = m_indices.Length / 3;
                m_currentPolygonCount = m_maxPolygonCount;
                m_reset = false;
            }
            else
            {
                m_currentPolygonCount = (m_currentPolygonCount < 0) ? 0 : 
                    (m_currentPolygonCount > m_maxPolygonCount) ? m_maxPolygonCount :
                    m_currentPolygonCount;

                int[] currentIndices = m_indices.Take(m_currentPolygonCount * 3).ToArray();
                m_mesh.sharedMesh.SetIndices(currentIndices, MeshTopology.Triangles, 0);
            }
        }
	}
}

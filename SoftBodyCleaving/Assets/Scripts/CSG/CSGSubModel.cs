using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CSG
{
    public class CSGSubModel
    {
        public CSGModel m_parent;
        public List<int> m_vertices = new List<int>();
        public List<int> m_indices = new List<int>();

        public CSGSubModel(CSGModel _parent)
        {
            m_parent = _parent;
        }

        public CSGSubModel MergeModels(CSGSubModel _modelIn)
        {
            CSGSubModel result = new CSGSubModel(m_parent);

            if (m_vertices.Count > _modelIn.m_vertices.Count)
            {
                result.m_vertices.AddRange(m_vertices);

                for (int vertexCount = 0; vertexCount < _modelIn.m_vertices.Count; vertexCount++)
                {
                    if (!result.m_vertices.Contains(_modelIn.m_vertices[vertexCount]))
                    {
                        result.m_vertices.Add(_modelIn.m_vertices[vertexCount]);
                    }
                }
            }
            else
            {
                result.m_vertices.AddRange(_modelIn.m_vertices);

                for (int vertexCount = 0; vertexCount < m_vertices.Count; vertexCount++)
                {
                    if (!result.m_vertices.Contains(m_vertices[vertexCount]))
                    {
                        result.m_vertices.Add(m_vertices[vertexCount]);
                    }
                }
            }

            result.m_indices.AddRange(m_indices);
            result.m_indices.AddRange(_modelIn.m_indices);

            return result;
        }

        public Mesh ToMesh()
        {
            Mesh mesh = new Mesh();
            
            Vector3[] vertices = new Vector3[m_vertices.Count];
            Vector3[] normals = new Vector3[m_vertices.Count];
            Vector2[] uvs = new Vector2[m_vertices.Count];
            Color[] colours = new Color[m_vertices.Count];

            for (int vertexIter = 0; vertexIter < m_vertices.Count; vertexIter++)
            {
                vertices[vertexIter] = m_parent.m_vertices[m_vertices[vertexIter]].m_position;
                normals[vertexIter] = m_parent.m_vertices[m_vertices[vertexIter]].m_normal;
                uvs[vertexIter] = m_parent.m_vertices[m_vertices[vertexIter]].m_uv;
                colours[vertexIter] = m_parent.m_vertices[m_vertices[vertexIter]].m_color;
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.colors = colours;
            mesh.uv = uvs;

            for (int indexIter = 0; indexIter < m_indices.Count; indexIter++)
            {
                m_indices[indexIter] = m_vertices.FindIndex(x => x == m_indices[indexIter]);
            }
            mesh.triangles = m_indices.ToArray();

            return mesh;
        }
    }
}

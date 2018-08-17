using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Parabox.CSG
{
    /**
	 * Representation of a mesh in CSG terms.  Contains methods for translating to and from UnityEngine.Mesh.
	 */
    public class CSG_Model
    {
        public List<CSG_Vertex> m_vertices;
        public List<int> m_indices;
        public List<CSGSubModel> m_subModels = new List<CSGSubModel>();

        public CSG_Model()
        {
            this.m_vertices = new List<CSG_Vertex>();
            this.m_indices = new List<int>();
        }

        /**
		 * Initialize a CSG_Model with the mesh of a gameObject.
		 */
        public CSG_Model(GameObject go)
        {
            m_vertices = new List<CSG_Vertex>();

            Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
            Transform trans = go.GetComponent<Transform>();

            int vertexCount = m.vertexCount;

            Vector3[] v = m.vertices;
            Vector3[] n = m.normals;
            Vector2[] u = m.uv;
            Color[] c = m.colors;

            for (int i = 0; i < v.Length; i++)
                m_vertices.Add(new CSG_Vertex(trans.TransformPoint(v[i]), trans.TransformDirection(n[i]), u == null ? Vector2.zero : u[i], c == null || c.Length != vertexCount ? Color.white : c[i]));

            m_indices = new List<int>(m.triangles);
        }

        public CSG_Model(List<CSG_Polygon> list)
        {
            this.m_vertices = new List<CSG_Vertex>();
            this.m_indices = new List<int>();

            int p = 0;
            for (int i = 0; i < list.Count; i++)
            {
                CSG_Polygon poly = list[i];

                for (int j = 2; j < poly.vertices.Count; j++)
                {
                    this.m_vertices.Add(poly.vertices[0]);
                    this.m_indices.Add(p++);

                    this.m_vertices.Add(poly.vertices[j - 1]);
                    this.m_indices.Add(p++);

                    this.m_vertices.Add(poly.vertices[j]);
                    this.m_indices.Add(p++);
                }
            }
        }

        public CSG_Model(List<CSG_Vertex> _vertices, List<int> _indices)
        {
            m_vertices = new List<CSG_Vertex>(_vertices);    
            m_indices = new List<int>(_indices);
        }

        public List<CSG_Polygon> ToPolygons()
        {
            List<CSG_Polygon> list = new List<CSG_Polygon>();

            for (int i = 0; i < m_indices.Count; i += 3)
            {
                List<CSG_Vertex> triangle = new List<CSG_Vertex>()
                {
                    m_vertices[m_indices[i+0]],
                    m_vertices[m_indices[i+1]],
                    m_vertices[m_indices[i+2]]
                };

                list.Add(new CSG_Polygon(triangle));
            }

            return list;
        }

        /**
		 * Converts a CSG_Model to a Unity mesh.
		 */
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
                v[i] = this.m_vertices[i].m_position;
                n[i] = this.m_vertices[i].m_normal;
                u[i] = this.m_vertices[i].m_uv;
                c[i] = this.m_vertices[i].m_color;
            }

            m.vertices = v;
            m.normals = n;
            m.colors = c;
            m.uv = u;
            m.triangles = this.m_indices.ToArray();

            return m;
        }

        public bool SubDivideMesh()
        {
            RemoveOverlappingVertices();

            Dictionary<int, CSGSubModel> vertexCheckList = new Dictionary<int, CSGSubModel>();

            //Setup check list for each vertex
            for (int vertexIter = 0; vertexIter < m_vertices.Count; vertexIter++)
            {
                vertexCheckList.Add(vertexIter, null);
            }

            List<CSGSubModel> builderSubModels = new List<CSGSubModel>();

            for (int polygonIter = 0; polygonIter < m_indices.Count / 3; polygonIter++)
            {
                int indexAIter = polygonIter * 3;
                int indexBIter = indexAIter + 1;
                int indexCIter = indexBIter + 1;

                List<CSGSubModel> activeSubModels = new List<CSGSubModel>();

                bool vertexAAssigned = (vertexCheckList[m_indices[indexAIter]] != null);
                if (vertexAAssigned)
                {
                    activeSubModels.Add(vertexCheckList[m_indices[indexAIter]]);
                }

                bool vertexBAssigned = (vertexCheckList[m_indices[indexBIter]] != null);
                if (vertexBAssigned)
                {
                    if (!activeSubModels.Contains(vertexCheckList[m_indices[indexBIter]]))
                    {
                        activeSubModels.Add(vertexCheckList[m_indices[indexBIter]]);
                    }
                }

                bool vertexCAssigned = (vertexCheckList[m_indices[indexCIter]] != null);
                if (vertexCAssigned)
                {
                    if (!activeSubModels.Contains(vertexCheckList[m_indices[indexBIter]]))
                    {
                        activeSubModels.Add(vertexCheckList[m_indices[indexCIter]]);
                    }
                }

                if (activeSubModels.Count == 0)
                {
                    CSGSubModel currentSubModel = new CSGSubModel(this);

                    currentSubModel.m_vertices.Add(m_indices[indexAIter]);
                    currentSubModel.m_vertices.Add(m_indices[indexBIter]);
                    currentSubModel.m_vertices.Add(m_indices[indexCIter]);

                    currentSubModel.m_indices.Add(m_indices[indexAIter]);
                    currentSubModel.m_indices.Add(m_indices[indexBIter]);
                    currentSubModel.m_indices.Add(m_indices[indexCIter]);

                    builderSubModels.Add(currentSubModel);

                    vertexCheckList[m_indices[indexAIter]] = currentSubModel;
                    vertexCheckList[m_indices[indexBIter]] = currentSubModel;
                    vertexCheckList[m_indices[indexCIter]] = currentSubModel;
                }
                else if (activeSubModels.Count > 1)
                {
                    do
                    {
                        CSGSubModel result = activeSubModels[0].MergeModels(activeSubModels[1]);
                        for (int vertexIter = 0; vertexIter < result.m_vertices.Count; vertexIter++)
                        {
                            vertexCheckList[result.m_vertices[vertexIter]] = result;
                        }

                        builderSubModels.Remove(activeSubModels[0]);
                        builderSubModels.Remove(activeSubModels[1]);

                        activeSubModels.RemoveAt(1);
                        activeSubModels.RemoveAt(0);

                        activeSubModels.Add(result);

                    } while (activeSubModels.Count > 1);

                    //Adds indices
                    activeSubModels[0].m_indices.Add(m_indices[indexAIter]);
                    activeSubModels[0].m_indices.Add(m_indices[indexBIter]);
                    activeSubModels[0].m_indices.Add(m_indices[indexCIter]);

                    builderSubModels.Add(activeSubModels[0]);
                    activeSubModels.Clear();
                }
                else
                {
                    //Add vertices if not already added
                    if (!activeSubModels[0].m_vertices.Contains(m_indices[indexAIter]))
                    {
                        activeSubModels[0].m_vertices.Add(m_indices[indexAIter]);
                    }

                    if (!activeSubModels[0].m_vertices.Contains(m_indices[indexBIter]))
                    {
                        activeSubModels[0].m_vertices.Add(m_indices[indexBIter]);
                    }

                    if (!activeSubModels[0].m_vertices.Contains(m_indices[indexCIter]))
                    {
                        activeSubModels[0].m_vertices.Add(m_indices[indexCIter]);
                    }

                    //Adds indices
                    activeSubModels[0].m_indices.Add(m_indices[indexAIter]);
                    activeSubModels[0].m_indices.Add(m_indices[indexBIter]);
                    activeSubModels[0].m_indices.Add(m_indices[indexCIter]);

                    //Set vertex check list
                    if (vertexAAssigned)
                    {
                        vertexCheckList[m_indices[indexBIter]] = activeSubModels[0];
                        vertexCheckList[m_indices[indexCIter]] = activeSubModels[0];
                    }
                    else if (vertexBAssigned)
                    {
                        vertexCheckList[m_indices[indexAIter]] = activeSubModels[0];
                        vertexCheckList[m_indices[indexCIter]] = activeSubModels[0];
                    }
                    else
                    {
                        vertexCheckList[m_indices[indexAIter]] = activeSubModels[0];
                        vertexCheckList[m_indices[indexBIter]] = activeSubModels[0];
                    }
                }
            }

            if(builderSubModels.Count > 1)
            {
                m_subModels.AddRange(builderSubModels);
                return true;
            }

            m_subModels.Clear();
            return false;
        }

        public void RemoveOverlappingVertices()
        {
            List<CSG_Vertex> vertices = new List<CSG_Vertex>();
            List<int> indices = new List<int>();

            //Position -> new Index
            Dictionary<Vector3, int> vertexPositonCheckList = new Dictionary<Vector3, int>();

            int existingVertex = -1;
            for (int indexIter = 0; indexIter < m_indices.Count; indexIter++)
            {
                if (!vertexPositonCheckList.TryGetValue(m_vertices[m_indices[indexIter]].m_position, out existingVertex))
                {
                    vertexPositonCheckList.Add(m_vertices[m_indices[indexIter]].m_position, vertices.Count);
                    indices.Add(vertices.Count);
                    vertices.Add(new CSG_Vertex(m_vertices[m_indices[indexIter]].m_position,
                        m_vertices[m_indices[indexIter]].m_normal, m_vertices[m_indices[indexIter]].m_uv,
                        m_vertices[m_indices[indexIter]].m_color));
                }
                else
                {
                    vertices[existingVertex].AddToSharedNormal(m_vertices[m_indices[indexIter]].m_normal);
                    indices.Add(existingVertex);
                }

                existingVertex = -1;
            }

            m_vertices.Clear();
            m_vertices.AddRange(vertices);

            m_indices.Clear();
            m_indices.AddRange(indices);
        }

        public void TransfromVertexToLocal(Transform _meshTransform)
        {
            for (int vertexIter = 0; vertexIter < m_vertices.Count; vertexIter++)
            {
                Vector3 position = m_vertices[vertexIter].m_position - _meshTransform.position;
                position.x /= _meshTransform.localScale.x;
                position.y /= _meshTransform.localScale.y;
                position.z /= _meshTransform.localScale.z;
                m_vertices[vertexIter].SetPosition(position);
            }
        }
    }
}
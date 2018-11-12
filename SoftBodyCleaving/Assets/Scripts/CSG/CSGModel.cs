// Original CSG.JS library by Evan Wallace (http://madebyevan.com), under the MIT license.
// GitHub: https://github.com/evanw/csg.js/

using Unity.Jobs;
using Unity.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CSG
{
    public class CSGModel
    {
        public List<CSGVertex> m_vertices;
        public List<int> m_indices;
        public float m_overlapRange = 0.01f;

        public List<CSGSubModel> m_subModels = new List<CSGSubModel>();

        public CSGModel()
        {
            m_vertices = new List<CSGVertex>();
            m_indices = new List<int>();
        }

        public CSGModel(List<CSGPolygon> _polygons)
        {
            m_vertices = new List<CSGVertex>();
            m_indices = new List<int>();

            int indexCount = 0;
            for (int polygonIter = 0; polygonIter < _polygons.Count; polygonIter++)
            {
                CSGPolygon currentPolygon = _polygons[polygonIter];

                for (int vertexIter = 2; vertexIter < currentPolygon.m_vertices.Count; vertexIter++)
                {
                    m_vertices.Add(currentPolygon.m_vertices[0]);
                    m_indices.Add(indexCount++);

                    m_vertices.Add(currentPolygon.m_vertices[vertexIter - 1]);
                    m_indices.Add(indexCount++);

                    m_vertices.Add(currentPolygon.m_vertices[vertexIter]);
                    m_indices.Add(indexCount++);
               }
            }
        }
        
        public CSGModel(GameObject _object)
        {
            m_vertices = new List<CSGVertex>();

            Mesh objectMesh = _object.GetComponent<MeshFilter>().sharedMesh;
            Transform objectTransform = _object.GetComponent<Transform>();

            int vertexCount = objectMesh.vertexCount;

            Vector3[] vertices = objectMesh.vertices;
            Vector3[] normals = objectMesh.normals;
            Vector2[] uvs = objectMesh.uv;
            Color[] colours = objectMesh.colors;

            for (int i = 0; i < vertices.Length; i++)
            {
                m_vertices.Add(new CSGVertex(objectTransform.TransformPoint(vertices[i]),
                    objectTransform.TransformDirection(normals[i]), uvs == null ?
                    Vector2.zero : uvs[i], colours == null || colours.Length != vertexCount ?
                    Color.white : colours[i]));
            }

            m_indices = new List<int>(objectMesh.triangles);            
        }

        public List<CSGPolygon> ToPolygons()
        {
            List<CSGPolygon> polygons = new List<CSGPolygon>();

            for (int indexIter = 0; indexIter < m_indices.Count; indexIter += 3)
            {
                List<CSGVertex> triangles = new List<CSGVertex>();
                triangles.Add(m_vertices[m_indices[indexIter]]);
                triangles.Add(m_vertices[m_indices[indexIter + 1]]);
                triangles.Add(m_vertices[m_indices[indexIter + 2]]);

                polygons.Add(new CSGPolygon(triangles));
            }

            return polygons;
        }

        /**
		 * Converts a CSG_Model to a Unity mesh.
		 */
        public Mesh ToMesh()
        {
            Mesh mesh = new Mesh();
            
            Vector3[] vertices = new Vector3[m_vertices.Count];
            Vector3[] normals = new Vector3[m_vertices.Count];
            Vector2[] uvs = new Vector2[m_vertices.Count];
            Color[] colours = new Color[m_vertices.Count];

            for (int vertexIter = 0; vertexIter < m_vertices.Count; vertexIter++)
            {
                vertices[vertexIter] = m_vertices[vertexIter].m_position;
                normals[vertexIter] = m_vertices[vertexIter].m_normal;
                uvs[vertexIter] = m_vertices[vertexIter].m_uv;
                colours[vertexIter] = m_vertices[vertexIter].m_color;
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.colors = colours;
            mesh.uv = uvs;
            mesh.triangles = m_indices.ToArray();

            return mesh;
        }

        public bool SubDivideMesh(CSGMeshingSettings _settings)
        {
            if (_settings.m_useMeshing)
            {
                if (_settings.m_removeOverlaps)
                {
                    RemoveOverlappingVertices(_settings);
                }

                if (_settings.m_waterTightMesh)
                {
                    MakeWaterTight(_settings);
                    //MakeWaterTightJ();
                }

                if (_settings.m_subdivideModel)
                {
                    return SubDivideModel();
                }
            }
            return false;
        }

        public void RemoveOverlappingVertices(CSGMeshingSettings _settings)
        {
            List<CSGVertex> vertices = new List<CSGVertex>();
            List<int> indices = new List<int>();

            //Position -> new Index
            Dictionary<Vector3, int> vertexPositonCheckList = new Dictionary<Vector3, int>();

            int existingVertex = -1;
            for (int indexIter = 0; indexIter < m_indices.Count; indexIter++)
            {
                Vector3 currentPosition = Round(m_vertices[m_indices[indexIter]].m_position, _settings.m_overlappingRounding);
                if (!vertexPositonCheckList.TryGetValue(currentPosition, out existingVertex))
                {
                    vertexPositonCheckList.Add(currentPosition, vertices.Count);
                    indices.Add(vertices.Count);
                    vertices.Add(new CSGVertex(currentPosition, m_vertices[m_indices[indexIter]].m_normal,
                        m_vertices[m_indices[indexIter]].m_uv, m_vertices[m_indices[indexIter]].m_color));
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

        public Vector3 Round(Vector3 _vector, int _decimalPlaces = 2)
        {
            float multiplier = 1;
            for (int i = 0; i < _decimalPlaces; i++)
            {
                multiplier *= 10f;
            }
            return new Vector3(
                Mathf.Round(_vector.x * multiplier) / multiplier,
                Mathf.Round(_vector.y * multiplier) / multiplier,
                Mathf.Round(_vector.z * multiplier) / multiplier);
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

        public void MakeWaterTightJ()
        {
            List<Vector3> postions = new List<Vector3>();
            m_vertices.ForEach(x => { postions.Add(x.m_position); });

            for (int vertexIter = 0; vertexIter < m_vertices.Count; vertexIter++)
            {
                int polygonCount = m_indices.Count / 3;

                NativeArray<Vector3> orignalVertices = new NativeArray<Vector3>(postions.ToArray(), Allocator.TempJob);
                NativeArray<int> orignalIndices = new NativeArray<int>(m_indices.ToArray(), Allocator.TempJob);
                NativeArray<int> removeIndices = new NativeArray<int>(new int[m_indices.Count], Allocator.TempJob);
                NativeArray<int> addIndices = new NativeArray<int>(new int[m_indices.Count * 2], Allocator.TempJob);

                WaterTightJob job = new WaterTightJob
                {
                    m_orignalVertices = orignalVertices,
                    m_orignalIndices = orignalIndices,
                    m_vertexIter = vertexIter,
                    m_removedIndices = removeIndices,
                    m_addedIndices = addIndices
                };

                JobHandle jobHandle = job.Schedule(polygonCount, 250);

                //if(vertexIter == 0)
                //{
                //    jobHandles.Add(job.Schedule(polygonCount, 250));
                //}
                //else
                //{
                //    jobHandles.Add(job.Schedule(polygonCount, 250, jobHandles[vertexIter - 1]));
                //}

                jobHandle.Complete();                
                for (int removeIter = removeIndices.Length - 1; removeIter > -1; removeIter--)
                {
                    if(removeIndices[removeIter] == 1)
                    {
                        m_indices.RemoveAt(removeIter);
                    }
                }

                m_indices.AddRange(addIndices);


                orignalVertices.Dispose();
                orignalIndices.Dispose();
                removeIndices.Dispose();
                addIndices.Dispose();
            }

            //jobHandles.Last().Complete();

        }

        public void MakeWaterTight(CSGMeshingSettings _settings)
        {
            int count = 0;
            bool subdividedPolygon = false;

            do
            {
                count++;
                subdividedPolygon = false;

                for (int vertexIter = 0; vertexIter < m_vertices.Count; vertexIter++)
                {
                    for (int polygonIter = 0; polygonIter < m_indices.Count / 3; polygonIter++)
                    {
                        int indexA = polygonIter * 3;
                        int indexB = (polygonIter * 3) + 1;
                        int indexC = (polygonIter * 3) + 2;

                        int vertexA = m_indices[indexA];
                        int vertexB = m_indices[indexB];
                        int vertexC = m_indices[indexC];

                        List<int> indices = new List<int>();

                        indices.AddRange(SubDividePolygon(vertexIter, vertexA, vertexB, vertexC, 0));
                        if (indices.Count > 0)
                        {
                            m_indices.RemoveRange(indexA, 3);
                            m_indices.AddRange(indices);
                            subdividedPolygon = true;
                            break;
                        }

                        indices.AddRange(SubDividePolygon(vertexIter, vertexB, vertexC, vertexA, 1));
                        if (indices.Count > 0)
                        {
                            m_indices.RemoveRange(indexA, 3);
                            m_indices.AddRange(indices);
                            subdividedPolygon = true;
                            break;
                        }

                        indices.AddRange(SubDividePolygon(vertexIter, vertexC, vertexA, vertexB, 2));
                        if (indices.Count > 0)
                        {
                            m_indices.RemoveRange(indexA, 3);
                            m_indices.AddRange(indices);
                            subdividedPolygon = true;
                            break;
                        }
                    }
                }
            } while (_settings.m_useInterpolation ? 
                count < _settings.m_proofingInterpolation : subdividedPolygon);
        }

        private bool PointOnLine(Vector3 _point, Vector3 _lineStart, Vector3 _lineEnd)
        {
            float lineDistance = Vector3.Distance(_lineStart, _point) + Vector3.Distance(_lineEnd, _point);
            if (lineDistance >= Vector3.Distance(_lineStart, _lineEnd) - m_overlapRange &&
                lineDistance <= Vector3.Distance(_lineStart, _lineEnd) + m_overlapRange)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// EdgeIndex indicates which edge on the triangle being checked (0 - 2)
        /// </summary>
        /// <param name="_splitVertex"></param>
        /// <param name="_beginVertex"></param>
        /// <param name="_endVertex"></param>
        /// <param name="_oppositeVertex"></param>
        /// <param name="_edgeIndex"></param>
        /// <returns></returns>
        private List<int> SubDividePolygon(int _splitVertex, int _beginVertex, int _endVertex, int _oppositeVertex, int _edgeIndex)
        {
            List<int> indices = new List<int>();

            if ((_splitVertex != _beginVertex) && (_splitVertex != _endVertex) && (_splitVertex != _oppositeVertex))
            {
                if (PointOnLine(m_vertices[_splitVertex].m_position, m_vertices[_beginVertex].m_position,
                    m_vertices[_endVertex].m_position))
                {
                    switch(_edgeIndex)
                    {
                        case 0:
                            {
                                indices.Add(_beginVertex);
                                indices.Add(_splitVertex);
                                indices.Add(_oppositeVertex);

                                indices.Add(_splitVertex);
                                indices.Add(_endVertex);
                                indices.Add(_oppositeVertex);
                                break;
                            }
                        case 1:
                            {
                                indices.Add(_oppositeVertex);
                                indices.Add(_splitVertex);
                                indices.Add(_endVertex);

                                indices.Add(_oppositeVertex);
                                indices.Add(_beginVertex);
                                indices.Add(_splitVertex);
                                break;
                            }
                        case 2:
                            {
                                indices.Add(_oppositeVertex);
                                indices.Add(_beginVertex);
                                indices.Add(_splitVertex);

                                indices.Add(_endVertex);
                                indices.Add(_oppositeVertex);
                                indices.Add(_splitVertex);
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }

                    return indices;
                }
            }

            return indices;
        }

        private bool SubDivideModel()
        {
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

            if (builderSubModels.Count > 1)
            {
                m_subModels.AddRange(builderSubModels);
                return true;
            }

            m_subModels.Clear();
            return false;
        }
    }
}
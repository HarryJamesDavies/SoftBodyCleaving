using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public struct WaterTightJob : IJobParallelFor
{
    public NativeArray<Vector3> m_orignalVertices;
    public NativeArray<int> m_orignalIndices;
    public int m_vertexIter;

    public NativeArray<int> m_removedIndices;
    public NativeArray<int> m_addedIndices;

    public void Execute(int _index)
    {
        int indexA = _index * 3;
        int indexB = (_index * 3) + 1;
        int indexC = (_index * 3) + 2;

        int vertexA = m_orignalIndices[indexA];
        int vertexB = m_orignalIndices[indexB];
        int vertexC = m_orignalIndices[indexC];

        int addedIndicesSI = _index * 6;

        List<int> indices = new List<int>();

        indices.AddRange(SubDividePolygon(m_vertexIter, vertexA, vertexB, vertexC, 0));
        if (indices.Count > 0)
        {
            UpdateIndices(indices, indexA, addedIndicesSI);
            return;
        }

        indices.AddRange(SubDividePolygon(m_vertexIter, vertexB, vertexC, vertexA, 1));
        if (indices.Count > 0)
        {
            UpdateIndices(indices, indexA, addedIndicesSI);
            return;
        }

        indices.AddRange(SubDividePolygon(m_vertexIter, vertexC, vertexA, vertexB, 2));
        if (indices.Count > 0)
        {
            UpdateIndices(indices, indexA, addedIndicesSI);
            return;
        }
    }

    private void UpdateIndices(List<int> _indices, int _indexA, int _addedIndicesSI)
    {
        for (int removeIter = 0; removeIter < 3; removeIter++)
        {
            m_removedIndices[_indexA + removeIter] = 1;
        }

        for (int addIter = 0; addIter < 6; addIter++)
        {
            m_addedIndices[_addedIndicesSI + addIter] = _indices[addIter];
        }
    }

    private bool PointOnLine(Vector3 _point, Vector3 _lineStart, Vector3 _lineEnd)
    {
        if (Vector3.Distance(_lineStart, _point) + Vector3.Distance(_lineEnd, _point)
            == Vector3.Distance(_lineStart, _lineEnd))
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
            if (PointOnLine(m_orignalVertices[_splitVertex], m_orignalVertices[_beginVertex],
                m_orignalVertices[_endVertex]))
            {
                switch (_edgeIndex)
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
}

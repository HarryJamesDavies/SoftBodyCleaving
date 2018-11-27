using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

namespace MSM
{
    public struct MSMMeshJob : IJob
    {
        public NativeArray<Vector3> m_massPositions;
        public NativeArray<int> m_massNeighbours;
        public NativeArray<int> m_massNeighboursCounts;

        public int m_massIndex;
        public float m_neighbourDistance;

        public void Execute()
        {
            m_massNeighboursCounts[m_massIndex] = 0;

            for (int massIter = 0; massIter < m_massPositions.Length; massIter++)
            {
                if (massIter != m_massIndex)
                {
                    if (Vector3.Distance(m_massPositions[m_massIndex], m_massPositions[massIter]) <= m_neighbourDistance)
                    {
                        m_massNeighbours[m_massNeighboursCounts[m_massIndex]] = massIter;
                        m_massNeighboursCounts[m_massIndex]++;
                    }
                }
            }
        }
    }
}

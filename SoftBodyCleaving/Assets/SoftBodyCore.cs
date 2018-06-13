using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoftBodyCore : MonoBehaviour
{
    //public ComputeShader m_gpuSoftBody;
    public SoftBodyMesh m_softBody;
    public Mesh m_mesh;
    public MeshFilter m_meshFilter;

    //public List<float> m_resultantForces = new List<float>();
    //private ComputeBuffer m_forceBuffer;

    //public int m_clearKernel;
    //public int m_calculateKernel;

    //private void Start()
    //{
    //    for (int forceIter = 0; forceIter < m_softBody.m_masses.Count; forceIter++)
    //    {
    //        m_resultantForces.Add(0.0f);
    //    }
    //    m_forceBuffer = new ComputeBuffer(m_resultantForces.Count, 4);
    //    m_forceBuffer.SetData(m_resultantForces);
    //    //m_gpuSoftBody.SetBuffer(m_calculateKernel)

    //    m_clearKernel = m_gpuSoftBody.FindKernel("ClearForces");
    //    m_calculateKernel = m_gpuSoftBody.FindKernel("CalculateForces");
    //    m_gpuSoftBody.Dispatch(m_clearKernel, m_softBody.m_masses.Count / 5, 1, 1);
    //}       

    //private void SetupCalculationStep()
    //{

    //}
}

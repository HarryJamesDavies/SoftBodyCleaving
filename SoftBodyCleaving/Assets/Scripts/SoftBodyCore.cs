using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoftBodyCore : MonoBehaviour
{
    public struct GPUSpring
    {
        public uint massA;
        public uint massB;
        public float equilibriumDistance;
    }

    struct GPUMass
    {
        public Vector3 position;
        public Vector3 velocity;
        public int resultantForceX;
        public int resultantForceY;
        public int resultantForceZ;
    }

    private const int THREADSPERPASS = 5;

    public ComputeShader m_gpuSoftBody;
    public SoftBodyMesh m_softBody;

    private ComputeBuffer m_massBuffer;
    private ComputeBuffer m_springBuffer;

    private GPUMass[] m_masses;
    private GPUSpring[] m_springs;

    private int m_clearKernel;
    private int m_calculateKernel;
    private int m_displacementKernel;

    private bool m_initialised = false;
    private bool m_enabled = true;

    private void OnDestroy()
    {
        m_massBuffer.Release();
        
        m_springBuffer.Release();
    }

    public void OnMeshGenerated(SoftBodyMesh _mesh)
    {
        m_softBody = _mesh;

        if (m_softBody)
        {
            GenerateGPUData();
            SetupShader();
            m_initialised = true;
        }
        else
        {
            m_initialised = false;
        }
    }

    private void GenerateGPUData()
    {
        m_masses = new GPUMass[m_softBody.m_masses.Count + m_softBody.m_masses.Count % THREADSPERPASS];
        for (int massIter = 0; massIter < m_softBody.m_masses.Count; massIter++)
        {
            m_masses[massIter] = new GPUMass();
            m_masses[massIter].position = m_softBody.transform.TransformPoint(
                m_softBody.GetVertex(m_softBody.m_masses[massIter].vertexGroup.m_vertices[0]));
            m_masses[massIter].velocity = Vector3.zero;
            m_masses[massIter].resultantForceX = 0;
            m_masses[massIter].resultantForceY = 0;
            m_masses[massIter].resultantForceZ = 0;
        }

        for (int massPadding = m_softBody.m_masses.Count; massPadding < m_softBody.m_masses.Count % THREADSPERPASS; massPadding++)
        {
            m_masses[massPadding] = new GPUMass();
            m_masses[massPadding].position = Vector3.zero;
            m_masses[massPadding].velocity = Vector3.zero;
            m_masses[massPadding].resultantForceX = 0;
            m_masses[massPadding].resultantForceY = 0;
            m_masses[massPadding].resultantForceZ = 0;
        }

        m_springs = new GPUSpring[m_softBody.m_springs.Count + m_softBody.m_springs.Count % THREADSPERPASS];
        for (int springIter = 0; springIter < m_softBody.m_springs.Count; springIter++)
        {
            m_springs[springIter] = new GPUSpring();
            m_springs[springIter].massA = (uint)m_softBody.m_springs[springIter].massA;
            m_springs[springIter].massB = (uint)m_softBody.m_springs[springIter].massB;
            m_springs[springIter].equilibriumDistance = m_softBody.m_springs[springIter].equilibriumDistance;
        }

        for (int springPadding = m_softBody.m_springs.Count; springPadding < m_softBody.m_springs.Count % THREADSPERPASS; springPadding++)
        {
            m_springs[springPadding] = new GPUSpring();
            m_springs[springPadding].massA = 0;
            m_springs[springPadding].massB = 0;
            m_springs[springPadding].equilibriumDistance = 0;

        }
    }

    private void SetupShader()
    {
        m_massBuffer = new ComputeBuffer(m_masses.Length, 36);
        m_massBuffer.SetData(m_masses);
        m_springBuffer = new ComputeBuffer(m_springs.Length, 12);
        m_springBuffer.SetData(m_springs);

        m_gpuSoftBody.SetFloat("springCoefficient", m_softBody.m_springCoefficient);
        m_gpuSoftBody.SetFloat("dragCoefficient", m_softBody.m_dragCoefficient);

        m_clearKernel = m_gpuSoftBody.FindKernel("ClearForces");
        m_gpuSoftBody.SetBuffer(m_clearKernel, "masses", m_massBuffer);

        m_calculateKernel = m_gpuSoftBody.FindKernel("CalculateForces");
        m_gpuSoftBody.SetBuffer(m_calculateKernel, "masses", m_massBuffer);
        m_gpuSoftBody.SetBuffer(m_calculateKernel, "springs", m_springBuffer);

        m_displacementKernel = m_gpuSoftBody.FindKernel("CalculateDisplacement");
        m_gpuSoftBody.SetBuffer(m_displacementKernel, "masses", m_massBuffer);
    }

    private void Update()
    {
        if (m_initialised && m_enabled)
        {
            GlobalForces.s_instance.CalculateGlobalForces();

            UpdateVariables();

            DispatchShader();

            for (int massIter = 0; massIter < m_softBody.m_masses.Count; massIter++)
            {
                ApplyDisplacement(m_softBody.m_masses[massIter]);
            }


            m_softBody.UpdateMesh();
        }
    }

    private void UpdateVariables()
    {
        m_gpuSoftBody.SetFloat("springCoefficient", m_softBody.m_springCoefficient);
        m_gpuSoftBody.SetFloat("dragCoefficient", m_softBody.m_dragCoefficient);
    }

    void DispatchShader()
    {
        UpdateGPUMasses();

        m_gpuSoftBody.SetFloat("deltaTime", Time.deltaTime);
        m_gpuSoftBody.SetVector("globalForce", GlobalForces.s_instance.m_globalForce);

        m_gpuSoftBody.Dispatch(m_clearKernel, m_masses.Length / THREADSPERPASS, 1, 1);
        m_gpuSoftBody.Dispatch(m_calculateKernel, m_springs.Length / THREADSPERPASS, 1, 1);
        m_gpuSoftBody.Dispatch(m_displacementKernel, m_masses.Length / THREADSPERPASS, 1, 1);

        UpdateCPUMasses();
    }

    void UpdateGPUMasses()
    {
        for (int massIter = 0; massIter < m_softBody.m_masses.Count; massIter++)
        {
            m_masses[massIter].position = m_softBody.GetVertex(m_softBody.m_masses[massIter].vertexGroup.m_vertices[0]);
        }
        m_massBuffer.SetData(m_masses);
    }

    void UpdateCPUMasses()
    {
        m_massBuffer.GetData(m_masses);
        for (int massIter = 0; massIter < m_softBody.m_masses.Count; massIter++)
        {
            m_softBody.m_masses[massIter].velocity = m_masses[massIter].velocity;
        }
    }

    void ApplyDisplacement(Mass _mass)
    {
        if (!_mass.m_fixed)
        {
            m_softBody.DisplaceVertex(_mass.vertexGroup, _mass.velocity * Time.deltaTime);
        }
    }

    public void SetEnabled(bool _state)
    {
        m_enabled = _state;
    }
}

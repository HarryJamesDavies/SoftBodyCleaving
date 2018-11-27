using UnityEngine;
using UnityEngine.Events;

namespace MSM
{
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
            public Vector3 normal;
            public Vector3 velocity;
            public int resultantForceX;
            public int resultantForceY;
            public int resultantForceZ;
        }

        private const int THREADSPERPASS = 16;

        public SoftBodyMesh m_softBody;
        private ComputeShader m_gpuSoftBody;

        private ComputeBuffer m_massBuffer;
        private ComputeBuffer m_springBuffer;
        private ComputeBuffer m_sphereColliderBuffer;

        private GPUMass[] m_masses;
        private GPUSpring[] m_springs;
        public GPUSphereCollider[] m_sphereColliders;

        private int m_clearKernel;
        private int m_springKernel;
        private int m_selfCollisionsKernel;
        private int m_sphereCollisionsKernel;
        private int m_displacementKernel;

        private bool m_initialise = false;
        private bool m_enabled = true;
        
        private void OnDestroy()
        {
            if (m_initialise)
            {
                m_massBuffer.Release();

                m_springBuffer.Release();

                if (m_sphereColliderBuffer != null)
                {
                    if (m_sphereColliderBuffer.IsValid())
                    {
                        m_sphereColliderBuffer.Release();
                    }
                }

                GeneralForces.s_instance.m_sphereColliderUpdate -= UpdateSphereColliders;
            }
        }

        public void Initialise(SoftBodyMesh _mesh)
        {
            m_softBody = _mesh;
            m_gpuSoftBody = ComputeShader.Instantiate(Resources.Load<ComputeShader>("GPUSoftBody"));

            GeneralForces.s_instance.m_sphereColliderUpdate += UpdateSphereColliders;
            if (GeneralForces.s_instance.GetSphereColliderCount() > 0)
            {
                UpdateSphereColliders();
            }

            if (m_softBody)
            {
                GenerateGPUData();
                SetupShader();
                m_initialise = true;
            }
            else
            {
                m_initialise = false;
            }
        }

        private void GenerateGPUData()
        {
            int nPaddingMasses = THREADSPERPASS - (m_softBody.m_masses.Count % THREADSPERPASS);
            m_masses = new GPUMass[m_softBody.m_masses.Count + nPaddingMasses];
            for (int massIter = 0; massIter < m_softBody.m_masses.Count; massIter++)
            {
                m_masses[massIter] = new GPUMass();
                m_masses[massIter].position = m_softBody.transform.TransformPoint(
                    m_softBody.m_masses[massIter].GetPostion());
                m_masses[massIter].velocity = Vector3.zero;
                m_masses[massIter].resultantForceX = 0;
                m_masses[massIter].resultantForceY = 0;
                m_masses[massIter].resultantForceZ = 0;
            }

            for (int massPadding = m_softBody.m_masses.Count; massPadding < m_masses.Length; massPadding++)
            {
                m_masses[massPadding] = new GPUMass();
                m_masses[massPadding].position = Vector3.zero;
                m_masses[massPadding].velocity = Vector3.zero;
                m_masses[massPadding].resultantForceX = 0;
                m_masses[massPadding].resultantForceY = 0;
                m_masses[massPadding].resultantForceZ = 0;
            }

            int nPaddingSprings = THREADSPERPASS - (m_softBody.m_springs.Count % THREADSPERPASS);
            m_springs = new GPUSpring[m_softBody.m_springs.Count + nPaddingSprings];
            for (int springIter = 0; springIter < m_softBody.m_springs.Count; springIter++)
            {
                m_springs[springIter] = new GPUSpring();
                m_springs[springIter].massA = (uint)m_softBody.m_springs[springIter].massA;
                m_springs[springIter].massB = (uint)m_softBody.m_springs[springIter].massB;
                m_springs[springIter].equilibriumDistance = m_softBody.m_springs[springIter].equilibriumDistance;
            }

            for (int springPadding = m_softBody.m_springs.Count; springPadding < m_springs.Length; springPadding++)
            {
                m_springs[springPadding] = new GPUSpring();
                m_springs[springPadding].massA = 0;
                m_springs[springPadding].massB = 0;
                m_springs[springPadding].equilibriumDistance = 0;
            }
        }

        private void SetupShader()
        {
            m_massBuffer = new ComputeBuffer(m_masses.Length, 48);
            m_massBuffer.SetData(m_masses);
            m_gpuSoftBody.SetInt("numMasses", m_masses.Length);
            m_springBuffer = new ComputeBuffer(m_springs.Length, 12);
            m_springBuffer.SetData(m_springs);

            m_gpuSoftBody.SetFloat("capSpringForce", m_softBody.m_settings.m_capSpringForce);
            m_gpuSoftBody.SetFloat("springCoefficient", m_softBody.m_settings.m_springCoefficient);
            m_gpuSoftBody.SetFloat("dragCoefficient", m_softBody.m_settings.m_dragCoefficient);
            m_gpuSoftBody.SetFloat("pressureCoefficient", (m_softBody.m_settings.m_usePressure) ?
                m_softBody.m_settings.m_pressureCoefficient : 0.0f);          

            float[] position = new float[3];
            position[0] = transform.position.x;
            position[1] = transform.position.y;
            position[2] = transform.position.z;
            m_gpuSoftBody.SetFloats("worldPosition", position);

            float[] scale = new float[3];
            scale[0] = transform.localScale.x;
            scale[1] = transform.localScale.y;
            scale[2] = transform.localScale.z;
            m_gpuSoftBody.SetFloats("localScale", scale);

            m_gpuSoftBody.SetFloat("selfCollisionRadius", m_softBody.m_settings.m_selfCollisionRadius);
            m_gpuSoftBody.SetFloat("selfCollisionForceCoefficient",
                m_softBody.m_settings.m_selfCollisionForceCoefficient);

            m_clearKernel = m_gpuSoftBody.FindKernel("ClearForces");
            m_gpuSoftBody.SetBuffer(m_clearKernel, "masses", m_massBuffer);

            m_springKernel = m_gpuSoftBody.FindKernel("CalculateSpringForces");
            m_gpuSoftBody.SetBuffer(m_springKernel, "masses", m_massBuffer);
            m_gpuSoftBody.SetBuffer(m_springKernel, "springs", m_springBuffer);

            m_selfCollisionsKernel = m_gpuSoftBody.FindKernel("CalculateSelfCollisions");
            m_gpuSoftBody.SetBuffer(m_selfCollisionsKernel, "masses", m_massBuffer);

            m_sphereCollisionsKernel = m_gpuSoftBody.FindKernel("CalculateSphereCollisions");
            m_gpuSoftBody.SetBuffer(m_sphereCollisionsKernel, "masses", m_massBuffer);

            m_displacementKernel = m_gpuSoftBody.FindKernel("CalculateDisplacement");
            m_gpuSoftBody.SetBuffer(m_displacementKernel, "masses", m_massBuffer);
        }

        private void Update()
        {
            if (m_initialise && m_enabled)
            {
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
            m_gpuSoftBody.SetFloat("capSpringForce", m_softBody.m_settings.m_capSpringForce);
            m_gpuSoftBody.SetFloat("springCoefficient", m_softBody.m_settings.m_springCoefficient);
            m_gpuSoftBody.SetFloat("dragCoefficient", m_softBody.m_settings.m_dragCoefficient);
            m_gpuSoftBody.SetFloat("pressureCoefficient", (m_softBody.m_settings.m_usePressure) ?
                m_softBody.m_settings.m_pressureCoefficient : 0.0f);

            float[] position = new float[3];
            position[0] = transform.position.x;
            position[1] = transform.position.y;
            position[2] = transform.position.z;
            m_gpuSoftBody.SetFloats("worldPosition", position);

            float[] scale = new float[3];
            scale[0] = transform.localScale.x;
            scale[1] = transform.localScale.y;
            scale[2] = transform.localScale.z;
            m_gpuSoftBody.SetFloats("localScale", scale);

            m_gpuSoftBody.SetFloat("selfCollisionRadius", m_softBody.m_settings.m_selfCollisionRadius);
            m_gpuSoftBody.SetFloat("selfCollisionForceCoefficient",
                m_softBody.m_settings.m_selfCollisionForceCoefficient);
        }

        public void UpdateSphereColliders()
        {
            if(m_sphereColliderBuffer != null)
            {
                if(m_sphereColliderBuffer.IsValid())
                {
                    m_sphereColliderBuffer.Dispose();
                    m_sphereColliderBuffer.Release();
                }
            }
            m_sphereColliders = GeneralForces.s_instance.GetSphereColliders();
            m_sphereColliderBuffer = new ComputeBuffer(m_sphereColliders.Length, 20);
            m_sphereColliderBuffer.SetData(m_sphereColliders);
            m_gpuSoftBody.SetBuffer(m_sphereCollisionsKernel, "sphereColliders", m_sphereColliderBuffer);

            m_gpuSoftBody.SetInt("numSphereColliders", m_sphereColliders.Length);
        }

        void DispatchShader()
        {
            UpdateGPUMasses();

            m_gpuSoftBody.SetFloat("deltaTime", Time.deltaTime);
            m_gpuSoftBody.SetVector("globalForce", GeneralForces.s_instance.GetGlobalForce());

            int massesPerPass = m_masses.Length / THREADSPERPASS;
            int springsPerPass = m_springs.Length / THREADSPERPASS;

            m_gpuSoftBody.Dispatch(m_clearKernel, massesPerPass, 1, 1);
            m_gpuSoftBody.Dispatch(m_springKernel, springsPerPass, 1, 1);

            if(m_softBody.m_settings.m_useCollisions)
            {
                if (m_softBody.m_settings.m_useSelfCollisions)
                {
                    m_gpuSoftBody.Dispatch(m_selfCollisionsKernel, massesPerPass, 1, 1);
                }

                if (m_sphereColliders != null && m_softBody.m_settings.m_useSphereCollisions)
                {
                    if (m_sphereColliders.Length > 0)
                    {
                        m_gpuSoftBody.Dispatch(m_sphereCollisionsKernel, massesPerPass, 1, 1);
                    }
                }
            }

            m_gpuSoftBody.Dispatch(m_displacementKernel, massesPerPass, 1, 1);

            UpdateCPUMasses();
        }

        void UpdateGPUMasses()
        {
            for (int massIter = 0; massIter < m_softBody.m_masses.Count; massIter++)
            {
                m_masses[massIter].position = m_softBody.m_masses[massIter].GetPostion();
                m_masses[massIter].normal = m_softBody.m_masses[massIter].GetNormal();
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
                if (_mass.vertexGroup != null)
                {
                    m_softBody.DisplaceVertex(_mass.vertexGroup, _mass.velocity * Time.deltaTime);
                }
                else
                {
                    _mass.m_postion += _mass.velocity * Time.deltaTime;
                }
            }
        }

        public void SetEnabled(bool _state)
        {
            m_enabled = _state;
        }
    }
}

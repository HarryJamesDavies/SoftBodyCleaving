using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine;

namespace MSM
{
    /// <summary>
    /// Calculates forces applied globally
    /// </summary>
    public class GeneralForces : MonoBehaviour
    {
        public static GeneralForces s_instance = null;
        public UnityAction m_sphereColliderUpdate;
        
        [SerializeField] private float m_windCoefficient = 2.0f;
        [SerializeField] private Vector3 m_windDirection = Vector3.forward;
        private Vector3 m_unnormalisedWindDirection = Vector3.zero;
        private float m_windStep = 0.0f;

        [SerializeField] private float m_gravityCoefficient = 2.0f;

        private Vector3 m_globalForce = Vector3.zero;
        
        public List<MSMSphereCollider> m_sphereColliders = new List<MSMSphereCollider>();
        private List<MSMSphereCollider> m_addSCs = new List<MSMSphereCollider>();
        private List<MSMSphereCollider> m_removeSCs = new List<MSMSphereCollider>();
        public GPUSphereCollider[] m_gpuSphereColliders;

        public TMPro.TMP_InputField m_windCoefficentInput;
        public TMPro.TMP_InputField m_windDirectionXInput;
        public TMPro.TMP_InputField m_windDirectionYInput;
        public TMPro.TMP_InputField m_windDirectionZInput;
        public TMPro.TMP_InputField m_gravityCoefficentInput;

        private void Awake()
        {
            CreateInstance();

            m_unnormalisedWindDirection = m_windDirection;
        }

        private void CreateInstance()
        {
            if (s_instance)
            {
                DestroyImmediate(this);
                return;
            }
            else
            {
                s_instance = this;
                CalculateGlobalForces();
                return;
            }
        }

        private void LateUpdate()
        {
            CalculateGlobalForces();
            CheckForSphereColliderChange();
        }

        private void CalculateGlobalForces()
        {
            m_globalForce = Vector3.zero;
            m_globalForce += m_windDirection * (Mathf.Sin(m_windStep) * m_windCoefficient);
            m_globalForce += Vector3.up * m_gravityCoefficient;
            m_windStep += Time.deltaTime;
        }

        public Vector3 GetGlobalForce()
        {
            return m_globalForce;
        }

        public int GetSphereColliderCount()
        {
            if (m_gpuSphereColliders != null)
            {
                return m_gpuSphereColliders.Length;
            }
            return 0;
        }

        public GPUSphereCollider[] GetSphereColliders()
        {
            return m_gpuSphereColliders;
        }

        public void AddSphereCollider(MSMSphereCollider _sphereCollider)
        {
            m_addSCs.Add(_sphereCollider);
        }

        public void RemoveSphereCollider(MSMSphereCollider _sphereCollider)
        {
            m_removeSCs.Remove(_sphereCollider);
        }

        private void CheckForSphereColliderChange()
        {
            bool dataChanged = false;

            if (m_addSCs.Count > 0)
            {
                m_sphereColliders.AddRange(m_addSCs);
                m_addSCs.Clear();
                dataChanged = true;
            }

            if (m_removeSCs.Count > 0)
            {
                for (int colliderIter = 0; colliderIter < m_removeSCs.Count; colliderIter++)
                {
                    m_sphereColliders.Remove(m_removeSCs[colliderIter]);
                }
                m_removeSCs.Clear();
                dataChanged = true;
            }

            if (dataChanged)
            {
                m_gpuSphereColliders = new GPUSphereCollider[m_sphereColliders.Count];
            }
            else if (m_sphereColliders.Count > 0)
            {
                UpdateSphereColliderBuffer();
            }
        }

        private void UpdateSphereColliderBuffer()
        {
            for (int colliderIter = 0; colliderIter < m_sphereColliders.Count; colliderIter++)
            {
                m_gpuSphereColliders[colliderIter] = m_sphereColliders[colliderIter].ToGPU();
            }

            if (m_sphereColliderUpdate != null)
            {
                m_sphereColliderUpdate.Invoke();
            }
        }

        public void SetWindCoefficient()
        {
            m_windCoefficient = float.Parse(m_windCoefficentInput.text); ;
        }

        public void SetWindDirectionX()
        {
            m_unnormalisedWindDirection.x = float.Parse(m_windDirectionXInput.text); ;
            m_windDirection = m_unnormalisedWindDirection.normalized;
        }

        public void SetWindDirectionY()
        {
            m_unnormalisedWindDirection.y = float.Parse(m_windDirectionYInput.text); ;
            m_windDirection = m_unnormalisedWindDirection.normalized;
        }

        public void SetWindDirectionZ()
        {
            m_unnormalisedWindDirection.z = float.Parse(m_windDirectionZInput.text);
            m_windDirection = m_unnormalisedWindDirection.normalized;
        }

        public void SetGravityCoefficient()
        {
            m_gravityCoefficient = float.Parse(m_gravityCoefficentInput.text);
        }
    }
}

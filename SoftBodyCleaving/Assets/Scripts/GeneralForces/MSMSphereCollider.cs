using UnityEngine;
using System;

namespace MSM
{
    [Serializable]
    public struct GPUSphereCollider
    {
        public Vector3 position;
        public float radius;
        public float forceCoefficient;
    };

    public class MSMSphereCollider : MonoBehaviour
    {
        public SphereCollider m_collider;
        public float m_forceCoefficient = 50.0f;
        public float m_spacingOffset = 1.0f;

        private void Start()
        {
            GeneralForces.s_instance.AddSphereCollider(this);
        }

        private void OnDestroy()
        {
            GeneralForces.s_instance.RemoveSphereCollider(this);
        }        

        public GPUSphereCollider ToGPU()
        {
            return new GPUSphereCollider()
            {
                position = transform.position,
                radius = (m_collider.radius * transform.localScale.x) + m_spacingOffset,
                forceCoefficient = m_forceCoefficient
            };
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public float GetRadius()
        {
            return m_collider.radius;
        }

        public static void DeepCopy(GPUSphereCollider _from, out GPUSphereCollider _to)
        {
            _to.position = _from.position;
            _to.radius = _from.radius;
            _to.forceCoefficient = _from.forceCoefficient;
        }
    }
}

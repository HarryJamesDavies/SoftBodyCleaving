using System;

namespace MSM
{
    [Serializable]
    public class SoftBodySettings
    {
        public float m_neighbourDistance = 10.0f;

        public float m_capSpringForce = 10.0f;
        public float m_springCoefficient = 10.0f;
        public float m_dragCoefficient = 0.1f;
        public float m_pressureCoefficient = 5.0f;

        public bool m_usePressure = false;

        public FixDirection m_fixedDirection = FixDirection.Top;
        public float m_fixedOffset = 5.0f;

        public bool m_initialiseOnStart = false;
        public bool m_initialiseOnUpdate = false;
        public bool m_useMeshInstance = true;

        public bool m_useInternals = true;
        public float m_internalOffset = 0.1f;

        public bool m_useCollisions = false;

        public bool m_useSelfCollisions = false;
        public float m_selfCollisionRadius = 1.0f;
        public float m_selfCollisionForceCoefficient = 10.0f;

        public bool m_useSphereCollisions = false;

        public SoftBodySettings(SoftBodySettings _settings)
        {
            m_neighbourDistance = _settings.m_neighbourDistance;
            m_capSpringForce = _settings.m_capSpringForce;
            m_springCoefficient = _settings.m_springCoefficient;
            m_dragCoefficient = _settings.m_dragCoefficient;
            m_pressureCoefficient = _settings.m_pressureCoefficient;

            m_usePressure = _settings.m_usePressure;

            m_fixedDirection = _settings.m_fixedDirection;
            m_fixedOffset = _settings.m_fixedOffset;

            m_initialiseOnStart = _settings.m_initialiseOnStart;
            m_useMeshInstance = _settings.m_useMeshInstance;

            m_useInternals = _settings.m_useInternals;
            m_internalOffset = _settings.m_internalOffset;

            m_useCollisions = _settings.m_useCollisions;

            m_useSelfCollisions = _settings.m_useSelfCollisions;
            m_selfCollisionRadius = _settings.m_selfCollisionRadius;
            m_selfCollisionForceCoefficient = _settings.m_selfCollisionForceCoefficient;

            m_useSphereCollisions = _settings.m_useSphereCollisions;
        }

        public SoftBodySettings()
        {

        }
    }
}

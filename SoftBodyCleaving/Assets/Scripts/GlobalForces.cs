using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalForces : MonoBehaviour
{
    public static GlobalForces s_instance = null;

    public float m_windCoefficient = 2.0f;
    private float m_windStep = 0.0f;
    public Vector3 m_windDirection = Vector3.forward;

    public float m_gravityCoefficient = 2.0f;

    public Vector3 m_globalForce = Vector3.zero;

    private void Awake()
    {
        CreateInstance();
    }

    private void CreateInstance()
    {
        if(s_instance)
        {
            DestroyImmediate(this);
            return;
        }
        else
        {
            s_instance = this;
            return;
        }
    }

    public void CalculateGlobalForces()
    {
        m_globalForce = Vector3.zero;
        m_globalForce += m_windDirection * Mathf.Sin(m_windStep) * m_windCoefficient;
        m_globalForce += Vector3.up * m_gravityCoefficient;
        m_windStep += Time.deltaTime;
    }
}

using UnityEngine;
using System.Collections;

public class SWSpring : UnityEngine.Object
{
    SWMesh m_mesh;

    SWMass m_massA;
    SWMass m_massB;

    float m_equilibriumDistance;

    public SWSpring Initialise(SWMesh _mesh, SWMass _massA, SWMass _massB, float _setLength = 0.0f)
    {
        m_mesh = _mesh;

        m_massA = _massA;
        m_massB = _massB;

        //Sets equilibrium distance and vector
        if (_setLength == 0.0f)
        {
            m_equilibriumDistance = Vector3.Distance(m_massB.transform.position, m_massA.transform.position); // il 
        }
        else
        {
            m_equilibriumDistance = _setLength; // il
        }

        return this;
    }

    public void CalculateForce()
    {
        if (!m_mesh.m_inDeadZone)
        {
            //Calculates spring force using hooke's law	
            float massDistance = Vector3.Distance(m_massB.transform.position, m_massA.transform.position); // dl
            float force = m_mesh.m_springCoefficient * (massDistance - m_equilibriumDistance); // f = k * (dl - il)

            //Calculate damping force combined with Hooke's law
            Vector3 massVector = m_massB.transform.position - m_massA.transform.position; // r
            massVector.Normalize();

            Vector3 forceVector = massVector * force; // F = (r * f)

            //Adds force to both masses
            m_massA.m_springForce += forceVector;
            m_massB.m_springForce += -forceVector;
        }
    }
}

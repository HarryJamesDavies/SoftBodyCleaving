using UnityEngine;
using System.Collections.Generic;

public class SWChain : SWMesh
{
    public float m_minimumMass = 2.0f;
    public bool m_useSetLength = false;
    public float m_setLength = 20.0f;

    [HideInInspector]
    public bool m_meshHasSnapped = false;
    public bool m_allowSnapping = false;
    public float m_maxDistance = 50.0f;
    public bool m_useDeadZone = false;
    public float m_minimumDistanceThreshold = 10.0f;
    public float m_maximumDistanceThreshold = 40.0f;

    public bool m_lockedAnchor = false;
    [HideInInspector]
    public GameObject m_anchor = null;
    [HideInInspector]
    public GameObject m_target = null;

    [HideInInspector]
    public float m_startingTargetMass = 0.0f;
    private float m_tetherMass = 5.0f;

    private bool m_movingToSetPoint = false;
    private Vector3 m_setPosition;
    private float m_lerpRate = 20.0f;

    void OnEnable()
    {
        if (m_target)
        {
            if(m_useSetLength)
            {
                m_movingToSetPoint = true;
                m_setPosition = ((m_target.transform.position - m_anchor.transform.position).normalized * m_setLength) + m_anchor.transform.position;
            }
            else
            {
                Initialise();
            }
        }
        else
        {
            Debug.LogWarning("Can't Initialise SWMesh without a target object");
        }
    }

    void Initialise()
    {
        m_resistance = (m_resistanceCoefficient * 100.0f) / m_target.GetComponent<Rigidbody2D>().mass;
        if (m_resistance < m_resistanceMinimum)
        {
            m_resistance = m_resistanceMinimum;
        }
        CreateMasses();
        CreateSprings();
    }

    public void ResetMass()
    {
        m_target.GetComponent<Rigidbody2D>().mass = m_startingTargetMass;
    }

    new
    void Update()
    {     
        if (!m_movingToSetPoint)
        {
            if (m_allowSnapping || m_useDeadZone)
            {
                float currentLength = Mathf.Abs(Vector3.Distance(m_anchor.transform.position, m_target.transform.position));

                if (m_allowSnapping)
                {
                    m_meshHasSnapped = currentLength >= m_maxDistance;
                }

                if (m_useDeadZone)
                {
                    m_inDeadZone = (currentLength <= m_maximumDistanceThreshold && currentLength >= m_minimumDistanceThreshold);
                }
            }
        }
        else
        {
            m_target.transform.position = Vector3.Lerp(m_target.transform.position, m_setPosition, (m_lerpRate * Time.deltaTime));

            if(Vector3.Distance(m_target.transform.position, m_setPosition) <= 2.0f)
            {
                m_movingToSetPoint = false;
                Initialise();
            }
        }

        base.Update();
    }

    void LateUpdate()
    {
        UpdateRender();
    }

    void CreateMasses()
    {
        SWMass mass = m_anchor.GetComponent<SWMass>();
        if (mass)
        {
            UpdateMass(mass, m_anchor.transform.position);
        }
        else
        {
            AddMass(m_anchor.GetComponent<Rigidbody2D>().mass, m_anchor.transform.position, m_anchor, true, m_lockedAnchor);
        }

        Vector3 targetVector = m_target.transform.position - m_anchor.transform.position;
        Vector3 targetPosition = m_target.transform.position;
        if (m_useSetLength)
        {
            targetVector = targetVector.normalized * m_setLength;
            targetPosition = m_anchor.transform.position + targetVector;
        }

        float interpolatedLength = targetVector.magnitude / m_interpolationLevel;
        targetVector.Normalize();

        for (int i = 1; i <= m_interpolationLevel - 1; i++)
        {
            AddMass(m_tetherMass, ((interpolatedLength * i) * targetVector) + m_anchor.transform.position);
        }


        Rigidbody2D targetRigidBody = m_target.GetComponent<Rigidbody2D>();
        m_startingTargetMass = targetRigidBody.mass;
        if (targetRigidBody.mass < m_minimumMass)
        {
            targetRigidBody.mass = m_minimumMass;
        }

        mass = m_target.GetComponent<SWMass>();
        if (mass)
        {
            UpdateMass(mass, targetPosition);
        }
        else
        {
            AddMass(m_target.GetComponent<Rigidbody2D>().mass, targetPosition, m_target);
        }

        UpdateRender();
    }

    void CreateSprings()
    {
        int massAIter = 1;
        int massBIter = 0;

        //Get first pair of masses
        SWMass massA = m_masses[massAIter];
        SWMass massB = m_masses[massBIter];
        m_springs.Add(new SWSpring().Initialise(this, massA, massB));

        //Loop through other masses connect them via springs
        for (int iter = 1; iter <= m_interpolationLevel - 1; iter++)
        {
            massAIter++;

            massA = m_masses[massAIter];
            massB = m_masses[massBIter];
            m_springs.Add(new SWSpring().Initialise(this, massA, massB));

            massBIter++;

            massA = m_masses[massAIter];
            massB = m_masses[massBIter];
            m_springs.Add(new SWSpring().Initialise(this, massA, massB));
        }

        return;
    }

    void UpdateRender()
    {
        List<Vector3> positions = new List<Vector3>();

        TPLineRenderer render = gameObject.GetComponent<TPLineRenderer>();

        if (!m_movingToSetPoint)
        {
            foreach (SWMass mass in m_masses)
            {
                positions.Add(mass.transform.position);
            }            
        }
        else
        {
            positions.Add(m_anchor.transform.position);
            positions.Add(m_target.transform.position);
        }

        render.SetPositions(positions.ToArray());
    }
}

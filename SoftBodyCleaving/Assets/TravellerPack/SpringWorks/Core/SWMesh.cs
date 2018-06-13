using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SWMesh : MonoBehaviour
{
    [HideInInspector]
    public List<SWMass> m_masses;
    [HideInInspector]
    public List<SWSpring> m_springs;

    public GameObject m_massPrefab;
    [HideInInspector]
    public Transform m_massHolder;

    public int m_meshID;

    public float m_springCoefficient = 5.0f;
    [Range(1, 100)]
    public int m_interpolationLevel = 1;
    public float m_resistanceCoefficient = 10.0f;
    public float m_resistanceMinimum = 50.0f;
    protected float m_resistance = 100.0f;

    [HideInInspector]
    public bool m_inDeadZone = false;

    void Start()
    {
        m_meshID = SWManager.Instance.GenerateID();
    }

    void OnDestroy()
    {
        SWManager.Instance.RemoveID(m_meshID);
    }

    protected void Update()
    {        
        UpdateSprings();
    }

    void FixedUpdate()
    {
        ApplyChanges();
    }

    //Calculates the spring force of each spring
    void UpdateSprings()
    {
        //Resets spring force to zero
        foreach (SWMass mass in m_masses)
        {
            mass.m_springForce = Vector3.zero;
        }

        //Calculates springs force
        foreach (SWSpring spring in m_springs)
        {
            spring.CalculateForce();
        }
    }

    //Applies masses and vertex displacement
    void ApplyChanges()
    {
        //Applies surface masses and vertex displacement
        foreach (SWMass mass in m_masses)
        {
            mass.ApplyDisplacement();
        }
    }

    protected void AddMass(float _mass, Vector3 _pos, GameObject _fixedObject = null, bool _fixed = false, bool _lockedAnchor = false)
    {
        if (_fixedObject)
        {
            _fixedObject.transform.position = _pos;
            _fixedObject.AddComponent<SWMass>();
            m_masses.Add(_fixedObject.GetComponent<SWMass>().Initialise(m_meshID, m_resistance, _fixed, _lockedAnchor));
        }
        else
        {
            GameObject mass = Instantiate(m_massPrefab, _pos, Quaternion.identity) as GameObject;
            mass.transform.SetParent(m_massHolder);
            m_masses.Add(mass.GetComponent<SWMass>().Initialise(m_meshID, m_resistance));
        }
    }

    protected void UpdateMass(SWMass _mass, Vector3 _pos, bool _massLock = false)
    {
        _mass.UpdateMass(_pos, m_resistance, _massLock, m_meshID);
        m_masses.Add(_mass);
    }
}

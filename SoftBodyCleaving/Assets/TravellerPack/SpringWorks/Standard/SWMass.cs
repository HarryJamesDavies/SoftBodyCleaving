using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SWMass : MonoBehaviour
{
    public Vector3 m_springForce;
    public Rigidbody2D m_rigidBody;

    private float m_anchorResistance = 0.0f;
    private bool m_fixed = false;
    private bool m_lockedAnchor = false;
    private List<int> m_meshID = new List<int>();

    public SWMass Initialise(int _meshID, float _anchorResistance, bool _fixed = false, bool _lockedAnchor = false)
    {
        m_fixed = _fixed;
        m_lockedAnchor = _lockedAnchor;
        m_anchorResistance = _anchorResistance;
        if(m_anchorResistance <= 0.0f)
        {
            m_anchorResistance = 100.0f;
            Debug.Log("AnchorResistance can't be 0");
        }

        m_rigidBody = GetComponent<Rigidbody2D>();

        m_meshID.Add(_meshID);

        return this;
    }

    public void UpdateMass(Vector3 _pos, float _anchorResistance, bool _massLock, int _meshID)
    {
        transform.position = _pos;
        m_anchorResistance = _anchorResistance;
        ToggleAnchorLock(_massLock);
        m_meshID.Add(_meshID);
    }

    //Applies vertex displacement based on previous calculation
    public void ApplyDisplacement()
    {
        if (!m_fixed)
        {
            m_rigidBody.AddForce(m_springForce, ForceMode2D.Impulse);
        }
        else
        {
            if (!m_lockedAnchor)
            {
                m_rigidBody.AddForce(m_springForce / m_anchorResistance, ForceMode2D.Impulse);
            }
        }
    }

    public void ToggleAnchorLock(bool _lock)
    {
        m_lockedAnchor = _lock;
    }

    public void AddChainID(int _meshID)
    {
        m_meshID.Add(_meshID);
    }

    public bool RemoveChainID(int _meshID)
    {
        m_meshID.Remove(_meshID);
        if(m_meshID.Count == 0)
        {
            return true;
        }
        return false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshSubSection
{
    public Vector3 m_minimum = Vector3.zero;
    public Vector3 m_maximum = Vector3.zero;

    public List<int> m_masses = new List<int>();
    public List<int> m_neighbours = new List<int>();

    public MeshSubSection(Vector3 _minimum, float _sectionLength)
    {
        m_minimum = _minimum;
        m_maximum = m_minimum + (Vector3.one * _sectionLength);
    }

    public bool WithinSubSection(Vector3 _postion)
    {
        if((_postion.x >= m_minimum.x) && (_postion.y >= m_minimum.y) &&
            (_postion.z >= m_minimum.z) && (_postion.x <= m_maximum.x) &&
            (_postion.y <= m_maximum.y) && (_postion.z <= m_maximum.z))
        {
            return true;
        }
        return false;
    }
}

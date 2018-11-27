using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoColliderManager : MonoBehaviour
{
    public List<DemoColliderController> m_colliders = new List<DemoColliderController>();
    private int m_currentCollider = 0;

    private void Start()
    {
        m_currentCollider = 0;
        m_colliders[m_currentCollider].SetActive(true);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetColliderActive(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetColliderActive(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetColliderActive(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetColliderActive(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SetColliderActive(4);
        }
    }

    private void SetColliderActive(int _collider)
    {
        m_colliders[m_currentCollider].SetActive(false);

        if(_collider < m_colliders.Count)
        {
            m_currentCollider = _collider;
            m_colliders[m_currentCollider].SetActive(true);
        }
    }
}

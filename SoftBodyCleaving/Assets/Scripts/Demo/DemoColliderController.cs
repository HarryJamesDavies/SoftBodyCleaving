using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoColliderController : MonoBehaviour
{
    private bool m_active = false;
    public float m_moveSpeed = 10.0f;

    private void Update()
    {
        if(m_active)
        {
            transform.position += transform.forward * m_moveSpeed * Input.GetAxis("ColliderVertical") * Time.deltaTime;
            transform.position += transform.right * m_moveSpeed * Input.GetAxis("ColliderHorizontal") * Time.deltaTime;
            transform.position += transform.up * m_moveSpeed * Input.GetAxis("ColliderUp") * Time.deltaTime;
        }
    }

    public void SetActive(bool _state)
    {
        m_active = _state;
    }
}

using UnityEngine;
using System.Collections.Generic;

public class ForceField : MonoBehaviour
{
    public float m_fieldStrength = 25.0f;
    public float m_minimumDistance = 5.0f;
    public List<Rigidbody2D> m_watchers = new List<Rigidbody2D>();

    void Awake()
    {
        CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = m_minimumDistance / ((transform.lossyScale.x + transform.lossyScale.y) / 2.0f);
    }

    void OnDestroy()
    {
        m_watchers.Clear();
        Destroy(gameObject.GetComponent<CircleCollider2D>());
    }

	void FixedUpdate ()
    {
	    foreach(Rigidbody2D other in m_watchers)
        {
            float force = m_fieldStrength / m_minimumDistance - Vector3.Distance(other.transform.position, transform.position);
            if(force < 0.0f)
            {
                force = 0.0f;
            }
            other.AddForce((other.transform.position - transform.position).normalized * force, ForceMode2D.Impulse);
        }
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 11)
        {
            m_watchers.Add(other.GetComponent<Rigidbody2D>());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == 11)
        {
            m_watchers.Remove(other.GetComponent<Rigidbody2D>());
        }
    }
}

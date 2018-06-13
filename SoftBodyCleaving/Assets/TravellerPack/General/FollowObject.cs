using UnityEngine;
using System.Collections.Generic;

public class FollowObject : MonoBehaviour
{
    public Transform m_target;
    public Transform m_center;
    public float m_deadZone = 2.0f;
    public float m_moveForce = 10.0f;

    private Vector2 m_forceTarget = Vector2.zero;
    private Vector2 m_force = Vector2.zero;
    private Rigidbody2D m_rigidBody;
    private int sectionCount = 8;

    private float m_drag = 0.0f;
    private float m_angularDrag = 0.0f;

    public void Initialise(Transform _target, Transform _center, bool _useCenterForce, float _moveForce = 10.0f, float _deadZone = 2.0f)
    {
        m_target = _target;
        m_center = _center;
        m_moveForce = _moveForce;
        m_deadZone = _deadZone;

        //transform.position = m_target.position;

        m_rigidBody = GetComponent<Rigidbody2D>();

        m_drag = m_rigidBody.drag;
        m_angularDrag = m_rigidBody.angularDrag;

        CalculateForceTarget();
    }

    public void SetPhysics(float _drag, float _angularDrag)
    {
        m_rigidBody.drag = _drag;
        m_rigidBody.angularDrag = _angularDrag;
    }

    void OnDestroy()
    {
        m_rigidBody.drag = m_drag;
        m_rigidBody.angularDrag = m_angularDrag;
    }

    Vector2 CalculateForceTarget(int _index)
    {
        float angle = (360.0f / sectionCount) * (_index - 1);
        return new Vector2(m_center.position.x + Mathf.Cos(angle), m_center.position.y + Mathf.Sin(angle));
    }

    int CalculateCurrentSection(Vector2 _objectPoistion)
    {
        float angle = (Mathf.Atan2(_objectPoistion.x - m_center.position.x, _objectPoistion.y - m_center.position.y) * Mathf.Rad2Deg) + 180.0f;
        return Mathf.FloorToInt(angle / (360.0f / sectionCount)) + 1;
    }

    void CalculateForceTarget()
    {
        int currentSection = CalculateCurrentSection(transform.position);
        int targetSection = CalculateCurrentSection(m_target.position);
        int difference = CalculateSectionDifference(currentSection, targetSection);

        if (difference > 4)
        {
            m_forceTarget = CalculateForceTarget(currentSection - 2);
        }
        else if (difference < -4)
        {
            m_forceTarget = CalculateForceTarget(currentSection + 2);
        }
        else
        {
            m_forceTarget = m_target.position;
        }
    }

    int CalculateSectionDifference(int _current, int _target)
    {
        int difference = _target - _current;

        if (difference > (sectionCount / 2))
        {
            difference = (_current - (_target - sectionCount)) * -1;
        }
        else if (difference < -(sectionCount / 2))
        {
            difference = (_current - (_target + sectionCount)) * -1;
        }

        return difference;
    }

    void LateUpdate()
    {
        CalculateForceTarget();

        Vector2 targetForce = new Vector2(transform.position.x - m_forceTarget.x, transform.position.y - m_forceTarget.y);
        if (targetForce.magnitude < m_deadZone)
        {
            m_force = Vector2.zero;
        }
        else
        {
            m_force = (targetForce.normalized * ((targetForce.magnitude - m_deadZone) / (1 - m_deadZone))) * m_moveForce;
        }
    }

    void FixedUpdate()
    {
        if (Mathf.Abs(m_force.x) > 0.0f && Mathf.Abs(m_force.y) > 0.0f)
        {
            m_rigidBody.AddForce(m_force, ForceMode2D.Impulse);
        }
    }
}

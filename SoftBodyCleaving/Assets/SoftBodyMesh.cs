﻿using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Mass
{
    public int vertex;
    public int index;
    public SoftBodyMesh mesh;
    public List<int> springs;
    public List<int> neighbours;
    public Vector3 force;
    public Vector3 velocity;
    public bool m_fixed;

    public Mass(SoftBodyMesh _mesh, int _vertex, int _index, bool _fixed)
    {
        mesh = _mesh;
        vertex = _vertex;
        index = _index;
        springs = new List<int>();
        neighbours = new List<int>();
        force = Vector3.zero;
        velocity = Vector3.zero;
        m_fixed = _fixed;
    }

    public bool CheckDuplicateSpring(int _otherMass)
    {
        for (int springIter = 0; springIter < springs.Count; springIter++)
        {
            if(mesh.m_springs[springs[springIter]].ContainsMasses(index, _otherMass))
            {
                return true;
            }
        }
        return false;
    }

    public void AddSpring(int _spring)
    {
        if (!springs.Contains(_spring))
        {
            springs.Add(_spring);
        }
    }

    public void AddNeighbour(int _mass)
    {
        if (!neighbours.Contains(_mass))
        {
            neighbours.Add(_mass);
        }
    }
}

[Serializable]
public struct Spring
{
    public int massA;
    public int massB;
    public float equilibriumDistance;

    public Spring(int _massA, int _massB, float _equilibriumDistance)
    {
        massA = _massA;
        massB = _massB;
        equilibriumDistance = _equilibriumDistance;
    }

    public bool ContainsMasses(int _massA, int _massB)
    {
        return ((massA == _massA) && (massB == _massB))
            || ((massA == _massB) && (massB == _massA));
    }
}

public class SoftBodyMesh : MonoBehaviour
{
    public MeshFilter m_meshFilter;
    private Mesh m_mesh;
    public List<Mass> m_masses = new List<Mass>();
    public List<Spring> m_springs = new List<Spring>();

    public bool m_highlightMasses = false;
    public GameObject m_highlightObject;

    public float m_neighbourDistance = 10.0f;
    public float m_springCoefficient = 2.0f;
    public float m_dragCoefficient = 2.0f;
    public float m_windCoefficient = 2.0f;
    public float m_windStep = 0.0f;
    public Vector3 m_windDirection = Vector3.forward;

    public Vector3 m_globalForce = Vector3.zero;

    private List<Vector3> m_vertices = new List<Vector3>();
    private List<Vector2> m_uvs = new List<Vector2>();
    private List<Color> m_colours = new List<Color>();
    private List<int> m_triangles = new List<int>();

    public void Start()
    {
        m_mesh = m_meshFilter.mesh;
        CreateSoftBodyFromMesh();
    }

    private void CreateSoftBodyFromMesh()
    {
        if (m_mesh != null)
        {
            GetData();
            GenerateMasses();
            GenerateNeighbours();
            GenerateSprings();
        }
    }

    private void GetData()
    {
        m_vertices = m_mesh.vertices.ToList();
        m_uvs = m_mesh.uv.ToList();
        m_colours = m_mesh.colors.ToList();
        m_triangles = m_mesh.triangles.ToList();
    }

    private void GenerateMasses()
    {
        for (int vertexIter = 0; vertexIter < m_vertices.Count; vertexIter++)
        {
            CreateMass(vertexIter);
        }
    }

    private void GenerateNeighbours()
    {
        List<Mass> checkMasses = new List<Mass>();
        checkMasses.AddRange(m_masses);

        int checkStart = 1;
        for (int massIter = 0; massIter < m_masses.Count; massIter++)
        {
            for (int checkIter = checkStart; checkIter < checkMasses.Count; checkIter++)
            {
                if(Vector3.Distance(m_vertices[m_masses[massIter].vertex],
                    m_vertices[checkMasses[checkIter].vertex]) <= m_neighbourDistance)
                {
                    m_masses[massIter].AddNeighbour(checkMasses[checkIter].index);
                    checkMasses[checkIter].AddNeighbour(m_masses[massIter].index);
                }
            }
            checkStart++;
        }
        checkMasses.Clear();
    }

    private void GenerateSprings()
    {
        for (int massIter = 0; massIter < m_masses.Count; massIter++)
        {
            Mass currentMass = m_masses[massIter];
            for (int neighbourIter = 0; neighbourIter < currentMass.neighbours.Count; neighbourIter++)
            {
                CreateSpring(currentMass.index, m_masses[currentMass.neighbours[neighbourIter]].index);
            }
        }
    }

    private void CreateMass(int _vertex)
    {
        m_masses.Add(new Mass(this, _vertex, m_masses.Count, (m_vertices[_vertex].y > 10.0f)));

        if(m_highlightMasses)
        {
            Instantiate(m_highlightObject, m_vertices[_vertex], Quaternion.identity, transform);
        }
    }

    private void CreateSpring(int _massA, int _massB)
    {
        if(!m_masses[_massA].CheckDuplicateSpring(_massB))
        {
            int springIndex = m_springs.Count;
            m_springs.Add(new Spring(_massA, _massB, GetMassDistance(_massA, _massB)));
            m_masses[_massA].AddSpring(springIndex);
            m_masses[_massB].AddSpring(springIndex);
        }
    }

    private float GetMassDistance(int _massA, int _massB)
    {
        return Vector3.Distance(m_vertices[m_masses[_massA].vertex],
            m_vertices[m_masses[_massB].vertex]);
    }

    private void Update()
    {
        CalculateGlobalForces();

        for (int springIter = 0; springIter < m_springs.Count; springIter++)
        {
            CalculateForces(m_springs[springIter]);
        }

        for (int massIter = 0; massIter < m_masses.Count; massIter++)
        {
            CalculateDisplacement(m_masses[massIter]);
        }

        for (int massIter = 0; massIter < m_masses.Count; massIter++)
        {
            ApplyDisplacement(m_masses[massIter]);
        }

        UpdateMesh();
    }

    void CalculateGlobalForces()
    {
        m_globalForce = Vector3.zero;
        m_globalForce += m_windDirection * Mathf.Sin(m_windStep);
        m_windStep += Time.deltaTime;
    }

    void CalculateForces(Spring _spring)
    {
        Vector3 massA = m_vertices[m_masses[_spring.massA].vertex];
        Vector3 massB = m_vertices[m_masses[_spring.massB].vertex];

        //Calculates spring force using hooke's law	
        float massDistance = Vector3.Distance(massB, massA); // dl
        float springForce = m_springCoefficient * (massDistance - _spring.equilibriumDistance); // f = k * (dl - il)
        Vector3 forceVector = Vector3.Normalize(massB - massA) * springForce; // r

        //Adds force to both masses
        m_masses[_spring.massA].force += forceVector;
        m_masses[_spring.massB].force -= forceVector;
    }

    void CalculateDisplacement(Mass _mass)
    {
        _mass.force += m_globalForce;
        _mass.velocity = (_mass.velocity + (_mass.force * Time.deltaTime));
        _mass.velocity -= m_dragCoefficient * _mass.velocity;
        _mass.force = Vector3.zero;
    }

    void ApplyDisplacement(Mass _mass)
    {
        if (!_mass.m_fixed)
        {
            m_vertices[_mass.vertex] += _mass.velocity * Time.deltaTime;
        }
    }

    void UpdateMesh()
    {
        m_mesh.Clear();
        m_mesh.vertices = m_vertices.ToArray();
        m_mesh.uv = m_uvs.ToArray();
        m_mesh.triangles = m_triangles.ToArray();
        m_mesh.colors = m_colours.ToArray();
    }
}
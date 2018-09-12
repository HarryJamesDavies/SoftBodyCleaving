using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Mass
{
    public VertexGroup vertexGroup;
    public int index;
    public SoftBodyMesh mesh;
    public List<int> springs;
    public List<int> neighbours;
    public Vector3 force;
    public Vector3 velocity;
    public bool m_fixed;

    public Mass(SoftBodyMesh _mesh, VertexGroup _group, int _index, bool _fixed)
    {
        mesh = _mesh;
        vertexGroup = _group;
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
            if (mesh.m_springs[springs[springIter]].ContainsMasses(index, _otherMass))
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

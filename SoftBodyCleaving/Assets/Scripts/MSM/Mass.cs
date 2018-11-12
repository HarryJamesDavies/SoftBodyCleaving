using System;
using System.Collections.Generic;
using UnityEngine;

namespace MSM
{
    [Serializable]
    public class Mass
    {
        public VertexGroup vertexGroup;
        public Vector3 m_postion;
        public int index;
        public SoftBodyMesh mesh;
        public List<int> springs;
        public List<int> neighbours;
        public Vector3 force;
        public Vector3 velocity;
        public Vector3 m_normal;
        public bool m_fixed;

        public Mass(SoftBodyMesh _mesh, VertexGroup _group, int _index, bool _fixed)
        {
            mesh = _mesh;
            vertexGroup = _group;
            m_postion = Vector3.zero;
            m_normal = Vector3.zero;
            index = _index;
            springs = new List<int>();
            neighbours = new List<int>();
            force = Vector3.zero;
            velocity = Vector3.zero;
            m_fixed = _fixed;
        }

        public Mass(SoftBodyMesh _mesh, Vector3 _postion, Vector3 _normal, int _index, bool _fixed)
        {
            mesh = _mesh;
            vertexGroup = null;
            m_postion = _postion;
            m_normal = _normal;
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

        public void AddNeighbours(List<int> _masses)
        {
            for (int massIter = 0; massIter < _masses.Count; massIter++)
            {
                if (!neighbours.Contains(_masses[massIter]))
                {
                    neighbours.Add(_masses[massIter]);
                }
            }
        }

        public Vector3 GetPostion()
        {
            Vector3 position = m_postion;
            if (vertexGroup != null)
            {
                if (vertexGroup.m_vertices.Count > 0)
                {
                    position = (vertexGroup.m_useAveragePosition) ? vertexGroup.m_averagePosition :
                        mesh.GetVertex(vertexGroup.m_vertices[0]);
                }
            }
            return position;
        }

        public Vector3 GetNormal()
        {
            Vector3 normal = m_normal;
            if (vertexGroup != null)
            {
                normal = vertexGroup.m_sharedNormal;
            }
            return normal;
        }
    }
}

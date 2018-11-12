// Original CSG.JS library by Evan Wallace (http://madebyevan.com), under the MIT license.
// GitHub: https://github.com/evanw/csg.js/

using System;
using UnityEngine;
using System.Collections.Generic;

namespace CSG
{
	public class CSGPlane
	{
		public Vector3 m_normal;
		public float m_planeWidth;

		[Flags]
		enum PolygonType
		{
			Coplanar 	= 0,
			Front 		= 1,
			Back 		= 2,
			Spanning 	= 3 		
		};

		public CSGPlane()
		{
			m_normal = Vector3.zero;
			m_planeWidth = 0f;
		}

		public CSGPlane(Vector3 _vertexA, Vector3 _vertexB, Vector3 _vertexC)
		{
			m_normal = Vector3.Cross(_vertexB - _vertexA, _vertexC - _vertexA);
			m_planeWidth = Vector3.Dot(m_normal, _vertexA);
        }

        public CSGPlane(Vector3 _normal, float _planeWidth)
        {
            m_normal = _normal;
            m_planeWidth = _planeWidth;
        }

        public CSGPlane DeepClone()
        {
            return new CSGPlane(m_normal, m_planeWidth);
        }

		public bool Valid()
		{
			return m_normal.magnitude > 0f;
		}

		public void Flip()
		{
			m_normal *= -1f;
			m_planeWidth *= -1f;
		}

        // Split `polygon` by this plane if needed, then put the polygon or polygon
        // fragments in the appropriate lists. Coplanar polygons go into either
        // `coplanarFront` or `coplanarBack` depending on their orientation with
        // respect to this plane. Polygons in front or in back of this plane go into
        // either `front` or `back`.
        public void SplitPolygon(CSGPolygon _polygon, List<CSGPolygon> _coplanarFront,
            List<CSGPolygon> _coplanarBack, List<CSGPolygon> _front, List<CSGPolygon> _back)
        {
            // Classify each point as well as the entire polygon into one of the above
            // four classes.
            PolygonType polygonType = 0;
            List<PolygonType> types = new List<PolygonType>();

            for (int i = 0; i < _polygon.m_vertices.Count; i++)
            {
                float t = Vector3.Dot(m_normal, _polygon.m_vertices[i].m_position) - m_planeWidth;
                PolygonType type = (t < -CSG.EPSILON) ? PolygonType.Back : ((t > CSG.EPSILON) ?
                    PolygonType.Front : PolygonType.Coplanar);
                polygonType |= type;
                types.Add(type);
            }

            // Put the polygon in the correct list, splitting it when necessary.
            switch (polygonType)
            {
                case PolygonType.Coplanar:
                    {
                        if (Vector3.Dot(m_normal, _polygon.m_plane.m_normal) > 0)
                        {
                            _coplanarFront.Add(_polygon);
                        }
                        else
                        {
                            _coplanarBack.Add(_polygon);
                        }
                    }
                    break;

                case PolygonType.Front:
                    {
                        _front.Add(_polygon);
                    }
                    break;

                case PolygonType.Back:
                    {
                        _back.Add(_polygon);
                    }
                    break;

                case PolygonType.Spanning:
                    {
                        List<CSGVertex> front = new List<CSGVertex>();
                        List<CSGVertex> back = new List<CSGVertex>();
                        
                        for (int vertexIter = 0; vertexIter < _polygon.m_vertices.Count; vertexIter++)
                        {
                            int nextVertexIndex = (vertexIter + 1) % _polygon.m_vertices.Count;

                            PolygonType currentType = types[vertexIter];
                            PolygonType nextType = types[nextVertexIndex];

                            CSGVertex currentVertex = _polygon.m_vertices[vertexIter];
                            CSGVertex nextVertex = _polygon.m_vertices[nextVertexIndex];

                            if (currentType != PolygonType.Back)
                            {
                                front.Add(currentVertex);
                            }

                            if (currentType != PolygonType.Front)
                            {
                                back.Add(currentVertex);
                            }

                            if ((currentType | nextType) == PolygonType.Spanning)
                            {
                                float step = (m_planeWidth - Vector3.Dot(m_normal, currentVertex.m_position)) / Vector3.Dot(m_normal, nextVertex.m_position - currentVertex.m_position);

                                CSGVertex vertex = CSGVertex.Interpolate(currentVertex, nextVertex, step);

                                front.Add(vertex);
                                back.Add(vertex);
                            }
                        }

                        if (front.Count >= 3)
                        {
                            _front.Add(new CSGPolygon(front));
                        }

                        if (back.Count >= 3)
                        {
                            _back.Add(new CSGPolygon(back));
                        }
                    }
                    break;
            } 
        }
	}
}
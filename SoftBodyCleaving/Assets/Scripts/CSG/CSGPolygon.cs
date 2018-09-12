using UnityEngine;
using System.Collections.Generic;

namespace CSG
{
	public class CSGPolygon
	{
		public List<CSGVertex> m_vertices;
		public CSGPlane m_plane;

		public CSGPolygon(List<CSGVertex> _vertices)
		{
			m_vertices = _vertices;
			m_plane = new CSGPlane(_vertices[0].m_position, _vertices[1].m_position, _vertices[2].m_position);
		}

		public void Flip()
		{
			m_vertices.Reverse();

			for(int i = 0; i < m_vertices.Count; i++)
				m_vertices[i].Flip();

			m_plane.Flip();
		}
	}
}
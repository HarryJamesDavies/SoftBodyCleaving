// Original CSG.JS library by Evan Wallace (http://madebyevan.com), under the MIT license.
// GitHub: https://github.com/evanw/csg.js/

using System.Collections.Generic;

namespace CSG
{
	public class CSGPolygon
	{
		public List<CSGVertex> m_vertices;
		public CSGPlane m_plane;

        public CSGPolygon(List<CSGVertex> _vertices, bool _flip = false)
        {
            m_vertices = _vertices;
            if (_flip)
            {
                m_vertices.Reverse();
            }
            m_plane = new CSGPlane(_vertices[0].m_position, _vertices[1].m_position, _vertices[2].m_position);
        }

        public void Flip()
		{
            //List<CSGVertex> vertices = m_vertices.GetRange(0, m_vertices.Count);
            //m_vertices.Clear();
            //m_vertices.Add(vertices[0]);
            //m_vertices.Add(vertices[2]);
            //m_vertices.Add(vertices[1]);
            //vertices.Clear();

            m_vertices.Reverse();

            for (int vertexIter = 0; vertexIter < m_vertices.Count; vertexIter++)
            {
                m_vertices[vertexIter].Flip();
            }

			m_plane.Flip();
		}

        public CSGPolygon DeepClone()
        {
            List<CSGVertex> vertices = new List<CSGVertex>();
            for (int vertexIter = 0; vertexIter < m_vertices.Count; vertexIter++)
            {
                vertices.Add(m_vertices[vertexIter].DeepClone());
            }

            return new CSGPolygon(vertices);
        }
	}
}
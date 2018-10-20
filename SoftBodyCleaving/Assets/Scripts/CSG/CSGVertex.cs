// Original CSG.JS library by Evan Wallace (http://madebyevan.com), under the MIT license.
// GitHub: https://github.com/evanw/csg.js/

using UnityEngine;

namespace CSG
{
	public class CSGVertex
	{
		public Vector3 m_position;
		public Color m_color;
		public Vector3 m_normal;
        public Vector3 m_sharedNormal;
		public Vector2 m_uv;

        public CSGVertex()
        {
            m_position = Vector3.zero;
            m_color = Color.white;
            m_normal = Vector3.up;
            m_sharedNormal = Vector3.up;
            m_uv = Vector2.one;
        }

		public CSGVertex(Vector3 _position, Vector3 _normal, Vector2 _uv, Color _color)
		{
			m_position = _position;
            m_sharedNormal = _normal;
            m_normal = m_sharedNormal.normalized;
			m_uv = _uv;
			m_color = _color;
		}

		public void Flip()
		{
			m_normal *= -1.0f;
            m_sharedNormal = m_normal.normalized;
		}

		public static CSGVertex Interpolate(CSGVertex _vertexA, CSGVertex _vertexB, float _step)
		{
			CSGVertex result = new CSGVertex();

            result.m_position = Vector3.Lerp(_vertexA.m_position, _vertexB.m_position, _step);
            result.m_normal = Vector3.Lerp(_vertexA.m_normal, _vertexB.m_normal, _step);
            result.m_uv = Vector2.Lerp(_vertexA.m_uv, _vertexB.m_uv, _step);
            result.m_color = (_vertexA.m_color + _vertexB.m_color) / 2f;

			return result;
		}

        /// <summary>
        /// Normal must be normalised first
        /// </summary>
        /// <param name="_normal"></param>
        public void AddToSharedNormal(Vector3 _normal)
        {
            m_sharedNormal += _normal;
            m_normal = m_sharedNormal.normalized;
        }

        public void SetNormal(Vector3 _normal)
        {
            m_sharedNormal = _normal;
            m_normal = m_sharedNormal.normalized;
        }

        public void SetPosition(Vector3 _position)
        {
            m_position.Set(_position.x, _position.y, _position.z);
        }
	}
}
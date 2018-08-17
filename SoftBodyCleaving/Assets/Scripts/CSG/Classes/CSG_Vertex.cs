using UnityEngine;

namespace Parabox.CSG
{
	/**
	 * Represents a single mesh vertex.  Contains fields for position, color, normal, and textures.
	 */
	public class CSG_Vertex
	{
		public Vector3 m_position;
		public Color m_color;
		public Vector3 m_normal;
        public Vector3 m_sharedNormal;
		public Vector2 m_uv;

        public CSG_Vertex()
        {
            m_position = Vector3.zero;
            m_color = Color.white;
            m_normal = Vector3.up;
            m_sharedNormal = Vector3.up;
            m_uv = Vector2.one;
        }

		public CSG_Vertex(Vector3 _position, Vector3 _normal, Vector2 _uv, Color _color)
		{
			m_position = _position;
            m_sharedNormal = _normal;
            m_normal = m_sharedNormal.normalized;
			m_uv = _uv;
			m_color = _color;
		}

		public void Flip()
		{
			m_normal *= -1f;
		}

		// Create a new vertex between this vertex and `other` by linearly
		// interpolating all properties using a parameter of `t`. Subclasses should
		// override this to interpolate additional properties.
		public static CSG_Vertex Interpolate(CSG_Vertex a, CSG_Vertex b, float t)
		{
			CSG_Vertex ret = new CSG_Vertex();

			ret.m_position = Vector3.Lerp(a.m_position, b.m_position, t);
			ret.m_normal = Vector3.Lerp(a.m_normal, b.m_normal, t);
			ret.m_uv = Vector2.Lerp(a.m_uv, b.m_uv, t);
			ret.m_color = (a.m_color + b.m_color) / 2f;

			return ret;
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
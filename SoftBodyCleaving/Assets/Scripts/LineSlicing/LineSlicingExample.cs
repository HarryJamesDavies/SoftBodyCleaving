using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSlicingExample : MonoBehaviour
{
    public bool m_makeSoftBody = true;
    public bool m_slicing = false;
    public Vector3 m_sliceStartPosition = Vector3.zero;
    public Vector3 m_sliceEndPosition = Vector3.zero;

	void Update ()
    {
		if(!m_slicing && Input.GetMouseButtonDown(0))
        {
            m_slicing = true;
            m_sliceStartPosition = Input.mousePosition;
        }

        if(m_slicing && Input.GetMouseButtonUp(0))
        {
            m_slicing = false;
            m_sliceEndPosition = Input.mousePosition;
            LineSlicer.CutChains(m_sliceStartPosition, m_sliceEndPosition, m_makeSoftBody);
        }
    }
}

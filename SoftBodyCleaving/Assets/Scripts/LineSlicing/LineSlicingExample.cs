using MSM;
using UnityEngine;

public class LineSlicingExample : MonoBehaviour
{
    [SerializeField] private bool m_makeSoftBody = true;

    [SerializeField] private SoftBodySettings m_settings = new SoftBodySettings();

    private bool m_slicing = false;
    private Vector3 m_sliceStartPosition = Vector3.zero;
    private Vector3 m_sliceEndPosition = Vector3.zero;


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

            if (m_makeSoftBody)
            {
                LineSlicer.CutChains(m_sliceStartPosition, m_sliceEndPosition, m_settings);
            }
            else
            {
                LineSlicer.CutChains(m_sliceStartPosition, m_sliceEndPosition);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeRecord : MonoBehaviour
{
    private static bool m_captureTime = false;
    private static bool m_releaseTime = false;

    public static void CaptureTime()
    {
        m_captureTime = true;
    }

    private void Update()
    {
        if(m_releaseTime)
        {
            m_releaseTime = false;
            Debug.Log(Time.deltaTime);
        }
    }

    private void LateUpdate()
    {
        if(m_captureTime)
        {
            m_releaseTime = true;
            m_captureTime = false;
        }
    }
}

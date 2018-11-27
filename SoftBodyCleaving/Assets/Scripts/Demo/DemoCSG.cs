using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoCSG : MonoBehaviour
{
    public TMPro.TMP_Dropdown m_sceneDropDown;

    public bool m_threeDimension = true;
    public ThreeDimCSGTest m_csg3D;
    public TwoDimCSGTest m_csg2D;

    public void DoOperation()
    {
        if(m_threeDimension)
        {
            m_csg3D.SetOperation((CSG.BooleanOperations)m_sceneDropDown.value);
        }
        else
        {
            m_csg2D.SetOperation((CSG.BooleanOperations)m_sceneDropDown.value);
        }
    }
}

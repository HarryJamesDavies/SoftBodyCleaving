using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProcessState
{
    Waiting,
    Set,
    Processing
}

public class CSGTest : MonoBehaviour
{
    public GameObject objectA;
    public GameObject objectB;
    public GameObject composite;

    public ProcessState m_state = ProcessState.Waiting;
    public ComputeShader m_gpuSoftBody;
    private bool m_active = false;

    void OnEnable ()
    {
        m_active = true;
        Debug.Log("Active");
    }

    private void Update()
    {
        switch (m_state)
        {
            case ProcessState.Waiting:
                {
                    if (m_active)
                    {
                        SoftBodyCore coreA = objectA.GetComponent<SoftBodyCore>();
                        if (coreA)
                        {
                            coreA.SetEnabled(false);
                        }

                        SoftBodyCore coreB = objectB.GetComponent<SoftBodyCore>();
                        if (coreB)
                        {
                            coreB.SetEnabled(false);
                        }

                        m_state = ProcessState.Set;
                        m_active = false;
                    }
                    break;
                }
            case ProcessState.Set:
                {
                    m_state = ProcessState.Processing;
                    break;
                }
            case ProcessState.Processing:
                {
                    composite = new GameObject();
                    MeshFilter filter = composite.AddComponent<MeshFilter>();
                    filter.sharedMesh = Parabox.CSG.CSG.Subtract(objectA, objectB);
                    composite.AddComponent<MeshRenderer>().sharedMaterial = objectA.GetComponent<MeshRenderer>().sharedMaterial;

                    //SoftBodyCore core = composite.AddComponent<SoftBodyCore>();
                    //SoftBodyMesh mesh = composite.AddComponent<SoftBodyMesh>();

                    //core.m_softBody = mesh;
                    //core.m_gpuSoftBody = m_gpuSoftBody;

                    //mesh.m_core = core;
                    //mesh.m_meshFilter = filter;

                    GameObject.Destroy(objectA);
                    GameObject.Destroy(objectB);

                    m_state = ProcessState.Waiting;
                    break;
                }
        }
    }
}

using System.Linq;
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
    public List<GameObject> composites;

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
                    List<Mesh> resultantMeshes = Parabox.CSG.CSG.Subtract(objectA, objectB);
                    for (int meshIter = 0; meshIter < resultantMeshes.Count; meshIter++)
                    {
                        CreateObjectFromMesh(resultantMeshes[meshIter]);
                    }

                    for (int meshIter = 0; meshIter < composites.Count; meshIter++)
                    {
                        Parabox.CSG.CSG.RealignMeshToAveragePosition(composites[meshIter]);
                    }

                    GameObject.Destroy(objectA);
                    GameObject.Destroy(objectB);

                    m_state = ProcessState.Waiting;
                    break;
                }
        }
    }

    private void CreateObjectFromMesh(Mesh _mesh)
    {
        composites.Add(new GameObject());
        MeshFilter filter = composites.Last().AddComponent<MeshFilter>();
        filter.sharedMesh = _mesh;
        composites.Last().AddComponent<MeshRenderer>().sharedMaterial = objectA.GetComponent<MeshRenderer>().sharedMaterial;
    }
}

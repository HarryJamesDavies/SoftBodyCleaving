using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using CSG;

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
    
    private ProcessState m_state = ProcessState.Waiting;

    private bool m_active = false;

    void OnEnable ()
    {
        m_active = true;
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
                    List<Mesh> resultantMeshes = CSG.CSG.Subtract(objectA, objectB);

                    List<GameObject> resultantObjects = new List<GameObject>();

                    for (int meshIter = 0; meshIter < resultantMeshes.Count; meshIter++)
                    {
                        resultantObjects.Add(CreateObjectFromMesh(resultantMeshes[meshIter]));
                    }

                    //for (int meshIter = 0; meshIter < composites.Count; meshIter++)
                    //{
                    //    CSG.CSG.RealignMeshToAveragePosition(composites[meshIter]);
                    //}

                    Destroy(objectA);
                    Destroy(objectB);

                    m_state = ProcessState.Waiting;

                    for (int resultantObjectIter = 0; resultantObjectIter < resultantObjects.Count; resultantObjectIter++)
                    {
                        MSM.MSM.MakeObjectSoftbody3D(resultantObjects[resultantObjectIter]);
                    }
                    
                    break;
                }
        }
    }

    private GameObject CreateObjectFromMesh(Mesh _mesh)
    {
        composites.Add(new GameObject());
        MeshFilter filter = composites.Last().AddComponent<MeshFilter>();
        filter.sharedMesh = _mesh;
        composites.Last().AddComponent<MeshRenderer>().sharedMaterial = objectA.GetComponent<MeshRenderer>().sharedMaterial;
        return composites.Last();
    }
}

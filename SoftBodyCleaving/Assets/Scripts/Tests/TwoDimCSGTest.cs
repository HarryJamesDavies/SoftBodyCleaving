using MSM;
using System.Collections.Generic;
using UnityEngine;

public class TwoDimCSGTest : MonoBehaviour
{
    [SerializeField] private GameObject m_componentA;
    [SerializeField] private GameObject m_componentB;

    [SerializeField] private CSG.BooleanOperations m_currentOperation = CSG.BooleanOperations.None;
    private CSG.BooleanOperations m_previousOperation = CSG.BooleanOperations.None;

    [SerializeField] private SoftBodySettings m_sbSettings = new SoftBodySettings();
    [SerializeField] private CSGSettings m_csgSettings = new CSGSettings();
    [SerializeField] private CSGMeshingSettings m_csgMeshSettings = new CSGMeshingSettings();

    private void Update()
    {
        if (m_currentOperation != m_previousOperation)
        {
            List<Mesh> resultantMeshes = PreformOperation(m_currentOperation);            
            MakeObjectsFromMeshes(resultantMeshes);

            if (m_csgSettings.m_destroyComponents)
            {
                Destroy(m_componentA);
                Destroy(m_componentB);
            }

            m_previousOperation = m_currentOperation;
        }
    }

    private List<Mesh> PreformOperation(CSG.BooleanOperations _operation)
    {
        if (m_csgSettings.m_makeUniqueMeshes)
        {
            MeshFilter filterA = m_componentA.GetComponent<MeshFilter>();
            Mesh meshA = (Mesh)Instantiate(filterA.sharedMesh);
            filterA.mesh = meshA;
            MeshFilter filterB = m_componentB.GetComponent<MeshFilter>();
            Mesh meshB = (Mesh)Instantiate(filterB.sharedMesh);
            filterB.mesh = meshB;
        }

        List<Mesh> resultantMeshes = new List<Mesh>();
        switch (_operation)
        {
            case CSG.BooleanOperations.None:
                {
                    break;
                }
            case CSG.BooleanOperations.Union:
                {
                    SetOperation(CSG.BooleanOperations.None);
                    resultantMeshes.AddRange(CSG.CSG.Union(m_componentA, m_componentB, m_csgMeshSettings));
                    break;
                }
            case CSG.BooleanOperations.Subtract:
                {
                    SetOperation(CSG.BooleanOperations.None);
                    resultantMeshes.AddRange(CSG.CSG.Subtract(m_componentA, m_componentB, m_csgMeshSettings));
                    break;
                }
            case CSG.BooleanOperations.Intersect:
                {
                    SetOperation(CSG.BooleanOperations.None);
                    resultantMeshes.AddRange(CSG.CSG.Intersect(m_componentA, m_componentB, m_csgMeshSettings));
                    break;
                }
            case CSG.BooleanOperations.NVUnion:
                {
                    SetOperation(CSG.BooleanOperations.None);
                    resultantMeshes.AddRange(CSG.CSG.NVUnion(m_componentA, m_componentB, m_csgMeshSettings));
                    break;
                }
            case CSG.BooleanOperations.NVSubtract:
                {
                    SetOperation(CSG.BooleanOperations.None);
                    resultantMeshes.AddRange(CSG.CSG.NVSubtract(m_componentA, m_componentB, m_csgMeshSettings));
                    break;
                }
            case CSG.BooleanOperations.HVUnion:
                {
                    SetOperation(CSG.BooleanOperations.None);
                    resultantMeshes.AddRange(CSG.CSG.HVUnion(m_componentA, m_componentB, m_csgMeshSettings));
                    break;
                }
            case CSG.BooleanOperations.HVSubtract:
                {
                    SetOperation(CSG.BooleanOperations.None);
                    resultantMeshes.AddRange(CSG.CSG.HVSubtract(m_componentA, m_componentB, m_csgMeshSettings));
                    break;
                }
            case CSG.BooleanOperations.HVIntersect:
                {
                    SetOperation(CSG.BooleanOperations.None);
                    resultantMeshes.AddRange(CSG.CSG.HVIntersect(m_componentA, m_componentB, m_csgMeshSettings));
                    break;
                }
            default:
                {
                    break;
                }
        }
        return resultantMeshes;
    }

    private void MakeObjectsFromMeshes(List<Mesh> _meshes)
    {
        for (int meshIter = 0; meshIter < _meshes.Count; meshIter++)
        {
            GameObject newObject = CSG.CSG.CreateObjectFromMesh(_meshes[meshIter],
                m_componentA.GetComponent<MeshRenderer>().sharedMaterial);

            if (m_csgSettings.m_makeSoftBodies)
            {
                MSM.MSM.MakeObjectSoftbodyObject(newObject, m_sbSettings);
            }
        }
    }

    private void SetOperation(CSG.BooleanOperations _operation)
    {
        m_previousOperation = m_currentOperation;
        m_currentOperation = _operation;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSM
{
    public class MSM
    {
        public static void MakeObjectSoftbodyChain(GameObject _object, Chain _chain, SoftBodySettings _settings)
        {
            SoftBodyMesh mesh = _object.AddComponent<SoftBodyMesh>();
            mesh.Initialise(_object.GetComponent<MeshFilter>(), _settings);
            mesh.CreateSoftBodyFromChain(_chain);

            SoftBodyCore core = _object.AddComponent<SoftBodyCore>();
            core.Initialise(mesh);
        }

        public static void MakeObjectSoftbodyObject(GameObject _object, SoftBodySettings _settings)
        {
            SoftBodyMesh mesh = _object.AddComponent<SoftBodyMesh>();
            mesh.Initialise(_object.GetComponent<MeshFilter>(), _settings);
            mesh.CreateSoftBodyFromMesh();

            SoftBodyCore core  = _object.AddComponent<SoftBodyCore>();
            core.Initialise(mesh);
        }
    }
}

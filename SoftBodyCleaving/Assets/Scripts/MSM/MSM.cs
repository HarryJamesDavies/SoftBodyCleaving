﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSM
{
    public class MSM
    {
        public static void MakeObjectSoftbody1D(GameObject _object, List<VertexGroup> _groups)
        {
            SoftBodyMesh mesh = _object.AddComponent<SoftBodyMesh>();
            mesh.Initialise(_object.GetComponent<MeshFilter>());
            mesh.Create1DSoftBodyFromGroups(_groups);

            SoftBodyCore core = _object.AddComponent<SoftBodyCore>();
            core.Initialise(mesh);
        }

        public static void MakeObjectSoftbody3D(GameObject _object)
        {
            SoftBodyMesh mesh = _object.AddComponent<SoftBodyMesh>();
            mesh.Initialise(_object.GetComponent<MeshFilter>());
            mesh.Create3DSoftBodyFromMesh();

            SoftBodyCore core  = _object.AddComponent<SoftBodyCore>();
            core.Initialise(mesh);
        }
    }
}
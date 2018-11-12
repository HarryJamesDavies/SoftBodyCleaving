using MSM;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionData
{
    public int m_intersectionPoint;
    public float m_intersectionDelta;
}

public class IntersectionResult
{
    public Chain m_upperChain;
    public Chain m_lowerChain;
}

public class LineSlicer
{
    public static void CutChains(Vector3 _sliceStartPosition, Vector3 _sliceEndPosition, SoftBodySettings _settings = null)
    {
        List<Chain> chains = ChainPool.s_instance.GetChains();
        Dictionary<int, IntersectionResult> results = new Dictionary<int, IntersectionResult>();

        for (int chainIter = 0; chainIter < chains.Count; chainIter++)
        {
            IntersectionResult result = CutChain(chains[chainIter], _sliceStartPosition, _sliceEndPosition);
            if (result != null)
            {
                results.Add(chainIter, result);
                if (_settings != null)
                {
                    MSM.MSM.MakeObjectSoftbodyChain(result.m_upperChain.gameObject, result.m_upperChain, _settings);
                    result.m_upperChain.gameObject.AddComponent<MeshExamine>().m_mesh = result.m_upperChain.gameObject.GetComponent<MeshFilter>();

                    MSM.MSM.MakeObjectSoftbodyChain(result.m_lowerChain.gameObject, result.m_lowerChain, _settings);
                    result.m_lowerChain.gameObject.AddComponent<MeshExamine>().m_mesh = result.m_lowerChain.gameObject.GetComponent<MeshFilter>();
                }
            }

        }

        List<Chain> resultingChains = new List<Chain>();
        for (int chainIter = 0; chainIter < chains.Count; chainIter++)
        {
            if (results.ContainsKey(chainIter))
            {
                resultingChains.Add(results[chainIter].m_upperChain);
                resultingChains.Add(results[chainIter].m_lowerChain);
                ChainPool.s_instance.DestroyChain(chainIter);
            }
            else
            {
                resultingChains.Add(chains[chainIter]);
            }
        }

        ChainPool.s_instance.ReplacePool(resultingChains);
    }

    private static IntersectionResult CutChain(Chain _chain, Vector3 _sliceStartPosition, Vector3 _sliceEndPosition)
    {
        bool intersects = false;
        IntersectionData data = new IntersectionData();

        for (int lineIter = 0; lineIter < _chain.m_corePoints.Count - 1; lineIter++)
        {
            float cutDelta = GetIntersectionPointDelta(_sliceStartPosition, _sliceEndPosition,
                Camera.main.WorldToScreenPoint(_chain.m_corePoints[lineIter]),
                Camera.main.WorldToScreenPoint(_chain.m_corePoints[lineIter + 1]), out intersects);

            if(intersects)
            {
                data.m_intersectionPoint = lineIter;
                data.m_intersectionDelta = cutDelta;
                break;
            }
        }

        if (intersects)
        {
            return SplitChain(_chain, data);
        }
        return null;
    }

    private static IntersectionResult SplitChain(Chain _chain, IntersectionData data)
    {
        bool backface = (_chain.m_vertexGroups[0].m_vertices.Count > 2);

        int lowerChainFirstIndex = data.m_intersectionPoint + 1;

        Vector3 coreDelta = (_chain.m_corePoints[lowerChainFirstIndex]
            - _chain.m_corePoints[data.m_intersectionPoint]);
        Vector3 corePoint = _chain.m_corePoints[data.m_intersectionPoint]
            + (coreDelta * data.m_intersectionDelta);

        Vector3 leftDelta = _chain.m_mesh.vertices[_chain.m_vertexGroups[lowerChainFirstIndex].m_vertices[0]] -
            _chain.m_mesh.vertices[_chain.m_vertexGroups[data.m_intersectionPoint].m_vertices[0]];
        Vector3 leftCutPosition = _chain.m_mesh.vertices[_chain.m_vertexGroups[data.m_intersectionPoint].m_vertices[0]] +
            (leftDelta * data.m_intersectionDelta);

        Vector3 rightDelta = _chain.m_mesh.vertices[_chain.m_vertexGroups[lowerChainFirstIndex].m_vertices[1]] -
            _chain.m_mesh.vertices[_chain.m_vertexGroups[data.m_intersectionPoint].m_vertices[1]];
        Vector3 rightCutPosition = _chain.m_mesh.vertices[_chain.m_vertexGroups[data.m_intersectionPoint].m_vertices[1]] +
            (rightDelta * data.m_intersectionDelta);
        
        int upperVertexIntersection = (data.m_intersectionPoint + 1) * 2;
        if (upperVertexIntersection < 0)
        {
            upperVertexIntersection = 0;
        }

        /////////////////////////////////////////////////////////////////////////////

        Chain upperChain = new GameObject("UpperChain").AddComponent<Chain>();
        upperChain.transform.position = _chain.transform.position;
        MeshFilter upperFilter = upperChain.gameObject.AddComponent<MeshFilter>();
        MeshRenderer upperRender = upperChain.gameObject.AddComponent<MeshRenderer>();
        upperRender.sharedMaterial = _chain.gameObject.GetComponent<MeshRenderer>().sharedMaterial;

        List<Vector3> upperCorePoints = _chain.m_corePoints.GetRange(0, lowerChainFirstIndex);
        upperCorePoints.Add(corePoint);

        Mesh upperMesh = SplitMesh(_chain, 0, data.m_intersectionPoint,
            leftCutPosition, rightCutPosition, true);
        upperMesh.vertices[upperVertexIntersection] = leftCutPosition;
        upperMesh.vertices[upperVertexIntersection + 1] = rightCutPosition;

        if (backface)
        {
            upperMesh.vertices[upperVertexIntersection] = leftCutPosition;
            upperMesh.vertices[upperVertexIntersection + 1] = rightCutPosition;
        }
        upperMesh.RecalculateNormals();

        List<VertexGroup> upperVertexGroups = _chain.m_vertexGroups.GetRange(0, lowerChainFirstIndex);

        if (backface)
        {
            int upperBackSideStartDelta = upperVertexGroups[0].m_vertices[2] - (upperCorePoints.Count * 2);

            for (int groupIter = 0; groupIter < upperVertexGroups.Count; groupIter++)
            {
                for (int vertexIter = 2; vertexIter < upperVertexGroups[groupIter].m_vertices.Count; vertexIter++)
                {
                    upperVertexGroups[groupIter].m_vertices[vertexIter] -= upperBackSideStartDelta;
                }
            }
        }

        VertexGroup upperGroup = new VertexGroup();
        upperGroup.SetAveragePosition(corePoint);
        upperGroup.m_vertices.Add(upperVertexIntersection);
        upperGroup.m_vertices.Add(upperVertexIntersection + 1);

        if (_chain.m_mesh.vertexCount > _chain.m_corePoints.Count * 2)
        {
            upperGroup.m_vertices.Add(upperMesh.vertexCount - 2);
            upperGroup.m_vertices.Add(upperMesh.vertexCount - 1);
        }

        upperVertexGroups.Add(upperGroup);

        upperChain.Initialise(upperCorePoints, upperVertexGroups, upperMesh);
        upperFilter.sharedMesh = upperMesh;

        ///////////////////////////////////////////////////////////////////////////////

        Chain lowerChain = new GameObject("LowerChain").AddComponent<Chain>();
        lowerChain.transform.position = _chain.transform.position;
        MeshFilter lowerFilter = lowerChain.gameObject.AddComponent<MeshFilter>();
        MeshRenderer lowerRender = lowerChain.gameObject.AddComponent<MeshRenderer>();
        lowerRender.sharedMaterial = _chain.gameObject.GetComponent<MeshRenderer>().sharedMaterial;

        List<Vector3> lowerCorePoints = _chain.m_corePoints.GetRange(lowerChainFirstIndex,
            _chain.m_corePoints.Count - lowerChainFirstIndex); 
        lowerCorePoints.Insert(0, corePoint);

        int lastVertexGroup = _chain.m_corePoints.Count - 1;

        Mesh lowerMesh = SplitMesh(_chain, lowerChainFirstIndex,
            lastVertexGroup, leftCutPosition, rightCutPosition, false);
        lowerMesh.vertices[0] = leftCutPosition;
        lowerMesh.vertices[1] = rightCutPosition;

        if (backface)
        {
            lowerMesh.vertices[(_chain.m_corePoints.Count - lowerChainFirstIndex)] = leftCutPosition;
            lowerMesh.vertices[(_chain.m_corePoints.Count - lowerChainFirstIndex) + 1] = rightCutPosition;
        }
        lowerMesh.RecalculateNormals();

        List<VertexGroup> lowerVertexGroups = _chain.m_vertexGroups.GetRange(lowerChainFirstIndex,
            _chain.m_corePoints.Count - lowerChainFirstIndex);
        
        int lowerStartDelta = data.m_intersectionPoint * 2;
        int lowerBackStartDelta = 0;

        if(backface)
        {
            lowerBackStartDelta = (data.m_intersectionPoint - 1) * 4;
        }

        for (int groupIter = 0; groupIter < lowerVertexGroups.Count; groupIter++)
        {
            for (int vertexIter = 0; vertexIter < lowerVertexGroups[groupIter].m_vertices.Count; vertexIter++)
            {
                lowerVertexGroups[groupIter].m_vertices[vertexIter] -= lowerStartDelta;

                if (vertexIter > 1)
                {
                    lowerVertexGroups[groupIter].m_vertices[vertexIter] -= lowerBackStartDelta;
                }
            }
        }

        VertexGroup lowerGroup = new VertexGroup();
        lowerGroup.SetAveragePosition(corePoint);
        lowerGroup.m_vertices.Add(0);
        lowerGroup.m_vertices.Add(1);

        if (backface)
        {
            lowerGroup.m_vertices.Add((lowerMesh.vertexCount / 2));
            lowerGroup.m_vertices.Add((lowerMesh.vertexCount / 2) + 1);
        }

        lowerVertexGroups.Insert(0, lowerGroup);

        lowerChain.Initialise(lowerCorePoints, lowerVertexGroups, lowerMesh);
        lowerFilter.sharedMesh = lowerMesh;

        ////////////////////////////////////////////////////////////////////////

        IntersectionResult result = new IntersectionResult();
        result.m_upperChain = upperChain;
        result.m_lowerChain = lowerChain;
        return result;
    }

    private static Mesh SplitMesh(Chain _chain, int _firstVertexGroup, int _lastVertexGroup,
        Vector3 _leftCutPosition, Vector3 _rightCutPosition, bool _upper)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        if(!_upper)
        {
            vertices.Add(_leftCutPosition);
            normals.Add(Vector3.one);

            vertices.Add(_rightCutPosition);
            normals.Add(Vector3.one);
        }

        for (int groupIter = _firstVertexGroup; groupIter <= _lastVertexGroup; groupIter++)
        {
            vertices.Add(_chain.m_mesh.vertices[_chain.m_vertexGroups[groupIter].m_vertices[0]]);
            normals.Add(Vector3.one);

            vertices.Add(_chain.m_mesh.vertices[_chain.m_vertexGroups[groupIter].m_vertices[1]]);
            normals.Add(Vector3.one);
        }

        if (_upper)
        {
            vertices.Add(_leftCutPosition);
            normals.Add(Vector3.one);

            vertices.Add(_rightCutPosition);
            normals.Add(Vector3.one);
        }

        for (int groupIter = 0; groupIter < (_lastVertexGroup - _firstVertexGroup) + 1; groupIter++)
        {
            int vertexStart = groupIter * 2;

            triangles.Add(vertexStart + 1);
            triangles.Add(vertexStart);
            triangles.Add(vertexStart + 2);

            triangles.Add(vertexStart + 1);
            triangles.Add(vertexStart + 2);
            triangles.Add(vertexStart + 3);
        }

        //BackFace
        if (_chain.m_mesh.vertexCount > _chain.m_corePoints.Count * 2)
        {
            int startingVertexCount = vertices.Count;
            for (int vertexIter = 0; vertexIter < startingVertexCount; vertexIter++)
            {
                vertices.Add(vertices[vertexIter]);
                normals.Add(Vector3.one);
            }

            for (int groupIter = 0; groupIter < (_lastVertexGroup - _firstVertexGroup) + 1; groupIter++)
            {
                int vertexStart = (groupIter * 2) + startingVertexCount;

                triangles.Add(vertexStart);
                triangles.Add(vertexStart + 1);
                triangles.Add(vertexStart + 2);

                triangles.Add(vertexStart + 2);
                triangles.Add(vertexStart + 1);
                triangles.Add(vertexStart + 3);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }

    public static Vector2 GetIntersectionPointPosition(Vector2 _lineAStart, Vector2 _lineAEnd, Vector2 _lineBStart, Vector2 _lineBEnd, out bool _doIntersect)
    {
        float lineDelta = (_lineBEnd.x - _lineBStart.x) * (_lineAEnd.y - _lineAStart.y) - (_lineBEnd.y - _lineBStart.y) * (_lineAEnd.x - _lineAStart.x);

        if (lineDelta == 0.0f)
        {
            _doIntersect = false;
            return Vector2.zero;
        }

        float intersectDelta = ((_lineAStart.x - _lineBStart.x) * (_lineAEnd.y - _lineAStart.y) - (_lineAStart.y - _lineBStart.y) * (_lineAEnd.x - _lineAStart.x)) / lineDelta;

        if ((intersectDelta > 1.0f) || (intersectDelta < 0.0f))
        {
            _doIntersect = false;
            return Vector2.zero;
        }

        _doIntersect = true;

        return new Vector2(
            _lineBStart.x + ((_lineBEnd.x - _lineBStart.x) * intersectDelta),
            _lineBStart.y + ((_lineBEnd.y - _lineBStart.y) * intersectDelta));
    }

    public static float GetIntersectionPointDelta(Vector2 _lineAStart, Vector2 _lineAEnd, Vector2 _lineBStart, Vector2 _lineBEnd, out bool _doIntersect)
    {
        float lineDelta = (_lineBEnd.x - _lineBStart.x) * (_lineAEnd.y - _lineAStart.y) - (_lineBEnd.y - _lineBStart.y) * (_lineAEnd.x - _lineAStart.x);

        if (lineDelta == 0.0f)
        {
            _doIntersect = false;
            return 0.0f;
        }

        float intersectDelta = ((_lineAStart.x - _lineBStart.x) * (_lineAEnd.y - _lineAStart.y) - (_lineAStart.y - _lineBStart.y) * (_lineAEnd.x - _lineAStart.x)) / lineDelta;

        if ((intersectDelta > 1.0f) || (intersectDelta < 0.0f))
        {
            _doIntersect = false;
            return 0.0f;
        }

        _doIntersect = true;

        return intersectDelta;
    }
}

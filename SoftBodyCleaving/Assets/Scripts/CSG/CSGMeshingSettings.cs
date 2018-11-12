using System;

[Serializable]
public class CSGMeshingSettings
{
    public bool m_useMeshing = true;
    public bool m_removeOverlaps = true;
    public int m_overlappingRounding = 2;
    public bool m_waterTightMesh = true;
    public bool m_useInterpolation = true;
    public int m_proofingInterpolation = 1;
    public bool m_subdivideModel = true;
}

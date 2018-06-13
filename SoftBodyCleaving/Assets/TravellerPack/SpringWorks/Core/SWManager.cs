using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SWManager : MonoBehaviour
{
    public static SWManager Instance = null;
    public List<int> m_meshIds = new List<int>();

    void Awake()
    {
        if(Instance)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    public int GenerateID()
    {
        int ID = Random.Range(0, 10000);

        while (CheckID(ID))
        {
            ID = Random.Range(0, 10000);
        }

        m_meshIds.Add(ID);

        return ID;
    }

    bool CheckID(int _ID)
    {
        foreach (int ID in m_meshIds)
        {
            if (ID == _ID)
            {
                return true;
            }
        }

        return false;
    }

    public void RemoveID(int _ID)
    {
        m_meshIds.Remove(_ID);
    }
}

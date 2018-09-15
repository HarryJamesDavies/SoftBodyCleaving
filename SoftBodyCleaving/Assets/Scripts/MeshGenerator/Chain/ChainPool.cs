using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainPool : MonoBehaviour
{
    public static ChainPool s_instance = null;
    public List<Chain> m_pool = new List<Chain>();

    private void Awake()
    {
        CreateInstance();
    }

    private void CreateInstance()
    {
        if(s_instance)
        {
            DestroyImmediate(this);
        }
        else
        {
            s_instance = this;
        }
    }

    public void AddToPool(Chain _chain)
    {
        m_pool.Add(_chain);
    }

    public void ReplacePool(List<Chain> _chains)
    {
        Clear();
        m_pool.AddRange(_chains);
    }


    public Chain GetChain(int _index)
    {
        return m_pool[_index];
    }

    public List<Chain> GetChains()
    {
        return m_pool;
    }

    public void RemoveChain(int _index)
    {
        m_pool.RemoveAt(_index);
    }

    public void Clear()
    {
        m_pool.Clear();
    }

    public void DestroyChain(int _index)
    {
        Destroy(m_pool[_index].gameObject);
    }

}

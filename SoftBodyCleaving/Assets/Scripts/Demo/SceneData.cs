using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class SceneData : MonoBehaviour
{
    public static SceneData s_instance = null;

    public TMPro.TMP_Dropdown m_objectDropDown;
    public Transform m_spawnPoint;

    public List<GameObject> m_scenePrefabs = new List<GameObject>();
    private List<GameObject> m_spawnedObjects = new List<GameObject>();

    void Awake()
    {
        CreateInstance();
    }

    private void CreateInstance()
    {
        if (s_instance)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            s_instance = this;
            return;
        }
    }

    private void OnDestroy()
    {
        //Clear between scenes
        if (s_instance == this)
        {
            s_instance = null;
        }
    }

    void Start()
    {
        SpawnPrefab(m_objectDropDown.value);
    }

    private void SpawnPrefab(int _prefabIndex)
    {
        m_spawnedObjects.Add(GameObject.Instantiate(m_scenePrefabs[_prefabIndex]));
        m_spawnedObjects.Last().transform.position = m_spawnPoint.position;
        m_spawnedObjects.Last().transform.rotation = m_spawnPoint.rotation;
    }

    public void Clear()
    {
        for (int objectIter = 0; objectIter < m_spawnedObjects.Count; objectIter++)
        {
            Destroy(m_spawnedObjects[objectIter]);
        }
        m_spawnedObjects.Clear();
    }

    public void SpawnSelected()
    {
        Clear();
        SpawnPrefab(m_objectDropDown.value);
    }

    public void AddObjectToSpawned(GameObject _object)
    {
        m_spawnedObjects.Add(_object);
    }
}

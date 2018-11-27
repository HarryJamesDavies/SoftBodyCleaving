using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnLateUpdate : MonoBehaviour
{
    private void LateUpdate()
    {
        Destroy(gameObject);
    }
}

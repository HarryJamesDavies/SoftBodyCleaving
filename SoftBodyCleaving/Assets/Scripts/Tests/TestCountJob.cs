using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

public class TestCountJob : MonoBehaviour
{
    public bool triggered = true;

	void Update ()
    {
        if (!triggered)
        {
            TestJob job = new TestJob
            {
                count = 0
            };

            job.Schedule(250, 250);

            Debug.Log(job.count);
            triggered = true;
        }
    }
}

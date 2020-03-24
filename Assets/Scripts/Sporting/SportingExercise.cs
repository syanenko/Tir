using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SportingExercise : MonoBehaviour
{
    public int repeat = 1;
    public float delay = 5;

    [System.Serializable]
    public class TrapRecord
    {
        public Trap trap;
        public float delay;
        public int repeat;
    }

    public TrapRecord[] Traps = new TrapRecord[4];

    private bool isRunning = false;

    //
    // Begin
    //
    public void StartExercise()
    {
        StartCoroutine(DoExercise());
    }

    //
    // Do exercise
    //
    IEnumerator DoExercise()
    {
        Debug.Log("-- Exercise started ...");
        Camera.main.GetComponent<Shot>().StartrShooting();
        isRunning = true;

        for (int i = 0; i < repeat; i++)
        {
            Reset();
            yield return new WaitForSeconds(delay);

            foreach (TrapRecord t in Traps)
            {
                for(int k = 0; k < t.repeat; k++)
                { 
                    if(!isRunning)
                    { 
                        goto Exit;
                    }

                    t.trap.StartTarget();
                    yield return new WaitForSeconds(t.delay);
                }
            }
        }

        Exit: Reset();
        Camera.main.GetComponent<Shot>().StopShooting();
        Debug.Log("-- Exercise finished");
    }

    //
    // End
    //
    public void Reset()
    {
        foreach (TrapRecord tr in Traps)
        {
            tr.trap.Reset();
        }
    }

    public void StopExercise()
    {
        isRunning = false;
    }
}

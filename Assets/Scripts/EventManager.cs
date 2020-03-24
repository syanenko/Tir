using UnityEngine;
using System.Collections;

public class EventManager : MonoBehaviour
{
    public int BrokenPartsNumber = 12;

    static Vector3 dir;
    static Ray ray;

    //
    // Start new attempt (IPSC)
    //
    public delegate void PrepereAttemptAction();
    public static event PrepereAttemptAction prepareAttempt;

    public static void PrepareAttempt()
    {
        if (prepareAttempt != null)
            prepareAttempt();
    }


    //
    // Hide all attempts (IPSC)
    //
    public delegate void ShowAttemptAction(int index);
    public static event ShowAttemptAction showAttempt;

    public static void ShowAttempt(int index)
    {
        if (showAttempt != null)
            showAttempt(index);
    }


    //
    // Shot action
    //
    public delegate void ShotAction(Vector3 shot);
    public static event ShotAction shotAction;

    public static void ShotDone(Vector3 shot)
    {
        if (shotAction != null)
            shotAction(shot);
    }


    //
    // Start
    //
    void Start()
    {
        EventManager.shotAction += ProcessShot;
    }

    //
    // OnDrawGizmos()
    //
    void OnDrawGizmos()
    {
        Gizmos.DrawRay(ray);
    }

    //
    // Procces shot action (Sporting)
    //
    void ProcessShot(Vector3 shot)
    {
        // Search hitted target
        ray = this.GetComponent<Camera>().ScreenPointToRay(shot);

        RaycastHit[] hits = Physics.RaycastAll(ray, 1000);

        if (hits.Length > 0)
        {
            // Select nearest target
            RaycastHit hitMinDistance = hits[0];
            foreach (RaycastHit h in hits)
            {
                if ((h.distance < hitMinDistance.distance))
                {
                    hitMinDistance = h;
                }
            }

            GameObject go = hitMinDistance.transform.gameObject;
            if (go.name == "Clay")
            {
                ParticleSystem ps = go.GetComponent<ParticleSystem>();
                ps.Emit(BrokenPartsNumber);
                go.transform.parent.gameObject.GetComponent<Trap>().Reset();

                Debug.Log("--- ShotProcessSpoting:ProcessShot - Hit: " + shot + "Object: " + hitMinDistance.transform.gameObject.name);
                return;
            } else if (go.tag == "Trap")
            {
                go.GetComponent<Trap>().OnClick();
            }
        }

        // Debug.Log("--- ShotProcessSpoting:ProcessShot - Missed: " + shot);
    }
}
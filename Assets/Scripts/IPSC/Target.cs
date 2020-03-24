using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetType
{
    IPSC_Standard,
    IPSC_Standard_A,
    IPSC_Standard_C,
    IPSC_Standard_D,
    IPSC_Popper,
    IPSC_Circle,
    IPSC_Square,
    Missed
}

//
// Target
// 
public class Target : MonoBehaviour {

    public TargetType type;
    public int points;
    public int penalty;

    public static int currentAttemptIndex = 0;
    public static float startAttemptTime = 0;
    public static float lastHitTime = 0;
    public GameObject currentAttempt;
    public GameObject mark;
    public GameObject missed;

    //
    // Attempts results storage
    //
    public static ArrayList AttemptResults;
    public class Result
    {
        public float time;
        public int points;
        public float hitFactor;

        public Result(float _time, int _points)
        {
            time = _time;
            points = _points;
            hitFactor = (float)points / time;
        }
    };

    private AudioSource audioSource;
    private Quaternion popperStanding = Quaternion.Euler(0, 0, 0);

    //
    // Calculate results by all targets for particular attempt
    //  
    public static void GetResult(out float time, out int points, out float hitFactor, int attempt = -1)
    {
        // Assuming current
        if (attempt == -1)
        {
            // Loop over all targets and do calculation
            GameObject Targets = GameObject.Find("Targets");
            points = 0;
            foreach (Transform child in Targets.transform)
            {
                Target target = child.gameObject.GetComponent<Target>();

                // Special case for poppers
                if (target == null)
                {
                    target = child.transform.GetChild(0).GetComponent<Target>();
                }

                points += target.GetPoints(currentAttemptIndex);
            }

            time = lastHitTime - startAttemptTime;
            Result res = new Result(time, points);
            hitFactor = res.hitFactor;

            AttemptResults.Add(res);
            currentAttemptIndex++;
        }
        else
        {
            time =      ((Result)AttemptResults[attempt]).time;
            points =    ((Result)AttemptResults[attempt]).points;
            hitFactor = ((Result)AttemptResults[attempt]).hitFactor;
        }
    }
        

    //
    // Start
    // 
    void Start()
    {
        Debug.Log("--- Targets:Start()");

        missed = GameObject.Find("Missed");
        if (missed == null)
        {
            Debug.Log("ProcessShot: Error: No object with tag 'Missed' found");
        }

        AttemptResults = new ArrayList();

        // Listen to events
        EventManager.prepareAttempt += PrepareAttempt;
        EventManager.showAttempt    += ShowAttempt;
        EventManager.shotAction     += ProcessShot;
    }


    //
    // Procces shot action
    //
    // TODO: Do it in sepatate method on camera to avoid double-detection !!!
    void ProcessShot(Vector3 shot)
    {
        Debug.Log("--- Targets:ProcessShot:shot: " + shot);

        // Map to world
        shot = Camera.main.ScreenToWorldPoint(shot);
        shot.z = -0.1F; // Move up from the target

        // Search hitted target
        RaycastHit2D[] hits = Physics2D.RaycastAll(shot, Vector2.zero);

        // Todo: process shadowed targets
        if (hits.Length > 0)
        {
            Debug.Log("--- Targets:ProcessShot:hit: " + shot);

            // Select nearest target
            RaycastHit2D hitMinDistance = hits[0];
            foreach (RaycastHit2D h in hits)
            {
                if ((h.distance < hitMinDistance.distance))
                {
                    hitMinDistance = h;
                }
            }

            hitMinDistance.transform.gameObject.GetComponent<Target>().Hit(shot);
        }
        else
        {
            Debug.Log("--- Targets:ProcessShot:missed: " + shot);
            missed.GetComponent<Target>().Hit(shot);
        }
    }


    //
    // Update
    //
    void Update()
    {
    }

    //
    // Sound of hit
    //
    void HitSound()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Play();
    }

    //
    // Prepare new attempt
    //
    void PrepareAttempt()
    {
        ResetTargets();
        HideAllAttempts();

        currentAttempt = new GameObject();
        currentAttempt.name = "A" + (currentAttemptIndex);
        currentAttempt.transform.SetParent(this.gameObject.transform);
    }


    //
    // Start new attempt
    //
    public static void StartAttempt()
    {
        startAttemptTime = Time.time;
    }


    //
    // Repare all targets
    //
    void ResetTargets()
    {
        // Repare poppers
        if (type == TargetType.IPSC_Popper)
        {
            // Back to stand position
            GetComponent<Animator>().Play("Default");
        }
    }

    //
    // Hide attempt
    //
    void HideAttempt(int index)
    {
        string name = "A" + index;
        GameObject att = this.transform.Find(name).gameObject;
        if (att == null)
        {
            Debug.Log("Targets:HideAttempt: can't find target's child with name '" + name + "'");
        } else
        { 
            att.SetActive(false);
        }
    }

    //
    // Hide all attempts
    //
    void HideAllAttempts()
    {
        for (int i = 0; i < currentAttemptIndex; i++)
        {
            string name = "A" + i;
            GameObject att = this.transform.Find(name).gameObject;
            att.SetActive(false);
        }
    }


    //
    // Show attempt
    //
    void ShowAttempt(int index)
    {
        HideAllAttempts();

        string name = "A" + index;
        GameObject att = this.transform.Find(name).gameObject;
        att.SetActive(true);
    }


    //
    // Hit processing
    //
    public void Hit(Vector3 shot)
    {
        Debug.Log("Target hit: " + type + ": " + shot);

        switch (type)
        {
            case TargetType.IPSC_Standard:
            case TargetType.IPSC_Standard_A:
            case TargetType.IPSC_Standard_C:
            case TargetType.IPSC_Standard_D: OnHitStandard(shot); break;

            case TargetType.IPSC_Popper: OnHitPopper(shot); break;
            case TargetType.IPSC_Circle: OnHitCircle(shot); break;
            case TargetType.IPSC_Square: OnHitSquare(shot); break;

            case TargetType.Missed: OnMissed(shot); break;

            default: Debug.Log("Error: Target:Hit: Unknown target type: " + type); break;
        }
    }

    //
    // Standart target hit processing
    //
    void OnHitStandard(Vector3 shot)
    {
        Debug.Log("OnHitStandard: " + shot);
        MarkHit(shot, mark);
    }

    //
    // Popper hit processing
    //
    void OnHitPopper(Vector3 shot)
    {
        Debug.Log("OnHitPopper: " + shot);
        HitSound();
        MarkHit(shot, mark);
        GetComponent<Animator>().Play("Fall");
    }

    //
    // Circel target hit processing
    //
    void OnHitCircle(Vector3 shot)
    {
        Debug.Log("OnHitCircle: " + shot);
        HitSound();
        GetComponent<Animator>().Play("Shake");
        MarkHit(shot, mark);
    }


    //
    // Squere target hit processing
    //
    void OnHitSquare(Vector3 shot)
    {
        Debug.Log("OnHitSquare: " + shot);
        HitSound();
        GetComponent<Animator>().Play("Shake");
        MarkHit(shot, mark);
    }

    // 
    // Missed shot
    //
    void OnMissed(Vector3 shot)
    {
        Debug.Log("OnMissed: " + shot);
        MarkHit(shot, mark);
    }

    //
    // Mark hit
    // 
    void MarkHit(Vector3 shot, GameObject mark)
    {
        GameObject m = GameObject.Instantiate(mark);
        m.transform.SetParent(currentAttempt.transform);
        m.transform.position = shot;
        lastHitTime = Time.time;
        m.GetComponent<Mark>().time = lastHitTime;
        m.name = "Hit_" + lastHitTime;
    }


    //
    // Get points assigned by this target at partticular attempt
    //
    public int GetPoints(int attempt)
    {
        switch (type)
        {
            case TargetType.IPSC_Standard: return CalcStandardPoints(attempt);
            case TargetType.IPSC_Square:
            case TargetType.IPSC_Circle:   return CalcMetalPoints(attempt);
            case TargetType.IPSC_Popper:   return CalcPopperPoints(attempt);
            case TargetType.Missed:        return 0;
        }

        Debug.Log("Targets:GetPoints: unknow target type: '" + type + "'");
        return -1;
    }


    //
    // Calculate IPSC standard target points
    //
    int CalcStandardPoints(int attempt)
    {
        List<string> zoneNames = new List<string>() { "IPSC_Standard_A", "IPSC_Standard_C", "IPSC_Standard_D" };
        string atn = "A" + attempt;
        ArrayList points = new ArrayList();

        foreach (string zn in zoneNames)
        {
            GameObject goz = transform.Find(zn).gameObject;
            GameObject at = goz.transform.Find(atn).gameObject;

            for (int i=0; i<at.transform.childCount; i++)
            {
                points.Add(goz.GetComponent<Target>().points);
            }
            if (points.Count > 1)
            {
                return ((int)points[0] + (int)points[1]);
            }
        }

        return penalty; 
    }

    //
    // Calculate IPSC metal (circle or square) points
    //
    int CalcMetalPoints(int attempt)
    {
        string name = "A" + attempt;
        GameObject at = this.transform.Find(name).gameObject;
        if (at.transform.childCount > 0)
            return points;
        else
            return penalty;
    }

    //
    // Calculate IPSC popper points
    //
    int CalcPopperPoints(int attempt)
    {
        string name = "A" + attempt;
        GameObject at = this.transform.Find(name).gameObject;
        if (at.transform.childCount > 0)
            return points;
        else
            return penalty;
    }}

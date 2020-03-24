using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//
// Shot record
//
[System.Serializable]
public class _Hit
{
    public _Hit(Vector3 _shot, TargetType _targetType, int _points)
    {
        shot = _shot;
        time = Time.time;
        targetType = _targetType;
        points = _points;
    }

    public Vector3 shot;
    public float time;
    public TargetType targetType;
    public int points;
};


//
// Attempt
//
[System.Serializable]
public class _Attempt
{
    public float points = 0;
    public float startTime;
    public float totalTime;
    public float hitFactor;
    public List<_Hit> hits;

    public _Attempt()
    {
        startTime = Time.time;
        hits = new List<_Hit>();
    }

    public float CalculateResult()
    {
        if (hits.Count > 0)
        {
            points = 0;
            totalTime = hits.Last().time - startTime;

            foreach (_Hit h in hits)
            {
                points += h.points;
            }

            hitFactor = points / totalTime;
        } else
        {
            hitFactor = 0;
        }
        
        return hitFactor;
    }
}

//
// Session
//
[System.Serializable]
public class Session
{
    public List<_Attempt> attempts;

    public Session()
    {
        attempts = new List<_Attempt>();
    }
}

//
// Main
// 
public class Counter : MonoBehaviour {

    public List<Session> sessions;

    void Start()
    {
        if (sessions == null)
        {
            sessions = new List<Session>();
        }

        StartSession();
    }

    //
    // Start new session
    //
    public void StartSession()
    {
        sessions.Add(new Session());
    }

    //
    // Start new atempt
    //
    public void StartAttempt()
    {
        sessions.Last().attempts.Add(new _Attempt());
    }

    //
    // Stop atempt
    //
    public void StopAttempt()
    {
        // sessions.Last().attempts.Last().CalculateResult();
    }

    //
    // Save hit
    //
    public void SaveHit(_Hit hit)
    {
        sessions.Last().attempts.Last().hits.Add(hit);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exercise : MonoBehaviour {

    public enum Name
    {
        ShootForScore,
        RandomShoot
    }

    public Name name = Name.ShootForScore;
    public int Repeat;

    public enum State
    {
        Shooting,
        Stop
    }

    public State state = State.Stop;

    public AudioClip clipMakeReady;
    public AudioClip clipShoot;
    public AudioClip clipStandby;

    public int delayBeforeMakeready = 3;
    public int delayBeforeShootMin = 1;
    public int delayBeforeShootMax = 5;

    // Result desired attempt
    public int attemptToShow = 0;

    public float time;
    public int points;
    public float hitFactor;

    private AudioSource audioSource;

    //
    // Start
    //
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        state = State.Stop;
    }

    //
    // Update
    //
    void Update()
    {
        SetControlLimits();
    }

    //
    // LImit controls range
    //
    void SetControlLimits()
    {
        if (Repeat < 1) Repeat = 1;
        if (Repeat > 10) Repeat = 10;
    }

    //
    // Beging exercise
    // 
    public void Begin()
    {
        if (state == State.Shooting)
            return;

        Debug.Log("--- Exercise Beging: " + name);

        EventManager.PrepareAttempt();
        Random.InitState((int)Time.time);
        StartCoroutine(PrepareAndShoot());
    }

    //
    // End exercise
    // 
    public void End()
    {
        if (state == State.Stop)
            return;

        Debug.Log("--- Exercise End: " + name);

        state = State.Stop;
        Camera.main.GetComponent<Shot>().StopShooting();

        Target.GetResult(out time, out points, out hitFactor);
    }

    //
    // Show attemt with particuler index
    //
    public void ShowAttempt()
    {
        EventManager.ShowAttempt(attemptToShow);
        Target.GetResult(out time, out points, out hitFactor, attemptToShow);
    }

    //
    // Give voice commands and start counter
    //
    IEnumerator PrepareAndShoot() 
    {
        int delayBeforeShoot = Random.Range(delayBeforeShootMin, delayBeforeShootMax);

        yield return new WaitForSeconds(delayBeforeMakeready);

        audioSource.clip = clipMakeReady;
        audioSource.Play();
        yield return new WaitForSeconds(audioSource.clip.length);
        yield return new WaitForSeconds(delayBeforeShoot);

        audioSource.clip = clipShoot;
        audioSource.Play();
        yield return new WaitForSeconds(audioSource.clip.length);

        Camera.main.GetComponent<Shot>().StartrShooting();
        state = State.Shooting;
        Target.StartAttempt();
    }
}

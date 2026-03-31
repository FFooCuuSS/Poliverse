using UnityEngine;
using System;
using System.Collections.Generic;

public class BeatManager : MonoBehaviour
{
    public static BeatManager Instance;
    public CloudSpawner spawner;

    public float bpm = 120f;
    public List<int> beatPattern; // 1: 이동, 0: 쉬기

    private float beatInterval;
    private float timer;
    private int index = 0;

    public event Action OnBeat; // 박자 이벤트

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        beatInterval = 60f / bpm;
        if (beatPattern.Count == 0)
            beatPattern.Add(1);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= beatInterval)
        {
            timer -= beatInterval;

            int current = beatPattern[index];

            if (current == 1)
            {
                OnBeat?.Invoke(); // ✔ 이동 신호
                spawner.Spawn();
            }

            index = (index + 1) % beatPattern.Count;
        }
    }
}
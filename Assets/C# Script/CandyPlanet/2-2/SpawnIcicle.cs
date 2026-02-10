using System.Collections;
using UnityEngine;

public class SpawnIcicle : MonoBehaviour
{
    public GameObject iciclePrefab;

    [Header("X Position")]
    [SerializeField] private float startX = -7f;
    [SerializeField] private float step = 1f;

    [Header("Spawn Delays")]
    [SerializeField] private float[] spawnDelays;

    private int index = 0;
    private float currentX;
    private bool waiting = false;

    void Awake()
    {
        currentX = startX;
    }

    void OnEnable()
    {
        Icicle.OnIcicleFalling += HandleIcicleFalling;
    }

    void OnDisable()
    {
        Icicle.OnIcicleFalling -= HandleIcicleFalling;
    }

    private void Start()
    {
        //StartCoroutine(SpawnLoop());
        StartCoroutine(DelayAndSpawn());
    }

    private void HandleIcicleFalling()
    {
        if (waiting || index >= spawnDelays.Length)
            return;

        StartCoroutine(DelayAndSpawn());
    }

    private IEnumerator DelayAndSpawn()
    {
        waiting = true;

        float delay = spawnDelays[index];
        yield return new WaitForSeconds(delay);

        Spawn();
        index++;
        waiting = false;
    }

    private void Spawn()
    {
        Vector3 pos = new Vector3(currentX, transform.position.y, transform.position.z);
        Instantiate(iciclePrefab, pos, Quaternion.identity);
        currentX += step;
    }






    private IEnumerator SpawnLoop()
    {
        float currentX = startX;

        for (int i = 0; i < spawnDelays.Length; i++)
        {
            yield return new WaitForSeconds(spawnDelays[i]);

            Vector3 spawnPos = new Vector3(
                currentX,
                transform.position.y,
                transform.position.z
            );

            Instantiate(iciclePrefab, spawnPos, Quaternion.identity);

            currentX += step;
        }
    }
}

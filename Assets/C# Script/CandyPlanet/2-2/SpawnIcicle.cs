using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnIcicle : MonoBehaviour
{
    public GameObject iciclePrefab;

    [Header("X Position")]
    [SerializeField] private float startX = -7f;
    [SerializeField] private float step = 1f;

    [Header("Spawn Delays")]
    public float[] spawnDelays;

    public int index = 0;
    private float currentX;
    private bool waiting = false;

    private MiniGame2_2 minigame2_2;
    [SerializeField] private CapsuleCollider2D player;
    

    void Awake()
    {
        currentX = startX;
        minigame2_2 = GetComponentInParent<MiniGame2_2>();
        player = GetComponent<CapsuleCollider2D>();
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
        SpawnNext();
    }

    private void HandleIcicleFalling()
    {
        if (waiting || index >= spawnDelays.Length)
            return;

        SpawnNext();
    }
    private void SpawnNext()
    {
        waiting = true;

        Vector3 pos = new Vector3(currentX, transform.position.y, transform.position.z);
        GameObject icicle = Instantiate(iciclePrefab, pos, Quaternion.identity);

        float delay = spawnDelays[index];
        icicle.GetComponent<Icicle>().StartIcicle(delay);

        currentX += step;
        index++;

        waiting = false;
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

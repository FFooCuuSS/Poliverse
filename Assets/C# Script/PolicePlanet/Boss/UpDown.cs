using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Manager;

public class UpDown : MonoBehaviour
{
    public GameObject ManagerObj;
    public bool goUp;
    public float speed;

    private Manager manager;
    private bool move = false;

    private void Start()
    {
        manager = ManagerObj.GetComponent<Manager>();
    }

    void Update()
    {
        if (move)
        {
            Vector3 pos = transform.position;
            float direction = goUp ? 1f : -1f;
            pos.y += speed * direction * Time.deltaTime;
            transform.position = pos;
        }        
    }

    private void OnMouseDown()
    {
        move = true;

        Invoke(nameof(SpawnWithDelay), 1f); 
        Destroy(gameObject, 2f); 
    }

    private void SpawnWithDelay()
    {
        PlatformType typeToSpawn = goUp ? PlatformType.Up : PlatformType.Down;
        manager.SpawnPlatform(typeToSpawn);
    }
}

using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using static Manager;

public class UpDown : MonoBehaviour
{
    public GameObject ManagerObj;
    public bool goUp;
    public float speed;

    private Manager manager;

    void Start()
    {
        if (ManagerObj != null)
        {
            manager = ManagerObj.GetComponent<Manager>();
        }
    }

    private void OnMouseDown()
    {
        if (manager.platformIsMoving) return;

        manager.randomText.HideText(); // 이전 텍스트 숨기기

        manager.platformIsMoving = true;
        manager.MovePerson(goUp);

        Invoke("MoveWithDelay", 0.65f);
        Invoke("SpawnWithDelay", 1f);

        Destroy(gameObject, 2f);
    }

    private void MoveWithDelay()
    {
        float direction = goUp ? 1f : -1f;
        Vector3 targetPos = transform.position + Vector3.up * direction * speed;

        transform.DOMove(targetPos, 0.6f)
                 .SetEase(Ease.OutQuad);
    }

    private void SpawnWithDelay()
    {
        PlatformType typeToSpawn = goUp ? PlatformType.Up : PlatformType.Down;
        manager.SpawnPlatform(typeToSpawn);

        manager.platformIsMoving = false;
    }
}

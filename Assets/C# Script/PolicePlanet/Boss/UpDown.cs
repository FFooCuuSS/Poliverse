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

        //manager.randomText.HideText(); // ���� �ؽ�Ʈ �����
        manager.platformIsMoving = true;

        manager.MovePerson(goUp);

        Invoke("MovePlatform", 0.65f);
    }

    private void MovePlatform()
    {
        float direction = goUp ? 1f : -1f;
        Vector3 targetPos = transform.position + Vector3.up * direction * speed;

        // �̵� - �ٿ ����
        transform.DOMove(targetPos, 0.3f)
                 .SetEase(Ease.OutQuad)
                 .OnComplete(() =>
                 {
                     // ���� - �ٿ ȿ��
                     transform.DOMove(transform.position - Vector3.up * direction * speed, 0.4f)
                              .SetEase(Ease.OutBounce)
                              .OnComplete(() =>
                              {
                                  manager.platformIsMoving = false;
                                  manager.spawnMan = true;
                              });
                 });
    }

    private void SpawnWithDelay()
    {
        PlatformType typeToSpawn = goUp ? PlatformType.Up : PlatformType.Down;
        manager.SpawnPlatform(typeToSpawn);

        manager.platformIsMoving = false;
    }
}

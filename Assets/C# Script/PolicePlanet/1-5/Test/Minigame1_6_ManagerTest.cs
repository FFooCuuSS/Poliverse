using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame1_6_ManagerTest : MiniGameBase
{
    public HandControlerTest handControler;
    public Transform mainHand;
    public GameObject case1_Obj;
    public GameObject case2_Obj;
    public GameObject handSpawn;

    public int collideCnt;
    // Start is called before the first frame update
    void Start()
    {
        collideCnt = 0;

        //오브젝트 숨기기
        case1_Obj.gameObject.SetActive(false);
        case2_Obj.gameObject.SetActive(false);

        //case 선택
        int randInt = Random.Range(1, 3);
        handControler.caseNum = randInt;

        //case에 따라 해당 오브젝트 생성
        if (randInt == 1) case1_Obj.gameObject.SetActive(true);
        else if (randInt == 2) case2_Obj.gameObject.SetActive(true);
        else Debug.Log("Prefab Null");

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            SpawnHand();
        }
    }
    void SpawnHand()
    {
        Vector2 spawnPos = new Vector2(mainHand.position.x, -1);
        // 필수 체크 (할당 안 했을 때 에러 방지)
        if (handSpawn == null || mainHand == null)
        {
            Debug.Log("handSpawn 또는 mainHand 할당 안됨");
            return;
        }
        handSpawn.SetActive(true);
        // mainHand 위치에 handSpawn 프리팹 생성
        Instantiate(handSpawn, spawnPos, Quaternion.identity);
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_3_10_remake : MiniGameBase, IPracticeDemoInput
{

    [Header("효과음")]
    [SerializeField] private AudioClip warningSound;
    [Header("효과음")]
    [SerializeField] private AudioClip clickSound;
    public GameObject warningSign1;
    public GameObject warningSign2;
    public GameObject enemy;
    //public GameObject warningSign3;

    public Transform[] spawnPoints; // 3개라고 가정
    float fixedEnemyPosX = 11f;

    // 현재 선택된 스폰 인덱스 저장
    private int index1;
    private int index2;
    private int count;

    protected override string MinigameTitle => "안전 운전";

    protected override string MinigameExplain => "바움쿠헨이 계속 굴러갑니다.";
    protected override string[] AdditionalMinigameExplains => new string[]
    {
        "장애물을 피해 좌우로 움직이세요."
    };

    [Header("자동 데모")]
    [SerializeField] private CarMoveRemake car;

    private bool isDemoMode;

    public bool IsDemoMode => isDemoMode;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;

        if (car == null)
            car = FindObjectOfType<CarMoveRemake>();

        if (car != null)
        {
            car.SetWarningSigns(warningSign1, warningSign2);
            car.SetDemoMode(isDemoMode);
        }

        Debug.Log("[3-10] 데모 모드: " + isDemoMode);
    }

    void Start()
    {
        count = 0;

        if (car == null)
            car = FindObjectOfType<CarMoveRemake>();

        if (car != null)
        {
            car.SetWarningSigns(warningSign1, warningSign2);
            car.SetDemoMode(isDemoMode);
        }
        else
        {
            Debug.LogError("[3-10] CarMoveRemake를 찾을 수 없습니다.");
        }

        if (spawnPoints == null || spawnPoints.Length < 3)
        {
            Debug.LogError("[3-10] Spawn Points를 3개 이상 연결해야 합니다.");
            return;
        }

        if (warningSign1 == null || warningSign2 == null || enemy == null)
        {
            Debug.LogError("[3-10] 경고 표지판 또는 Enemy가 연결되지 않았습니다.");
            return;
        }

        warningSign1.SetActive(false);
        warningSign2.SetActive(false);

        base.StartGame();
        SetRandomPositions();
    }

    public override void OnRhythmEvent(string action)
    {
        if (action == "Show")
        {
            if (warningSound != null)
            {
                if (GameRoot.Instance != null && GameRoot.Instance.Audio != null)
                {
                    GameRoot.Instance.Audio.PlaySfx(warningSound);
                }
            }

            count++;
            if(count>=5)
            {
                //Debug.Log("[Show] Show 이벤트 들어옴, 5회 이상 발생하여 게임 종료");
                
                return;
            }
            Debug.Log("[Show] Show 이벤트 들어옴");
            ShowObject();
        }

        if (action == "Spawn")
        {
            if (clickSound != null)
            {
                if (GameRoot.Instance != null && GameRoot.Instance.Audio != null)
                {
                    GameRoot.Instance.Audio.PlaySfx(clickSound);
                }
            }
            Debug.Log("[Spawn] Spawn 이벤트 들어옴");
            SpawnObject();
        }
    }

    // 랜덤으로 서로 다른 2개 인덱스 뽑기
    void SetRandomPositions()
    {
        index1 = Random.Range(0, spawnPoints.Length);

        // index2는 index1과 겹치지 않게
        do
        {
            index2 = Random.Range(0, spawnPoints.Length);
        } while (index2 == index1);

        // 위치 배치
        warningSign1.transform.position = spawnPoints[index1].position;
        warningSign2.transform.position = spawnPoints[index2].position;
    }

    // Show 처리
    void ShowObject()
    {
        SetRandomPositions();

        // 표시
        warningSign1.SetActive(true);
        warningSign2.SetActive(true);

        // 2초 후 숨기기 + 재배치
        CancelInvoke(nameof(Hide));
        Invoke(nameof(Hide), 2f);
    }

    // 2초 뒤 실행
    void Hide()
    {
        warningSign1.SetActive(false);
        warningSign2.SetActive(false);

    }

    // Spawn 처리
    void SpawnObject()
    {
        // spawnPoints 위치 가져오되 x만 11로 고정
        Vector3 pos1 = spawnPoints[index1].position;
        Vector3 pos2 = spawnPoints[index2].position;

        pos1.x = fixedEnemyPosX;
        pos2.x = fixedEnemyPosX;

        Instantiate(enemy, pos1, Quaternion.identity);
        Instantiate(enemy, pos2, Quaternion.identity);
    }
}

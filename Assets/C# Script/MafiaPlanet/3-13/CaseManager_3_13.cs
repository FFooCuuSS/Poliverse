using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaseManager_3_13 : MonoBehaviour
{
    // 물체 종류를 구분하기 위한 열거형
    public enum ObjectType
    {
        Stick,
        Knife,
        Gun,
        Tie
    }

    // 물체가 등장하는 방향
    public enum MoveDirection
    {
        TopToBottom,
        BottomToTop,
        LeftToRight,
        RightToLeft
    }

    // 현재 선택된 케이스
    public enum CaseType
    {
        Case1,
        Case2
    }

    [System.Serializable]
    public class ObjectPrefabData
    {
        [Header("물체 종류")]
        public ObjectType objectType;

        [Header("생성할 프리팹")]
        public GameObject prefab;
    }

    [System.Serializable]
    public class ObjectPositionData
    {
        [Header("물체 종류")]
        public ObjectType objectType;

        [Header("위에서 아래로 이동할 때 시작 위치")]
        public Vector2 topStart;

        [Header("아래에서 위로 이동할 때 시작 위치")]
        public Vector2 bottomStart;

        [Header("왼쪽에서 오른쪽으로 이동할 때 시작 위치")]
        public Vector2 leftStart;

        [Header("오른쪽에서 왼쪽으로 이동할 때 시작 위치")]
        public Vector2 rightStart;

        [Header("최종 도착 위치")]
        public Transform targetPoint;
    }

    [Header("물체 프리팹")]
    [SerializeField] private ObjectPrefabData[] objectPrefabs;

    [Header("Case1 성공 시 물체 고정용 프리팹")]
    [SerializeField] private GameObject case1SuccessStick;
    [SerializeField] private GameObject case1SuccessKnife;
    [SerializeField] private GameObject case1SuccessGun;
    [SerializeField] private GameObject case1SuccessTie;

    [Header("Case2 성공 시 물체 고정용 프리팹")]
    [SerializeField] private GameObject case2SuccessStick;
    [SerializeField] private GameObject case2SuccessKnife;
    [SerializeField] private GameObject case2SuccessGun;
    [SerializeField] private GameObject case2SuccessTie;

    [Header("Case1 도착 위치")]
    [SerializeField] private Transform case1StickTargetPoint;
    [SerializeField] private Transform case1KnifeTargetPoint;
    [SerializeField] private Transform case1GunTargetPoint;
    [SerializeField] private Transform case1TieTargetPoint;

    [Header("Case2 도착 위치")]
    [SerializeField] private Transform case2StickTargetPoint;
    [SerializeField] private Transform case2KnifeTargetPoint;
    [SerializeField] private Transform case2GunTargetPoint;
    [SerializeField] private Transform case2TieTargetPoint;

    [Header("Case1 컨테이너")]
    [SerializeField] private GameObject case1Container;

    [Header("Case2 컨테이너")]
    [SerializeField] private GameObject case2Container;

    [Header("생성된 물체 부모")]
    [SerializeField] private Transform objectParent;

    [Header("생성 시간 설정")]
    [Tooltip("게임 시작 후 첫 물체가 생성되는 시간")]
    [SerializeField] private float firstSpawnTime = 2.5f;

    [Tooltip("물체가 생성되는 간격")]
    [SerializeField] private float spawnInterval = 2f;

    [Header("이동 설정")]
    [Tooltip("시작 위치에서 목표 위치까지 걸리는 시간")]
    [SerializeField] private float travelTime = 1f;

    [Header("랜덤 케이스 설정")]
    [Tooltip("체크 해제 시 아래 Fixed Case를 사용")]
    [SerializeField] private bool useRandomCase = true;

    [SerializeField] private CaseType fixedCase = CaseType.Case1;

    [Header("이동 방식")]
    [Tooltip("같은 판에서 방향이 중복되어도 되는지")]
    [SerializeField] private bool allowDuplicateDirection = true;

    private readonly List<ObjectPositionData> case1PositionData =
    new List<ObjectPositionData>();

    private readonly List<ObjectPositionData> case2PositionData =
        new List<ObjectPositionData>();

    // 무작위로 섞은 물체 순서
    private readonly List<ObjectType> shuffledObjectOrder =
        new List<ObjectType>();

    // 중복 방지용 방향 목록
    private readonly List<MoveDirection> availableDirections =
        new List<MoveDirection>();

    // 현재 생성된 물체들
    private readonly List<MovingObject_3_13> spawnedObjects =
        new List<MovingObject_3_13>();

    // 현재 선택된 케이스
    private CaseType currentCase;

    // 게임 시작 후 흐른 시간
    private float timer;

    // 현재 몇 번째 물체까지 생성했는지
    private int spawnIndex;

    // 게임 진행 여부
    private bool isPlaying;

    public CaseType CurrentCase => currentCase;

    private void Awake()
    {
        InitializeCase1PositionData();
        InitializeCase2PositionData();
        if (case1Container != null)
        {
            case1Container.SetActive(false);
        }

        if (case2Container != null)
        {
            case2Container.SetActive(false);
        }
    }

    private void Start()
    {
        StartCaseGame();
    }

    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        timer += Time.deltaTime;

        CheckSpawnTime();
        RemoveDestroyedObjects();
    }
    private void ResetSuccessObjects()
    {
        // Case1 성공 오브젝트 초기화
        if (case1SuccessStick != null)
        {
            case1SuccessStick.SetActive(false);
        }

        if (case1SuccessKnife != null)
        {
            case1SuccessKnife.SetActive(false);
        }

        if (case1SuccessGun != null)
        {
            case1SuccessGun.SetActive(false);
        }

        if (case1SuccessTie != null)
        {
            case1SuccessTie.SetActive(false);
        }

        // Case2 성공 오브젝트 초기화
        if (case2SuccessStick != null)
        {
            case2SuccessStick.SetActive(false);
        }

        if (case2SuccessKnife != null)
        {
            case2SuccessKnife.SetActive(false);
        }

        if (case2SuccessGun != null)
        {
            case2SuccessGun.SetActive(false);
        }

        if (case2SuccessTie != null)
        {
            case2SuccessTie.SetActive(false);
        }
    }
    /// <summary>
    /// Case1 좌표를 코드 내부에서 설정한다.
    /// </summary>
    private void InitializeCase1PositionData()
    {
        case1PositionData.Clear();

        // Stick
        case1PositionData.Add(
            new ObjectPositionData
            {
                objectType = ObjectType.Stick,
                topStart = new Vector2(0f, 6.1f),
                bottomStart = new Vector2(0f, -6.1f),
                leftStart = new Vector2(-12.5f, 2.1f),
                rightStart = new Vector2(12.5f, 2.1f),

                // Inspector에서 연결한 Stick 도착 위치
                targetPoint = case1StickTargetPoint
            }
        );

        // Knife
        case1PositionData.Add(
            new ObjectPositionData
            {
                objectType = ObjectType.Knife,
                topStart = new Vector2(0.8f, 7f),
                bottomStart = new Vector2(0.8f, -7f),
                leftStart = new Vector2(-12f, -2f),
                rightStart = new Vector2(12f, -2f),

                // Inspector에서 연결한 Knife 도착 위치
                targetPoint = case1KnifeTargetPoint
            }
        );

        // Gun
        case1PositionData.Add(
            new ObjectPositionData
            {
                objectType = ObjectType.Gun,
                topStart = new Vector2(0.83f, 7f),
                bottomStart = new Vector2(0.83f, -7f),
                leftStart = new Vector2(-12f, -0.38f),
                rightStart = new Vector2(12f, -0.38f),

                // Inspector에서 연결한 Gun 도착 위치
                targetPoint = case1GunTargetPoint
            }
        );

        // Tie
        case1PositionData.Add(
            new ObjectPositionData
            {
                objectType = ObjectType.Tie,
                topStart = new Vector2(-1.64f, 8f),
                bottomStart = new Vector2(-1.64f, -8f),
                leftStart = new Vector2(-11f, -1.19f),
                rightStart = new Vector2(11f, -1.19f),

                // Inspector에서 연결한 Tie 도착 위치
                targetPoint = case1TieTargetPoint
            }
        );
    }
    private void InitializeCase2PositionData()
    {
        case2PositionData.Clear();

        // Stick
        case2PositionData.Add(
            new ObjectPositionData
            {
                objectType = ObjectType.Stick,

                // 상하 이동: x = 0.0 고정
                topStart = new Vector2(0.0f, 6.1f),
                bottomStart = new Vector2(0.0f, -6.1f),

                // 좌우 이동: y = -2.85 고정
                leftStart = new Vector2(-12.5f, -2.85f),
                rightStart = new Vector2(12.5f, -2.85f),

                targetPoint = case2StickTargetPoint
            }
        );

        // Knife
        case2PositionData.Add(
            new ObjectPositionData
            {
                objectType = ObjectType.Knife,

                // 상하 이동: x = 0.84 고정
                topStart = new Vector2(0.84f, 7f),
                bottomStart = new Vector2(0.84f, -7f),

                // 좌우 이동: y = -0.35 고정
                leftStart = new Vector2(-12f, -0.35f),
                rightStart = new Vector2(12f, -0.35f),

                targetPoint = case2KnifeTargetPoint
            }
        );

        // Gun
        case2PositionData.Add(
            new ObjectPositionData
            {
                objectType = ObjectType.Gun,

                // 상하 이동: x = 0.83 고정
                topStart = new Vector2(0.83f, 7f),
                bottomStart = new Vector2(0.83f, -7f),

                // 좌우 이동: y = 1.3 고정
                leftStart = new Vector2(-12f, 1.3f),
                rightStart = new Vector2(12f, 1.3f),

                targetPoint = case2GunTargetPoint
            }
        );

        // Tie
        case2PositionData.Add(
            new ObjectPositionData
            {
                objectType = ObjectType.Tie,

                // 상하 이동: x = -1.64 고정
                topStart = new Vector2(-1.64f, 8f),
                bottomStart = new Vector2(-1.64f, -8f),

                // 좌우 이동: y = 0.45 고정
                leftStart = new Vector2(-11f, 0.45f),
                rightStart = new Vector2(11f, 0.45f),

                targetPoint = case2TieTargetPoint
            }
        );
    }
    /// <summary>
    /// 외부 또는 Start에서 게임을 시작한다.
    /// </summary>
    public void StartCaseGame()
    {
        StopAllCoroutines();
        ClearSpawnedObjects();

        timer = 0f;
        spawnIndex = 0;
        isPlaying = true;
        ResetSuccessObjects();
        SelectCase();
        SetContainerActive();
        MakeRandomObjectOrder();
        ResetDirectionList();

        Debug.Log(
            "[3-13] 케이스 게임 시작 / 선택된 케이스: " +
            currentCase
        );
    }
    public void ShowSuccessObject(ObjectType objectType)
    {
        switch (currentCase)
        {
            case CaseType.Case1:
                ShowCase1SuccessObject(objectType);
                break;

            case CaseType.Case2:
                ShowCase2SuccessObject(objectType);
                break;
        }
    }
    /// <summary>
    /// Case1에서 성공한 물체에 맞는 오브젝트를 활성화한다.
    /// </summary>
    private void ShowCase1SuccessObject(ObjectType objectType)
    {
        switch (objectType)
        {
            case ObjectType.Stick:

                if (case1SuccessStick != null)
                {
                    case1SuccessStick.SetActive(true);
                }

                break;

            case ObjectType.Knife:

                if (case1SuccessKnife != null)
                {
                    case1SuccessKnife.SetActive(true);
                }

                break;

            case ObjectType.Gun:

                if (case1SuccessGun != null)
                {
                    case1SuccessGun.SetActive(true);
                }

                break;

            case ObjectType.Tie:

                if (case1SuccessTie != null)
                {
                    case1SuccessTie.SetActive(true);
                }

                break;
        }

        Debug.Log(
            "[3-13] Case1 성공 표시 활성화 / 물체: " +
            objectType
        );
    }/// <summary>
     /// Case2에서 성공한 물체에 맞는 오브젝트를 활성화한다.
     /// </summary>
    private void ShowCase2SuccessObject(ObjectType objectType)
    {
        switch (objectType)
        {
            case ObjectType.Stick:

                if (case2SuccessStick != null)
                {
                    case2SuccessStick.SetActive(true);
                }

                break;

            case ObjectType.Knife:

                if (case2SuccessKnife != null)
                {
                    case2SuccessKnife.SetActive(true);
                }

                break;

            case ObjectType.Gun:

                if (case2SuccessGun != null)
                {
                    case2SuccessGun.SetActive(true);
                }

                break;

            case ObjectType.Tie:

                if (case2SuccessTie != null)
                {
                    case2SuccessTie.SetActive(true);
                }

                break;
        }

        Debug.Log(
            "[3-13] Case2 성공 표시 활성화 / 물체: " +
            objectType
        );
    }

    /// <summary>
    /// Case1 또는 Case2를 결정한다.
    /// </summary>
    private void SelectCase()
    {
        if (useRandomCase)
        {
            currentCase = Random.Range(0, 2) == 0
                    ? CaseType.Case1
                    : CaseType.Case2;
        }
        else
        {
            currentCase = fixedCase;
        }

        Debug.Log("[3-13] 선택된 Case : " + currentCase);
    }

    /// <summary>
    /// 선택된 케이스의 컨테이너만 활성화한다.
    /// </summary>
    private void SetContainerActive()
    {
        // 일단 둘 다 끈다.
        if (case1Container != null)
        {
            case1Container.SetActive(false);
        }

        if (case2Container != null)
        {
            case2Container.SetActive(false);
        }

        // 선택된 Case만 켠다.
        switch (currentCase)
        {
            case CaseType.Case1:

                if (case1Container != null)
                {
                    case1Container.SetActive(true);
                }

                break;

            case CaseType.Case2:

                if (case2Container != null)
                {
                    case2Container.SetActive(true);
                }

                break;
        }

        Debug.Log("현재 Case : " + currentCase);
    }

    /// <summary>
    /// Stick, Knife, Gun, Tie 순서를 무작위로 섞는다.
    /// 각 물체는 정확히 한 번씩 등장한다.
    /// </summary>
    private void MakeRandomObjectOrder()
    {
        shuffledObjectOrder.Clear();

        shuffledObjectOrder.Add(ObjectType.Stick);
        shuffledObjectOrder.Add(ObjectType.Knife);
        shuffledObjectOrder.Add(ObjectType.Gun);
        shuffledObjectOrder.Add(ObjectType.Tie);

        // Fisher-Yates 방식으로 순서를 섞는다.
        for (int i = shuffledObjectOrder.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            ObjectType temp = shuffledObjectOrder[i];
            shuffledObjectOrder[i] =
                shuffledObjectOrder[randomIndex];
            shuffledObjectOrder[randomIndex] = temp;
        }

        Debug.Log(
            "[3-13] 생성 순서: " +
            string.Join(", ", shuffledObjectOrder)
        );
    }

    /// <summary>
    /// 상하좌우 방향 목록을 초기화한다.
    /// </summary>
    private void ResetDirectionList()
    {
        availableDirections.Clear();

        availableDirections.Add(MoveDirection.TopToBottom);
        availableDirections.Add(MoveDirection.BottomToTop);
        availableDirections.Add(MoveDirection.LeftToRight);
        availableDirections.Add(MoveDirection.RightToLeft);
    }

    /// <summary>
    /// 첫 생성 시간 이후 일정 간격으로 물체를 생성한다.
    /// </summary>
    private void CheckSpawnTime()
    {
        if (spawnIndex >= shuffledObjectOrder.Count)
        {
            return;
        }

        float nextSpawnTime =
            firstSpawnTime +
            spawnIndex * spawnInterval;

        if (timer < nextSpawnTime)
        {
            return;
        }

        // 프레임이 밀려도 생성이 누락되지 않도록 반복 확인한다.
        while (spawnIndex < shuffledObjectOrder.Count)
        {
            nextSpawnTime =
                firstSpawnTime +
                spawnIndex * spawnInterval;

            if (timer < nextSpawnTime)
            {
                break;
            }

            SpawnNextObject();
            spawnIndex++;
        }

        if (spawnIndex >= shuffledObjectOrder.Count)
        {
            Debug.Log("[3-13] 4개 물체 생성 완료");
        }
    }

    /// <summary>
    /// 무작위 순서의 다음 물체를 생성한다.
    /// </summary>
    private void SpawnNextObject()
    {
        ObjectType objectType =
            shuffledObjectOrder[spawnIndex];

        GameObject prefab =
            GetPrefab(objectType);

        ObjectPositionData positionData =
            GetPositionData(objectType);

        if (prefab == null)
        {
            Debug.LogWarning(
                "[3-13] " +
                objectType +
                " 프리팹이 연결되지 않았습니다."
            );

            return;
        }

        if (positionData == null)
        {
            Debug.LogWarning(
                "[3-13] " +
                objectType +
                " 위치 데이터를 찾지 못했습니다."
            );

            return;
        }

        if (positionData.targetPoint == null)
        {
            Debug.LogWarning(
                "[3-13] " +
                currentCase +
                " / " +
                objectType +
                " TargetPoint가 연결되지 않았습니다."
            );

            return;
        }

        MoveDirection direction =
            GetRandomDirection();

        Vector3 startPosition =
            GetStartPosition(
                positionData,
                direction
            );

        Vector3 targetPosition =
            positionData.targetPoint.position;

        GameObject spawnedObject = Instantiate(
            prefab,
            startPosition,
            Quaternion.identity,
            objectParent
        );

        MovingObject_3_13 movingObject =
            spawnedObject.GetComponent<MovingObject_3_13>();

        if (movingObject == null)
        {
            Debug.LogError(
                "[3-13] " +
                prefab.name +
                " 프리팹에 MovingObject_3_13이 없습니다."
            );

            Destroy(spawnedObject);
            return;
        }

        movingObject.Initialize(
            startPosition,
            targetPosition,
            travelTime,
            objectType,
            direction
        );

        spawnedObjects.Add(movingObject);

        Debug.Log(
            "[3-13] 생성 #" +
            (spawnIndex + 1) +
            " / 물체: " +
            objectType +
            " / 방향: " +
            direction +
            " / 시작: " +
            startPosition +
            " / 도착: " +
            targetPosition +
            " / 이동 시간: " +
            travelTime
        );
    }

    /// <summary>
    /// 물체 종류에 맞는 프리팹을 반환한다.
    /// </summary>
    private GameObject GetPrefab(ObjectType objectType)
    {
        if (objectPrefabs == null)
        {
            return null;
        }

        for (int i = 0; i < objectPrefabs.Length; i++)
        {
            if (objectPrefabs[i].objectType == objectType)
            {
                return objectPrefabs[i].prefab;
            }
        }

        return null;
    }

    /// <summary>
    /// 현재 케이스와 물체 종류에 맞는 위치 데이터를 반환한다.
    /// </summary>
    private ObjectPositionData GetPositionData(ObjectType objectType)
    {
        List<ObjectPositionData> selectedData =
            currentCase == CaseType.Case1
                ? case1PositionData
                : case2PositionData;

        for (int i = 0; i < selectedData.Count; i++)
        {
            if (selectedData[i].objectType == objectType)
            {
                return selectedData[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Case1 TargetPoint를 외부에서 연결할 때 사용한다.
    /// </summary>
    public void SetCase1TargetPoint(
        ObjectType objectType,
        Transform targetPoint
    )
    {
        for (int i = 0;
             i < case1PositionData.Count;
             i++)
        {
            if (case1PositionData[i].objectType ==
                objectType)
            {
                case1PositionData[i].targetPoint =
                    targetPoint;

                return;
            }
        }
    }

    /// <summary>
    /// 이동 방향을 무작위로 선택한다.
    /// </summary>
    private MoveDirection GetRandomDirection()
    {
        // 방향 중복을 허용하는 경우 매번 완전 랜덤 선택
        if (allowDuplicateDirection)
        {
            return (MoveDirection)Random.Range(0, 4);
        }

        // 네 물체가 각각 다른 방향을 사용하도록 한다.
        if (availableDirections.Count == 0)
        {
            ResetDirectionList();
        }

        int randomIndex =
            Random.Range(0, availableDirections.Count);

        MoveDirection selectedDirection =
            availableDirections[randomIndex];

        availableDirections.RemoveAt(randomIndex);

        return selectedDirection;
    }

    /// <summary>
    /// 방향에 맞는 시작 좌표를 반환한다.
    /// </summary>
    private Vector3 GetStartPosition(
        ObjectPositionData positionData,
        MoveDirection direction
    )
    {
        Vector2 selectedPosition;

        switch (direction)
        {
            case MoveDirection.TopToBottom:
                selectedPosition =
                    positionData.topStart;
                break;

            case MoveDirection.BottomToTop:
                selectedPosition =
                    positionData.bottomStart;
                break;

            case MoveDirection.LeftToRight:
                selectedPosition =
                    positionData.leftStart;
                break;

            case MoveDirection.RightToLeft:
                selectedPosition =
                    positionData.rightStart;
                break;

            default:
                selectedPosition =
                    positionData.topStart;
                break;
        }

        float targetZ = 0f;

        if (positionData.targetPoint != null)
        {
            targetZ =
                positionData.targetPoint.position.z;
        }

        return new Vector3(
            selectedPosition.x,
            selectedPosition.y,
            targetZ
        );
    }

    /// <summary>
    /// 게임 진행을 정지한다.
    /// </summary>
    public void StopCaseGame()
    {
        isPlaying = false;

        Debug.Log(
            "[3-13] 케이스 게임 정지 / 선택된 케이스: " +
            currentCase
        );
    }

    /// <summary>
    /// 현재 생성된 물체를 모두 제거한다.
    /// </summary>
    public void ClearSpawnedObjects()
    {
        for (int i = spawnedObjects.Count - 1;
             i >= 0;
             i--)
        {
            if (spawnedObjects[i] != null)
            {
                Destroy(
                    spawnedObjects[i].gameObject
                );
            }
        }

        spawnedObjects.Clear();
    }

    /// <summary>
    /// 파괴된 물체의 리스트 참조를 정리한다.
    /// </summary>
    private void RemoveDestroyedObjects()
    {
        for (int i = spawnedObjects.Count - 1;
             i >= 0;
             i--)
        {
            if (spawnedObjects[i] == null)
            {
                spawnedObjects.RemoveAt(i);
            }
        }
    }
}
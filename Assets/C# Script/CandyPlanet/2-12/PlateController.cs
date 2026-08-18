using UnityEngine;
using DG.Tweening;

/// <summary>
/// 3. 접시(그릇) 이동 로직
///
/// 각 레인(트랙)을 클릭/터치하면 그 레인으로 접시가 바로 스냅 이동합니다.
/// 클릭한 화면 좌표를 월드 좌표로 변환한 뒤, laneAnchors 중 X좌표가 가장 가까운 레인을 선택합니다.
/// (레인마다 별도 콜라이더를 만들 필요 없이 laneAnchors의 X 위치만 있으면 동작함)
/// </summary>
public class PlateController : MonoBehaviour
{
    [SerializeField] private Transform[] laneAnchors; // 각 레인의 위치를 나타내는 Transform 배열
    [SerializeField] private float snapMoveDuration = 0.15f;
    [SerializeField] private Ease snapEase = Ease.OutQuad;

    [Tooltip("이 거리보다 멀리 클릭하면 무시 (화면 밖/엉뚱한 곳 클릭 방지용, world unit 기준). 0이면 항상 가장 가까운 레인 선택")]
    [SerializeField] private float maxClickDistance = 0f;

    // Minigame_2_12 등 외부 스크립트가 같은 레인 배열을 참조할 때 사용 (중복 설정 방지)
    public Transform[] LaneAnchors => laneAnchors;
    public int CurrentLane => _currentLaneIndex;

    private int _currentLaneIndex;
    private bool _inputEnabled;

    private void Awake()
    {
        _currentLaneIndex = laneAnchors.Length / 2; // 가운데 레인에서 시작
        SnapToLane(_currentLaneIndex, instant: true);
    }

    public void SetInputEnabled(bool enabled)
    {
        _inputEnabled = enabled;
    }

    private void Update()
    {
        if (!_inputEnabled) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
            TrySelectLaneAtScreenPosition(Input.mousePosition);
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            TrySelectLaneAtScreenPosition(Input.GetTouch(0).position);
#endif
    }

    private void TrySelectLaneAtScreenPosition(Vector2 screenPos)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[PlateController] Camera.main이 없어서 클릭 위치를 계산할 수 없습니다.");
            return;
        }

        // 오소그래픽 2D 카메라 기준: z는 카메라와 레인 오브젝트 사이 거리만 맞으면 X/Y에 영향 없음.
        // 레인/접시가 z=0 평면에 있다고 가정. (다르면 이 값만 조정하면 됨)
        float distanceFromCamera = Mathf.Abs(cam.transform.position.z - laneAnchors[0].position.z);
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distanceFromCamera));

        int nearestLane = -1;
        float nearestDist = float.MaxValue;

        for (int i = 0; i < laneAnchors.Length; i++)
        {
            float dist = Mathf.Abs(laneAnchors[i].position.x - worldPos.x);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearestLane = i;
            }
        }

        if (nearestLane < 0) return;
        if (maxClickDistance > 0f && nearestDist > maxClickDistance) return; // 너무 먼 곳 클릭은 무시

        if (nearestLane != _currentLaneIndex)
        {
            _currentLaneIndex = nearestLane;
            SnapToLane(_currentLaneIndex, instant: false);
        }
    }

    private void SnapToLane(int laneIndex, bool instant)
    {
        Vector3 target = new Vector3(laneAnchors[laneIndex].position.x, transform.position.y, transform.position.z);

        if (instant)
        {
            transform.position = target;
        }
        else
        {
            transform.DOKill();
            transform.DOMove(target, snapMoveDuration).SetEase(snapEase);
        }
    }
}
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ContainerTarget : MonoBehaviour
{
    [Header("Lane Index (0,1,2)")]
    public int laneIndex;

    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();

        // Trigger/NonTrigger 둘 다 상관 없음. bounds 체크만 쓸 거라서.
        // 다만 컨테이너 콜라이더 크기는 경찰이 지나갈 만큼 넉넉하게 잡아야 함.
    }

    // 경찰 콜라이더 "중심점"이 컨테이너 bounds 내부면 컨테이너 위라고 판정
    public bool IsPoliceOver(Collider2D policeCol)
    {
        if (col == null) return false;
        if (policeCol == null) return false;

        Vector3 policeCenter = policeCol.bounds.center;
        return col.bounds.Contains(policeCenter);
    }
}

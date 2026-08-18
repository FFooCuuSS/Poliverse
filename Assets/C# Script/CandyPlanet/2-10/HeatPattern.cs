using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 라운드 1개(=6초 패턴 1개) 분량의 시스템 콜 데이터.
/// dropTimes는 인스펙터에서 배열 크기와 각 값(초 단위)을 자유롭게 조정할 수 있다.
///   예) [1, 2, 3]  -> 1초, 2초, 3초에 온도계가 내려감
///   예) [1, 3]     -> 1초, 3초에만 온도계가 내려감 (2초는 쉬어감)
/// </summary>
[System.Serializable]
public class HeatPatternEntry
{
    [Tooltip("이 라운드에서 온도계가 내려가는 시점들(초). 예: [1,2,3] 또는 [1,3] 등")]
    public float[] dropTimes = new float[] { 1f, 2f, 3f };
}

/// <summary>
/// 미니게임 2-10(초콜릿 젓기)의 "시스템 콜(Call)" 패턴 데이터.
///
/// 라운드(패턴) 하나는 총 6초로 구성된다.
///  - 0 ~ playerPhaseOffset(기본 3초) : 시스템이 dropTimes에 적힌 시각마다 온도계를 한 번씩 내린다.
///  - playerPhaseOffset ~ 6초         : 플레이어가 같은 간격(=dropTimes + playerPhaseOffset)으로
///                                       화면을 스와이프해서 맞춰야 한다.
///
/// patterns 리스트에 라운드를 여러 개 추가하면, TemperatureController가 리스트 순서대로
/// 라운드를 이어서 재생한다. 예) [1,2,3] 라운드 다음에 [1,3] 라운드가 이어지는 식으로
/// 인스펙터에서 라운드를 자유롭게 추가/삭제/재배열할 수 있다.
/// </summary>
public class HeatPattern : MonoBehaviour
{
    [Header("라운드별 패턴 목록 (리스트 순서대로 재생됨)")]
    [SerializeField] private List<HeatPatternEntry> patterns = new List<HeatPatternEntry> { new HeatPatternEntry() };

    [Header("플레이어 응답 구간")]
    [Tooltip("시스템 구간 길이(=플레이어 구간이 시작되는 시각, 초). 모든 라운드 공통. 기본 3초 (전체 6초 패턴 기준)")]
    [SerializeField] private float playerPhaseOffset = 3f;

    /// <summary>등록된 라운드(패턴) 개수.</summary>
    public int PatternCount => patterns != null ? patterns.Count : 0;

    /// <summary>시스템 구간 길이 = 플레이어 구간이 시작되는 시각(초). 모든 라운드 공통.</summary>
    public float PlayerPhaseOffset => playerPhaseOffset;

    /// <summary>index번째 라운드에 등록된 박자 개수.</summary>
    public int DropCount(int index)
    {
        if (!IsValidIndex(index)) return 0;

        var times = patterns[index].dropTimes;
        return times != null ? times.Length : 0;
    }

    /// <summary>모든 라운드의 박자 개수 합(=이 미니게임 전체에서 플레이어가 맞춰야 할 총 입력 개수).</summary>
    public int TotalDropCount
    {
        get
        {
            int total = 0;
            for (int i = 0; i < PatternCount; i++)
                total += DropCount(i);
            return total;
        }
    }

    /// <summary>
    /// index번째 라운드의 dropTimes를 오름차순으로 정렬한 복사본으로 반환한다.
    /// (원본 배열 순서를 건드리지 않기 위해 항상 새 배열을 만들어 반환)
    /// </summary>
    public float[] GetSortedDropTimes(int index)
    {
        if (!IsValidIndex(index)) return new float[0];

        float[] times = patterns[index].dropTimes;
        if (times == null) return new float[0];

        float[] sorted = new float[times.Length];
        System.Array.Copy(times, sorted, times.Length);
        System.Array.Sort(sorted);
        return sorted;
    }

    private bool IsValidIndex(int index)
    {
        return patterns != null && index >= 0 && index < patterns.Count;
    }

    private void OnValidate()
    {
        if (patterns == null) return;

        // 값이 음수이거나 플레이어 구간 시작 시각을 넘지 않도록 인스펙터 입력값을 보정한다.
        foreach (var entry in patterns)
        {
            if (entry?.dropTimes == null) continue;

            for (int i = 0; i < entry.dropTimes.Length; i++)
                entry.dropTimes[i] = Mathf.Clamp(entry.dropTimes[i], 0f, playerPhaseOffset);
        }
    }
}
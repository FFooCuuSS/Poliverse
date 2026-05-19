using UnityEngine;

public enum HeatAction
{
    Down,
    Up
}

public class HeatPattern : MonoBehaviour
{
    // 원하는 패턴 설정
    public HeatAction[] pattern;
}

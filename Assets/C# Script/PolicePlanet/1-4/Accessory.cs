using UnityEngine;

public class Accessory : MonoBehaviour
{
    public bool IsRemoved { get; private set; }

    private Minigame_1_4 minigame;

    public void Init(Minigame_1_4 game)
    {
        minigame = game;
        IsRemoved = false;
    }

    // 실제 슬라이드 입력 시 호출
    public void OnSlide()
    {
        if (IsRemoved) return;
        minigame.OnPlayerInput("Swipe");
    }

    public void Remove()
    {
        IsRemoved = true;
        gameObject.SetActive(false); // 또는 애니메이션
    }
}

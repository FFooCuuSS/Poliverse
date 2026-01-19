using UnityEngine;

public class Accessory : MonoBehaviour
{
    public bool IsRemoved { get; private set; }
    public bool InputLocked { get; private set; } = true;

    private Minigame_1_4 minigame;

    private DragAndDrop drag;   // ★ 추가
    void Awake()
    {
        drag = GetComponent<DragAndDrop>();
    }

    public void Init(Minigame_1_4 game)
    {
        minigame = game;
        IsRemoved = false;
        InputLocked = false;
        UnlockInput();
    }

    public void LockInput()
    {
        InputLocked = true;
        if (drag != null)
            drag.banDragging = true;   // ★ 핵심
    }

    public void UnlockInput()
    {
        InputLocked = false;
        if (drag != null)
            drag.banDragging = false;  // ★ 핵심
    }


    private void OnMouseUp()
    {
        if (InputLocked) return;
        OnSlide();
        Debug.Log("onslide 호출");
    }

    // 실제 슬라이드 입력 시 호출
    public void OnSlide()
    {
        if (InputLocked) return;
        if (IsRemoved) return;
        if (minigame == null) return;

        minigame.OnPlayerInput("Swipe");
    }

    public void Remove()
    {
        IsRemoved = true;
        gameObject.SetActive(false); // 또는 애니메이션
    }
}

using UnityEngine;
using DG.Tweening;
public class Accessory : MonoBehaviour
{
    public bool IsRemoved { get; private set; }
    public bool InputLocked { get; private set; } = true;

    private Minigame_1_4 minigame;

    private DragAndDrop drag;

    private Montage curMontage;
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

    public void SetMontage(Montage montage)
    {
        curMontage = montage;
        Debug.Log($"{name} SetMontage → {montage?.name}");
    }

    public void LockInput()
    {
        InputLocked = true;
        if (drag != null)
            drag.banDragging = true;   
    }

    public void UnlockInput()
    {
        InputLocked = false;
        if (drag != null)
            drag.banDragging = false;  
    }


    private void OnMouseUp()
    {
        if (InputLocked) return;
        OnSlide();
        
    }

    // 실제 슬라이드 입력 시 호출
    public void OnSlide()
    {
        if (InputLocked) return;
        if (IsRemoved) return;
        if (minigame == null) return;
        if (curMontage != null)
        {
            curMontage.PlayHit();
        }
        minigame.OnPlayerInput("Swipe");

        
    }

    public void Remove()
    {
        if (IsRemoved) return;

        IsRemoved = true;
        LockInput();

        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
        if (srs.Length == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        // 알파 페이드
        foreach (var sr in srs)
        {
            sr.DOFade(0f, 0.25f);
        }

        // 페이드 끝나고 비활성화
        DOVirtual.DelayedCall(0.25f, () =>
        {
            gameObject.SetActive(false);
        });
    }

    public void ResetAccessory()
    {
        IsRemoved = false;
        gameObject.SetActive(true);
    }
}

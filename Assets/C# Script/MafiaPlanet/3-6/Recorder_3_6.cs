using UnityEngine;
using UnityEngine.EventSystems;

public class Recorder_3_6 : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private Minigame_3_6 minigame;

    public void OnPointerDown(PointerEventData eventData)
    {
        StartRecord();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopRecord();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 손가락/마우스가 버튼 밖으로 나가면 녹음 해제하고 싶으면 유지.
        // 계속 누른 상태를 인정하고 싶으면 이 줄은 빼도 됨.
        StopRecord();
    }

    // 버튼 EventTrigger에서 직접 연결할 수 있게 public으로 둠
    public void StartRecord()
    {
        if (minigame != null)
            minigame.StartRecording();
    }

    public void StopRecord()
    {
        if (minigame != null)
            minigame.StopRecording();
    }

    private void OnDisable()
    {
        StopRecord();
    }
}
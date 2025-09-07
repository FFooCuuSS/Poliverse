using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottleClickManager2_4 : MonoBehaviour
{
    public static BottleClickManager2_4 Instance;
    private Bottle2_4 selectedBottle;

    private void Awake() => Instance = this;

    public void OnBottleClicked(Bottle2_4 bottle)
    {
        if (selectedBottle == null)
        {
            selectedBottle = bottle;
            Debug.Log($"{bottle.name} 선택됨.");
            selectedBottle._Outline.SetActive(true);
        }
        else
        {
            if (selectedBottle == bottle)
            {
                Debug.Log("같은 병을 두 번 클릭했습니다!");
                
                return; // 아무 동작도 하지 않음
                
            }

            PourTopToEmptySlot(selectedBottle, bottle);
            selectedBottle._Outline.SetActive(false);
            selectedBottle = null;
        }
    }

    void PourTopToEmptySlot(Bottle2_4 from, Bottle2_4 to)
    {
        Color topColor = from.PeekLiquid();

        if (topColor == Color.clear)
        {
            Debug.Log($"{from.name}에는 이동할 색상이 없습니다!");
            return;
        }

        // 두 번째 병에서 맨 위 투명(Color.clear) 위치 찾기
        Color[] toArray = to.liquids.ToArray(); // Stack -> 배열
        int emptyIndex = -1;

        // 배열 맨 앞이 맨 위
        for (int i = 0; i < toArray.Length; i++)
        {
            if (toArray[i] == Color.clear)
            {
                emptyIndex = i;
                break;
            }
        }

        // 만약 병이 아직 capacity가 안 찼다면 가장 아래가 빈 칸
        if (emptyIndex == -1 && to.liquids.Count < to.capacity)
        {
            emptyIndex = to.liquids.Count;
        }

        if (emptyIndex == -1)
        {
            Debug.Log($"{to.name}에는 빈 칸이 없습니다!");
            return;
        }

        // 첫 번째 병에서 색상 제거
        from.RemoveLiquid();
        

        // 두 번째 병에 투명 칸이 있는 경우 가장 아래에 삽입
        Stack<Color> newStack = new Stack<Color>();

        // 기존 liquids를 배열 뒤에서부터 Stack으로 다시 쌓으면서
        // emptyIndex 위치에 topColor 삽입
        for (int i = toArray.Length - 1; i >= 0; i--)
        {
            if (i == emptyIndex)
                newStack.Push(topColor);

            newStack.Push(toArray[i]);
        }

        // 만약 capacity-1 이하라서 emptyIndex가 배열 밖이면 topColor를 맨 위로 추가
        if (emptyIndex >= toArray.Length)
            newStack.Push(topColor);

        to.liquids = newStack;

        // 시각 갱신
        from.UpdateVisual(true);
        to.UpdateVisual(true);

        Debug.Log($"{from.name}의 top 색상을 {to.name}의 빈 칸에 이동!");
        GameManager2_4.Instance.CheckWin();
    }
}

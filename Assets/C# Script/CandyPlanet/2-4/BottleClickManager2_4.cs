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
            Debug.Log($"{bottle.name} ���õ�.");
            selectedBottle._Outline.SetActive(true);
        }
        else
        {
            if (selectedBottle == bottle)
            {
                Debug.Log("���� ���� �� �� Ŭ���߽��ϴ�!");
                
                return; // �ƹ� ���۵� ���� ����
                
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
            Debug.Log($"{from.name}���� �̵��� ������ �����ϴ�!");
            return;
        }

        // �� ��° ������ �� �� ����(Color.clear) ��ġ ã��
        Color[] toArray = to.liquids.ToArray(); // Stack -> �迭
        int emptyIndex = -1;

        // �迭 �� ���� �� ��
        for (int i = 0; i < toArray.Length; i++)
        {
            if (toArray[i] == Color.clear)
            {
                emptyIndex = i;
                break;
            }
        }

        // ���� ���� ���� capacity�� �� á�ٸ� ���� �Ʒ��� �� ĭ
        if (emptyIndex == -1 && to.liquids.Count < to.capacity)
        {
            emptyIndex = to.liquids.Count;
        }

        if (emptyIndex == -1)
        {
            Debug.Log($"{to.name}���� �� ĭ�� �����ϴ�!");
            return;
        }

        // ù ��° ������ ���� ����
        from.RemoveLiquid();
        

        // �� ��° ���� ���� ĭ�� �ִ� ��� ���� �Ʒ��� ����
        Stack<Color> newStack = new Stack<Color>();

        // ���� liquids�� �迭 �ڿ������� Stack���� �ٽ� �����鼭
        // emptyIndex ��ġ�� topColor ����
        for (int i = toArray.Length - 1; i >= 0; i--)
        {
            if (i == emptyIndex)
                newStack.Push(topColor);

            newStack.Push(toArray[i]);
        }

        // ���� capacity-1 ���϶� emptyIndex�� �迭 ���̸� topColor�� �� ���� �߰�
        if (emptyIndex >= toArray.Length)
            newStack.Push(topColor);

        to.liquids = newStack;

        // �ð� ����
        from.UpdateVisual(true);
        to.UpdateVisual(true);

        Debug.Log($"{from.name}�� top ������ {to.name}�� �� ĭ�� �̵�!");
        GameManager2_4.Instance.CheckWin();
    }
}

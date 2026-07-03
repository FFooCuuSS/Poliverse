using System.Collections.Generic;
using UnityEngine;

public class WindowCell_3_14 : MonoBehaviour
{
    [Header("Generated Root")]
    [SerializeField] private Transform visualRoot;

    private readonly List<GameObject> offPieces = new List<GameObject>();
    private readonly List<GameObject> onPieces = new List<GameObject>();

    private bool isOn;

    public void BuildVisuals(
        GameObject offPrefab,
        GameObject onPrefab,
        int piecesPerCell,
        float pieceSpacingX
    )
    {
        if (visualRoot == null)
        {
            Transform found = transform.Find("GeneratedVisual");

            if (found != null)
            {
                visualRoot = found;
            }
            else
            {
                GameObject root = new GameObject("GeneratedVisual");
                root.transform.SetParent(transform, false);
                visualRoot = root.transform;
            }
        }

        ClearGeneratedVisuals();

        piecesPerCell = Mathf.Max(1, piecesPerCell);

        for (int i = 0; i < piecesPerCell; i++)
        {
            float offsetX = (i - (piecesPerCell - 1) * 0.5f) * pieceSpacingX;
            Vector3 localPos = new Vector3(offsetX, 0f, 0f);

            if (offPrefab != null)
            {
                GameObject offObj = Instantiate(offPrefab, visualRoot);
                offObj.transform.localPosition = localPos;
                offObj.transform.localRotation = Quaternion.identity;
                offPieces.Add(offObj);
            }

            if (onPrefab != null)
            {
                GameObject onObj = Instantiate(onPrefab, visualRoot);
                onObj.transform.localPosition = localPos;
                onObj.transform.localRotation = Quaternion.identity;
                onPieces.Add(onObj);
            }
        }

        SetOffImmediate();
    }

    private void ClearGeneratedVisuals()
    {
        offPieces.Clear();
        onPieces.Clear();

        if (visualRoot == null) return;

        for (int i = visualRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(visualRoot.GetChild(i).gameObject);
        }
    }

    public void SetOnImmediate()
    {
        isOn = true;
        ApplyState();
    }

    public void SetOffImmediate()
    {
        isOn = false;
        ApplyState();
    }

    public void SetOn()
    {
        SetOnImmediate();
    }

    public void SetOff()
    {
        SetOffImmediate();
    }

    public void TurnOffImmediate()
    {
        SetOffImmediate();
    }

    public void TurnOff()
    {
        SetOffImmediate();
    }

    public void Flash(float duration)
    {
        // 이번 구조에서는 Show가 "켜졌다가 꺼지는 연출"이 아니라
        // 이미 켜져 있는 칸을 순서대로 끄는 신호다.
        SetOffImmediate();
    }

    public void SetInputReady()
    {
        // 색 피드백 안 씀.
    }

    public void MarkSuccess()
    {
        // 성공했을 때도 해당 칸은 꺼진 상태 유지.
        SetOffImmediate();
    }

    public void MarkMiss()
    {
        // 실패해도 라운드가 이어지므로 상태는 건드리지 않아도 되지만,
        // Show에서 꺼졌다는 전제를 유지하기 위해 꺼둔다.
        SetOffImmediate();
    }

    private void ApplyState()
    {
        for (int i = 0; i < offPieces.Count; i++)
        {
            if (offPieces[i] != null)
                offPieces[i].SetActive(!isOn);
        }

        for (int i = 0; i < onPieces.Count; i++)
        {
            if (onPieces[i] != null)
                onPieces[i].SetActive(isOn);
        }
    }
}
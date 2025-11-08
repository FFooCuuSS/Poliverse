using UnityEngine;

// Single code item click handler (ASCII-only)
public class CodeItem : MonoBehaviour
{
    public int codeId;          // 0..5
    private Renderer rend;

    public void Setup(int id)
    {
        codeId = id;
        rend = GetComponentInChildren<Renderer>();
    }

    private void OnMouseDown()
    {
        if (MemoryGameController.Instance != null)
        {
            MemoryGameController.Instance.OnCodeClicked(codeId, rend);
        }
    }
}

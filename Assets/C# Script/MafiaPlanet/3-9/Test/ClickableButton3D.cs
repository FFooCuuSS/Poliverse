using UnityEngine;

// 2D button that reacts to mouse clicks (for Enter / Erase)
public class ClickableButton3D : MonoBehaviour
{
    public enum ButtonType { Enter, Erase }
    public ButtonType type;

    // OnMouseDown works with Collider2D too
    private void OnMouseDown()
    {
        if (MemoryGameController.Instance == null) return;

        if (type == ButtonType.Enter)
        {
            MemoryGameController.Instance.OnEnterClicked();
        }
        else if (type == ButtonType.Erase)
        {
            MemoryGameController.Instance.OnEraseClicked();
        }
    }
}

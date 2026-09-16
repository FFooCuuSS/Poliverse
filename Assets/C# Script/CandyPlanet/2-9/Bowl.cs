using UnityEngine;

public class Bowl : MonoBehaviour
{
    [System.Serializable]
    public class SpriteStage
    {
        public int countThreshold; // 이 개수 이상이면 해당 스프라이트로 변경
        public Sprite sprite;
    }

    [SerializeField] private SpriteRenderer spriteRenderer;

    [Tooltip("countThreshold 오름차순으로 등록할 것 (예: 5, 10, 15 ...)")]
    [SerializeField] private SpriteStage[] stages;

    private int debrisCount = 0;

    public void OnDebrisCaught()
    {
        debrisCount++;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        // stages를 뒤에서부터 확인해서, 현재 count가 만족하는 가장 높은 단계로 변경
        for (int i = stages.Length - 1; i >= 0; i--)
        {
            if (debrisCount >= stages[i].countThreshold)
            {
                spriteRenderer.sprite = stages[i].sprite;
                break;
            }
        }
    }
}
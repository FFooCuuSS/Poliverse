using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield_2_14 : MonoBehaviour
{
    [Header("효과음")]
    [SerializeField] private AudioClip bounceSound;

    public Transform player;
    public float radius = 2f;

    private Minigame_2_14 miniGame;

    private bool isDemoMode = false;
    private Vector3 lastDemoDir = Vector3.right;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
    }

    private void Update()
    {
        if (player == null) return;

        Vector3 targetDir = Vector3.zero;

        if (isDemoMode)
        {
            // [데모 모드] 가장 가까운 음식을 찾음
            GameObject targetFood = FindClosestFood();
            if (targetFood != null)
            {
                // 음식이 있으면 그 방향을 타겟으로 잡고, 마지막 방향으로 갱신
                targetDir = (targetFood.transform.position - player.position).normalized;
                lastDemoDir = targetDir;
            }
            else
            {
                // [개선] 음식이 없으면 제자리로 돌아가지 않고, 마지막으로 막았던 방향(lastDemoDir)을 그대로 유지
                targetDir = lastDemoDir;
            }
        }
        else
        {
            // [일반 플레이 모드] 마우스 위치를 타겟으로 설정
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            targetDir = (mousePos - player.position).normalized;
        }

        // 위치와 회전 적용
        transform.position = player.position + targetDir * radius;

        float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Start()
    {
        miniGame = GetComponentInParent<Minigame_2_14>();
    }

    private GameObject FindClosestFood()
    {
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        GameObject closest = null;
        float minDist = float.MaxValue;

        foreach (var food in foods)
        {
            if (food == null) continue;
            float dist = Vector3.Distance(player.position, food.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = food;
            }
        }
        return closest;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Food")) return;

        var foodMove = col.GetComponent<FoodMove_2_14>();

        if (foodMove != null)
        {
            foodMove.StopMovement();
            if (bounceSound != null && GameRoot.Instance?.Audio != null)
            {
                GameRoot.Instance.Audio.PlaySfx(bounceSound);
            }
        }

        miniGame?.ReportManualSuccess();

        Destroy(col.gameObject);
    }
}

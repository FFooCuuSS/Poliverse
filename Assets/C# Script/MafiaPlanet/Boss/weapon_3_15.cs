using UnityEngine;

public class weapon_3_15 : MonoBehaviour
{
    [SerializeField] private bool isMagicWand = false;
    [SerializeField] private float movingSpeed = 1f;
    [SerializeField] private float lifeTime = 10f;

    public bool IsMagicWand => isMagicWand;

    private manager_3_15 manager;
    private float xMoving;

    // 스포너가 생성 직후 호출
    public void Init(manager_3_15 mgr, float xMove)
    {
        manager = mgr;
        xMoving = xMove;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += Vector3.right * xMoving * Time.deltaTime * movingSpeed;
    }

    private void OnMouseDown()
    {
        gameObject.SetActive(false);
        if (manager != null) manager.OnWeaponClicked(isMagicWand);
    }
}
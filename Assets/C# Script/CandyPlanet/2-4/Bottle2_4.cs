using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class Bottle2_4 : MonoBehaviour
{
    private Vector3 target;
    [SerializeField] public float moveSpeed = 5f;
    private bool isFilled = false;

    public void SetTarget(Vector3 pos)
    {
        target = pos;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            Destroy(gameObject);
        }
    }

    public void FillBottle(GameObject liquidObject)
    {
        if (isFilled || liquidObject == null) return;
        isFilled = true;

        Debug.Log("음료 따르기!");

        liquidObject.transform.SetParent(this.transform);

        liquidObject.transform.DOLocalMove(Vector3.zero, 0.1f).SetEase(Ease.OutQuad);
    }
}

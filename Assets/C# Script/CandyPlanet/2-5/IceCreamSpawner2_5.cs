using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCreamSpawner2_5 : MonoBehaviour
{
    [SerializeField] private GameObject iceCreamPrefab;

    [SerializeField] private Sprite[] iceCreamSprites;

    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform rightSidePoint;

    public IceCream2_5 SpawnIceCream()
    {
        GameObject obj = Instantiate(
            iceCreamPrefab,
            startPoint.position,
            Quaternion.identity
        );

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

        if (sr != null && iceCreamSprites != null && iceCreamSprites.Length > 0)
        {
            sr.sprite = iceCreamSprites[
                Random.Range(0, iceCreamSprites.Length)
            ];
        }

        IceCream2_5 iceCream = obj.GetComponent<IceCream2_5>();

        if (iceCream == null)
        {
            Debug.LogError("IceCream2_5 컴포넌트가 프리팹에 없습니다.");
            Destroy(obj);
            return null;
        }

        iceCream.SetFlyTarget(rightSidePoint.position);

        return iceCream;
    }
}

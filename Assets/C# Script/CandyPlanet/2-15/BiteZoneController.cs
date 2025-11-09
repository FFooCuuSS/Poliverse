using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class BiteZoneController : MonoBehaviour
{
    public string sliceTag = "FoodPiece"; // 삭제할 조각 Tag

    private Collider2D capsuleCollider;

    private void Awake()
    {
        capsuleCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.NoFilter();

            List<Collider2D> results = new List<Collider2D>();
            int count = capsuleCollider.OverlapCollider(filter, results);

            for (int i = 0; i < count; i++)
            {
                Collider2D col = results[i];

                if (col.CompareTag(sliceTag))
                {
                    // Tracker 호출
                    FoodPiecesTracker tracker = col.GetComponentInParent<FoodPiecesTracker>();
                    Debug.Log(tracker);
                    if (tracker != null)
                        tracker.PieceEaten();

                    Destroy(col.gameObject);
                    Debug.Log($"{col.name} 삭제됨");
                }
            }
        }
    }
}

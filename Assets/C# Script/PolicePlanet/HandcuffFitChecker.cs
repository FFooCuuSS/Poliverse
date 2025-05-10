using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class HandcuffFitChecker : MonoBehaviour
{
    [SerializeField] private List<Collider> handColliders;
    private CircleCollider2D cuffCollider;

    public float criticalPoint = 50f;
    private float leftHandPercent;
    private float rightHandPercent;


    private void Start()
    {
        cuffCollider = GetComponent<CircleCollider2D>();
    }
    private void Update()
    {
        BoundCheck();
        isAllCheck();
    }

    private void BoundCheck()
    {
        Bounds cuffBounds = cuffCollider.bounds;

        foreach (var handCol in handColliders)
        {
            Bounds handBounds = handCol.bounds;

            if (cuffBounds.Intersects(handBounds))
            {
                Bounds intersect = GetIntersectionBounds(cuffBounds, handBounds);
                float intersectVolume = intersect.size.x * intersect.size.y;
                float handVolume = handBounds.size.x * handBounds.size.y;
                float overlapPercent = intersectVolume * 100 / handVolume;

                if(handCol.tag == "LeftHand")
                {
                    leftHandPercent = overlapPercent;
                   // Debug.Log($"left: {leftHandPercent}");
                }
                if(handCol.tag == "RightHand")
                {
                    rightHandPercent = overlapPercent;
                   // Debug.Log($"right: {rightHandPercent}");

                }
            }
        }
    }

    private void isAllCheck()
    {
        Debug.Log(leftHandPercent);
        Debug.Log(rightHandPercent);

        if ((leftHandPercent > criticalPoint) && (rightHandPercent > criticalPoint))
        {
            Debug.Log("GameClear");
        }
    }

    private Bounds GetIntersectionBounds(Bounds a, Bounds b)
    {
        Vector3 min = Vector3.Max(a.min, b.min);
        Vector3 max = Vector3.Min(a.max, b.max);
        return new Bounds((min + max) / 2, max - min);
        //두 영역의 겹치는 최소점, 최대점을 알고 중간지점 중심으로 새로운 bounds 생성

    }




}

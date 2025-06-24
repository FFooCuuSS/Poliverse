using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2_3 : MonoBehaviour
{
    [Header("ũ�� ����")]
    public Vector3 shrinkScale = new Vector3(0.5f, 0.5f, 1f);
    public Vector3 normalScale = Vector3.one;
    public float restoreDelay = 1.0f;  // ���� ��� �ð�

    private bool isShrinking = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isShrinking)
        {
            StartCoroutine(ShrinkAndRestore());
        }
    }

    IEnumerator ShrinkAndRestore()
    {
        isShrinking = true;
        transform.localScale = shrinkScale;
        yield return new WaitForSeconds(restoreDelay);
        transform.localScale = normalScale;
        isShrinking = false;
    }

}

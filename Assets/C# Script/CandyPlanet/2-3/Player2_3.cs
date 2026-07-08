using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2_3 : MonoBehaviour
{
    [Header("크기 설정")]
    public Vector3 shrinkScale = new Vector3(0.5f, 0.5f, 1f);
    public Vector3 normalScale = Vector3.one;

    public float restoreDelay = 1.0f;

    private bool isShrinking = false;

    [SerializeField]
    private Minigame_2_3 minigame;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (minigame != null && minigame.IsInputOpen)
            {
                minigame.OnPlayerInput();

                StartCoroutine(ShrinkAndRestore());
            }
        }
    }

    IEnumerator ShrinkAndRestore()
    {
        if (isShrinking)
            yield break;

        isShrinking = true;

        transform.localScale = shrinkScale;

        yield return new WaitForSeconds(restoreDelay);

        transform.localScale = normalScale;

        isShrinking = false;
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DropCake : MonoBehaviour
{
    public float targetY;
    public float delay = 5f;
    public float duration = 3f; //내려오는데 걸리는 시간

    private void Start()
    {
        StartCoroutine(WaitSecond());
    }

    IEnumerator WaitSecond()
    {
        Debug.Log("5초 세기 시작");
        yield return new WaitForSeconds(delay); 
        StartCoroutine(MoveOverTime());
    }

    private IEnumerator MoveOverTime()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x, targetY, startPos.z);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;

            yield return null; 
        }

        transform.position = endPos;
    }

    
}
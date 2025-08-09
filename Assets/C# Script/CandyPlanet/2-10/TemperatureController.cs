using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class TemperatureController : MonoBehaviour
{
    public ScoopDrag scoopDrag;
    public float maxSpeed = 20f;
    public float minTemperature = 0f;
    public float maxTemperature = 100f;
    public float minY;
    public float maxY;
    public Transform thermometerObj;

    private float curTemperature;

    private void Update()
    {
        float speed = scoopDrag.CurSpeed;
        float targetTemperature = Mathf.Clamp01(speed / maxSpeed) * maxTemperature;

        curTemperature = Mathf.Lerp(curTemperature, targetTemperature, Time.deltaTime * 5f);

        Debug.Log($"현재 온도: {curTemperature:F1}도");
        SetTemperature( curTemperature );
    }

    public void SetTemperature(float newTemp)
    {
        curTemperature = Mathf.Clamp(newTemp, minTemperature, maxTemperature);
        float t = (curTemperature - minTemperature) / (maxTemperature - minTemperature);
        float newY = Mathf.Lerp(minY, maxY, t);

        Vector3 newPos = thermometerObj.localPosition;
        newPos.y = newY;
        thermometerObj.localPosition = newPos;
    }
}

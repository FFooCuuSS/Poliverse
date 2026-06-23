using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClearBar_3_6 : MonoBehaviour
{
    public GameObject Stage_3_6;
    public GameObject RecorderObj;
    public GameObject PhoneObj;

    private Minigame_3_6 minigame_3_6;
    private Recorder_3_6 recorder;
    private Phone_3_6 phone_3_6;

    [SerializeField] private float upSpeed;
    [SerializeField] private float downSpeed;

    private Slider ClearSlider;
    private bool isCleared = false;

    void Start()
    {
        minigame_3_6 = Stage_3_6.GetComponent<Minigame_3_6>();
        recorder = RecorderObj.GetComponent<Recorder_3_6>();
        phone_3_6 = PhoneObj.GetComponent<Phone_3_6>();

        ClearSlider = GetComponent<Slider>();
        ClearSlider.value = 0;
        Invoke("LateStart", 0.1f);
    }

    void LateStart()
    {
        phone_3_6.PhoneCalling();
    }

    void Update()
    {
        if (isCleared) return;

        if (ClearSlider.value >= 1f)
        {
            ClearSlider.value = 1f;  // 혹시 넘칠 경우 고정
            isCleared = true;
            return;
        }

        if (phone_3_6.BoolCheck())
        {
            ClearSlider.value += upSpeed * Time.deltaTime;
        }
        else
        {
            ClearSlider.value -= downSpeed * Time.deltaTime;
        }

        // 음수 방지
        if (ClearSlider.value < 0f)
        {
            ClearSlider.value = 0f;
        }
    }
}

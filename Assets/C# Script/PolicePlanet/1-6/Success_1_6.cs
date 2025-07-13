using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Success_1_6 : MonoBehaviour
{
    public GameObject stage_1_6;
    private Minigame_1_6 minigame_1_6;

    public GameObject isDefend;
    public GameObject isOffend;
    public GameObject isOfficer;

    public GameObject defend;
    public GameObject offend;
    public GameObject officer;


    // 각 오브젝트의 상태별 스프라이트 배열 (index 0: 성공, 1: 실패 등)
    [SerializeField] private Sprite[] defendSprites;
    [SerializeField] private Sprite[] offendSprites;
    [SerializeField] private Sprite[] officerSprites;

    private bool isSuccessed = false;

    private void Start()
    {
        minigame_1_6 = stage_1_6.GetComponent<Minigame_1_6>();
    }

    private void Update()
    {
        if (!isSuccessed && isSuccess())
        {
            isSuccessed = true;
            minigame_1_6.Succeed();
            ApplySuccessSprites(); // 스프라이트 변경
        }
    }

    bool isSuccess()
    {
        var defend = isDefend.GetComponent<DefendBox>();
        var offend = isOffend.GetComponent<OffendBox>();
        var officer = isOfficer.GetComponent<OfficeBox>();
        return defend.isDefense && offend.isOffend && officer.isOfficer;
    }

    void ApplySuccessSprites()
    {
        defend.GetComponent<SpriteRenderer>().sprite = defendSprites[0];
        offend.GetComponent<SpriteRenderer>().sprite = offendSprites[0];
        officer.GetComponent<SpriteRenderer>().sprite = officerSprites[0];
    }

    public void ApplyFailureSprites()
    {
        defend.GetComponent<SpriteRenderer>().sprite = defendSprites[1];
        offend.GetComponent<SpriteRenderer>().sprite = offendSprites[1];
        officer.GetComponent<SpriteRenderer>().sprite = officerSprites[1];
    }
}

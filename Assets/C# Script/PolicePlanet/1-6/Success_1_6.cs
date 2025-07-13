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


    // �� ������Ʈ�� ���º� ��������Ʈ �迭 (index 0: ����, 1: ���� ��)
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
            ApplySuccessSprites(); // ��������Ʈ ����
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

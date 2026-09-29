using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SupStepData
{
    [Header("Sup-Step 이미지")]
    public Sprite image;

    [Header("단계 텍스트")]
    [TextArea]
    public string text1;

    [Header("타이틀 텍스트")]
    [TextArea]
    public string text2;
}

[System.Serializable]
public class PlanetTrainingData
{
    public string planetName;

    [Header("이 행성의 Sup-Step")]
    public SupStepData[] supSteps;
}


public class TrainingManager : MonoBehaviour
{
    [Header("Planet")]
    [SerializeField]
    private PlanetList planetList;

    [SerializeField]
    private PlanetTrainingData[] planets;


    [Header("Sup-Step Images")]
    [SerializeField]
    private Image[] supStepImages;


    [Header("Sup-Step Text")]
    [SerializeField]
    private TMP_Text[] supStepTexts1;


    [Header("Sup-Step Title Text")]
    [SerializeField]
    private TMP_Text[] supStepTexts2;


    [Header("Training Buttons")]
    [SerializeField]
    private Button[] trainingButtons;


    [Header("Training Button Images")]
    [SerializeField]
    private Sprite normalButtonSprite;

    [SerializeField]
    private Sprite selectedButtonSprite;

    [Header("현재 Training 표시")]
    [SerializeField]
    private TMP_Text currentTrainingText;


    // 현재 선택된 훈련 번호
    // 1 ~ 5
    private int currentTrainingId = 1;


    // 다른 스크립트에서 현재 훈련 번호 가져오기
    public int GetCurrentTrainingId()
    {
        return currentTrainingId;
    }


    private void Start()
    {
        SetupTrainingButtons();
    }


    private void OnEnable()
    {
        if (planetList == null)
        {
            Debug.LogWarning(
                "[TrainingManager] PlanetList가 연결되지 않았습니다."
            );

            return;
        }


        int currentPlanetIndex =
            planetList.CallingCurrentIndex();


        Debug.Log(
            "[TrainingManager] Training 패널 열림 - 현재 행성 : "
            + currentPlanetIndex
        );

        // 현재 행성에 맞는 Training 버튼 개수 설정
        UpdateTrainingButtonVisibility(currentPlanetIndex);

        // 패널을 열면 항상 Training 1
        currentTrainingId = 1;

        UpdateCurrentTrainingText();

        ShowTrainingForPlanet(
            currentPlanetIndex,
            0
        );


        UpdateTrainingButtonImages(0);
    }


    // Training 1~5 버튼 연결
    private void SetupTrainingButtons()
    {
        if (trainingButtons == null)
            return;


        for (int i = 0; i < trainingButtons.Length; i++)
        {
            if (trainingButtons[i] == null)
                continue;


            int trainingIndex = i;


            trainingButtons[i].onClick.AddListener(
                () => ShowTraining(trainingIndex)
            );
        }
    }


    // Training 버튼을 눌렀을 때
    private void ShowTraining(int trainingIndex)
    {
        if (planetList == null)
        {
            Debug.LogWarning(
                "[TrainingManager] PlanetList가 없습니다."
            );

            return;
        }

        int currentPlanetIndex =
            planetList.CallingCurrentIndex();

        // 1번 행성은 Training 1~4까지만 존재
        if (currentPlanetIndex == 0 && trainingIndex >= 4)
        {
            return;
        }

        currentTrainingId =
            trainingIndex + 1;

        UpdateCurrentTrainingText();

        Debug.Log(
            "[TrainingManager] 현재 훈련 : "
            + currentTrainingId
        );

        ShowTrainingForPlanet(
            currentPlanetIndex,
            trainingIndex
        );

        UpdateTrainingButtonImages(
            trainingIndex
        );
    }

    // 현재 선택된 훈련 번호 표시
    private void UpdateCurrentTrainingText()
    {
        if (currentTrainingText == null)
            return;

        currentTrainingText.text =
            "훈련 " + currentTrainingId;
    }


    // Training 버튼 이미지 변경
    private void UpdateTrainingButtonImages(
        int selectedIndex)
    {
        if (trainingButtons == null)
            return;


        for (int i = 0; i < trainingButtons.Length; i++)
        {
            if (trainingButtons[i] == null)
                continue;


            Image buttonImage =
                trainingButtons[i].GetComponent<Image>();


            if (buttonImage == null)
                continue;


            if (i == selectedIndex)
            {
                buttonImage.sprite =
                    selectedButtonSprite;
            }
            else
            {
                buttonImage.sprite =
                    normalButtonSprite;
            }
        }
    }


    // 선택된 행성 + 선택된 Training의 정보 표시
    private void ShowTrainingForPlanet(
        int planetIndex,
        int trainingIndex)
    {
        if (planets == null ||
            planets.Length == 0)
        {
            Debug.LogWarning(
                "[TrainingManager] PlanetTrainingData가 없습니다."
            );

            return;
        }


        if (planetIndex < 0 ||
            planetIndex >= planets.Length)
        {
            Debug.LogWarning(
                "[TrainingManager] 잘못된 행성 번호 : "
                + planetIndex
            );

            return;
        }


        SupStepData[] supSteps =
            planets[planetIndex].supSteps;


        if (supSteps == null)
        {
            ClearSupStep();
            return;
        }


        // 기존 내용 삭제
        ClearSupStep();


        // Training 1 = 0,1,2
        // Training 2 = 3,4,5
        // Training 3 = 6,7,8
        // Training 4 = 9,10,11
        // Training 5 = 12,13,14
        int startIndex =
    trainingIndex * 3;

        // 현재 Training에서 실제로 존재하는 Sup-Step 개수
        int remainingCount =
            Mathf.Min(3, supSteps.Length - startIndex);

        // 3개 위치를 먼저 모두 초기화
        ClearSupStep();

        // 실제 존재하는 Sup-Step 표시
        for (int i = 0; i < remainingCount; i++)
        {
            int supStepIndex =
                startIndex + i;

            int displayIndex = i;

            // 마지막 Training에 1개만 있다면
            // 가운데 위치 사용
            if (remainingCount == 1)
            {
                displayIndex = 1;
            }

            // 이미지
            if (displayIndex < supStepImages.Length)
            {
                supStepImages[displayIndex].sprite =
                    supSteps[supStepIndex].image;

                supStepImages[displayIndex].enabled = true;
            }

            // 단계 텍스트
            if (displayIndex < supStepTexts1.Length)
            {
                supStepTexts1[displayIndex].text =
                    supSteps[supStepIndex].text1;

                supStepTexts1[displayIndex].enabled = true;
            }

            // 타이틀 텍스트
            if (displayIndex < supStepTexts2.Length)
            {
                supStepTexts2[displayIndex].text =
                    supSteps[supStepIndex].text2;

                supStepTexts2[displayIndex].enabled = true;
            }
        }
    }

    // 이미지 + 텍스트 초기화
    private void ClearSupStep()
    {
        if (supStepImages != null)
        {
            for (int i = 0;
                 i < supStepImages.Length;
                 i++)
            {
                if (supStepImages[i] == null)
                    continue;


                supStepImages[i].sprite = null;
                supStepImages[i].enabled = false;
            }
        }


        if (supStepTexts1 != null)
        {
            for (int i = 0;
                 i < supStepTexts1.Length;
                 i++)
            {
                if (supStepTexts1[i] == null)
                    continue;


                supStepTexts1[i].text = "";
                supStepTexts1[i].enabled = false;
            }
        }


        if (supStepTexts2 != null)
        {
            for (int i = 0;
                 i < supStepTexts2.Length;
                 i++)
            {
                if (supStepTexts2[i] == null)
                    continue;


                supStepTexts2[i].text = "";
                supStepTexts2[i].enabled = false;
            }
        }
    }

    // 현재 행성에 맞게 Training 버튼 표시
    private void UpdateTrainingButtonVisibility(int planetIndex)
    {
        if (trainingButtons == null)
            return;

        // 1번 행성 = Training 4개
        // 나머지 행성 = Training 5개
        int trainingCount = (planetIndex == 0) ? 4 : 5;

        for (int i = 0; i < trainingButtons.Length; i++)
        {
            if (trainingButtons[i] == null)
                continue;

            trainingButtons[i].gameObject.SetActive(i < trainingCount);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject trainingButtonPrefab; // 트레이닝 버튼 프리팹 (Training 1, 2...)
    [SerializeField] private GameObject cardPrefab;           // 카드 프리팹 (1-1, 1-2...)

    [Header("Containers")]
    [SerializeField] private Transform trainingButtonContainer; // 오른쪽 버튼들이 들어갈 부모 오브젝트
    [SerializeField] private Transform cardContainer;           // 왼쪽 카드들이 들어갈 부모 오브젝트

    [Header("Settings")]
    [SerializeField] private float cardSpacing = 20f;           // 카드 간격

    private List<GameObject> activeCards = new List<GameObject>();

    private void Start()
    {
        // 1. 카드 부모 오브젝트(cardContainer)에 Horizontal Layout Group 설정
        SetupCardContainerLayout();

        // 2. 예시: 총 10개의 서브 스테이지가 있을 때 트레이닝 버튼들 생성
        InitializeTrainingMenu(totalSubStages: 10);
    }

    // 카드 컨테이너의 레이아웃 설정 (자동 중앙 정렬)
    private void SetupCardContainerLayout()
    {
        HorizontalLayoutGroup layout = cardContainer.GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
        {
            layout = cardContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        // 핵심: 자식 요소들이 가운데 정렬되도록 설정
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = cardSpacing;

        // 크기 제어 설정 (필요 시 조절)
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    // 트레이닝 버튼 목록 생성 (1~5)
    public void InitializeTrainingMenu(int totalSubStages)
    {
        // 기존 버튼 삭제
        foreach (Transform child in trainingButtonContainer)
        {
            Destroy(child.gameObject);
        }

        int itemsPerPage = 3;
        int totalPages = Mathf.CeilToInt((float)totalSubStages / itemsPerPage);

        for (int i = 0; i < totalPages; i++)
        {
            int pageIndex = i; // 람다식 캡처용
            GameObject btnObj = Instantiate(trainingButtonPrefab, trainingButtonContainer);

            // 버튼 텍스트 설정 (버튼 자식에 Text/TMP가 있을 경우)
            Text btnText = btnObj.GetComponentInChildren<Text>();
            if (btnText != null) btnText.text = $"Training{pageIndex + 1}";

            // 버튼 클릭 이벤트 연결
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnClickTrainingButton(pageIndex, totalSubStages));
            }
        }

        // 기본으로 첫 번째 트레이닝(1번) 열기
        if (totalPages > 0)
        {
            OnClickTrainingButton(0, totalSubStages);
        }
    }

    // 트레이닝 버튼 눌렀을 때 실행되는 함수
    private void OnClickTrainingButton(int pageIndex, int totalSubStages)
    {
        // 기존 카드 삭제
        foreach (GameObject card in activeCards)
        {
            Destroy(card);
        }
        activeCards.Clear();

        int itemsPerPage = 3;
        int startIndex = pageIndex * itemsPerPage;
        // 남은 개수에 맞춰 생성할 카드 수 계산 (10개일 경우 마지막 페이지는 1개만 생성)
        int count = Mathf.Min(itemsPerPage, totalSubStages - startIndex);

        // 카드 생성
        for (int i = 0; i < count; i++)
        {
            int stageNumber = startIndex + i + 1; // 1, 2, 3 ... 10

            GameObject cardObj = Instantiate(cardPrefab, cardContainer);
            activeCards.Add(cardObj);

            // 카드 텍스트 설정 (예: "1-1", "1-2", "1-10")
            Text cardText = cardObj.GetComponentInChildren<Text>();
            if (cardText != null)
            {
                cardText.text = $"1-{stageNumber}";
            }
        }

        // Horizontal Layout Group이 자식 생성 직후 갱신되도록 호출
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardContainer.GetComponent<RectTransform>());
    }
}

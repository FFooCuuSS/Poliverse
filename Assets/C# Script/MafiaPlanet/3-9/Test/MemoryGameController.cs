using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main game controller (ASCII-only: no Unicode in strings/comments)
public class MemoryGameController : MonoBehaviour
{
    public Transform[] gameStartPos; // size 3

    public Transform[] setPositions; // size 6

    public Transform[] containedPos; // size 3

    public GameObject[] codePrefabs; // size 6

    public float previewSeconds = 2f;

    public Material defaultMat;
    public Material selectedMat;

    private readonly List<int> answerSeq = new List<int>(3);
    private readonly List<int> inputSeq = new List<int>(3);

    private readonly List<GameObject> spawnedSix = new List<GameObject>(6);

    private int currentSelectedId = -1;
    private Renderer currentSelectedRenderer = null;

    // Input gate
    private bool canInput = false;

    public static MemoryGameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(GameFlow());
    }

    private IEnumerator GameFlow()
    {
        // Guards
        if (codePrefabs == null || codePrefabs.Length != 6)
        {
            Debug.LogError("[MemoryGame] codePrefabs must contain 6 items.");
            yield break;
        }
        if (gameStartPos == null || gameStartPos.Length != 3)
        {
            Debug.LogError("[MemoryGame] gameStartPos must contain 3 items.");
            yield break;
        }
        if (setPositions == null || setPositions.Length != 6)
        {
            Debug.LogError("[MemoryGame] setPositions must contain 6 items.");
            yield break;
        }
        if (containedPos == null || containedPos.Length != 3)
        {
            Debug.LogError("[MemoryGame] containedPos must contain 3 items.");
            yield break;
        }

        // 1) Preview 3 codes in order (show then destroy)
        yield return StartCoroutine(ShowPreview3());

        // 2) Spawn 6 codes (one of each type) at 6 shuffled positions
        SpawnSixCodesRandom();

        // 3) Enable input
        inputSeq.Clear();
        currentSelectedId = -1;
        currentSelectedRenderer = null;
        canInput = true;

        Debug.Log("[MemoryGame] Input enabled: click a code -> click ENTER to confirm, ERASE to undo. After 3 picks, auto-judge.");
    }

    private IEnumerator ShowPreview3()
    {
        answerSeq.Clear();

        // Pick 3 unique ids from 0..5
        List<int> bag = new List<int>() { 0, 1, 2, 3, 4, 5 };
        for (int i = 0; i < 3; i++)
        {
            int pickIdx = Random.Range(0, bag.Count);
            int codeId = bag[pickIdx];
            bag.RemoveAt(pickIdx);
            answerSeq.Add(codeId);
        }

        // Instantiate previews at gameStartPos[0..2]
        List<GameObject> previews = new List<GameObject>(3);
        for (int i = 0; i < 3; i++)
        {
            GameObject inst = Instantiate(
                codePrefabs[answerSeq[i]],
                gameStartPos[i].position,
                gameStartPos[i].rotation
            );
            inst.name = $"Preview_{i}_ID{answerSeq[i]}";
            previews.Add(inst);
        }

        Debug.Log($"[MemoryGame] Answer (order): {answerSeq[0]}, {answerSeq[1]}, {answerSeq[2]}");

        yield return new WaitForSeconds(previewSeconds);

        // Destroy previews
        for (int i = 0; i < previews.Count; i++)
        {
            if (previews[i]) Destroy(previews[i]);
        }
    }

    private void SpawnSixCodesRandom()
    {
        // Cleanup old
        for (int i = 0; i < spawnedSix.Count; i++)
            if (spawnedSix[i]) Destroy(spawnedSix[i]);
        spawnedSix.Clear();

        // Shuffle positions
        int[] order = GetRandomOrder(setPositions.Length);

        // Spawn one of each code id (0..5)
        for (int i = 0; i < 6; i++)
        {
            int codeId = i;
            Transform dst = setPositions[order[i]];

            GameObject inst = Instantiate(codePrefabs[codeId], dst.position, dst.rotation);
            inst.name = $"Play_ID{codeId}_at_{dst.name}";

            // Attach click component
            CodeItem item = inst.GetComponent<CodeItem>();
            if (item == null) item = inst.AddComponent<CodeItem>();
            item.Setup(codeId);

            spawnedSix.Add(inst);
        }
    }

    public void OnCodeClicked(int codeId, Renderer rend)
    {
        if (!canInput) return;

        // Unhighlight previous
        if (currentSelectedRenderer && defaultMat)
            currentSelectedRenderer.material = defaultMat;

        // Set new selection (latest click wins)
        currentSelectedId = codeId;
        currentSelectedRenderer = rend;

        // Highlight new
        if (currentSelectedRenderer && selectedMat)
            currentSelectedRenderer.material = selectedMat;

        Debug.Log($"[MemoryGame] Selected: ID={codeId}. Click ENTER to confirm.");
    }
    private readonly List<GameObject> chosenInstances = new List<GameObject>(); // Enter로 생성된 것 추적
    public void OnEnterClicked()
    {
        if (!canInput) return;
        if (currentSelectedId < 0) { Debug.Log("No selection."); return; }
        if (inputSeq.Count >= 3) { Debug.Log("Already 3 picks."); return; }

        int idx = inputSeq.Count; // 0 -> 1 -> 2
        Transform pos = containedPos[idx];

        // ▶ 여기서 생성하고 리스트에 기록
        GameObject inst = Instantiate(codePrefabs[currentSelectedId], pos.position, pos.rotation);
        inst.name = $"Chosen_{idx}_ID{currentSelectedId}";
        chosenInstances.Add(inst);                // ★ 생성한 인스턴스 기록

        inputSeq.Add(currentSelectedId);

        // 선택 해제(하이라이트 복구 등)
        if (currentSelectedRenderer && defaultMat) currentSelectedRenderer.material = defaultMat;
        currentSelectedRenderer = null;
        currentSelectedId = -1;

        if (inputSeq.Count == 3)
        {
            canInput = false;
            StartCoroutine(JudgeAfterDelay());
        }
    }

    public void OnEraseClicked()
    {
        if (!canInput) return;

        int cur = inputSeq.Count;
        if (cur <= 0)
        {
            Debug.Log("Nothing to erase.");
            return;
        }

        // ▶ 리스트의 마지막 인스턴스를 정확히 파괴
        int lastIndex = cur - 1;
        if (lastIndex >= 0 && lastIndex < chosenInstances.Count && chosenInstances[lastIndex] != null)
        {
            Destroy(chosenInstances[lastIndex]);  // ★ 정확히 내가 만든 마지막 것을 삭제
        }
        // 리스트에서도 제거
        if (lastIndex >= 0 && lastIndex < chosenInstances.Count)
            chosenInstances.RemoveAt(lastIndex);

        // 시퀀스도 롤백
        inputSeq.RemoveAt(lastIndex);
    }

    private IEnumerator JudgeAfterDelay()
    {
        yield return new WaitForSeconds(0.2f);

        bool ok = true;
        for (int i = 0; i < 3; i++)
        {
            if (inputSeq[i] != answerSeq[i])
            {
                ok = false;
                break;
            }
        }

        if (ok)
            Debug.Log($"[MemoryGame] SUCCESS. Answer [{string.Join(",", answerSeq)}] == Input [{string.Join(",", inputSeq)}]");
        else
            Debug.Log($"[MemoryGame] FAIL. Answer [{string.Join(",", answerSeq)}] != Input [{string.Join(",", inputSeq)}]");
    }

    private int[] GetRandomOrder(int n)
    {
        int[] order = new int[n];
        for (int i = 0; i < n; i++) order[i] = i;
        for (int i = 0; i < n; i++)
        {
            int r = Random.Range(i, n);
            int tmp = order[i]; order[i] = order[r]; order[r] = tmp;
        }
        return order;
    }
}

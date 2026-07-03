using UnityEngine;

public class CutsceneSlideView : MonoBehaviour
{
    private Transform stepRoot;
    private Transform[] steps;
    private int currentStepIndex;

    public RectTransform RectTransform { get; private set; }

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(CutSceneLoader loader)
    {
        if (transform.childCount == 0)
        {
            Debug.LogError($"{name}: 첫 번째 자식에 StepRoot가 필요함.");
            return;
        }

        stepRoot = transform.GetChild(1);

        steps = new Transform[stepRoot.childCount];

        for (int i = 0; i < stepRoot.childCount; i++)
        {
            steps[i] = stepRoot.GetChild(i);
        }

        ResetSteps(loader);
    }

    public bool PlayNextStep(CutSceneLoader loader)
    {
        if (steps == null || currentStepIndex >= steps.Length)
            return false;

        Transform step = steps[currentStepIndex];

        step.gameObject.SetActive(true);

        CutsceneCue[] cues = step.GetComponentsInChildren<CutsceneCue>(true);

        foreach (CutsceneCue cue in cues)
        {
            cue.Play(loader);
        }

        currentStepIndex++;

        return true;
    }

    public void ResetSteps(CutSceneLoader loader)
    {
        currentStepIndex = 0;

        if (steps == null) return;

        foreach (Transform step in steps)
        {
            CutsceneCue[] cues = step.GetComponentsInChildren<CutsceneCue>(true);

            foreach (CutsceneCue cue in cues)
            {
                cue.ResetCue(loader);
            }

            step.gameObject.SetActive(false);
        }
    }

    public void ShowAllSteps(CutSceneLoader loader)
    {
        if (steps == null) return;

        foreach (Transform step in steps)
        {
            step.gameObject.SetActive(true);

            CutsceneCue[] cues = step.GetComponentsInChildren<CutsceneCue>(true);

            foreach (CutsceneCue cue in cues)
            {
                cue.Restore(loader);
            }
        }

        currentStepIndex = steps.Length;
    }
}
using UnityEngine;

public abstract class CutsceneCue : MonoBehaviour
{
    public virtual void Play(CutSceneLoader loader) { }
    public virtual void Restore(CutSceneLoader loader) { }
    public virtual void ResetCue(CutSceneLoader loader) { }
}
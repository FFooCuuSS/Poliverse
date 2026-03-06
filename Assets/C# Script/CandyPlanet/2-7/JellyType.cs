using UnityEngine;

[CreateAssetMenu(menuName = "Minigame/Jelly Type")]
public class JellyType : ScriptableObject
{
    public float moveSpeed;
    public float bounceForce;
    public float xStep;

    public float waveAmount;
    public float waveSpeed;

    public Sprite sr;

    public GameObject jellyPrefab;
}
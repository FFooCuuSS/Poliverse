using UnityEngine;
using UnityEngine.Video;

public class BackgroundVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RenderTexture targetRenderTexture;

    void Start()
    {
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "Lobby_Background.mp4");

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = targetRenderTexture;

        videoPlayer.isLooping = true;
        videoPlayer.Play();
    }
}

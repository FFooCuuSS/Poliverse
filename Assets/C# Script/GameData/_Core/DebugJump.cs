/// <summary>
/// 에디터/개발 빌드에서 특정 미니게임부터 바로 시작하기 위한 전달값.
/// 씬 전환 사이에만 쓰이고, 한 번 읽으면 비워진다.
/// </summary>
public static class DebugJump
{
    // 0 = 처음부터, 4 = 5번째 미니게임(예: 1-5)부터
    private static int pendingStartIndex = 0;

    public static void Request(int startIndex)
    {
        pendingStartIndex = startIndex < 0 ? 0 : startIndex;
    }

    public static int ConsumeStartIndex()
    {
        int value = pendingStartIndex;
        pendingStartIndex = 0;
        return value;
    }

    public static void Clear()
    {
        pendingStartIndex = 0;
    }
}
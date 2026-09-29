using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

// 3-12 잠입 계획 세우기
// CSV: Warn(적 예고) → Turn(적 회전) → Swipe(이동) → Swipe(이동) 4박 루프
public class Minigame_3_12 : MiniGameBase
{
    public enum Dir { Up, Right, Down, Left }

    [System.Serializable]
    public class EnemySetup
    {
        [Tooltip("x = 열, y = 행 (0행이 맨 위)")]
        public Vector2Int cell;
        public Dir[] pattern = { Dir.Up };
    }

    private class Enemy
    {
        public EnemySetup setup;
        public int patternIndex;
        public Dir dir;
        public Dir? next;
        public SpriteRenderer body;
        public SpriteRenderer cone;
        public SpriteRenderer ghost;
        public Vector3 baseScale;
    }

    // ───────── 기본 설정 ─────────
    protected override float TimerDuration => 25f;
    protected override string MinigameTitle => "잠입 계획 세우기";
    protected override string MinigameExplain => "경비원이 고개를 돌린 뒤, 박자에 맞춰 다음 칸으로 이동하세요!";
    protected override string[] AdditionalMinigameExplains => new[]
    {
        "빨간 시야에 들어가면 발각돼요. 앞이 막혀 있으면 기다려도 괜찮아요."
    };

    // 100BPM, 한 박 0.6초 기준. hitWindow는 반 박으로 맞춰 3·4박 노드가 겹치지 않게 함
    public override float perfectWindowOverride => 0.08f;
    public override float goodWindowOverride => 0.18f;
    public override float hitWindowOverride => 0.3f;

    // ───────── Inspector ─────────
    [Header("Map (L = 길, D = 벽 / 0번 줄이 맨 위)")]
    [SerializeField] private string[] mapRows = { "LLDDDLLL", "DLLLDLDL", "DDDLLLDL", "DDDDDDDL" };

    [Tooltip("출발 → 출구 순서. 마지막 칸이 출구")]
    [SerializeField]
    private Vector2Int[] path =
    {
        new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(2,1),
        new Vector2Int(3,1), new Vector2Int(3,2), new Vector2Int(4,2), new Vector2Int(5,2),
        new Vector2Int(5,1), new Vector2Int(5,0), new Vector2Int(6,0), new Vector2Int(7,0),
        new Vector2Int(7,1), new Vector2Int(7,2), new Vector2Int(7,3)
    };

    [Tooltip("(0,0) 칸의 중심 위치. 비우면 이 오브젝트 위치")]
    [SerializeField] private Transform gridOrigin;
    [SerializeField] private float cellSize = 1f;

    [Header("Sprites")]
    [SerializeField] private Sprite tileLight;
    [SerializeField] private Sprite tileDark;
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private Sprite enemySprite;
    [Tooltip("오른쪽을 향하는 시야각. 피벗을 부채꼴 중심에 두고 여백은 Sprite Editor에서 잘라두기")]
    [SerializeField] private Sprite coneSprite;
    [SerializeField] private float playerScale = 0.8f;
    [SerializeField] private float enemyScale = 0.85f;
    [SerializeField] private float coneScale = 0.9f;
    [SerializeField] private int baseSortingOrder = 0;

    [Header("Colors")]
    [SerializeField] private Color visitedTint = new Color(0.78f, 0.78f, 0.78f);
    [SerializeField] private Color exitTint = new Color(0.6f, 1f, 0.7f);
    [SerializeField] private Color caughtTint = new Color(1f, 0.35f, 0.35f);
    [SerializeField] private Color blockedTint = new Color(0.6f, 0.6f, 0.6f);
    [SerializeField, Range(0f, 1f)] private float ghostAlpha = 0.4f;
    [SerializeField] private float beatDuration = 0.6f;

    [Header("Enemies")]
    [SerializeField]
    private List<EnemySetup> enemySetups = new List<EnemySetup>
    {
        new EnemySetup { cell = new Vector2Int(2,0), pattern = new[] { Dir.Left, Dir.Right, Dir.Down, Dir.Right } },
        new EnemySetup { cell = new Vector2Int(2,2), pattern = new[] { Dir.Right, Dir.Left, Dir.Up, Dir.Down } },
        new EnemySetup { cell = new Vector2Int(4,1), pattern = new[] { Dir.Down, Dir.Up, Dir.Left, Dir.Right, Dir.Up } },
        new EnemySetup { cell = new Vector2Int(6,1), pattern = new[] { Dir.Up, Dir.Left, Dir.Right, Dir.Up } },
        new EnemySetup { cell = new Vector2Int(6,2), pattern = new[] { Dir.Left, Dir.Down, Dir.Right, Dir.Down } },
    };
    [Tooltip("처음 N사이클은 적이 벽 쪽만 봄 (적응 구간)")]
    [SerializeField] private int warmupCycles = 1;

    [Header("Input")]
    [SerializeField] private float swipeThresholdInch = 0.15f;

    [Header("SFX (Resources/SFX/ 이름, 비우면 재생 안 함)")]
    [SerializeField] private string sfxMove = "";
    [SerializeField] private string sfxBlocked = "";
    [SerializeField] private string sfxCaught = "";
    [SerializeField] private string sfxEscape = "";

    // ───────── Runtime ─────────
    private static readonly Vector2Int[] DirVec =
    {
        new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0)
    };
    private static readonly float[] DirAngle = { 90f, 0f, -90f, 180f };

    private int rows, cols;
    private SpriteRenderer[,] tiles;
    private SpriteRenderer player;
    private Vector3 playerBaseScale;
    private readonly List<Enemy> enemies = new List<Enemy>();
    private Transform boardRoot;

    private int pathIndex;
    private int countedNodes;   // 점수 대상 노드 수 (정당한 쉼은 제외)
    private int cycle;
    private bool escaped;
    private bool playing;
    private Dir? pendingDir;

    private bool pressing, swipeFired;
    private Vector2 pressPos;
    private Vector2Int pressCell;

    private Vector2Int Current => path[pathIndex];
    private bool HasForward => pathIndex + 1 < path.Length;
    private Vector2Int Forward => path[pathIndex + 1];

    // ───────── Lifecycle ─────────
    protected override void Awake()
    {
        base.Awake();
        ValidateSetup();
        BuildBoard();
        ResetBoard();
    }

    public override void StartGame()
    {
        base.StartGame();
        ResetBoard();
        playing = true;
    }

    public override ScoreResult FinalizeScoreSession()
    {
        // 정당한 쉼과 탈출 후 남은 노드를 전체 노드 수에서 제외
        if (!escaped)
            SetRuntimeTotalNodeCount(countedNodes);

        playing = false;
        return base.FinalizeScoreSession();
    }

    // ───────── Board ─────────
    private void BuildBoard()
    {
        rows = mapRows.Length;
        cols = mapRows[0].Length;

        boardRoot = new GameObject("Board").transform;
        boardRoot.SetParent(transform, false);

        tiles = new SpriteRenderer[cols, rows];
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                var c = new Vector2Int(x, y);
                tiles[x, y] = CreateSprite($"Tile_{x}_{y}", IsWall(c) ? tileDark : tileLight,
                    CellToWorld(c), baseSortingOrder, 1f);
            }
        }

        foreach (var setup in enemySetups)
        {
            var e = new Enemy { setup = setup };
            e.body = CreateSprite("Enemy", enemySprite, CellToWorld(setup.cell), baseSortingOrder + 3, enemyScale);
            e.cone = CreateSprite("Cone", coneSprite, Vector3.zero, baseSortingOrder + 1, coneScale);
            e.ghost = CreateSprite("ConeGhost", coneSprite, Vector3.zero, baseSortingOrder + 2, coneScale);
            e.baseScale = e.body.transform.localScale;
            enemies.Add(e);
        }

        player = CreateSprite("Player", playerSprite, CellToWorld(path[0]), baseSortingOrder + 4, playerScale);
        playerBaseScale = player.transform.localScale;
    }

    private void ResetBoard()
    {
        pathIndex = 0;
        countedNodes = 0;
        cycle = 0;
        escaped = false;
        pendingDir = null;
        pressing = false;

        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
            {
                tiles[x, y].DOKill();
                tiles[x, y].color = Color.white;
            }

        Vector2Int exit = path[path.Length - 1];
        tiles[exit.x, exit.y].DOColor(exitTint, beatDuration)
            .SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);

        player.transform.DOKill();
        player.DOKill();
        player.color = Color.white;
        player.transform.position = CellToWorld(Current);
        player.transform.localScale = playerBaseScale;

        foreach (var e in enemies)
        {
            e.patternIndex = 0;
            Dir first = e.setup.pattern.Length > 0 ? e.setup.pattern[0] : Dir.Up;
            if (warmupCycles > 0) first = PreferWall(e, first);
            e.dir = MakeSafe(e, first);
            e.next = null;
            PlaceCone(e.cone, e, e.dir);
            HideGhost(e);
        }
    }

    // ───────── Rhythm ─────────
    public override void OnRhythmEvent(string action)
    {
        switch (action)
        {
            case "Warn": OnWarn(); break;
            case "Turn": OnTurn(); break;
        }
    }

    // 1박: 다음 방향 결정 + 보정 + 예고 표시
    private void OnWarn()
    {
        if (escaped) return;
        cycle++;

        foreach (var e in enemies)
        {
            if (e.setup.pattern.Length > 0)
                e.patternIndex = (e.patternIndex + 1) % e.setup.pattern.Length;

            Dir want = e.setup.pattern.Length > 0 ? e.setup.pattern[e.patternIndex] : e.dir;
            if (cycle <= warmupCycles) want = PreferWall(e, want);

            e.next = MakeSafe(e, want);
            ShowGhost(e);
            PulseEnemy(e);
        }
    }

    // 2박: 회전
    private void OnTurn()
    {
        if (escaped) return;

        foreach (var e in enemies)
        {
            if (e.next.HasValue)
            {
                e.dir = e.next.Value;
                e.next = null;
            }
            PlaceCone(e.cone, e, e.dir);
            HideGhost(e);
            PulseEnemy(e);
        }

        // 보정 규칙상 발생하지 않아야 함 (안전장치)
        if (IsInCone(Current))
            Debug.LogWarning("[StealthPlan] 회전이 플레이어 칸을 비춤 - 패턴 보정 확인 필요");
    }

    public override void OnJudgement(JudgementResult judgement)
    {
        if (escaped) return;

        // 입력 없이 만료된 노드 (CheckMisses 자동 Miss)
        if (!pendingDir.HasValue)
        {
            OnNodeExpired();
            return;
        }

        Dir d = pendingDir.Value;
        pendingDir = null;
        countedNodes++;

        if (judgement == JudgementResult.Miss)
        {
            BlockedFx();
            return;
        }

        // 역방향, 벽 방향 → 이동 없이 Miss (base 미호출 = Miss로 남음)
        if (!HasForward || Current + DirVec[(int)d] != Forward)
        {
            BlockedFx();
            return;
        }

        if (IsInCone(Forward))
        {
            CaughtFx(Forward);
            return;
        }

        pathIndex++;
        MovePlayerFx();
        base.OnJudgement(judgement);

        if (pathIndex == path.Length - 1)
            Escape();
    }

    private void OnNodeExpired()
    {
        // 앞 칸이 시야 안이면 정당한 쉼 → 점수 대상에서 제외
        bool legitRest = HasForward && IsInCone(Forward);
        if (!legitRest)
            countedNodes++;
    }

    private void Escape()
    {
        escaped = true;
        SetRuntimeTotalNodeCount(countedNodes);

        foreach (var e in enemies) HideGhost(e);

        Vector2Int exit = path[path.Length - 1];
        tiles[exit.x, exit.y].DOKill();
        tiles[exit.x, exit.y].color = exitTint;

        player.transform.DOPunchScale(playerBaseScale * 0.3f, 0.3f, 2, 0.5f)
            .SetDelay(0.1f).SetLink(gameObject);
        PlaySFX(sfxEscape);

        Success();
    }

    // 연습 예시보기: 안전하면 앞으로 한 칸 (점수 없음)
    public override void ExecutePracticeAction(int actionIndex, string actionType)
    {
        if (escaped || actionType != "Swipe" || !HasForward) return;
        if (IsInCone(Forward)) return;

        pathIndex++;
        MovePlayerFx();
        if (pathIndex == path.Length - 1) escaped = true;
    }

    // ───────── Input ─────────
    private void Update()
    {
        if (!playing || escaped) return;

        if (Input.GetMouseButtonDown(0))
        {
            pressing = true;
            swipeFired = false;
            pressPos = Input.mousePosition;
            pressCell = ScreenToCell(pressPos);
        }

        // 임계값을 넘는 순간 판정 (손을 뗄 때 판정하면 체감상 늦음)
        if (pressing && !swipeFired && Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - pressPos;
            if (delta.magnitude >= SwipeThresholdPx)
            {
                swipeFired = true;
                HandleInput(DirFromDelta(delta));
            }
        }

        if (pressing && Input.GetMouseButtonUp(0))
        {
            pressing = false;
            if (!swipeFired)
            {
                int i = System.Array.IndexOf(DirVec, pressCell - Current);
                if (i >= 0) HandleInput((Dir)i);
            }
        }
    }

    private void HandleInput(Dir d)
    {
        if (IsInputLocked) return;

        pendingDir = d;
        OnPlayerInput("Swipe");   // 판정되면 OnJudgement가 동기로 호출되어 pendingDir을 소비

        if (pendingDir.HasValue)  // 판정 가능한 노드 없음 = 박자 밖 입력
        {
            pendingDir = null;
            OffBeatFx();
        }
    }

    private float SwipeThresholdPx => (Screen.dpi > 0 ? Screen.dpi : 160f) * swipeThresholdInch;

    private static Dir DirFromDelta(Vector2 d)
    {
        if (Mathf.Abs(d.x) > Mathf.Abs(d.y))
            return d.x > 0 ? Dir.Right : Dir.Left;
        return d.y > 0 ? Dir.Up : Dir.Down;
    }

    private Vector2Int ScreenToCell(Vector2 screen)
    {
        Camera cam = Camera.main;
        if (cam == null) return new Vector2Int(-99, -99);

        Vector3 w = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, Mathf.Abs(cam.transform.position.z)));
        Vector3 local = w - Origin;
        return new Vector2Int(Mathf.RoundToInt(local.x / cellSize), Mathf.RoundToInt(-local.y / cellSize));
    }

    // ───────── Enemy rules ─────────
    private Vector2Int ConeTarget(Enemy e, Dir d) => e.setup.cell + DirVec[(int)d];

    private bool IsInCone(Vector2Int c)
    {
        foreach (var e in enemies)
        {
            Vector2Int t = ConeTarget(e, e.dir);
            if (InBounds(t) && t == c) return true;
        }
        return false;
    }

    // 규칙: 플레이어가 서 있는 칸과 다음 경로 칸은 절대 비추지 않음
    private bool Blocks(Enemy e, Dir d)
    {
        Vector2Int t = ConeTarget(e, d);
        return t == Current || (HasForward && t == Forward);
    }

    private Dir MakeSafe(Enemy e, Dir want)
    {
        if (!Blocks(e, want)) return want;

        // 벽/맵 밖을 보는 방향 우선
        for (int pass = 0; pass < 2; pass++)
        {
            for (int i = 0; i < 4; i++)
            {
                Dir d = (Dir)i;
                bool wallSide = IsWallOrOut(ConeTarget(e, d));
                if ((pass == 0) != wallSide) continue;
                if (!Blocks(e, d)) return d;
            }
        }

        Debug.LogWarning($"[StealthPlan] 적 {e.setup.cell} 보정 불가");
        return want;
    }

    private Dir PreferWall(Enemy e, Dir want)
    {
        if (IsWallOrOut(ConeTarget(e, want))) return want;
        for (int i = 0; i < 4; i++)
            if (IsWallOrOut(ConeTarget(e, (Dir)i))) return (Dir)i;
        return want;
    }

    // ───────── FX ─────────
    private void PlaceCone(SpriteRenderer sr, Enemy e, Dir d)
    {
        Vector2Int t = ConeTarget(e, d);
        if (!InBounds(t))
        {
            sr.enabled = false;
            return;
        }
        sr.enabled = true;
        sr.transform.position = CellToWorld(t);
        sr.transform.rotation = Quaternion.Euler(0f, 0f, DirAngle[(int)d]);
    }

    private void ShowGhost(Enemy e)
    {
        if (!e.next.HasValue) return;
        PlaceCone(e.ghost, e, e.next.Value);

        e.ghost.DOKill();
        e.ghost.color = new Color(1f, 1f, 1f, ghostAlpha);
        e.ghost.DOFade(ghostAlpha * 0.25f, beatDuration * 0.25f)
            .SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
    }

    private void HideGhost(Enemy e)
    {
        e.ghost.DOKill();
        e.ghost.enabled = false;
    }

    private void PulseEnemy(Enemy e)
    {
        e.body.transform.DOKill(true);
        e.body.transform.DOPunchScale(e.baseScale * 0.15f, 0.12f, 1, 0f).SetLink(gameObject);
    }

    private void MovePlayerFx()
    {
        if (pathIndex > 0)
        {
            Vector2Int prev = path[pathIndex - 1];
            tiles[prev.x, prev.y].color = visitedTint;
        }

        player.transform.DOKill(true);
        player.transform.DOMove(CellToWorld(Current), 0.1f).SetEase(Ease.OutQuad).SetLink(gameObject);
        player.transform.DOPunchScale(playerBaseScale * 0.12f, 0.12f, 1, 0f).SetLink(gameObject);
        PlaySFX(sfxMove);
    }

    private void BlockedFx()
    {
        player.transform.DOKill(true);
        player.transform.DOShakePosition(0.15f, new Vector3(0.08f * cellSize, 0f, 0f), 20, 0f)
            .SetLink(gameObject);
        FlashPlayer(blockedTint);
        PlaySFX(sfxBlocked);
    }

    private void CaughtFx(Vector2Int target)
    {
        Vector3 from = CellToWorld(Current);
        Vector3 mid = Vector3.Lerp(from, CellToWorld(target), 0.4f);

        player.transform.DOKill(true);
        DOTween.Sequence()
            .Append(player.transform.DOMove(mid, 0.06f))
            .Append(player.transform.DOMove(from, 0.12f).SetEase(Ease.OutBack))
            .SetLink(gameObject);
        FlashPlayer(caughtTint);
        PlaySFX(sfxCaught);
    }

    private void OffBeatFx()
    {
        player.transform.DOKill(true);
        player.transform.DOPunchScale(-playerBaseScale * 0.08f, 0.1f, 1, 0f).SetLink(gameObject);
    }

    private void FlashPlayer(Color c)
    {
        player.DOKill();
        player.color = c;
        player.DOColor(Color.white, 0.3f).SetLink(gameObject);
    }

    // ───────── Utils ─────────
    private Vector3 Origin => gridOrigin != null ? gridOrigin.position : transform.position;

    private Vector3 CellToWorld(Vector2Int c) =>
        Origin + new Vector3(c.x * cellSize, -c.y * cellSize, 0f);

    private bool InBounds(Vector2Int c) => c.x >= 0 && c.x < cols && c.y >= 0 && c.y < rows;

    private bool IsWall(Vector2Int c) => mapRows[c.y][c.x] == 'D';

    private bool IsWallOrOut(Vector2Int c) => !InBounds(c) || IsWall(c);

    private SpriteRenderer CreateSprite(string name, Sprite sprite, Vector3 pos, int order, float sizeInCells)
    {
        var go = new GameObject(name);
        go.transform.SetParent(boardRoot, false);
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;

        if (sprite != null)
        {
            float w = sprite.bounds.size.x;
            if (w > 0f) go.transform.localScale = Vector3.one * (cellSize * sizeInCells / w);
        }
        return sr;
    }

    private void ValidateSetup()
    {
        if (mapRows == null || mapRows.Length == 0 || path == null || path.Length < 2)
        {
            Debug.LogError("[StealthPlan] mapRows 또는 path 설정이 비어 있음");
            return;
        }

        int w = mapRows[0].Length;
        foreach (var r in mapRows)
            if (r.Length != w) Debug.LogError("[StealthPlan] mapRows 줄 길이가 다름");

        for (int i = 0; i < path.Length; i++)
        {
            Vector2Int c = path[i];
            if (c.y < 0 || c.y >= mapRows.Length || c.x < 0 || c.x >= w || mapRows[c.y][c.x] != 'L')
                Debug.LogError($"[StealthPlan] path[{i}] {c} 가 길 타일이 아님");

            if (i > 0 && (Mathf.Abs(c.x - path[i - 1].x) + Mathf.Abs(c.y - path[i - 1].y)) != 1)
                Debug.LogError($"[StealthPlan] path[{i - 1}] → path[{i}] 가 인접하지 않음");
        }
    }
}
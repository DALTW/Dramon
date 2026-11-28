using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>
    /// 숲 보드의 상태와 규칙: 둥지 배치, 안전한 칸 밝히기(연쇄), 경고 표식, 둥지 상태(봉인·결투 중·정화), 승리 판정.
    /// "어떤 도구로 골랐을 때 무엇을 할지"는 TileActionResolver가 정하고, 보드는 요청받은 일만 한다.
    /// 칸은 Scene에 미리 배치된 ForestTile을 그대로 쓴다. 단, 스테이지 진행(StageProgress)이 숲 크기를 정하면
    /// Scene 칸이 그 크기와 다를 때만 칸 Prefab으로 '스테이지 칸'을 실행 중에 만들고 Scene 칸은 잠시 숨긴다 (Scene 칸은 지우거나 옮기지 않음).
    /// </summary>
    [RequireComponent(typeof(NestPlacer))]
    public class ForestBoard : MonoBehaviour
    {
        [Header("보드 크기 ('칸 다시 만들기' 버튼에서만 사용)")]
        [Tooltip("가로 칸 수. 바꾼 뒤 Inspector 아래 '칸 다시 만들기'를 눌러야 Scene에 반영돼요")]
        [SerializeField, Min(2)] int width = 9;
        [Tooltip("세로 칸 수. 바꾼 뒤 '칸 다시 만들기'를 눌러야 반영돼요")]
        [SerializeField, Min(2)] int height = 9;
        [Tooltip("칸 사이 간격(월드 단위, 1 = 32px). '칸 다시 만들기' 때 배치에 쓰여요")]
        [SerializeField, Min(0.1f)] float tileSpacing = 1f;

        [Header("규칙")]
        [Tooltip("켜면 안전한 칸을 모두 열었을 때 남은 둥지에 경고 표식을 자동으로 세우고 보드를 잠가요 (옛 지뢰찾기 방식). 레몬 표시 단계에서는 꺼 두세요")]
        [SerializeField] bool autoCompleteWhenSafeCleared = true;

        [Header("연결")]
        [Tooltip("'칸 다시 만들기'에 쓸 숲 칸 Prefab")]
        [SerializeField] ForestTile tilePrefab;
        [Tooltip("둥지 배치를 맡는 컴포넌트 (같은 오브젝트에 붙어 있음)")]
        [SerializeField] NestPlacer nestPlacer;
        [Tooltip("이 보드의 칸 목록. Scene에서 칸을 지우거나 옮겨도 목록에 남은 칸으로 게임이 진행돼요")]
        [SerializeField] List<ForestTile> tiles = new List<ForestTile>();

        [Header("스테이지 칸 (실행 중)")]
        [Tooltip("스테이지 크기로 만든 칸을 넣을 부모. 비우면 보드 아래에 'StageTiles'를 실행 중에 만들어요")]
        [SerializeField] Transform stageTileParent;

        /// <summary>첫 행동으로 둥지가 배치됐을 때</summary>
        public event Action FirstRevealed;
        /// <summary>둥지를 뺀 모든 칸이 밝혀졌을 때</summary>
        public event Action Cleared;
        /// <summary>경고 표식 수가 바뀌었을 때</summary>
        public event Action<int> MarkerCountChanged;
        /// <summary>칸이 밝혀지거나 레몬이 놓이는 등 보드 상태가 바뀌었을 때</summary>
        public event Action BoardChanged;

        /// <summary>레몬을 놓은 결과</summary>
        public enum LemonPlacement { Ignored, OnNest, OnSafeTile }

        enum NestStatus { Hidden, Active, Sealed }

        class TileState
        {
            public ForestTile view;
            public bool nest, revealed, marked, lemoned;
            public NestStatus nestStatus;
            public int nearbyNests;
        }

        public static readonly Vector2Int[] NeighborOffsets =
        {
            new(-1, -1), new(0, -1), new(1, -1),
            new(-1, 0),              new(1, 0),
            new(-1, 1),  new(0, 1),  new(1, 1),
        };

        readonly Dictionary<Vector2Int, TileState> states = new();
        readonly List<ForestTile> activeTiles = new();
        readonly List<ForestTile> stageTiles = new();
        int activeWidth, activeHeight;
        bool nestsPlaced;
        int nestTotal;
        int revealedSafeCount;

        /// <summary>지금 쓰는 숲의 가로 칸 수 (스테이지 칸이면 스테이지 크기)</summary>
        public int Width => activeWidth > 0 ? activeWidth : width;
        /// <summary>지금 쓰는 숲의 세로 칸 수</summary>
        public int Height => activeHeight > 0 ? activeHeight : height;
        public float TileSpacing => tileSpacing;
        public ForestTile TilePrefab => tilePrefab;
        /// <summary>지금 게임에 쓰는 칸 (Scene 칸 또는 스테이지 칸)</summary>
        public IReadOnlyList<ForestTile> Tiles => activeTiles.Count > 0 ? activeTiles : tiles;
        /// <summary>검 사냥 대상(깨어난 둥지)으로 남은 수</summary>
        public int SealedNestCount
        {
            get
            {
                int n = 0;
                foreach (var s in states.Values) if (s.nest && s.lemoned && s.nestStatus == NestStatus.Hidden) n++;
                return n;
            }
        }

        /// <summary>true면 입력을 받지 않는다 (게임 오버·클리어·결투 중).</summary>
        public bool IsLocked { get; private set; }
        public int MarkerCount { get; private set; }
        /// <summary>둥지 배치가 끝났는지 (첫 칸을 연 뒤 true)</summary>
        public bool NestsPlaced => nestsPlaced;
        /// <summary>굴에 맞게 놓인 레몬 수</summary>
        public int LemonCount { get; private set; }
        /// <summary>둥지가 아닌 칸이 모두 밝혀졌는지</summary>
        public bool AllSafeRevealed => nestsPlaced && revealedSafeCount >= states.Count - nestTotal;
        public int NestCount => nestsPlaced ? nestTotal : Mathf.Clamp(nestPlacer.NestCount, 1, Mathf.Max(1, states.Count - 1));
        /// <summary>남은 둥지 수 = 전체 둥지 - 경고 표식 - 이미 처리된(봉인·결투) 둥지</summary>
        public int NestsLeft => NestCount - MarkerCount - CountHandledNests();

        void Reset()
        {
            nestPlacer = GetComponent<NestPlacer>();
        }

        void Awake()
        {
            if (nestPlacer == null) nestPlacer = GetComponent<NestPlacer>();
            UseSceneTiles();
            BuildLookup();
            ResetBoard();
        }

        void UseSceneTiles()
        {
            activeTiles.Clear();
            foreach (var t in tiles) if (t != null) activeTiles.Add(t);
            activeWidth = activeHeight = 0;
        }

        /// <summary>
        /// 숲을 w×h 크기로 맞춘다 (스테이지 시작 때 StageProgress가 호출). Scene 칸이 정확히 그 크기면 Scene 칸을 그대로 쓰고,
        /// 아니면 칸 Prefab으로 스테이지 칸을 새로 만들고 Scene 칸은 숨긴다. 끝나면 보드를 처음 상태로 되돌린다.
        /// </summary>
        public void BuildStageTiles(int w, int h)
        {
            w = Mathf.Max(2, w);
            h = Mathf.Max(2, h);
            ClearStageTiles();
            if (SceneTilesMatch(w, h))
            {
                SetSceneTilesVisible(true);
                UseSceneTiles();
            }
            else if (tilePrefab == null)
            {
                Debug.LogWarning("[ForestBoard] 'Tile Prefab'이 비어 있어서 스테이지 크기로 칸을 만들 수 없어요. Scene 칸을 그대로 써요.", this);
                SetSceneTilesVisible(true);
                UseSceneTiles();
            }
            else
            {
                SetSceneTilesVisible(false);
                var parent = StageParent();
                activeTiles.Clear();
                for (int y = h - 1; y >= 0; y--)
                for (int x = 0; x < w; x++)
                {
                    var tile = Instantiate(tilePrefab, parent);
                    tile.name = $"StageTile_{x}_{y}";
                    tile.transform.localPosition = GetTileLocalPosition(x, y, w, h);
                    tile.AssignRuntime(this, new Vector2Int(x, y));
                    stageTiles.Add(tile);
                    activeTiles.Add(tile);
                }
                activeWidth = w;
                activeHeight = h;
            }
            BuildLookup();
            ResetBoard();
        }

        bool SceneTilesMatch(int w, int h)
        {
            int count = 0;
            var seen = new HashSet<Vector2Int>();
            foreach (var t in tiles)
            {
                if (t == null) continue;
                var p = t.GridPosition;
                if (p.x < 0 || p.y < 0 || p.x >= w || p.y >= h || !seen.Add(p)) return false;
                count++;
            }
            return count == w * h;
        }

        void SetSceneTilesVisible(bool visible)
        {
            foreach (var t in tiles) if (t != null && t.gameObject.activeSelf != visible) t.gameObject.SetActive(visible);
        }

        void ClearStageTiles()
        {
            foreach (var t in stageTiles) if (t != null) Destroy(t.gameObject);
            stageTiles.Clear();
        }

        Transform StageParent()
        {
            if (stageTileParent != null) return stageTileParent;
            var existing = transform.Find("StageTiles");
            if (existing != null) return existing;
            var go = new GameObject("StageTiles");
            go.transform.SetParent(transform, false);
            return go.transform;
        }

        /// <summary>지금 숲의 월드 범위 (칸 가장자리까지)</summary>
        public Bounds WorldBounds
        {
            get
            {
                bool any = false;
                var b = new Bounds();
                foreach (var t in activeTiles)
                {
                    if (t == null) continue;
                    var c = new Bounds(t.transform.position, Vector3.one * tileSpacing * t.transform.lossyScale.x);
                    if (!any) { b = c; any = true; } else b.Encapsulate(c);
                }
                return any ? b : new Bounds(transform.position, Vector3.zero);
            }
        }

        void BuildLookup()
        {
            states.Clear();
            foreach (var tile in activeTiles)
            {
                if (tile == null) continue;
                if (states.ContainsKey(tile.GridPosition))
                {
                    Debug.LogWarning($"[ForestBoard] 좌표 {tile.GridPosition}가 겹치는 칸이 있어요: {tile.name}", tile);
                    continue;
                }
                states[tile.GridPosition] = new TileState { view = tile };
            }
        }

        /// <summary>모든 칸을 가려진 숲으로 되돌린다. 칸의 위치·배치는 건드리지 않는다.</summary>
        public void ResetBoard()
        {
            foreach (var s in states.Values)
            {
                s.nest = s.revealed = s.marked = s.lemoned = false;
                s.nestStatus = NestStatus.Hidden;
                s.nearbyNests = 0;
                s.view.ShowHidden();
            }
            nestsPlaced = false;
            nestTotal = 0;
            revealedSafeCount = 0;
            LemonCount = 0;
            IsLocked = false;
            SetMarkerCount(0);
            BoardChanged?.Invoke();
        }

        public void SetLocked(bool locked) => IsLocked = locked;

        // ── 조회 ─────────────────────────────────
        public bool HasTile(Vector2Int pos) => states.ContainsKey(pos);
        public bool IsRevealed(Vector2Int pos) => states.TryGetValue(pos, out var s) && s.revealed;
        public bool IsMarked(Vector2Int pos) => states.TryGetValue(pos, out var s) && s.marked;
        /// <summary>아직 아무것도 하지 않은 칸(가려짐, 표식 없음)인지</summary>
        public bool IsUntouched(Vector2Int pos) => states.TryGetValue(pos, out var s) && !s.revealed && !s.marked && !s.lemoned;
        /// <summary>레몬이 놓인 칸인지</summary>
        public bool HasLemon(Vector2Int pos) => states.TryGetValue(pos, out var s) && s.lemoned;
        /// <summary>둥지인지. 둥지가 배치되기 전에는 항상 false (EnsureNestsPlaced 먼저 호출)</summary>
        public bool IsNest(Vector2Int pos) => states.TryGetValue(pos, out var s) && s.nest;
        /// <summary>검 사냥 대상인 '봉인된 굴'인지: 레몬으로 막아 두고 아직 결투·정화하지 않은 굴</summary>
        public bool IsSealedNest(Vector2Int pos) => states.TryGetValue(pos, out var s) && s.nest && s.lemoned && s.nestStatus == NestStatus.Hidden;

        /// <summary>
        /// 밝혀진 숫자 칸 주변에서 '둥지로 확인된 칸'(경고 표식 + 봉인·결투한 둥지) 수가 숫자와 같으면 true.
        /// 이때 주변의 나머지 칸을 한꺼번에 골라도 된다.
        /// </summary>
        public bool CanActAround(Vector2Int pos)
        {
            if (!states.TryGetValue(pos, out var s) || !s.revealed || s.nest || s.nearbyNests == 0) return false;
            int known = 0;
            foreach (var n in Neighbors(pos))
                if (n.marked || n.lemoned || (n.nest && n.revealed)) known++;
            return known == s.nearbyNests;
        }

        // ── 행동 ─────────────────────────────────
        /// <summary>첫 행동 전이라면 firstPos와 주변을 피해 둥지를 배치한다.</summary>
        public void EnsureNestsPlaced(Vector2Int firstPos)
        {
            if (nestsPlaced) return;
            PlaceNests(firstPos);
            FirstRevealed?.Invoke();
        }

        /// <summary>안전한 칸을 밝힌다. 숫자 표시, 0이면 연쇄로 밝힘. 마지막 안전 칸이면 클리어.</summary>
        public void RevealSafe(Vector2Int pos)
        {
            if (IsLocked || !states.TryGetValue(pos, out var s) || s.revealed || s.marked || s.lemoned || s.nest) return;
            FloodReveal(pos);
            BoardChanged?.Invoke();
            if (autoCompleteWhenSafeCleared && AllSafeRevealed)
            {
                IsLocked = true;
                MarkAllNests();
                Cleared?.Invoke();
            }
        }

        /// <summary>둥지를 봉인한다 (레몬). 결투 없이 정화된 둥지로 표시.</summary>
        public void SealNest(Vector2Int pos)
        {
            if (!TryGetNest(pos, out var s)) return;
            s.revealed = true;
            s.nestStatus = NestStatus.Sealed;
            s.view.ShowNestPurified();
        }

        /// <summary>둥지를 결투 중 상태로 바꾼다 (검).</summary>
        public void ActivateNest(Vector2Int pos)
        {
            if (!TryGetNest(pos, out var s)) return;
            s.revealed = true;
            s.nestStatus = NestStatus.Active;
            s.view.ShowNestActive();
        }

        /// <summary>검 사냥: 봉인된 굴(레몬으로 막은 굴)에 들어가 결투 중 상태로 바꾼다.</summary>
        /// nestSprites에 드래곤 종류별 둥지 그림을 주면 그 그림을 쓴다 (비어 있으면 기본 그림).
        public void ActivateSealedNest(Vector2Int pos, NestSpriteSet nestSprites = null)
        {
            if (!IsSealedNest(pos)) return;
            var s = states[pos];
            s.nestStatus = NestStatus.Active;
            s.view.ShowNestActive(nestSprites != null ? nestSprites.ActiveFrames : null);
        }

        /// <summary>결투에서 이긴 둥지를 정화된 상태로 바꾼다.</summary>
        /// nestSprites에 드래곤 종류별 둥지 그림을 주면 그 정화된 둥지 그림을 쓴다 (비어 있으면 기본 그림).
        public void PurifyNest(Vector2Int pos, NestSpriteSet nestSprites = null)
        {
            if (!states.TryGetValue(pos, out var s) || !s.nest) return;
            s.revealed = true;
            s.nestStatus = NestStatus.Sealed;
            s.view.ShowNestPurified(nestSprites != null ? nestSprites.Purified : null);
        }

        /// <summary>
        /// 가려진 칸에 레몬을 놓는다. 둥지면 레몬이 남고(OnNest), 둥지가 아니면 실수(OnSafeTile).
        /// 레몬을 놓은 칸은 다시 열거나 레몬을 뺄 수 없다.
        /// </summary>
        public LemonPlacement PlaceLemon(Vector2Int pos)
        {
            if (IsLocked || !nestsPlaced || !states.TryGetValue(pos, out var s) || s.revealed || s.marked || s.lemoned)
                return LemonPlacement.Ignored;

            s.lemoned = true;
            if (s.nest)
            {
                LemonCount++;
                s.view.ShowLemonOnForest();
                BoardChanged?.Invoke();
                return LemonPlacement.OnNest;
            }

            s.view.ShowWrongLemon();
            BoardChanged?.Invoke();
            return LemonPlacement.OnSafeTile;
        }

        /// <summary>레몬으로 막아 둔 굴(아직 결투하지 않은 깨어난 둥지)을 모두 '봉인된 굴'(Nest_Revealed + 레몬) 모습으로 바꾼다.</summary>
        public void ShowLemonedNestsAsSealed()
        {
            foreach (var s in states.Values)
                if (s.nest && s.lemoned && s.nestStatus == NestStatus.Hidden) s.view.ShowSealedNest();
        }

        /// <summary>검 사냥 대상인 봉인된 굴 좌표 목록 (깨어난 둥지를 고를 때 씀)</summary>
        public List<Vector2Int> GetSealedNests()
        {
            var list = new List<Vector2Int>();
            foreach (var pair in states)
                if (pair.Value.nest && pair.Value.lemoned && pair.Value.nestStatus == NestStatus.Hidden) list.Add(pair.Key);
            return list;
        }

        /// <summary>깨어나지 않은 둥지: 레몬 봉인으로 끝 (결투 없음, 정화된 둥지 모습)</summary>
        public void SetNestDormant(Vector2Int pos)
        {
            if (!IsSealedNest(pos)) return;
            var s = states[pos];
            s.nestStatus = NestStatus.Sealed;
            s.view.ShowNestPurified();
        }

        public void ToggleMarker(Vector2Int pos)
        {
            if (IsLocked || !states.TryGetValue(pos, out var s) || s.revealed || s.lemoned) return;
            s.marked = !s.marked;
            if (s.marked) s.view.ShowWarningMarker(); else s.view.ShowHidden();
            SetMarkerCount(MarkerCount + (s.marked ? 1 : -1));
        }

        /// <summary>아직 드러나지 않은 둥지를 모두 보여준다 (게임 오버 연출용).</summary>
        public void RevealAllNests()
        {
            foreach (var s in states.Values)
                if (s.nest && !s.revealed && !s.marked && !s.lemoned) s.view.ShowNest();
        }

        // ── 내부 ─────────────────────────────────
        bool TryGetNest(Vector2Int pos, out TileState s)
        {
            return states.TryGetValue(pos, out s) && s.nest && !s.revealed && !s.marked && !s.lemoned && !IsLocked;
        }

        int CountHandledNests()
        {
            int count = 0;
            foreach (var s in states.Values) if (s.nest && s.revealed) count++;
            return count;
        }

        void PlaceNests(Vector2Int firstPos)
        {
            var nests = nestPlacer.Place(states.Keys, firstPos);
            foreach (var p in nests) states[p].nest = true;
            nestTotal = nests.Count;

            foreach (var pair in states)
            {
                int count = 0;
                foreach (var n in Neighbors(pair.Key)) if (n.nest) count++;
                pair.Value.nearbyNests = count;
            }
            nestsPlaced = true;
        }

        void FloodReveal(Vector2Int start)
        {
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                var s = states[p];
                if (s.revealed || s.marked || s.lemoned || s.nest) continue;

                s.revealed = true;
                revealedSafeCount++;
                s.view.ShowRevealed(s.nearbyNests);

                if (s.nearbyNests != 0) continue;
                foreach (var offset in NeighborOffsets)
                {
                    var np = p + offset;
                    if (states.TryGetValue(np, out var n) && !n.revealed) queue.Enqueue(np);
                }
            }
        }

        IEnumerable<TileState> Neighbors(Vector2Int pos)
        {
            foreach (var offset in NeighborOffsets)
                if (states.TryGetValue(pos + offset, out var n)) yield return n;
        }

        void MarkAllNests()
        {
            int newMarkers = 0;
            foreach (var s in states.Values)
            {
                if (!s.nest || s.marked || s.revealed) continue;
                s.marked = true;
                s.view.ShowWarningMarker();
                newMarkers++;
            }
            SetMarkerCount(MarkerCount + newMarkers);
        }

        void SetMarkerCount(int value)
        {
            MarkerCount = value;
            MarkerCountChanged?.Invoke(value);
        }

        /// <summary>'칸 다시 만들기' 때 칸이 놓일 로컬 위치 (보드 중심 기준).</summary>
        public Vector3 GetDefaultTileLocalPosition(int x, int y) => GetTileLocalPosition(x, y, width, height);

        /// <summary>w×h 숲에서 칸이 놓일 로컬 위치 (보드 중심 기준)</summary>
        public Vector3 GetTileLocalPosition(int x, int y, int w, int h) =>
            new Vector3((x - (w - 1) * 0.5f) * tileSpacing, (y - (h - 1) * 0.5f) * tileSpacing, 0f);
    }
}

# Dramon

Unity로 개발 중인 2D 탑다운 게임 **Dramon** 프로젝트입니다.
지뢰찾기(Minesweeper)의 규칙과 구조를 기반으로, 그 위에 Dramon만의 테마와 결투 시스템을 얹어 만들고 있습니다.

> 플레이어는 어두운 숲을 탐험하며 드래곤의 공격을 피해 숲 전체를 밝혀야 합니다.
> 숲 곳곳에는 드래곤의 둥지가 숨어 있고, 드래곤은 레몬을 싫어합니다.
> 레몬으로 숲을 밝히고, 둥지를 만나면 검을 들고 드래곤과 카드 결투를 벌입니다.

## 개발 환경

| 항목 | 내용 |
| --- | --- |
| 엔진 | Unity 6.6 (`6000.6.9f1`) |
| 렌더 파이프라인 | URP 2D (Renderer2D) + Pixel Perfect Camera (32 PPU, 640×360 기준) |
| 입력 | Input System (New Input System 전용) |
| UI | uGUI (Canvas) |

## 시작하기

1. 저장소를 클론합니다.
   ```bash
   git clone https://github.com/DALTW/Dramon.git
   ```
2. Unity Hub에서 **Add → 클론한 폴더**를 선택해 프로젝트를 추가합니다. (Unity `6000.6.9f1` 필요)
3. `Assets/Dramon/Scenes/Forest.unity`를 열고 ▶ Play를 누릅니다.
   - Game 뷰 해상도를 1920×1080으로 두면 픽셀아트가 실제 비율로 보입니다.

## 조작법

| 입력 | 동작 |
| --- | --- |
| 좌클릭 | 들고 있는 도구(레몬/검)로 칸 고르기 |
| 우클릭 | 경고 표식 놓기/빼기 (표식이 있는 칸은 좌클릭이 막힘) |
| 숫자 칸 좌클릭 · 휠 클릭 | 주변의 확인된 둥지 수가 숫자와 같으면 남은 주변 칸을 한꺼번에 고르기 |
| `R` | 도구 바꾸기 (레몬 ↔ 검) — 결투 중에는 막힘 |
| Restart 버튼 | 숲을 처음부터 다시 시작 |

첫 번째로 고르는 칸과 그 주변 8칸에는 둥지가 나오지 않습니다.

## 게임 규칙

### 지뢰찾기 → Dramon 대응

| 지뢰찾기 | Dramon |
| --- | --- |
| 보드 | 어두운 숲 (칸 = 숲의 한 구역) |
| 지뢰 | 드래곤 둥지 |
| 숫자 | 주변 8칸의 둥지 수 (드래곤의 기척) |
| 깃발 | 경고 표식 (빨간 리본을 단 나무 말뚝) |
| 모든 안전 칸 열기 = 승리 | 숲 전체를 밝히면 승리 |

### 도구별 결과

| 들고 있는 도구 | 안전한 칸 | 드래곤 둥지 |
| --- | --- | --- |
| 레몬 | 레몬을 두고 칸이 밝혀짐 (0이면 연쇄로 밝혀짐) | 드래곤이 피해 가서 결투 없이 봉인 (보상 없음) |
| 검 | 칸은 밝혀지지만 헛손질로 점수 감점 | 드래곤과 결투 시작 |

- 결투에서 이기면 둥지가 정화되고 탐험을 이어갑니다. 지면 즉시 게임 오버입니다.
- 결투 승리 보상은 카드 획득입니다. (결투 시스템 단계에서 구현 예정)
- 카드 결투는 아직 구현 전이라, 지금은 임시 패널의 **[결투 승리] / [결투 패배]** 버튼으로 결과를 정합니다.

### 점수

레몬은 무제한이지만 많이 쓸수록 점수가 줄어듭니다.

```
점수 = 기본 점수(1000) − 쓴 레몬 수 × 10 − 검 헛손질 수 × 30
```

세 값은 `Managers/ExplorationScore`의 Inspector에서 바꿀 수 있습니다.

## 폴더 구조

```
Assets/
├─ Dramon/
│  ├─ Art/            # 아트 에셋 (임포트 설정·파일 이름은 변경 금지)
│  │  ├─ Board/  Lemon/  Player/  Dragon/  Battle/  UI/
│  ├─ Scenes/         # Forest.unity (메인 Scene)
│  ├─ Prefabs/
│  │  ├─ Board/       # ForestTile
│  │  └─ UI/          # HudButton
│  ├─ Settings/       # ForestTileSpriteSet, ToolSettings (ScriptableObject)
│  ├─ Scripts/
│  │  ├─ Board/       # 숲 보드 규칙, 칸 표시, 둥지 배치, 프레임 애니메이션
│  │  ├─ Exploration/ # 탐험 상태, 입력, 도구별 결과 처리, 점수
│  │  ├─ Tools/       # 도구 종류, 도구 전환, 도구 설정
│  │  ├─ Duel/        # 결투 시작·결과 (현재 임시)
│  │  └─ UI/          # HUD, 도구 커서, 도구 표시, 임시 결투 패널
│  └─ Editor/         # Scene 구성·기능 설치·보드 재생성 에디터 도구
└─ Minesweeper/       # 초기 지뢰찾기 프로토타입 (참고용, Assets/Scenes/Minesweeper.unity)
```

## Scene 구성 (`Forest.unity`)

```
Forest
├─ Main Camera              Pixel Perfect Camera
├─ Global Light 2D
├─ Managers
│  ├─ ExplorationState      탐험 단계 (대기·탐험·결투·클리어·게임 오버)
│  ├─ ForestInput           마우스 입력 → 칸 명령, Hover 표시
│  ├─ ExplorationScore      점수 계산
│  ├─ ToolSwitcher          R키로 레몬 ↔ 검 전환
│  ├─ TileActions
│  │  ├─ TileActionResolver 도구 + 칸 종류 → 담당 스크립트 선택
│  │  ├─ LemonPlacer        레몬 + 안전한 칸
│  │  ├─ NestSealer         레몬 + 둥지
│  │  └─ SwordMissHandler   검 + 안전한 칸
│  └─ DuelManager           검 + 둥지 (결투 시작·결과)
├─ Forest
│  └─ ForestBoard           ForestBoard + NestPlacer
│     └─ Tile_x_y …         ForestTile Prefab (Ground / Number 자식)
└─ UI
   ├─ HUD Canvas
   │  ├─ TopBar             남은 둥지 · 점수 · 결과 · Restart
   │  ├─ ToolIndicator      현재 도구 아이콘 + "R: 도구 바꾸기"
   │  └─ DuelPlaceholder    결투 중에만 켜지는 임시 패널
   ├─ Cursor Canvas
   │  └─ ToolCursor         마우스를 따라다니는 도구 커서
   └─ EventSystem
```

## 스크립트

| 폴더 | 스크립트 | 부착 위치 | 역할 |
| --- | --- | --- | --- |
| Board | `ForestBoard` | ForestBoard | 둥지 배치 요청, 안전한 칸 밝히기(연쇄), 경고 표식, 둥지 상태(봉인·결투 중·정화), 승리 판정 |
| Board | `NestPlacer` | ForestBoard | 첫 행동 이후 둥지 배치 (둥지 수·첫 칸 안전·고정 시드) |
| Board | `ForestTile` | ForestTile Prefab | 칸 하나의 외형 (바닥·숫자 레이어) |
| Board | `ForestTileSpriteSet` | Settings 에셋 | 칸 상태별 스프라이트 묶음 |
| Board | `SpriteFrameAnimator` | ForestTile/Ground | 결투 중 둥지 프레임 애니메이션 |
| Exploration | `ExplorationState` | Managers | 탐험 단계 관리 |
| Exploration | `ForestInput` | Managers | 마우스 입력 처리 |
| Exploration | `TileActionResolver` | Managers/TileActions | 도구 + 칸 종류로 결과 결정 |
| Exploration | `LemonPlacer` / `NestSealer` / `SwordMissHandler` | Managers/TileActions | 결과별 처리 |
| Exploration | `ExplorationScore` | Managers | 점수 계산 |
| Tools | `PlayerTool` | — | 도구 종류 (Lemon, Sword) |
| Tools | `ToolSwitcher` | Managers | 도구 전환 입력 (Input Action, 기본 R) |
| Tools | `ToolSettings` | Settings 에셋 | 도구별 커서·크기·Hotspot·레몬 소모량 |
| Duel | `DuelManager` | Managers | 결투 시작·승리·패배 처리 |
| UI | `NestCounterUI` / `ScoreUI` / `ExplorationResultUI` | TopBar | 남은 둥지 수, 점수, 결과 문구 |
| UI | `ToolCursorView` | Cursor Canvas/ToolCursor | 도구 커서 표시, 창 밖·결투 중에는 일반 커서 |
| UI | `ToolIndicatorUI` | HUD Canvas/ToolIndicator | 현재 도구와 전환 키 안내 |
| UI | `DuelPlaceholderUI` | HUD Canvas/DuelPlaceholder | 임시 결투 패널 표시 |

## 에디터에서 수정하기

- **보드 크기**: `ForestBoard`의 Width / Height 변경 후 **"칸 다시 만들기"**
- **보드 모양 직접 편집**: 칸을 복제·삭제·이동한 뒤 **"자식 칸 다시 등록"**
- **둥지 수 · 첫 칸 안전 · 고정 시드**: `ForestBoard`의 `NestPlacer`
- **칸 외형**: `Assets/Dramon/Settings/ForestTileSpriteSet`
- **도구 커서·아이콘·Hotspot·레몬 소모량**: `Assets/Dramon/Settings/ToolSettings`
- **도구 전환 키**: `Managers/ToolSwitcher` → Switch Action의 바인딩 변경 (Use Reference를 켜면 Input Action 에셋의 액션 연결 가능)
- **점수 규칙**: `Managers/ExplorationScore`
- **HUD 위치·크기**: Scene에서 `UI/HUD Canvas` 아래 오브젝트를 직접 이동

Scene에 배치한 오브젝트와 Inspector 설정은 실행 중 스크립트가 덮어쓰지 않습니다.

### 에디터 메뉴 (`Dramon`)

| 메뉴 | 내용 |
| --- | --- |
| `1단계 - Forest Scene 만들기` | Forest Scene이 없을 때 처음 구성 (이미 있으면 아무것도 바꾸지 않음) |
| `기능 설치 - 도구 전환 (현재 Scene)` | 열린 Forest Scene에 도구 전환 기능을 추가 (이미 있는 오브젝트는 그대로 둠) |
| `Minesweeper/Build Minesweeper Scene` | 초기 지뢰찾기 프로토타입 Scene 구성 |

## 개발 단계

| 단계 | 내용 | 상태 |
| --- | --- | --- |
| 1단계 | 순수 지뢰찾기 (보드, 칸 열기, 숫자, 연쇄, 깃발, 승리/패배) | ✅ 완료 |
| 추가 기능 | 도구 전환 (레몬 ↔ 검), 도구 커서, 도구 HUD, 점수 | ✅ 완료 |
| 2단계 | Dramon 테마 (풀 타일 변형, 레몬 연출, 레몬 향 이펙트, 어둠/밝힘 연출) | ⏳ 예정 |
| 3단계 | 결투 화면 전환 (결투 배경, 드래곤, 플레이어, 카드 UI 배치) | ⏳ 예정 |
| 4단계 | 카드 결투 규칙 | ⏳ 예정 |

## 작업 기록

### 2026-11-22
- **Dramon 기획 반영 및 1단계 완료**
  - 기존 지뢰찾기 구현을 검토해 Dramon 구조로 옮겨 확장하기로 결정
  - `Assets/Dramon` 아래에 숲 보드(`ForestBoard`, `ForestTile`, `NestPlacer`, `ForestTileSpriteSet`) 작성
  - `Forest.unity` Scene 구성: 숲 칸 Prefab 81개, Pixel Perfect Camera, HUD(9-slice 창, 버튼 Sprite Swap)
  - 숲 타일·숫자·경고 표식·둥지 에셋 연결, 마우스 올림(Hover) 표시
- **도구 전환 기능 추가 (레몬 ↔ 검)**
  - R키(Input Action)로 도구 전환, 결투 중 전환 차단
  - 도구 모양 커서(UI Image)와 전환 연출, 창 밖·결투 중에는 일반 커서
  - 왼쪽 아래 도구 표시 HUD (아이콘 + "R: 도구 바꾸기")
  - 도구 + 칸 종류별 결과 처리 분리 (레몬 두기 / 둥지 봉인 / 검 헛손질 / 결투 시작)
  - 둥지 상태 추가: 결투 중(Nest_Active 프레임 반복), 봉인·정화(Nest_Purified)
  - 점수 시스템: 레몬 사용·검 헛손질 감점
  - 카드 결투 전까지 쓰는 임시 결투 패널 ([결투 승리] / [결투 패배])
  - 재시작 단축키를 없애고 Restart 버튼으로만 재시작

### 2026-11-21
- GitHub 저장소 연결 및 첫 커밋 (Unity `.gitignore` / `.gitattributes` 추가)
- README 작성

### 2026-11-18
- Unity 6.6 2D URP 프로젝트 생성
- 지뢰찾기 프로토타입 제작
- Scene·Inspector에서 직접 편집할 수 있는 구조로 재작성
  - 보드·칸을 Scene 오브젝트와 Prefab으로 배치, 스프라이트를 PNG 에셋으로 저장
  - 스크립트를 역할별(Board / Game / UI)로 분리
  - 보드 재생성·난이도별 카메라 저장 에디터 도구 추가

## 개발 원칙

- 맵 · 캐릭터 · 카메라 · UI 등은 Scene / Hierarchy에 실제 오브젝트로 배치하고, 반복되는 오브젝트는 Prefab으로 만든다.
- 플레이를 눌러야 나타나는 방식으로 만들지 않는다. 자동화 스크립트를 쓰더라도 결과는 Scene / Prefab으로 저장한다.
- 스크립트는 에셋별·기능별로 나누고, 하나의 스크립트는 하나의 역할만 맡으며 이름만 보고 기능을 알 수 있게 짓는다.
- 조절할 값은 Inspector에 노출하고, 필요한 값에는 한국어 Tooltip을 단다.
- Art 폴더의 임포트 설정과 파일 이름은 바꾸지 않는다.

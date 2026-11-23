# Dramon

Unity로 개발 중인 2D 탑다운 게임 **Dramon** 프로젝트입니다.
지뢰찾기(Minesweeper)의 규칙과 구조를 기반으로, 그 위에 Dramon만의 테마와 결투 시스템을 얹어 만들고 있습니다.

> 플레이어는 어두운 숲을 탐험하며 드래곤의 굴을 찾아냅니다.
> 드래곤은 레몬을 싫어해서, 굴이라고 생각하는 곳에 레몬을 놓아 막아 둡니다.
> 굴을 모두 막고 나면 검을 들고 굴에 잠든 드래곤과 카드 결투를 벌이고,
> 얻은 보상금과 재료로 상점에서 능력을 강화합니다.

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

## 게임 흐름

| 단계 | 내용 | 상태 |
| --- | --- | --- |
| 1. 레몬 표시 | 지뢰찾기처럼 칸을 열고, 굴이라고 생각하는 칸에 레몬을 놓는다. 걸린 시간으로 보상금 지급 | ✅ 구현 |
| 2. 검 사냥 | 레몬으로 막아 둔 굴을 검으로 고르면 드래곤이 나타나 결투가 시작된다 | ✅ 등장 연출까지 구현 (결투는 임시 버튼) |
| 3. 카드 결투 | 카드 게임 형식의 결투. 이기면 드래곤이 재료를 떨어뜨림 | ⏳ 예정 |
| 4. 상점 | 보상금과 재료를 판 돈으로 능력 강화·아이템 구매 | ⏳ 예정 (버튼만 배치) |

## 조작법

### 레몬 표시 단계 (커서: 레몬)

| 입력 | 동작 |
| --- | --- |
| 왼쪽 클릭 | 칸 열기 (숫자 = 주변 8칸의 굴 수, 0이면 연쇄로 열림) |
| 오른쪽 클릭 | 그 칸에 레몬 놓기 (굴 표시) |
| 숫자 칸 왼쪽 클릭 · 휠 클릭 | 주변 레몬 수가 숫자와 같으면 남은 주변 칸을 한꺼번에 열기 |
| Restart 버튼 | 처음부터 다시 시작 |

- 첫 번째로 여는 칸과 그 주변 8칸은 항상 안전합니다. 첫 칸을 열기 전에는 레몬을 놓을 수 없습니다.
- 레몬을 굴이 아닌 칸에 놓거나, 굴을 직접 열면 **즉시 게임 오버**입니다.
- 레몬을 놓은 칸은 다시 열거나 레몬을 뺄 수 없습니다.

### 검 사냥 단계 (커서: 검)

| 입력 | 동작 |
| --- | --- |
| 봉인된 굴 클릭 | 드래곤 등장 → 결투 화면 |
| 상점 버튼 | 이 단계에서만 활성화 (상점 기능은 예정) |

## 게임 규칙

### 레몬 표시 단계

- **완료 조건**: 굴을 모두 레몬으로 표시하고, 안전한 칸도 모두 열어야 단계가 끝납니다.
- **시간**: 첫 칸을 연 순간부터 재고, 단계가 끝나는 순간 멈춥니다.
- **보상금**: 걸린 시간에 따라 지급되어 플레이어 지갑에 들어갑니다.

  | 걸린 시간 | 보상금 |
  | --- | --- |
  | 60초 이하 | 100 |
  | 120초 이하 | 70 |
  | 180초 이하 | 40 |
  | 그 이상 | 20 |

- 완료하면 안내 창(걸린 시간, 보상금)이 뜨고, 확인을 누르면 커서가 검으로 바뀌며 검 사냥 단계가 시작됩니다.
- 레몬으로 막은 굴은 "봉인된 굴"(드러난 둥지 위에 레몬)로 바뀝니다.

### 드래곤 등장 (검 사냥 단계)

봉인된 굴을 검으로 고르면, 이 숲에서 나올 수 있는 드래곤 목록(`ForestDragonSpawnList`)에서 한 마리가 무작위로 나옵니다.
바로 전에 나온 드래곤은 연속으로 나오지 않습니다.

등장 연출 (포켓몬 팬게임의 야생 몬스터 등장 방식을 참고, 각 단계 시간·켜고 끄기는 Inspector에서 조절):

1. 둥지가 깨어나는 둥지 그림으로 바뀌고, 둥지 위에 깨어나는 드래곤(빛나는 수면 그림) + 화면 번쩍임
2. 화면을 검게 덮는 전환
3. 결투 화면: 배경과 드래곤 쪽·플레이어 쪽 발판이 양옆에서 슬라이드 인
4. 드래곤이 어두운 색으로 들어왔다가 원래 색으로 돌아옴 (울음소리 자리 있음)
5. 드래곤 **체력 막대만** 표시 (이름·레벨 등 다른 정보 없음)
6. 메시지: "앗! 둥지에서 ○○ 드래곤이 깨어났다!"
7. 임시 [결투 승리] / [결투 패배] 버튼

- 이기면 숲 화면으로 돌아오고, 그 둥지는 해당 드래곤의 정화된 둥지로 바뀝니다.
- 지면 즉시 게임 오버입니다.

### 드래곤 10종

| 번호 | 이름 | 에셋 폴더 |
| --- | --- | --- |
| 01 | 화염 드래곤 | `Art/Dragon/Types/01_Ember` |
| 02 | 숲 드래곤 | `Art/Dragon/Types/02_Forest` |
| 03 | 폭풍 드래곤 | `Art/Dragon/Types/03_Storm` |
| 04 | 그림자 드래곤 | `Art/Dragon/Types/04_Shadow` |
| 05 | 황금 드래곤 | `Art/Dragon/Types/05_Gold` |
| 06 | 서리 드래곤 | `Art/Dragon/Types/06_Frost` |
| 07 | 독늪 드래곤 | `Art/Dragon/Types/07_Toxic` |
| 08 | 해류 드래곤 | `Art/Dragon/Types/08_Tide` |
| 09 | 바위 드래곤 | `Art/Dragon/Types/09_Stone` |
| 10 | 수정 드래곤 | `Art/Dragon/Types/10_Crystal` |

종류마다 결투 기본 / 공격 / 피격, 필드 수면 / 빛나는 수면 그림이 있습니다.
드래곤별 둥지 그림은 아직 없어서 `NestSprites##` 에셋에 기본 둥지 그림이 임시로 연결되어 있습니다.

## 폴더 구조

```
Assets/
├─ Dramon/
│  ├─ Art/            # 아트 에셋 (임포트 설정·파일 이름은 변경 금지)
│  │  ├─ Board/  Lemon/  Player/  Battle/  UI/
│  │  └─ Dragon/Types/01_Ember … 10_Crystal
│  ├─ Scenes/         # Forest.unity (메인 Scene)
│  ├─ Prefabs/
│  │  ├─ Board/       # ForestTile (Ground / Number / Lemon 레이어)
│  │  └─ UI/          # HudButton
│  ├─ Settings/       # ForestTileSpriteSet, MarkingRewardSettings, PhaseCursorSettings, ToolSettings
│  │  └─ Dragons/     # DragonSpecies 10종, ForestDragonSpawnList, Nests/NestSprites 10종
│  ├─ Scripts/
│  │  ├─ Board/       # 숲 보드 규칙, 칸 표시, 둥지 배치, 프레임 애니메이션
│  │  ├─ Game/        # 게임 단계 관리 (GamePhaseController)
│  │  ├─ Marking/     # 레몬 표시 단계: 입력, 칸 열기, 레몬 놓기, 집계, 타이머, 보상금
│  │  ├─ SwordHunt/   # 검 사냥 단계: 봉인된 굴 감지, 드래곤 등장 시작
│  │  ├─ Encounter/   # 등장 연출: 번쩍임, 화면 전환, 슬라이드 인, 메시지, 깨어나는 드래곤
│  │  ├─ Duel/        # 드래곤 데이터, 등장 목록, 결투 세션, 드래곤 체력·그림
│  │  ├─ Economy/     # 플레이어 지갑
│  │  ├─ UI/          # HUD, 커서, 완료·게임 오버·결투 화면, 체력 표시
│  │  ├─ Exploration/ # (예전 도구 전환 방식, 꺼 둠)
│  │  └─ Tools/       # (예전 도구 전환 방식, 꺼 둠)
│  └─ Editor/         # Scene 구성·기능 설치·보드 재생성 에디터 도구
└─ Minesweeper/       # 초기 지뢰찾기 프로토타입 (참고용, Assets/Scenes/Minesweeper.unity)
```

## Scene 구성 (`Forest.unity`)

```
Forest
├─ Main Camera                Pixel Perfect Camera
├─ Global Light 2D
├─ Managers
│  ├─ GamePhaseController     단계 관리 (레몬 표시 → 완료 안내 → 검 사냥 → 등장 → 결투 / 게임 오버)
│  ├─ PlayerWallet            돈 보관
│  ├─ Marking                 MarkingInput, TileOpener, LemonMarker, NestMarkCounter, MarkingTimer, MarkingRewardCalculator
│  ├─ SwordHunt               SealedNestHoverDetector, DragonEncounterTrigger,
│  │                          DuelSession (드래곤 선택·결투 시작/종료, DragonHealth)
│  └─ (회색, 꺼 둠)           예전 도구 전환 방식 오브젝트들
├─ Forest
│  └─ ForestBoard             ForestBoard + NestPlacer
│     └─ Tile_x_y …           ForestTile Prefab 81개
├─ Effects                    NestWakeSprite (깨어나는 드래곤)
└─ UI
   ├─ HUD Canvas
   │  ├─ TopBar               남은 굴 수(레몬 카운터) · 경과 시간 · 단계 이름 · 상점 · Restart
   │  ├─ MarkingResult        레몬 표시 완료 창 (평소 꺼짐)
   │  ├─ GameOver             게임 오버 창 (평소 꺼짐)
   │  ├─ DuelScreen           결투 화면: 배경, 드래곤 쪽·플레이어 쪽, 체력 창, 메시지 창, 임시 버튼 (평소 꺼짐)
   │  └─ EncounterOverlay     화면 번쩍임 · 화면 전환
   ├─ Cursor Canvas
   │  └─ PhaseCursor          단계별 커서 (레몬 / 검)
   └─ EventSystem
```

## 스크립트

| 폴더 | 스크립트 | 역할 |
| --- | --- | --- |
| Board | `ForestBoard` | 둥지 배치, 칸 열기(연쇄), 레몬 놓기, 둥지 상태(봉인·결투 중·정화), 완료 판정 |
| Board | `NestPlacer` | 첫 칸을 연 뒤 둥지 배치 (둥지 수·첫 칸 안전·고정 시드) |
| Board | `ForestTile` | 칸 하나의 외형 (바닥·숫자·레몬 레이어) |
| Board | `ForestTileSpriteSet` | 칸 상태별 스프라이트 묶음 |
| Board | `SpriteFrameAnimator` | 둥지 프레임 애니메이션 |
| Game | `GamePhaseController` | 현재 단계 관리, 단계가 바뀌면 알림 |
| Marking | `MarkingInput` | 왼쪽 클릭 = 칸 열기, 오른쪽 클릭 = 레몬 놓기, Hover |
| Marking | `TileOpener` / `LemonMarker` | 칸 열기 / 레몬 놓기·판정 |
| Marking | `NestMarkCounter` | 남은 굴 수 집계, 완료 조건 확인 |
| Marking | `MarkingTimer` / `MarkingRewardCalculator` / `MarkingRewardSettings` | 경과 시간 / 시간 → 보상금 / 보상 구간 표 |
| SwordHunt | `SealedNestHoverDetector` / `DragonEncounterTrigger` | 커서 아래 봉인된 굴 감지 / 등장 시작 |
| Encounter | `EncounterIntroSequence` | 등장 연출 순서 진행 |
| Encounter | `ScreenFlashUI` / `ScreenWipeTransition` / `DuelStageSlideIn` / `EncounterMessageUI` / `NestWakeSpriteView` | 연출 단위별 처리 |
| Duel | `DragonSpecies` / `NestSpriteSet` / `DragonSpawnList` | 드래곤 데이터 / 둥지 그림 / 나올 수 있는 드래곤 목록 |
| Duel | `DuelSession` / `DuelManager` | 결투 시작·종료 |
| Duel | `DragonHealth` / `DragonBattlerView` | 드래곤 체력 / 결투 화면 드래곤 그림 |
| Economy | `PlayerWallet` | 돈 보관 (넣기 / 쓰기) |
| UI | `PhaseCursorView` / `PhaseCursorSettings` | 단계별 커서 |
| UI | `NestCounterUI` / `MarkingTimerUI` / `PhaseNameUI` / `ShopButtonUI` | HUD 표시 |
| UI | `MarkingResultUI` / `GameOverUI` / `DuelScreenUI` / `DragonHealthUI` | 창·결투 화면·체력 막대 |

## 에디터에서 수정하기

- **보드 크기**: `ForestBoard`의 Width / Height 변경 후 **"칸 다시 만들기"**
- **둥지 수 · 첫 칸 안전 · 고정 시드**: `ForestBoard`의 `NestPlacer`
- **완료 조건 (안전한 칸도 열어야 하는지)**: `Managers/Marking/NestMarkCounter`
- **보상 구간**: `Settings/MarkingRewardSettings`
- **커서 (스프라이트·크기·클릭 기준점)**: `Settings/PhaseCursorSettings`
- **드래곤 (이름·체력·그림·둥지 그림·울음소리)**: `Settings/Dragons/Dragon##_*`
- **나올 수 있는 드래곤 목록**: `Settings/Dragons/ForestDragonSpawnList`
- **등장 연출 시간·켜고 끄기·등장 메시지 문구**: `EncounterIntroSequence`
- **표시 문구**: 각 UI 컴포넌트 (`GameOverUI`, `PhaseNameUI`, `MarkingResultUI` 등)
- **HUD·창 위치와 크기**: Scene에서 `UI/HUD Canvas` 아래 오브젝트를 직접 이동

Scene에 배치한 오브젝트와 Inspector 설정은 실행 중 스크립트가 덮어쓰지 않습니다.

### 에디터 메뉴 (`Dramon`)

| 메뉴 | 내용 |
| --- | --- |
| `1단계 - Forest Scene 만들기` | Forest Scene이 없을 때 처음 구성 (이미 있으면 아무것도 바꾸지 않음) |
| `기능 설치 - 도구 전환 (현재 Scene)` | (예전 방식) 도구 전환 기능 추가 |
| `기능 설치 - 레몬 표시 단계 (현재 Scene)` | 레몬 표시 단계 추가, 예전 도구 전환 오브젝트는 꺼 둠 |
| `기능 설치 - 드래곤 등장·결투 화면 (현재 Scene)` | 드래곤 데이터·등장 연출·결투 화면 추가 |
| `Minesweeper/Build Minesweeper Scene` | 초기 지뢰찾기 프로토타입 Scene 구성 |

## 작업 기록

### 2026-11-23
- **설계 변경: 레몬 표시 → 검 사냥 → 상점**
  - 예전 R키 도구 전환(레몬 ↔ 검)은 지우지 않고 꺼 둠 (`ToolSwitcher`의 Switching Enabled 끔, 관련 오브젝트 비활성)
- **레몬 표시 단계 구현**
  - 왼쪽 클릭 = 칸 열기, 오른쪽 클릭 = 레몬 놓기. 굴이 아닌 칸에 레몬을 놓거나 굴을 직접 열면 게임 오버
  - 완료 조건: 굴을 모두 레몬으로 표시 + 안전한 칸 모두 열기
  - 경과 시간 측정, 시간 구간별 보상금(60초 100 / 120초 70 / 180초 40 / 그 이상 20) → 플레이어 지갑
  - 완료 창, 게임 오버 창, 봉인된 굴(Nest_Revealed + Lemon_Ground) 표시
  - 단계별 커서 (레몬 → 검), 창이 떠 있을 때는 일반 커서
  - HUD: 레몬 카운터(남은 굴 수), 경과 시간, 단계 이름, 상점 버튼(검 사냥 단계에서만 활성)
- **드래곤 등장 시스템 구현**
  - 포켓몬 팬게임(Pokémon Essentials 기반)의 야생 몬스터 등장 방식을 분석해 연출 순서를 참고
  - 드래곤 10종 그림 추가, `DragonSpecies` 10종·`ForestDragonSpawnList`·`NestSpriteSet` 10종 데이터 작성
  - 검으로 봉인된 굴을 고르면 드래곤 등장: 번쩍임 → 화면 전환 → 슬라이드 인 → 색 복귀 → 체력 막대만 표시 → 메시지
  - 결투 화면(배경·발판·드래곤·플레이어·체력 창·메시지 창)을 Scene에 배치, 결투 결과는 임시 버튼
  - 검 커서 그림(`Cursor_Sword`) 교체

### 2026-11-22
- **Dramon 기획 반영 및 1단계 완료**
  - 기존 지뢰찾기 구현을 검토해 Dramon 구조로 옮겨 확장하기로 결정
  - `Assets/Dramon` 아래에 숲 보드(`ForestBoard`, `ForestTile`, `NestPlacer`, `ForestTileSpriteSet`) 작성
  - `Forest.unity` Scene 구성: 숲 칸 Prefab 81개, Pixel Perfect Camera, HUD(9-slice 창, 버튼 Sprite Swap)
  - 숲 타일·숫자·경고 표식·둥지 에셋 연결, 마우스 올림(Hover) 표시
- **도구 전환 기능 추가 (레몬 ↔ 검)** — 2026-11-23 설계 변경으로 현재는 꺼 둠
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
- 예전 기능은 바로 지우지 않고 Inspector 토글이나 비활성으로 꺼 둔다.
- Art 폴더의 임포트 설정과 파일 이름은 바꾸지 않는다.

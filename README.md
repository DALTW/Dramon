# Dramon

Unity로 개발 중인 2D 게임 **Dramon** 프로젝트입니다.
현재는 지뢰찾기(Minesweeper)를 기반 시스템으로 두고, 여기서부터 게임을 확장해 나갈 예정입니다.

## 개발 환경

| 항목 | 내용 |
| --- | --- |
| 엔진 | Unity 6.6 (`6000.6.9f1`) |
| 렌더 파이프라인 | URP 2D (Renderer2D) |
| 입력 | Input System (New Input System 전용) |
| UI | uGUI (Canvas) |

## 시작하기

1. 저장소를 클론합니다.
   ```bash
   git clone https://github.com/DALTW/Dramon.git
   ```
2. Unity Hub에서 **Add → 클론한 폴더**를 선택해 프로젝트를 추가합니다. (Unity `6000.6.9f1` 필요)
3. `Assets/Scenes/Minesweeper.unity`를 열고 ▶ Play를 누릅니다.

## 조작법

| 입력 | 동작 |
| --- | --- |
| 좌클릭 | 칸 열기 |
| 우클릭 | 깃발 꽂기 / 해제 |
| 숫자 칸 좌클릭 · 휠 클릭 | 주변 깃발 수가 숫자와 같으면 주변 칸 한꺼번에 열기 |
| `R` | 재시작 |
| `1` / `2` / `3` | 초급(9×9, 지뢰 10) / 중급(16×16, 40) / 고급(30×16, 99) |

첫 클릭한 칸과 그 주변 8칸에는 지뢰가 나오지 않습니다.

## 폴더 구조

```
Assets/
├─ Scenes/
│  └─ Minesweeper.unity        # 메인 Scene
└─ Minesweeper/
   ├─ Art/                     # 칸 스프라이트 (PNG, 32px)
   ├─ Prefabs/                 # Cell, HudButton
   ├─ Settings/                # CellSpriteSet (칸 외형 묶음)
   ├─ Scripts/
   │  ├─ Board/                # 보드 규칙, 칸 표시, 지뢰 배치
   │  ├─ Game/                 # 게임 상태, 난이도, 입력, 단축키
   │  └─ UI/                   # 지뢰 수, 타이머, 결과 표시
   └─ Editor/                  # 에디터 전용 도구 (보드 재생성 등)
```

## Scene 구성

```
Minesweeper
├─ Main Camera / Global Light 2D
├─ Managers
│  ├─ GameStateManager      진행 상태(대기·진행·승리·패배)와 타이머
│  ├─ DifficultySelector    난이도 전환, 난이도별 카메라 위치
│  ├─ BoardInput            마우스 클릭 → 칸 명령
│  └─ Hotkeys               R / 1·2·3 단축키
├─ Boards
│  ├─ Board_Beginner        MinesweeperBoard + MinePlacer
│  ├─ Board_Intermediate
│  └─ Board_Expert
└─ UI
   ├─ HUD Canvas / TopBar   지뢰 수 · 시간 · 결과 · 버튼
   └─ EventSystem
```

## 스크립트

| 스크립트 | 위치(부착 대상) | 역할 |
| --- | --- | --- |
| `MinesweeperBoard` | `Board_*` | 열기 · 깃발 · 주변 열기 · 승패 판정 |
| `MinePlacer` | `Board_*` | 첫 클릭 시 지뢰 배치 |
| `CellView` | `Cell` Prefab | 칸 하나의 외형 표시 |
| `CellSpriteSet` | `Settings/CellSpriteSet` 에셋 | 칸 상태별 스프라이트 묶음 |
| `GameStateManager` | `Managers/GameStateManager` | 게임 상태 · 경과 시간 |
| `DifficultySelector` | `Managers/DifficultySelector` | 난이도 보드 전환 · 카메라 이동 |
| `BoardInput` | `Managers/BoardInput` | 마우스 입력 처리 |
| `MinesweeperHotkeys` | `Managers/Hotkeys` | 키보드 단축키 |
| `MineCounterUI` / `TimerUI` / `GameResultUI` | `UI/HUD Canvas/TopBar/*` | 상단 HUD 표시 |

## 에디터에서 수정하기

- **지뢰 수 · 첫 클릭 안전 · 고정 시드**: `Board_*` → `MinePlacer`
- **보드 크기**: `Board_*` → `MinesweeperBoard`의 Width / Height 변경 후 **"칸 다시 만들기"**
- **보드 모양 직접 편집**: 칸을 복제·삭제·이동한 뒤 **"자식 칸 다시 등록"**
- **칸 외형**: `Assets/Minesweeper/Settings/CellSpriteSet`의 스프라이트 교체
- **난이도별 카메라**: `DifficultySelector`에서 **"이 난이도 보기"** → 카메라 조정 → **"현재 카메라 저장"**
- **버튼 동작**: 각 버튼의 `OnClick` 이벤트

Scene에 배치한 오브젝트와 Inspector 설정은 실행 중 스크립트가 덮어쓰지 않습니다.
(난이도를 바꿀 때 카메라를 옮기는 기능만 예외이며, 난이도별 `Move Camera`로 끌 수 있습니다.)

## 개발 원칙

- 맵 · 캐릭터 · 카메라 · UI 등은 Scene / Hierarchy에 실제 오브젝트로 배치하고, 반복되는 오브젝트는 Prefab으로 만든다.
- 플레이를 눌러야 나타나는 방식으로 만들지 않는다. 자동화 스크립트를 쓰더라도 결과는 Scene / Prefab으로 저장한다.
- 스크립트는 하나의 역할만 맡도록 나누고, 이름만 보고 기능을 알 수 있게 짓는다.
- 조절할 값은 Inspector에 노출하고, 필요한 값에는 한국어 Tooltip을 단다.

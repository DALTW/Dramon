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
| 4. 상점 | 검 사냥 중 상점 버튼으로 열기. 재료 판매 → 능력 강화·아이템 구매 | ✅ 구현 (강화·아이템 효과는 카드 결투에서 사용 예정) |

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
| 상점 버튼 | 상점 열기 (이 단계에서만 활성화) |

### 상점 (커서: 기본)

| 입력 | 동작 |
| --- | --- |
| 탭 버튼 | 재료 판매 / 능력 강화 / 아이템 구매 전환 |
| 마우스 휠 · 드래그 | 목록 스크롤 |
| 닫기 버튼 · Esc | 상점 닫기 → 검 사냥으로 (확인 창이 떠 있으면 Esc는 확인 창부터 닫음) |

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
3. 결투 화면: 화면이 검게 덮인 동안 배경이 그 드래곤의 **결투 둥지**로 바뀌고, 드래곤 쪽·플레이어 쪽이 둥지 발판 위치로 양옆에서 슬라이드 인. 이때부터 드래곤은 **날갯짓 비행 모션**(프레임 반복 + 위아래 흔들림)을 계속함
4. 드래곤이 어두운 색으로 들어왔다가 원래 색으로 돌아옴 (울음소리 자리 있음)
5. 드래곤 **체력 막대만** 표시 (이름·레벨 등 다른 정보 없음)
6. 메시지: "앗! 둥지에서 ○○ 드래곤이 깨어났다!"
7. 임시 [결투 승리] / [결투 패배] 버튼

- 이기면 드래곤이 **쓰러지는 모션**(패배 프레임, 마지막 프레임에서 멈춤)을 보인 뒤 재료를 떨어뜨리고 **전리품 창**이 뜹니다. [확인]을 누르면 숲 화면으로 돌아오고, 그 둥지는 해당 드래곤의 정화된 둥지로 바뀝니다.
- 지면 재료 없이 즉시 게임 오버입니다.

### 재료 드랍 (결투 승리)

- 드래곤의 재료 드랍표에서 재료마다 확률을 따로 굴립니다. 나오면 최소~최대 개수 중 하나 (흔함 1~3개, 보통 1~2개, 희귀 1개).
- 아무것도 안 나오면 흔함 재료 1개를 보장합니다 (`LootRoller`에서 켜고 끄기).
- 메시지 창: "○○ 드래곤이(가) ○○을(를) 떨어뜨렸다!"
- 얻은 재료는 인벤토리에 재료별 개수로 쌓이고, HUD에 "재료 n개"로 표시됩니다. 상점에서 팔 수 있습니다.
- 게임 오버만으로는 돈·재료를 그대로 두고, **Restart(다시 시작)** 를 누르면 둘 다 비웁니다.

| 번호 | 흔함 (80%) | 보통 (40%) | 희귀 (10%) |
| --- | --- | --- | --- |
| 01 화염 | 불꽃 비늘 15 | 용암 발톱 40 | 불씨 심장 120 |
| 02 숲 | 이끼 비늘 15 | 가지 뿔 조각 40 | 생명의 씨앗 120 |
| 03 폭풍 | 뇌운 비늘 15 | 폭풍 날개막 40 | 번개 결정 120 |
| 04 그림자 | 흑요 비늘 20 | 핏빛 날개막 50 | 그림자 눈 150 |
| 05 황금 | 황금 비늘 30 | 보석 박힌 발톱 70 | 황금 진주 200 |
| 06 서리 | 서리 비늘 15 | 얼음 송곳니 40 | 영원의 얼음 결정 120 |
| 07 독늪 | 독 비늘 15 | 독샘 주머니 40 | 보랏빛 독침 120 |
| 08 해류 | 물결 비늘 15 | 산호 뿔 조각 40 | 바다 진주 120 |
| 09 바위 | 바위 비늘 15 | 화강 발톱 40 | 대지의 핵 120 |
| 10 수정 | 수정 비늘 20 | 수정 뿔 조각 50 | 마력 수정 핵 150 |

(숫자 = 판매가)

### 상점

- 검 사냥 단계에서만 열립니다. 열려 있는 동안 단계 이름은 "상점"이고, 보드·둥지는 클릭되지 않습니다.
- 돈은 창 오른쪽 위에 `숫자 G`로 표시하고, 바뀔 때마다 숫자가 잠깐 커집니다.
- **재료 판매**: 가진 재료만 보입니다. [1개 팔기] · [모두 팔기] · [흔함 재료 전부 팔기]. 희귀 재료를 [모두 팔기]할 때는 한 번 더 묻습니다.
- **능력 강화** (최대 5단계, 효과는 카드 결투 단계에서 사용 예정)

| 강화 | 단계마다 | 단계별 가격 |
| --- | --- | --- |
| 검 단련 | 검 공격력 +1 | 50 / 80 / 120 / 170 / 230 |
| 방패 보강 | 방패 방어력 +1 | 50 / 80 / 120 / 170 / 230 |
| 체력 증가 | 최대 체력 +10 | 40 / 70 / 110 / 160 / 220 |
| 공격력 증가 | 공격력 +1 | 60 / 90 / 130 / 180 / 240 |

- **아이템 구매** (결투용 소모품, 효과는 카드 결투 단계에서 사용 예정): 회복 약초 30 G · 레몬 폭탄 45 G · 연막 구르기 40 G. 재고 무제한, 아이템마다 최대 5개 보유.
- 살 수 없으면 버튼이 어두워지고 이유(돈이 모자라요 / 최대 단계 / 품절 / 더 가질 수 없어요)가 보입니다.
- **Restart(다시 시작)** 를 누르면 돈·재료·아이템·강화 단계를 모두 비웁니다.

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

종류마다 모션 시트(`Dragon##_*_MotionSheet`: 비행 4 · 공격 4 · 피격 2 · 패배 2 프레임), 필드 수면 / 빛나는 수면 그림과 결투 둥지 배경(`Art/Battle/Nests/BattleBG_Nest##_*`)이 있습니다.
드래곤별 둥지 그림은 아직 없어서 `NestSprites##` 에셋에 기본 둥지 그림이 임시로 연결되어 있습니다.

## 폴더 구조

```
Assets/
├─ Dramon/
│  ├─ Art/            # 아트 에셋 (임포트 설정·파일 이름은 변경 금지)
│  │  ├─ Board/  Lemon/  Player/  Battle/  UI/
│  │  ├─ Battle/Nests/  # 드래곤별 결투 둥지 배경 10장
│  │  ├─ Items/Materials/  # 재료 아이콘 시트 (원본 + 투명 사본, 아이콘 30개로 자름)
│  │  └─ Dragon/Types/01_Ember … 10_Crystal
│  ├─ Scenes/         # Forest.unity (메인 Scene)
│  ├─ Prefabs/
│  │  ├─ Board/       # ForestTile (Ground / Number / Lemon 레이어)
│  │  └─ UI/          # HudButton, LootRow (전리품 창 재료 한 줄)
│  │     └─ Shop/     # ShopSellRow, ShopUpgradeRow, ShopBuyRow (상점 목록 한 줄)
│  ├─ Settings/       # ForestTileSpriteSet, MarkingRewardSettings, PhaseCursorSettings, ToolSettings
│  │  ├─ Dragons/     # DragonSpecies 10종, ForestDragonSpawnList, Nests/NestSprites 10종
│  │  ├─ Materials/   # 재료 MaterialData 30개 (Mat01_Ember_Common …)
│  │  └─ Shop/        # ShopCatalog, Upgrades/ 강화 4종, Items/ 아이템 3종
│  ├─ Scripts/
│  │  ├─ Board/       # 숲 보드 규칙, 칸 표시, 둥지 배치, 프레임 애니메이션
│  │  ├─ Game/        # 게임 단계 관리 (GamePhaseController)
│  │  ├─ Marking/     # 레몬 표시 단계: 입력, 칸 열기, 레몬 놓기, 집계, 타이머, 보상금
│  │  ├─ SwordHunt/   # 검 사냥 단계: 봉인된 굴 감지, 드래곤 등장 시작
│  │  ├─ Encounter/   # 등장 연출: 번쩍임, 화면 전환, 슬라이드 인, 메시지, 깨어나는 드래곤
│  │  ├─ Duel/        # 드래곤 데이터, 등장 목록, 결투 세션, 드래곤 체력·그림, 결투 배경, 재료 드랍
│  │  ├─ Items/       # 재료 데이터 (MaterialData)
│  │  ├─ Economy/     # 플레이어 지갑, 인벤토리, 강화 단계, 아이템 가방
│  │  ├─ Shop/        # 상점 데이터(강화·아이템·목록), 판매·강화·구매 처리
│  │  ├─ UI/          # HUD, 커서, 완료·게임 오버·결투 화면, 체력 표시 (UI/Shop = 상점 창)
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
│  ├─ GamePhaseController     단계 관리 (레몬 표시 → 완료 안내 → 검 사냥 ⇄ 상점, 검 사냥 → 등장 → 결투 / 게임 오버)
│  ├─ PlayerWallet            돈 보관
│  ├─ PlayerInventory         재료 보관 (재료별 개수)
│  ├─ PlayerUpgrades          강화 단계 보관 (능력별 보너스 계산)
│  ├─ PlayerItemBag           산 아이템 보관 (아이템별 개수)
│  ├─ Shop                    MaterialSeller (재료 판매), UpgradePurchaser (강화 구매), ItemPurchaser (아이템 구매·재고)
│  ├─ Marking                 MarkingInput, TileOpener, LemonMarker, NestMarkCounter, MarkingTimer, MarkingRewardCalculator
│  ├─ SwordHunt               SealedNestHoverDetector, DragonEncounterTrigger,
│  │                          DuelSession (드래곤 선택·결투 시작/종료, DragonHealth), LootRoller (재료 드랍)
│  └─ (회색, 꺼 둠)           예전 도구 전환 방식 오브젝트들
├─ Forest
│  └─ ForestBoard             ForestBoard + NestPlacer
│     └─ Tile_x_y …           ForestTile Prefab 81개
├─ Effects                    NestWakeSprite (깨어나는 드래곤)
└─ UI
   ├─ HUD Canvas
   │  ├─ TopBar               남은 굴 수(레몬 카운터) · 경과 시간 · 단계 이름 · 재료 개수(MaterialPanel) · 상점 · Restart
   │  ├─ MarkingResult        레몬 표시 완료 창 (평소 꺼짐)
   │  ├─ GameOver             게임 오버 창 (평소 꺼짐)
   │  ├─ DuelScreen           결투 화면: 배경, 드래곤 쪽(Dragon/Motion/Sprite = 비행 모션)·플레이어 쪽, 체력 창, 메시지 창, 임시 버튼, 전리품 창(LootWindow) (평소 꺼짐)
   │  ├─ ShopWindow           상점 창 (ShopWindowUI · ShopConfirmDialog)
   │  │  └─ Panel (평소 꺼짐) Dim · Window(Title · Money · CloseButton · Tabs · Pages/SellPage·UpgradePage·BuyPage · Toast) · ConfirmDialog
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
| Duel | `DragonHealth` / `DragonBattlerView` | 드래곤 체력 / 결투 화면 드래곤 그림 (비행·공격·피격·패배 모션, 크기 배율, 테스트 단축키) |
| Duel | `DragonFrameAnimator` / `DragonHoverMotion` | 프레임 재생 (반복 / 한 번 / 마지막 프레임에서 멈춤, Pivot 맞추기) / 비행 중 위아래 흔들림 |
| Duel | `DuelBackgroundView` | 결투 배경을 드래곤별 결투 둥지로 교체 / 기본 배경 복귀 |
| Duel | `LootRoller` | 드랍표로 얻을 재료 굴리기 (흔함 재료 보장) |
| Items | `MaterialData` | 재료 하나의 이름·아이콘·희귀도·판매가 |
| Economy | `PlayerWallet` | 돈 보관 (넣기 / 쓰기) |
| Economy | `PlayerInventory` | 재료별 개수 보관 (넣기 / 빼기) |
| Economy | `PlayerUpgrades` / `PlayerItemBag` | 강화 단계 보관·능력별 보너스 / 산 아이템 개수 보관 |
| Shop | `ShopUpgradeData` / `ShopItemData` / `ShopCatalog` / `UpgradeStat` | 강화 한 종류 / 아이템 한 종류 / 상점에 보일 목록·재료 순서 / 능력 종류 |
| Shop | `MaterialSeller` / `UpgradePurchaser` / `ItemPurchaser` | 재료 판매 / 강화 구매 (가격·최대 단계 확인) / 아이템 구매 (재고·최대 보유 확인) |
| UI | `PhaseCursorView` / `PhaseCursorSettings` | 단계별 커서 |
| UI | `NestCounterUI` / `MarkingTimerUI` / `PhaseNameUI` / `ShopButtonUI` | HUD 표시 |
| UI | `MarkingResultUI` / `GameOverUI` / `DuelScreenUI` / `DragonHealthUI` | 창·결투 화면·체력 막대 |
| UI | `LootResultUI` / `LootRowView` / `MaterialCountUI` | 전리품 창 / 재료 한 줄 / HUD 재료 개수 |
| UI/Shop | `ShopWindowUI` | 상점 열기·닫기(Esc), 탭 전환, Play 중 테스트 버튼 |
| UI/Shop | `ShopSellListUI` / `ShopUpgradeListUI` / `ShopBuyListUI` | 탭별 목록 만들기·버튼 처리 |
| UI/Shop | `ShopSellRow` / `ShopUpgradeRow` / `ShopBuyRow` | 목록 한 줄 표시 |
| UI/Shop | `ShopMoneyUI` / `ShopToastUI` / `ShopConfirmDialog` | 돈 표시(G) / 거래 안내 문구·효과음 / 확인 창 |

## 에디터에서 수정하기

- **보드 크기**: `ForestBoard`의 Width / Height 변경 후 **"칸 다시 만들기"**
- **둥지 수 · 첫 칸 안전 · 고정 시드**: `ForestBoard`의 `NestPlacer`
- **완료 조건 (안전한 칸도 열어야 하는지)**: `Managers/Marking/NestMarkCounter`
- **보상 구간**: `Settings/MarkingRewardSettings`
- **커서 (스프라이트·크기·클릭 기준점)**: `Settings/PhaseCursorSettings`
- **드래곤 (이름·체력·그림·둥지 그림·울음소리)**: `Settings/Dragons/Dragon##_*`
- **드래곤 모션 (프레임·fps·흔들림)**: `Dragon##_*`의 [모션 프레임]·[비행 흔들림] / **드래곤별 위치·크기**: `Dragon##_*`의 [결투 둥지] 드래곤 발판 위치·결투 크기 배율 / **크기 배율·테스트 단축키**: `DuelScreen/Panel/DragonSide/Dragon`의 `DragonBattlerView`
- **결투 둥지 배경·발판 위치**: `Dragon##_*`의 [결투 둥지] (발판 위치는 배경 왼쪽 아래 = 0,0 비율)
- **재료 드랍 확률·개수**: `Dragon##_*`의 [재료 드랍표] / **재료 이름·아이콘·판매가**: `Settings/Materials/Mat##_*`
- **흔함 재료 보장**: `Managers/SwordHunt/LootRoller` / **전리품 문구**: `DuelScreen`의 `LootResultUI`
- **다시 시작할 때 비우기**: `PlayerWallet` / `PlayerInventory` / `PlayerUpgrades` / `PlayerItemBag`
- **강화 (이름·아이콘·설명·능력·단계마다 오르는 양·최대 단계·단계별 가격)**: `Settings/Shop/Upgrades/Upgrade_*`
- **아이템 (이름·아이콘·설명·가격·재고·최대 보유)**: `Settings/Shop/Items/Item_*`
- **상점에 보일 강화·아이템과 순서, 판매 목록 재료 순서**: `Settings/Shop/ShopCatalog`
- **상점 문구·희귀도 색·확인 창 켜고 끄기**: `ShopWindow/Panel/Window/Pages/*Page`의 `Shop*ListUI` / **효과음·안내 시간**: `.../Window/Toast`의 `ShopToastUI` / **동전 아이콘**: `.../Window/Money`의 `ShopMoneyUI`
- **상점 테스트 (돈·재료 넣기, 강화 초기화)**: Play 중 `HUD Canvas/ShopWindow`의 `ShopWindowUI` Inspector 아래 버튼
- **테스트 (고정 드래곤, 드랍 100%)**: `Managers/SwordHunt/DuelSession`의 [테스트] (에디터에서만)
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
| `기능 설치 - 드래곤 비행 모션 (현재 Scene)` | 모션 시트 Import·4×3 자르기, 드래곤 10종 모션 프레임·기본값 연결, 모션 전용 자식(Dragon/Motion/Sprite) 추가 |
| `기능 설치 - 결투 둥지 배경·재료 드랍 (현재 Scene)` | 둥지 배경·재료 아이콘 Import·자르기, 재료 데이터, 드랍표, 전리품 창·재료 개수 HUD 추가 |
| `기능 설치 - 상점 (현재 Scene)` | 강화 4종·아이템 3종·상점 목록 데이터, 목록 줄 Prefab 3개, 강화·아이템 보관과 판매·구매 처리, 상점 창 추가, 상점 버튼 연결 |
| `Minesweeper/Build Minesweeper Scene` | 초기 지뢰찾기 프로토타입 Scene 구성 |

## 작업 기록

### 2026-11-26
- **상점 (재료 판매 · 능력 강화 · 아이템 구매)**
  - 검 사냥 중 HUD 상점 버튼으로 열고 [닫기]·Esc로 닫음. 열려 있는 동안 새 단계 "상점" (보드·둥지 클릭 막힘)
  - 재료 판매: 1개 / 모두 / 흔함 재료 전부, 희귀 재료 모두 팔기는 확인 창
  - 능력 강화 4종 (검 단련 · 방패 보강 · 체력 증가 · 공격력 증가), 최대 5단계, 단계별 가격
  - 아이템 3종 (회복 약초 · 레몬 폭탄 · 연막 구르기), 재고·최대 보유 개수 설정 가능
  - 돈은 `숫자 G`로 표시, 거래마다 안내 문구 (효과음 칸은 비어 있음)
  - Restart 시 돈·재료·아이템·강화를 모두 비움
  - 강화·아이템 효과는 보관만 하고, 카드 결투 단계에서 `PlayerUpgrades.GetBonus` / `PlayerItemBag`으로 가져다 쓸 예정
- **드래곤 비행 모션 (프레임 애니메이션)**
  - 드래곤별 모션 시트 10장을 `Art/Dragon/Types/##_*/Dragon##_*_MotionSheet.png`로 복사·가공
    - 칸 테두리에서 시작하는 flood fill로 배경(검정·흰색·짙은 회색)만 투명하게, 몸과 날개 사이에 갇힌 배경도 제거
    - 오른쪽을 보는 칸만 좌우 반전 (원본에 이미 왼쪽을 보는 칸이 섞여 있어 칸마다 판단)
    - 4열 × 3행 자르기 (Fly_0~3 / Attack_0~3 / Hurt_0~1·Defeat_0~1), Pivot = 발밑 가운데, 비행 프레임 17칸은 몸 위치가 맞도록 Pivot 미세 조정
    - 확인용 `Art/Dragon/Types/_MotionPreview.png` (10종 Fly_0~3)
  - `DragonSpecies`에 모션 프레임·fps·비행 흔들림 추가, 드래곤별 성격에 맞는 기본값
  - 결투 화면 슬라이드 인부터 비행 모션 반복 + 위아래 흔들림 (정수 픽셀), 공격·피격은 한 번 재생 후 비행으로
  - 결투 승리 → 패배 모션(쓰러짐에서 멈춤) → 전리품 창 순서로 연결
  - 테스트: `DragonBattlerView` 오른쪽 클릭 메뉴(비행·공격·피격·패배), Play 중 1 공격 / 2 피격 / 3 패배
- **드래곤 위치를 둥지 안으로**
  - 드래곤 발판 위치를 둥지 안쪽 바닥(앞쪽)으로 다시 재고, 드래곤마다 결투 크기 배율(0.74~0.94)을 정해 날 때 화면 위로 잘리지 않게 함
  - 패배 프레임(Defeat_0·1)의 Pivot을 쓰러진 몸의 가운데·맨 아래로 맞춰, 쓰러지는 모습이 10종 모두 둥지 안에서 끝나게 함

### 2026-11-25
- **드래곤별 결투 둥지 배경**
  - 둥지 배경 10장을 `Art/Battle/Nests`로 복사 (Sprite, Point, 압축 없음, Max 2048)
  - 그림마다 드래곤 둥지·플레이어 발판의 윗면 가운데를 재서 `DragonSpecies`에 배경과 발판 위치 입력
  - 등장 연출 중 화면이 검게 덮인 동안 배경을 바꾸고, 드래곤·플레이어를 그 발판 위치로 슬라이드 인
  - 숲으로 돌아오거나 게임 오버·다시 시작이면 기본 배경과 Scene 배치 위치로 복귀
- **승리 시 재료 드랍**
  - 재료 아이콘 시트의 검은 배경을 투명하게 만든 사본을 만들고 아이콘 30개로 자름
  - 재료 데이터 `MaterialData` 30개, 드래곤별 드랍표(흔함 80% / 보통 40% / 희귀 10%) 작성
  - 같은 재료가 여러 개 나올 수 있게 함 (흔함 1~3, 보통 1~2, 희귀 1)
  - 전리품 창 + 드랍 메시지, [확인]을 누르면 숲으로. 인벤토리에 쌓고 HUD에 "재료 n개" 표시
  - Restart를 누를 때만 돈·재료를 비우고, 게임 오버만으로는 유지

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

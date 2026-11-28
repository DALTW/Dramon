# Dramon

Unity로 개발 중인 2D 탑다운 게임 **Dramon** 프로젝트입니다.
지뢰찾기(Minesweeper)의 규칙과 구조를 기반으로, 그 위에 Dramon만의 테마와 결투 시스템을 얹어 만들고 있습니다.

> 플레이어는 어두운 숲을 탐험하며 드래곤의 굴을 찾아냅니다.
> 드래곤은 레몬을 싫어해서, 굴이라고 생각하는 곳에 레몬을 놓아 막아 둡니다.
> 굴을 모두 막고 나면 검을 들고 굴에 잠든 드래곤과 카드 결투를 벌이고,
> 얻은 보상금과 재료로 상점에서 능력을 강화합니다.

## 키워드 게임 제작

Dramon은 게임 개발 동아리 **Yggdrasil**의 **랜덤 키워드 게임** 과제로 시작한 게임입니다.
게임잼 주제 생성기에서 단어 세 개(명사 2개 + 형용사 1개)를 무작위로 뽑고, 그 키워드로 게임을 기획·제작합니다.

### 뽑은 키워드: 용 · 검 · 신

| 키워드 | 품사 | 게임에 담은 방식 |
| --- | --- | --- |
| **용** | 명사 | 드래곤. 지뢰찾기의 지뢰가 숲에 숨은 **드래곤의 굴(둥지)**이 됩니다. 결투 상대인 드래곤 10종과, 일반 모드 마지막에 랜덤으로 정해지는 **보스 드래곤**까지 이어집니다. |
| **검** | 명사 | 굴을 모두 막은 뒤 커서가 레몬에서 **검**으로 바뀌고, 검으로 막아 둔 굴을 골라 드래곤을 사냥합니다. 결투는 검을 든 주인공의 **카드 결투**로 진행됩니다. |
| **신** | 형용사 (맛이 시다) | 신맛의 대표 **레몬**. "드래곤은 레몬을 싫어한다"는 설정을 만들어, 지뢰찾기의 깃발 대신 레몬을 놓아 굴을 막습니다. 카드 결투에서도 에너지가 **레몬 에너지**이고, 레몬 폭탄·상큼한 기운으로 드래곤에게 **산미** 상태(공격마다 추가 피해)를 겁니다. |

### 키워드에서 게임으로

1. **지뢰찾기를 뼈대로**: 숨은 위험(지뢰)을 숫자로 추리해 찾는 구조가 "숲에 숨은 드래곤의 굴"과 잘 맞아서 지뢰찾기 규칙을 그대로 가져왔습니다.
2. **용 + 신 → 레몬으로 굴 막기**: 신 레몬을 싫어하는 드래곤이라는 설정으로, 깃발 꽂기를 "레몬으로 굴 막기"로 바꿨습니다. 표시를 틀리면 즉시 게임 오버, 빨리 끝낼수록 보상금이 늘어납니다.
3. **검 → 사냥과 결투**: 지뢰찾기에는 없는 다음 단계로, 레몬으로 막은 굴을 검으로 골라 드래곤과 카드 결투를 하게 했습니다.
4. **한 판으로 끝나지 않게**: 결투 보상(드래곤 재료)을 상점에 팔아 카드를 사고 강화하며, 스테이지마다 숲과 둥지가 늘어나는 덱빌딩 구조로 확장했습니다. 일반 모드(보스전·엔딩)와, 일반 모드를 클리어하면 열리는 무한 모드가 있습니다.

| 지뢰찾기 | Dramon |
| --- | --- |
| 지뢰 | 드래곤의 굴 (용) |
| 깃발 | 레몬으로 굴 막기 (신) |
| 숫자 | 주변 8칸의 굴 수 |
| 지뢰를 밟음 | 굴을 열거나 레몬을 잘못 놓으면 게임 오버 |
| 없음 | 검 사냥 · 카드 결투 (검) · 상점 · 스테이지 · 보스 |

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
| 3. 카드 결투 | 손패·레몬 에너지·턴 종료로 드래곤과 싸움. 이기면 재료 드랍, 지면 게임 오버 | 🔨 1단계 구현 (기본 규칙·기본 덱·할퀴기·연타·방어) |
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
│  │  ├─ Items/Shop/   # 상점 아이콘 (강화 4 · 아이템 3 · 동전, 원본 시트)
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
   │  ├─ DuelScreen           결투 화면: 배경, 드래곤 쪽(Dragon/Motion/Sprite = 비행 모션)·플레이어 쪽(PlayerSide/Hero/Motion/Sprite = 검사, 예전 Player는 꺼 둠), 플레이어 체력 창 자리(꺼 둠), 체력 창, 메시지 창, 임시 버튼, 전리품 창(LootWindow) (평소 꺼짐)
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
| Hero | `HeroBattlerView` | 결투 화면 검사: 대기·걷기·카드 동작·피격·속성 반응·승리·쓰러짐 (DragonFrameAnimator·DragonHoverMotion 재사용) |
| Hero | `HeroSpriteSet` / `HeroReactionType` | 검사 동작별 프레임·fps / 속성 반응 종류 |
| Duel | `DuelLayoutSettings` | 결투 화면 구도 값 (검사 위치·크기, 플레이어 체력 창) |
| Battle | `CardBattleController` | 카드 결투 진행 (내 턴 ↔ 드래곤 턴, 승패 → DuelSession) |
| Battle | `PlayerTurn` / `DragonTurn` | 내 턴 (뽑기·카드 사용·턴 끝) / 드래곤 턴 (방어 초기화·행동) |
| Battle | `CardEffectResolver` / `DragonIntentPicker` / `DragonActionExecutor` | 카드 효과 처리 / 다음 행동 고르기 / 드래곤 행동 실행 |
| Battle | `PlayerDeck` / `DrawPile` / `DiscardPile` / `ExhaustPile` / `Hand` | 영구 덱 / 뽑을·버린·소멸 더미 / 손패 |
| Battle | `PlayerBattleStats` / `DragonBattleStats` | 플레이어 체력·방어·에너지 / 드래곤 방어 (체력은 DragonHealth) |
| Battle | `BattleRules` / `CardData` / `StarterDeck` / `DragonActionData` | 규칙 / 카드 / 기본 덱 / 드래곤 행동 데이터 |
| UI/Battle | `HandView` / `CardView` | 손패 부채꼴·마우스·드래그 / 카드 한 장 |
| UI/Battle | `EnergyUI` / `PileCountUI` / `EndTurnButtonUI` / `PlayerHealthUI` / `DragonHealthTextUI` / `DragonBlockUI` / `BattleLogMessage` | 에너지 / 더미 장수 / 턴 종료 / 플레이어 체력 창 / 드래곤 체력 숫자 / 드래곤 방어 / 진행 문구 |
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
- **검사 동작 프레임·fps**: `Settings/Hero/HeroSpriteSet` / **숨쉬기·쓰러짐 시간·테스트 단축키**: `DuelScreen/Panel/PlayerSide/Hero`의 `HeroBattlerView`
- **검사 크기·위치 구도**: `Settings/DuelLayoutSettings` → [포켓몬 구도 적용] / **드래곤별 검사 반응**: `Dragon##_*`의 [플레이어 피격 반응]
- **드래곤 공격 뒤 검사 반응까지 시간**: `Managers/SwordHunt/DuelSession`
- **카드 결투 규칙 (체력·에너지·뽑기·손패 최대)**: `Settings/Battle/BattleRules` / **카드**: `Settings/Cards/Card_*` (이름·종류·코스트·그림·설명·효과·소멸·검사 동작·상점 가격) / **기본 덱**: `Settings/Cards/StarterDeck`
- **드래곤 행동 순서**: `Dragon##_*`의 [카드 결투 행동] / **행동 수치**: `Settings/Dragons/Actions/Act##_*`
- **손패 배치·마우스 올림·사용 방식**: `DuelScreen/Panel/Battle/Hand`의 `HandView` / **카드 모양**: `Prefabs/UI/Battle/CardView`
- **진행 시간·문구**: `Managers/Battle/CardBattleController`·`DragonTurn`·`Dragon/DragonActionExecutor`·`PlayerTurn` / **결투 테스트 버튼**: Play 중 `CardBattleController` Inspector 아래
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
| `기능 설치 - 결투 검사·포켓몬 구도 (현재 Scene)` | 검사 시트 Import·자르기, HeroSpriteSet·DuelLayoutSettings, 드래곤 10종 피격 반응, PlayerSide/Hero·플레이어 체력 창 추가 |
| `기능 설치 - 카드 결투 (현재 Scene)` | 규칙·카드 13종·기본 덱·드래곤 행동 데이터, CardView Prefab, Managers/Battle, 결투 화면 손패·에너지·더미·턴 종료·체력 창 |
| `기능 설치 - v3 1단계 (결투 상태 효과·스테이지 데이터, 현재 Scene)` | 스테이지·보스·모드 데이터, 새 카드 2종·카드 강화 수치·보스 힌트, 상태 효과·의도 아이콘·떠오르는 숫자·모션 연결 (카드 결투가 없으면 먼저 설치) |
| `기능 설치 - v3 2단계 (스테이지 진행, 현재 Scene)` | 스테이지 진행·깨어난 둥지 고르기, 카메라 칸 크기 맞춤·이동, 스테이지 번호·클리어 창, 레몬 표시 보상 시간·결투 배율 연결 (v3 1단계가 없으면 먼저 설치) |
| `기능 설치 - v3 3단계 (상점 개편, 현재 Scene)` | 상점 탭을 재료 판매·카드 구매·카드 강화·덱(제거)·체력 증가로, 카드 구매·강화·제거 처리, 카드 한 줄 Prefab, 스테이지 클리어 창 [상점] (아이템 구매 탭은 숨김) |
| `기능 설치 - v3 4단계 (모드·보스, 현재 Scene)` | 기록 저장·보스전, 붉은 기운·보스 이름 띠(임시), 보스 힌트·모드 선택 화면(배경·제목 그림 칸)·엔딩·게임 오버 기록 창 |
| `기능 설치 - v3 정식 그림 (현재 Scene)` | v3 정식 그림 Import 설정·이펙트 시트 자르기, 레몬 폭탄·연막 구르기 카드 그림, 잠든 둥지, 클리어 띠·붉은 기운·이름 띠·모드 선택·엔딩 그림, 연타·필살 숨결 예고, 반사·무효 아이콘 연결, 분노·숨결 이펙트 오브젝트 추가 (임시 그림이거나 비어 있을 때만 바꿈) |
| `포켓몬 구도 적용 (현재 Scene)` | DuelLayoutSettings 값을 PlayerSide·플레이어 체력 창에 써 넣기 (누를 때만) |
| `기능 설치 - 상점 (현재 Scene)` | 강화 4종·아이템 3종·상점 목록 데이터, 목록 줄 Prefab 3개, 강화·아이템 보관과 판매·구매 처리, 상점 창 추가, 상점 버튼 연결 |
| `Minesweeper/Build Minesweeper Scene` | 초기 지뢰찾기 프로토타입 Scene 구성 |

## 작업 기록

### 2026-11-29
- **README: 키워드 게임 제작 설명 추가** — 동아리 랜덤 키워드 과제에서 뽑은 키워드(용·검·신)와 각 키워드를 게임에 담은 방식, 지뢰찾기와의 대응표

### 2026-11-28
- **v3 1단계: 카드 결투 2단계 + 스테이지·보스·모드 데이터**
  - 정한 것: 결투 2단계를 v3 1단계에 합침 / 드래곤 배율은 v3 규칙(스테이지 배율 × 같은 스테이지 결투마다 +5%), 예전 +6% 규칙은 `BattleRules`에서 꺼 둠(코드는 남김) / 카드 강화는 추천값 / 결투 중 아이템 칸은 만들지 않음
  - 데이터: `Settings/Stage/StageRules`(둥지·숲 크기·깨어난 둥지·에너지·뽑기·보상 시간·체력/공격 배율·클리어 골드, 1~3 스테이지 숲 크기는 5×5·6×6·7×7로 덮어씀), `Settings/Stage/BossRules`, `Settings/Modes/Mode_Normal`·`Mode_Endless`
  - 새 카드: 레몬 폭탄(특수 1, 피해 8 + 산미 2, 소멸, 70G) / 연막 구르기(방어 1, 다음 드래곤 공격 1번 무효, 소멸, 80G). 그림은 임시로 레몬 던지기·구르기 그림을 쓰고 카드의 `Temp Asset Note`에 적어 둠
  - 카드 15종 강화 수치(카드 종류 단위 "+", 강화 후 효과·코스트·가격) — 상점 강화 탭은 3단계에서
  - 상태 효과: 산미(공격마다 +산미 피해, 드래곤 턴 끝 1 감소), 화상(내 턴 끝 피해, 방어로 막음)·중독(방어 무시), 감전(에너지 −1)·빙결(뽑기 −1)·젖음(방어 −50%)은 한 턴, 공포(공격 −25%)·출혈(카드 낼 때마다 1 피해)은 내 턴 끝 1 감소
  - 피해 순서: 카드 피해 + 강화 → 공포 −25%(내림) → + 산미 → 드래곤 방어
  - 드래곤 숨결 상태·회복·강화(다음 공격 +), 반격 자세 반사, 연막 구르기 무효, 검사 카드 모션·피격·속성 반응, 드래곤 공격·피격 모션
  - 화면: 드래곤 다음 행동 아이콘 + 수치(드래곤 체력 숫자 아래), 상태 아이콘 줄(플레이어 체력 창 위, 드래곤 체력 창 아래 오른쪽), 떠오르는 숫자(피해 빨강·방어 파랑·회복 초록·상태 보라), 카드 설명 숫자는 최종값(오르면 초록·내리면 빨강), 강화 카드 이름 뒤 "+"
  - 체력 증가 강화가 결투 시작 체력에 더해짐. 에너지·뽑기 장수는 `StageRules`에서 (1 스테이지 = 3 / 5장)
  - 테스트: `CardBattleController` Inspector에 상태 효과 주기 버튼(화상·중독·감전·빙결·젖음·공포·출혈·드래곤 산미), `DragonScaling`의 Test Force Stage(배율 강제)
- **v3 2단계: 스테이지 진행**
  - 판 시작 = 1 스테이지. 스테이지마다 `StageRules` 값으로 숲 크기(5×5 → … 최대 24×16)·둥지 수(4 + 2씩)를 맞춤
  - 숲 칸: Scene에 놓인 칸이 그 크기와 같으면 그대로 쓰고, 다르면 칸 Prefab으로 실행 중에 `ForestBoard/StageTiles`를 만들고 Scene 칸은 잠시 숨김 (Scene 칸은 지우거나 옮기지 않음). `StageProgress`의 Apply Stage Board를 끄면 예전처럼 Scene 칸 그대로
  - 레몬 표시가 끝나면 깨어난 둥지(2 + (스테이지−1)÷2, 최대 6)를 무작위로 고르고, 나머지 둥지는 정화된 둥지 모습으로 끝 (검을 대도 반응 없음)
  - 깨어난 둥지의 드래곤을 모두 쓰러뜨리면 스테이지 클리어 창(새 단계 `StageClear`): 클리어 보상 30 × 스테이지 골드, 다음 스테이지 안내, [다음 스테이지]
  - 다음 스테이지로 가도 돈·재료·덱·강화는 그대로, Restart만 1 스테이지로
  - 최대 에너지·뽑기는 스테이지 값 (3/5 → 4/5 → 5/6 → 6/6 … 최대 10/8), 드래곤은 스테이지 배율 × 같은 스테이지 결투마다 +5%
  - 레몬 표시 보상 시간 = 칸 수 × 0.8 / 1.6 / 2.4초 (보상 금액은 `MarkingRewardSettings` 그대로)
  - 숲이 커지면 칸을 32px에서 최소 20px까지 줄이고(픽셀 퍼펙트 카메라 Assets PPU), 그래도 넘치면 방향키·WASD·마우스 휠로 카메라 이동
  - 화면: 위 막대 아래 왼쪽에 스테이지 번호(결투 중엔 숨김), 스테이지 클리어 창(그림 칸 Banner는 비어 있음 — 임시)
  - 테스트: `StageProgress`의 Test Start Stage, Inspector [스테이지 클리어]·[이 스테이지부터 다시 시작], `AwakenedNestPicker`의 Test Awakened Count
- **v3 3단계: 상점 개편**
  - 탭: 재료 판매 / 카드 구매 / 카드 강화 / 덱 / 체력 증가. 예전 아이템 구매 탭(BuyTab·BuyPage)은 숨김만 (켜면 다시 보임)
  - `ShopCatalog`에 카드 목록 추가(상점 카드 11종, 가격순). 강화 목록에서 검 단련·방패 보강·공격력 증가를 빼고 아이템 목록은 비움 (에셋·스크립트는 그대로)
  - 카드 구매: 카드의 Shop Price, 카드마다 최대 보유 수(Max Owned)까지. 산 카드는 바로 덱에
  - 카드 강화: 덱에 있는 카드 종류마다 "지금 → 강화 후" 설명·코스트, 강화 가격. 한 번 강화하면 덱의 그 카드 모두 "+"
  - 덱: 카드 종류·장수 보기, [제거]는 확인 창을 거쳐 1장 빼기. 가격 50골드부터 뺄 때마다 +25, 덱이 5장 이하면 못 뺌 (Restart하면 처음 가격·처음 덱)
  - 체력 증가: 단계당 +10, 최대 단계는 모드별 (일반 5, 무한 50 — `GameModeData`의 Max Health Upgrade Level)
  - 스테이지 클리어 창에 [상점] 버튼, HUD 상점 버튼도 클리어 창에서 열림. 상점을 닫으면 클리어 창으로 돌아옴
  - 스크립트: `CardPurchaser`·`CardUpgrader`·`CardRemover`(Managers/Shop), `ShopCardBuyListUI`·`ShopCardUpgradeListUI`·`ShopDeckListUI`(상점 페이지), `ShopCardRow`(Prefabs/UI/Shop/ShopCardRow)
- **v3 4단계: 모드와 보스**
  - 모드 선택 화면(`HUD Canvas/ModeSelect`, Play를 누르면 열림): [일반 모드] [무한 모드]. 무한 모드는 일반 모드를 클리어해야 열림 (잠긴 동안 흐린 버튼 + "일반 모드를 클리어하면 열려요"). 아래에 기록 표시
  - 기록 저장(`Managers/Stage/ProgressSave`, PlayerPrefs): 일반 모드 클리어 여부·최고 클리어 시간·마지막 보스, 무한 모드 최고 스테이지
  - 일반 모드 보스(`Managers/Stage/BossFight`): 판을 시작할 때 10종 중 1마리를 무작위로 보스로 정하고 이번 판 일반 결투에서 뺌 (DragonSpawnList 에셋은 그대로)
  - 3 스테이지부터 스테이지 시작 때 보스 힌트 (`DragonSpecies`의 Boss Hint, 위 막대 아래 가운데 4초)
  - 5 스테이지의 깨어난 둥지를 모두 정화하면 숲 가운데에서 보스전: 5 스테이지 값 × 체력 1.8·공격 1.2, 1.25배 크기, 붉은 기운(임시), 이름 띠 "숲의 주인 – ○○ 드래곤"(임시), 에너지 6
  - 체력 절반 아래에서 분노: 문구 + "분노!" + 기운이 진해짐, 행동 순서에 필살 숨결(숨결 피해 × 1.5, 상태 2배)을 더하고 바로 예고
  - 보스를 이기면 엔딩 화면(`HUD Canvas/Ending`): 쓰러뜨린 보스, 클리어 시간·최고 기록, 무한 모드 열림 안내, [모드 선택] [다시 하기]
  - 게임 오버 때 기록 창(`HUD Canvas/RunRecord`): 무한 모드면 도달·최고 스테이지, 일반 모드면 쓰러진 스테이지, [모드 선택]
  - 무한 모드: 보스 없음, 10종 모두 무작위, 스테이지 끝없음, 체력 증가 50단계까지
  - 테스트: `StageProgress` Inspector [보스전 바로 시작]·[골드 +500]·[무한 모드 잠금 해제]·[기록 지우기], `BossFight`의 Test Boss(보스 고정), `ModeSelectUI`의 Show On Start
- **v3 5단계: 마무리 (전체 흐름 테스트·에셋 목록)**
  - 시험용 임시 스크립트로 전체 흐름을 2번 끝까지 돌림 (실제 버튼에 클릭 이벤트를 보내서 누름, 오류 로그 0): 모드 선택(무한 모드 잠김) → 일반 1~5 스테이지 (레몬 표시 → 검 사냥 → 결투 → 클리어 → 상점 5개 탭에서 사고·강화·빼기) → 보스전·분노 → 엔딩 → 무한 모드 열림 → 무한 1~3 스테이지 → 4 스테이지 게임 오버 기록 → 일반 모드 게임 오버 기록. 덱 빼기는 따로 확인 (확인 창 → 50·75·100·125·150 G, 5장이 되면 버튼이 흐려지고 "덱이 너무 작아요"). 임시 스크립트는 지웠고, 테스트 기록(PlayerPrefs)도 지움
  - 고친 것: 보스(1.25배)의 날개·머리가 화면 위로 잘림 (10종 중 7종) → `DragonBattlerView`에 Lower When Enlarged(기본 1) 추가. 실행 중 크게 키운 만큼 드래곤(Dragon 오브젝트)을 아래로 내려서 위쪽 끝이 보통 크기 때와 같은 높이. 보통 결투(배율 1)는 그대로, 보스전이 끝나면 원래 자리로. Scene 배치는 건드리지 않음
  - 추가: 모드 선택 화면 그림 칸 `HUD Canvas/ModeSelect/Panel/TEMP_Background`(배경, 화면 전체)·`Panel/Window/TEMP_TitleLogo`(제목 그림 240×40). `ModeSelectUI`의 Background Picture·Title Picture·Title Label에 연결, 그림이 비어 있으면 숨기고 제목 그림을 넣으면 "Dramon" 글자를 숨김. `기능 설치 - v3 4단계`가 없을 때만 만듦
  - 테스트 중 본 것 (값·설정이라 고치지 않음): 2 스테이지부터 드래곤 체력이 크게 뜀 (`StageRules` 체력 배율 1 → 3, 46 → 145~164). 강화 없이 카드만 단순하게 쓰면 2~3 스테이지 결투마다 체력 35~40을 잃고 4 스테이지에서 쓰러짐 / 같은 드래곤이 연속으로 나올 수 있음 (`ForestDragonSpawnList`의 Avoid Same Twice In A Row 꺼짐) / 반격 자세(반사)·연막 구르기(무효)는 화면에 표시가 없음 / 보스를 이기면 전리품 창 → 엔딩 순서
  - 정식 그림·소리가 필요한 자리는 5단계 보고의 에셋 목록 표에 정리
- **v3 정식 그림 넣기 (Meshy 18장)**
  - 받은 그림(`Downloads/Meshy_images_20261128_222438`)을 마젠타 배경 빼기·자르기·크기 맞추기만 해서 새 파일로 넣음 (기존 그림은 바꾸지 않음). Import: Sprite, Point, 압축 없음, PPU 32
    - `Art/Battle/Cards/Art/CardArt_LemonBomb`(카드 그림 시트의 레몬 폭탄 칸)·`CardArt_SmokeRoll`, `Art/Battle/Cards/Composed/Card_LemonBomb`(비용 보석 안은 다른 카드처럼 레몬색으로 채움)
    - `Art/Battle/Cards/Icons/Icon_Intent_MultiHit`·`Icon_Intent_Finisher`·`Icon_Status_Reflect`·`Icon_Status_Negate`, `Art/Board/Nest_Dormant`(32×32, 아래 "DRAGON NEST" 글자는 잘라냄)
    - `Art/UI/UI_StageClear_Banner`·`UI_BossNameBand`(양끝 모양은 두고 가운데만 늘려 460×44)·`UI_Title_Logo`·`UI_Icon_Lock`·`UI_ModeSelect_BG`(640×360)·`UI_Ending_Picture`(520×160로 위아래 자름)
    - `Art/Battle/FX/FX_BossAura`, `FX_BossEnrage_Sheet`(4칸 128×128), `FX_Breath_Sheet`(5칸 160×120, 흰색 — 실행 중 속성 색을 곱함)
  - 정한 것: 연막 구르기 완성 카드(Meshy 그림 속 인물이 검은 닌자)는 넣지 않고 Composed Image를 비워 둠 → 기본 방어 틀 + 검사가 나오는 카드 그림으로 보임. 카드 그림 시트의 나머지 14칸(기존 카드의 새 버전)은 쓰지 않음
  - 새 칸: `ForestTileSpriteSet` Nest Dormant(잠든 둥지, 비우면 정화된 둥지), `DragonIntentUI` Finisher Icon·Boss Fight(필살 숨결 예고), `StatusIconsUI` Guard Source·Reflect Icon·Negate Icon(플레이어 줄에 반격 자세 반사량·연막 구르기 남은 횟수), `BossFight` Enrage Effect, `DragonActionExecutor` Breath Effect
  - 새 스크립트: `UIFramePlayer`(UI Image 프레임 재생, 한 번 보이고 숨김), `BreathEffectView`(숨결 때 드래곤 피격 반응별 색으로 숨결 재생, 색 목록은 Inspector)
  - 새 오브젝트: `DuelScreen/Panel/DragonSide/Dragon/EnrageFX`(분노 때 한 번), `DuelScreen/Panel/BreathFX`(드래곤과 검사 사이, 15° 기울임 — 위치·크기·각도는 Scene에서 바꿔도 됨)
  - 바꾼 값: 붉은 기운 `BossAuraView` Color 빨강 → 흰색(그림 색 그대로), 밝기 0.15~0.45 → 0.5~0.85 (분노 0.35~0.8 → 0.8~1), 이름 띠 `TEMP_Band`는 Simple·흰색. 임시 에셋 메모(Temp Asset Note)는 비움
  - Play로 확인: 모드 선택(배경·로고·잠금 → 클리어 뒤 잠금 사라짐), 잠든 둥지, 폭풍 드래곤 연타 예고, 새 카드 2장(손패·상점), 반사·무효 아이콘, 숨결 4색, 클리어 띠, 보스 기운·이름 띠·분노 이펙트·필살 숨결 예고, 엔딩 그림. 오류 로그 0. 테스트 기록은 지움
- **규칙 변경: 레몬을 놓은 둥지 모두와 결투**
  - 깨어난 둥지만 고르던 방식(나머지는 잠든 둥지로 끝)이 이미 클리어한 둥지처럼 보여 헷갈려서, 모든 둥지가 깨어나 결투하도록 바꿈: `Managers/Stage/AwakenedNestPicker`의 Use Awakened Count 끔 (켜면 예전 방식, 코드·`StageRules`의 깨어난 둥지 값은 그대로)
  - 결투 수 = 둥지 수 (1~5 스테이지 4·6·8·10·12, 일반 모드 전체 40번 — 예전 14번). 드래곤 배율은 같은 스테이지 결투마다 +5%라 뒤쪽 결투일수록 더 세짐
  - 스테이지 클리어 창 "다음: … 결투 N"이 Use Awakened Count에 맞게 나오도록 `StageClearUI`에 Nest Picker 칸 추가·연결 (`기능 설치 - v3 2단계`도 비어 있으면 연결)

### 2026-11-27
- **카드 결투 1단계** (임시 승패 버튼 → 실제 카드 결투)
  - 정한 것: Restart하면 산 카드도 비움 / 카드는 클릭·위로 끌기 둘 다 / 결투마다 체력 가득 / 카드 빼기 기능은 이번엔 없음
  - 규칙(`Settings/Battle/BattleRules`): 체력 50, 턴마다 에너지 3·5장 뽑기, 손패 최대 10. 방어는 내 턴 시작에 0, 드래곤 방어는 드래곤 턴 시작에 0
  - 기본 덱 10장(검 베기 4·방패 막기 4·레몬 던지기 1·구르기 1), 카드 데이터 13종(`Settings/Cards`), 드래곤 10종 행동(`Settings/Dragons/Actions`)과 체력(40~52)
  - 드래곤은 행동 목록을 순서대로 반복. 1단계에서는 할퀴기·연타·방어와 숨결 피해만 (숨결 상태·회복·강화·산미는 2단계)
  - 화면: 손패(부채꼴, 마우스 올리면 커짐, 클릭·위로 끌어서 사용, 에너지가 모자라면 흐리게), 에너지, 뽑을·버린 더미, 턴 종료, 플레이어 체력 창(막대·숫자·방어), 드래곤 체력 숫자·방어
  - 메시지 창은 손패 자리를 비우려고 위 가운데로 옮김 (Scene에서 다시 옮겨도 유지)
  - 임시 승패 버튼은 `DuelScreenUI`의 [테스트] 켜기에서만 보임 (에디터 전용)
- **상점 아이콘 교체**
  - 상점 아이콘 시트(분홍 배경)에서 배경을 투명하게 하고 글자를 뺀 아이콘 8개를 `Art/Items/Shop/Shop_*.png`로 잘라 넣음 (원본은 `ShopIcons_Sheet.png`)
  - 능력 강화 4종·아이템 3종 아이콘을 임시 카드 그림에서 새 아이콘으로 교체, 상점 돈 표시에 동전 아이콘 추가
  - `기능 설치 - 상점`은 아이콘이 비어 있거나 처음 넣은 임시 카드 그림일 때만 바꿈 (직접 고른 그림은 그대로)
- **플레이어 위치를 발판 가운데로**
  - 드래곤 10종의 플레이어 발판 위치를 발판 윗면 가운데로 다시 맞춘 뒤, 검사로 바꾸면서 0.03씩 위로 올림
- **결투 화면 플레이어를 검사(레몬 검사)로 교체**
  - 시트 A v2(마젠타 배경)를 투명 사본 `Hero_LemonSwordsman_Back_SheetA_v2.png`로 가공 (분홍 가장자리는 안쪽 색으로 채움), 예전 시트 A는 그대로 두고 쓰지 않음
  - 시트 A v2·B: Sprite Multiple, Point, 압축 없음, Max 2048, PPU 32, 4×4(256×256)로 다시 자르기, Pivot = 칸마다 발밑 가운데로 미세 조정
  - `HeroSpriteSet`(동작별 프레임·fps), `HeroReactionType`(속성 반응), `DuelLayoutSettings`(검사 위치·크기, 플레이어 체력 창 자리), `HeroBattlerView`(검사 프레임 재생)
  - 등장 슬라이드 인 중 걷기 → 도착하면 대기(3fps + 숨쉬기 1px), 승리 = 승리 자세(전리품 창까지), 패배 = 피격 → 쓰러짐을 잠깐 보여 준 뒤 게임 오버
  - 카드 결투에서 쓸 함수: PlaySlash / PlayBlock / PlayRoll / PlayLemonThrow / PlayLemonSqueeze / PlayHerb / PlayDrawCard / PlayHit / PlayDragonBreathReaction, 드래곤 공격 + 검사 반응 = `DuelSession.PlayDragonAttack()`
  - `DragonSpecies`에 플레이어 피격 반응 (화염·황금 화상, 숲·독늪 중독, 폭풍 감전, 그림자 공포, 서리 빙결, 해류 젖음, 바위 돌 맞음, 수정 결정 박힘)
  - 크기·위치는 요청에 따라 예전 뒷모습 플레이어와 같게 (키 약 64px, 드래곤별 작은 발판). `DuelLayoutSettings`의 Stand On Nest Platform을 끄고 [포켓몬 구도 적용]을 누르면 화면 앞쪽 큰 구도로 바꿀 수 있음
  - 플레이어 체력 창 자리(빈 틀, 꺼 둠): 오른쪽 아래, 메시지 창 바로 위
  - 테스트: `HeroBattlerView` 오른쪽 클릭 메뉴(모든 동작), Play 중 1 베기 / 2 방패 / 3 레몬 던지기 / 4 속성 반응 (겹치지 않게 드래곤 테스트 단축키는 꺼 둠)

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

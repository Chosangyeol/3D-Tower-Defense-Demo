# 프로젝트 작업 규칙

## 프로젝트 개요
- 탑뷰 배치 + 1인칭 전투 하이브리드 타워 디펜스. 현재 목표는 **프로토타입**.
- 사용자와는 한국어로 소통한다.
- 기획 상세는 `docs/GDD.md`, 진행 상황은 `docs/devlog/` 참고.

## 확정된 결정
| 항목 | 결정 |
|------|------|
| 엔진 | Unity 6.4 (6000.4.4f1), URP, Input System |
| 플랫폼 | PC 전용 (키보드·마우스 기준) |
| 인원 | 싱글 전용 (멀티플레이 제외) |
| 적 경로 | 고정 경로 (메이즈 방식 아님). 경로 칸에는 타워 설치 불가 |
| 적의 기지 공격 | 모든 적은 기지 앞에 겹쳐 서서 기지가 무너질 때까지 공격. 웨이브는 모두 처치해야 끝남 |
| 보스 | 10웨이브마다. 웨이브 데이터로 지정, 능력은 `EnemyAbility`로 확장. 소환 군주(10), 파쇄 거인(20), 강화판 최종 보스(30) |
| 한 판의 구성 | 스테이지 없이 30웨이브. 최종 보스 처치로 클리어, 이후 플레이어가 제약을 골라 처음부터 재도전 |
| 웨이브 | 구성은 직접 작성, 강도는 웨이브 배율 × 제약 |
| 증강 | 후보 3개 중 1개, 증강별 최대 중첩. 고르는 동안 게임 정지 |
| 증강 등급 | 일반·상위. 나올 때마다 획득 방식의 확률로 무작위, 후보는 모두 같은 등급. 웨이브·제단 80:20, 보스 30:70 |
| 다시 뽑기 | 한 판 2번. 같은 등급에서 지금 후보를 제외하며, 새 후보 3개를 못 채우면 사용 불가 |
| 증강 획득 | 방식을 데이터로 넣고 빼며 시험. 지금은 정해진 웨이브(5·15·25)와 보스 처치, 제단 구매는 구현 후 꺼 둠 |
| 미정 | 영구 성장, 증강의 최종 획득 방식 |
| 플레이어 무기 | 마법. 프로토타입은 1개: 단일 대상 투사체, 마나 소모·회복 |
| 웨이브 흐름 | 웨이브마다 배치 ↔ 전투 반복. 준비 시간 뒤 자동 시작 + 바로 시작 가능. 첫 웨이브 준비 시간은 넉넉하게. 시점은 수동 전환, 전투 중 설치·제거 가능 |
| 게임 루프 | A안: 마을 = 전장. 한 맵에서 건설·채집·생산 → 배치 → 1인칭 전투 반복 |
| 자원 시스템 | 채집(1인칭) → 생산 건물로 자동화 → 공용 창고. 운송 시스템 없음 |
| 자원 종류 | 목재, 광석, 마력 결정(희귀: 채집으로만 얻고 연구에만 사용) |
| 자원 획득 | 시작 자원, 처치 보상, 웨이브 완료 보상, 1인칭 채집(E 길게, 노드는 고갈 후 재생) |
| 비용·환급 | 타워 환급은 준비 시간 전액, 웨이브 중 50%. 연구는 환급 없음 |
| 타워 | 3종: 기본 포탑(추적 탄), 대포(쏜 위치에 낙하, 범위 피해), 감속 탑(피해 없는 감속 파동, 공격 간격 > 감속 시간) |
| 타워 증강·연구의 대상 | 모든 타워에 적용되는 것과 특정 타워 전용으로 나눔. 대상을 비우면 모든 타워 |
| 연구 | 타워·건물·플레이어 3탭 트리, 선행을 마치면 다음 단계 해금. 택 1은 그 판 동안 고정. 항목·수치·특수 능력은 추후 확정하므로 데이터로 언제든 수정할 수 있어야 함 |
| 설치 조작 | 하단 목록·1~9로 설치 모드, 설치하면 해제(Shift 유지), 우클릭·ESC·같은 버튼으로 취소. 설치물은 클릭해 선택한 뒤 제거 버튼. 타워 외 건물에도 재사용 |
| 연구소 (예정) | 연구는 연구소 건물을 지어야 가능하게 바꿀 예정. 아직 구현하지 않음, 세부는 미정 |
| 타워 해금 | 처음에는 기본 포탑만. 건물 탭에서 해금하면 설치와 전용 연구가 함께 열림. 맵에 짓는 건물은 만들지 않음 |
| 프로토타입 범위 | 공통 코어(카메라 전환, 그리드 배치, 적 웨이브, 타워 공격, 1인칭 전투, 승패)는 완료. 이후 자원·비용·채집·연구를 추가 (생산 건물 자동화, 마을 건설은 아직 범위 밖) |

아직 미정인 항목은 `docs/GDD.md` 8장 "결정 대기" 참고. 미정 항목을 임의로 확정하지 말고 사용자에게 묻는다.

## 기획서 동기화
- 기획서 원본은 온라인 문서(Claude Docs): https://claude.ai/artifact/Sy9tgfdnFdhfk3gkPFGgF8
- `docs/GDD.md`는 그 사본이다. 직접 수정하지 않는다.
- 온라인 기획서를 수정하면 같은 작업 안에서 `docs/GDD.md`도 갱신하고 커밋·push한다.
  - 상단의 "마지막 동기화" 날짜를 갱신한다.
  - 문서 안의 다이어그램은 Mermaid로 옮긴다.
  - 사용자가 온라인 문서를 직접 수정했다고 하면, 문서를 읽어 `docs/GDD.md`에 반영한다.
- Claude Docs 커넥터가 없는 세션에서는 온라인 문서를 수정할 수 없다. 그 경우 사용자에게 알린다.

## Unity
- 에셋 직렬화는 Force Text 유지. `.meta` 파일은 반드시 함께 커밋.
- 카메라 전환(탑뷰 ↔ 1인칭)은 Cinemachine으로 구현할 계획.
- Unity MCP: Coplay MCP for Unity 사용 (`https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`).
  - Unity 에디터가 켜져 있고 MCP for Unity 창에서 서버가 연결된 상태여야 동작한다.
  - `.mcp.json` 등 PC별 경로가 들어간 설정은 커밋하지 않는다.

## 프로젝트 구조
- `Assets/` 최상위 폴더는 번호를 붙인다. 새 폴더는 다음 번호를 쓴다.

| 폴더 | 내용 |
|------|------|
| `00. Scene` | 씬 |
| `01. Scripts` | 코드. 기능별 하위 폴더 |
| `02. Datas` | ScriptableObject 데이터. 종류별 하위 폴더 |
| `03. Prefabs` | 프리팹. 종류별 하위 폴더 |
| `04. Materials` | 머티리얼. 용도별 하위 폴더 |
| `05. Input` | 입력 설정 |
| `06. Settings` | 렌더 파이프라인 설정 |

- 하위 분류가 더 필요하면 추가한다.
- 코드의 네임스페이스는 `TowerDefense.<기능 폴더>` (예외: `Input` 폴더는 `TowerDefense.Controls`, `Editor` 폴더는 `TowerDefense.EditorTools`).
- 씬과 프리팹은 에디터에서 직접 관리한다. 씬 생성 메뉴는 폐기했으며 다시 만들지 않는다.

## 코드 규칙
- 게임 데이터 ScriptableObject는 `GameData`를 상속하고 `[GameDataCategory("이름", "폴더", 순서)]`를 붙인다.
  - 그러면 데이터 관리 창(`Tools > Tower Defense > Data Manager`)에 자동으로 나타나고, 새 에셋은 `02. Datas/<폴더>`에 만들어진다.
  - 에셋을 찾고 만들고 지우는 기능은 `GameDataRepository`에 둔다. 창(`DataManagerWindow`)에는 화면만 둔다.
  - CSV 연동은 직렬화 필드를 자동으로 읽으므로 데이터 클래스마다 코드를 추가하지 않는다. 원본은 ScriptableObject이며 CSV로는 편집·추가만 한다.
  - 필드 이름을 바꾸면 CSV 열 이름도 바뀐다. 이름을 바꿀 때는 `FormerlySerializedAs`를 붙이고, CSV는 다시 내보낸다.
- 입력은 `GameInput` 한 곳에서만 받는다. 컴포넌트에서 키·마우스를 직접 읽거나 `InputAction`을 새로 만들지 않는다.
  - 키는 `Assets/05. Input/GameControls.inputactions`에서 정의한다 (`Global`, `Build`, `Combat` 맵).
  - 설정 화면에서 키를 바꿀 수 있도록 바인딩 저장·불러오기를 `GameInput`이 맡는다.
- 그리드에 설치하는 것은 `StructureDefinition` / `Structure`를 상속하고 `BuildMenu`의 목록에 넣는다. 설치·선택·제거는 `PlacementController`와 `StructureBuilder`가 공통으로 처리하므로 종류마다 따로 만들지 않는다.
  - 설치 막대와 맵의 클릭은 `PlacementController` 한곳에서 처리한다.
- 체력과 피해는 `Health` / `IDamageable`을 사용한다. 소속(`Team`)이 다른 대상에게만 피해를 준다.
- 선택 화면처럼 게임을 멈출 때는 `GamePause`를 사용한다. 시간, 입력(포인터·클릭 제외), 커서를 함께 처리한다.
  - 시간이 멈춰도 `Update`는 돌기 때문에, 시간과 관계없이 상태가 바뀌는 코드는 `Time.deltaTime`이 0일 때를 확인한다.

## 데이터 구조
- 조정하는 수치는 컴포넌트에 넣지 않고 `02. Datas`의 ScriptableObject에 둔다.

| 데이터 | 클래스 | 읽는 곳 |
|--------|--------|---------|
| 타워 | `TowerDefinition` | `Tower`, 배치 |
| 타워 공격 방식 | `TowerAttack` | `Tower`. 새 방식은 `TowerAttack`을 상속하고, 조정하는 수치는 타워 데이터에 둔다 |
| 적 (보스 포함) | `EnemyDefinition` | `Enemy`: 이동, 기지 공격, 보스 표시 |
| 적 능력 | `EnemyAbility` | `Enemy`. 새 능력은 `EnemyAbility`와 `EnemyAbilityRuntime`을 상속하고, 실행 상태는 Runtime에 둔다 |
| 마법 | `SpellDefinition` | `PlayerSpellCaster` |
| 플레이어 능력치 | `PlayerStatsDefinition` | 이동, 마나 |
| 웨이브 | `WaveDefinition` | 진행 데이터 |
| 진행 | `CampaignDefinition` | `GameSetup`을 통해 웨이브 순서, 준비 시간, 웨이브 배율, 제약 목록 |
| 맵 | `MapDefinition` | `GameSetup`을 통해 경로, 기지, 시작 자원, 연구 목록 |
| 제약 | `ConstraintDefinition` | `RunConstraints`, 제약 화면 |
| 연구 | `UpgradeDefinition` | `ResearchState`, 연구 화면. 맵 데이터의 연구 목록에 넣어야 나온다 |
| 연구 갈래 | `ResearchBranchDefinition` | 연구 화면의 탭과 세로 줄, 효과를 적용할 타워 |
| 효과 | `GameEffect` | 연구·제약·증강이 함께 사용. `RunModifiers`에 적용 |
| 증강 | `AugmentDefinition` | `AugmentSystem`. 후보 목록은 진행 데이터에 있다 |
| 증강 획득 방식 | `AugmentSource` | `AugmentSystem`. 진행 데이터의 목록에 넣으면 켜진다. 새 방식은 `AugmentSource`와 `AugmentSourceRuntime`을 상속한다 |
| 자원 | `ResourceDefinition` | `ResourceWallet` |
| 자원 노드 | `ResourceNodeDefinition` | `ResourceNode`, `PlayerGatherer` |

- 실행 중에는 데이터 에셋의 값을 바꾸지 않는다.
  - 판 안의 모든 수치 변화(연구, 제약, 증강)는 `RunModifiers`에 더하고, 값을 쓰는 쪽은 `RunModifiers.Get`으로 읽는다.
  - 새로 조정 가능한 수치는 `Stat`에 **새 번호**로 추가한다. (기존 번호는 데이터에 저장되어 있어 바꾸지 않는다)
- 연구와 증강의 타워 효과는 대상 타워 종류 전체에 적용하고, 대상을 비우면 모든 타워에 적용한다. 연구의 대상은 갈래의 타워다. 특수 기능은 `GameEffect`를 상속해서 만든다.
- 연구의 구성(항목, 순서, 택 1, 해금할 타워)은 데이터만으로 바꾼다. 코드에 특정 연구를 적지 않는다.
  - 선행 연구는 모두 필요하다. 같은 갈래에서 `choiceGroup`이 같은 연구는 하나만 완료할 수 있다.
  - `unlockTowers`에 적힌 타워는 처음에 잠겨 있다.
  - 설명을 비우면 효과에서 자동으로 만든다. 새 효과 종류는 `Describe`를 구현한다.
  - 새 `Stat`은 `StatEffect`의 분류(타워 수치, 비율, 시간)에도 반영한다.
  - 효과 에셋은 연구마다 따로 둔다. 여러 곳이 함께 쓰면 한쪽을 고칠 때 다른 쪽도 바뀐다.
  - 다중 공격, 감속, 관통, 폭발, 기지 회복처럼 이미 있는 특수 기능은 `Stat`의 수치라서 `StatEffect` 데이터만으로 연구·제약·증강에 붙인다.
  - 판을 시작할 때만 읽는 수치(`StartResources`, `BaseHealth`)는 증강과 연구로 바꿔도 적용되지 않는다.
- 판과 판 사이에 남는 기록은 `RunRecord`를 통해서만 읽고 쓴다.
- 개인 설정(시점 감도 등)은 게임 데이터에 넣지 않는다.
- 비용과 보상은 `ResourceAmount` 목록으로 적고, 자원은 `ResourceWallet`을 통해서만 늘리고 줄인다.
- `Resources`는 Unity의 특수 폴더 이름이라 폴더 이름으로 쓰지 않는다. (자원 코드는 `Economy`, 데이터는 `ResourceTypes`)

## Git
- 커밋 메시지는 한국어로 작성한다.
- 작업 전 `git pull`, 작업 후 commit·push. 여러 PC에서 작업한다.

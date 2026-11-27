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
| 플레이어 무기 | 마법. 프로토타입은 1개: 단일 대상 투사체, 마나 소모·회복 |
| 웨이브 흐름 | 웨이브마다 배치 ↔ 전투 반복. 준비 시간 뒤 자동 시작 + 바로 시작 가능. 첫 웨이브 준비 시간은 넉넉하게. 시점은 수동 전환, 전투 중 설치·제거 가능 |
| 게임 루프 | A안: 마을 = 전장. 한 맵에서 건설·채집·생산 → 배치 → 1인칭 전투 반복 |
| 자원 시스템 | 채집(1인칭) → 생산 건물로 자동화 → 공용 창고. 운송 시스템 없음 |
| 자원 종류 | 목재, 광석, 마력 결정(희귀: 채집으로만 얻고 연구에만 사용) |
| 자원 획득 | 시작 자원, 처치 보상, 웨이브 완료 보상, 1인칭 채집(E 길게, 노드는 고갈 후 재생) |
| 비용·환급 | 타워 환급은 준비 시간 전액, 웨이브 중 50%. 연구는 타워 종류 전체 적용, 환급 없음 |
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
- 체력과 피해는 `Health` / `IDamageable`을 사용한다. 소속(`Team`)이 다른 대상에게만 피해를 준다.

## 데이터 구조
- 조정하는 수치는 컴포넌트에 넣지 않고 `02. Datas`의 ScriptableObject에 둔다.

| 데이터 | 클래스 | 읽는 곳 |
|--------|--------|---------|
| 타워 | `TowerDefinition` | `Tower`, 배치 |
| 적 | `EnemyDefinition` | `Enemy`, 기지 피해 |
| 마법 | `SpellDefinition` | `PlayerSpellCaster` |
| 플레이어 능력치 | `PlayerStatsDefinition` | 이동, 마나 |
| 웨이브 | `WaveDefinition` | 스테이지 |
| 스테이지 | `StageDefinition` | `StageController`를 통해 웨이브, 경로, 기지 |
| 업그레이드 / 효과 | `UpgradeDefinition` / `UpgradeEffect` | `ResearchState`, 연구 화면 (목록은 스테이지 데이터) |
| 자원 | `ResourceDefinition` | `ResourceWallet` |
| 자원 노드 | `ResourceNodeDefinition` | `ResourceNode`, `PlayerGatherer` |

- 실행 중에는 데이터 에셋의 값을 바꾸지 않는다. 강화·연구 결과는 `AttackSpec`처럼 따로 계산해서 보관한다.
- 연구는 타워 종류 전체에 적용한다. 특수 기능은 `UpgradeEffect`를 상속하고 `AttackSpec`에 항목을 추가한다.
- 개인 설정(시점 감도 등)은 게임 데이터에 넣지 않는다.
- 비용과 보상은 `ResourceAmount` 목록으로 적고, 자원은 `ResourceWallet`을 통해서만 늘리고 줄인다.
- `Resources`는 Unity의 특수 폴더 이름이라 폴더 이름으로 쓰지 않는다. (자원 코드는 `Economy`, 데이터는 `ResourceTypes`)

## Git
- 커밋 메시지는 한국어로 작성한다.
- 작업 전 `git pull`, 작업 후 commit·push. 여러 PC에서 작업한다.

# RAW 개발 영역 및 코드 소유권

## 1. 목적

이 문서는 캐릭터·스킬 개발과 멀티플레이·DB 개발의 책임을 분리해 다음 문제를 방지하는 것을 목적으로 한다.

- 같은 코드와 Unity 에셋을 동시에 수정하면서 발생하는 Git 충돌
- 로컬용 코드와 멀티플레이용 코드의 중복 구현
- 스킬 규칙이 네트워크 코드에 복사되는 문제
- 네트워크 코드가 캐릭터·스킬 구현에 직접 섞이는 문제
- 담당자 승인 없이 공용 계약이나 다른 담당 영역이 변경되는 문제

이 문서에서 말하는 소유권은 해당 영역의 최종 설계·수정·리뷰 책임을 의미한다. 다른 담당자의 영역을 변경해야 할 때는 작업 전에 담당자의 동의를 받고 Pull Request에서 승인을 받아야 한다.

## 2. 담당자

| 담당자 | 담당 영역 |
| --- | --- |
| 이진수 | 캐릭터, 스킬, 애니메이션, 이펙트, 게임플레이 데이터 |
| 이현규 | 멀티플레이, 서버 권한, 네트워크 동기화, DB 및 영속 데이터 |

## 3. 캐릭터·스킬 담당 영역

### 소유 경로

```text
RAW_unity/Assets/_RAW/Scripts/Character/
RAW_unity/Assets/_RAW/Scripts/Skill/
RAW_unity/Assets/_RAW/Art/Character/
RAW_unity/Assets/_RAW/Data/Skills/
RAW_unity/Assets/_RAW/Prefabs/Character/
RAW_unity/Assets/_RAW/Prefabs/Indicators/
```

### 담당 책임

- 캐릭터 입력, 이동, 방향 전환 및 행동 제어
- 스킬 타겟 선택과 사거리 접근
- 스킬 시전, 취소, 선딜 및 후딜 규칙
- `SkillSpec` 코드와 Inspector 항목
- 스킬 데미지 계산식과 타격 규칙
- 단일 타격, 다단 타격, 투사체, 범위 판정 등 게임플레이 동작
- Animation Clip, Animator Controller, State 및 Transition
- 캐릭터와 스킬 이펙트 프리팹
- `SkillCatalog`와 스킬 ScriptableObject
- 공용 캐릭터 프리팹인 `Dummy.prefab`
- 캐릭터·스킬 전용 테스트 Scene

### 직접 추가하지 않는 항목

- `NetworkBehaviour`
- RPC
- `NetworkVariable`
- `NetworkObjectReference`
- Client ID 또는 Owner ID를 사용한 분기
- DB 연결 및 저장 형식
- `NetworkPlayer.prefab`과 `NetworkRuntime.prefab` 변경

스킬 개발자는 스킬이 누구에게, 언제, 몇 번, 얼마의 영향을 주는지를 결정한다. 해당 결과를 서버에서 실행하고 동기화하는 방식은 멀티·DB 담당자가 연결한다.

## 4. 멀티·DB 담당 영역

### 소유 경로

```text
RAW_unity/Assets/_RAW/Scripts/Network/
RAW_unity/Assets/_RAW/Prefabs/Network/
```

향후 DB 코드가 분리되면 다음 경로도 멀티·DB 담당자가 소유한다.

```text
RAW_unity/Assets/_RAW/Scripts/Persistence/
```

### 담당 책임

- 네트워크 연결, 승인, 세션 및 Spawn
- 서버 권한 스킬 요청과 검증
- HP, MP, 쿨다운 및 상태 동기화
- `NetworkCharacterState`와 `NetworkEnemyState`
- `NetworkSkillController`와 네트워크 요청 데이터
- NetworkAnimator와 NetworkTransform 설정
- 장비와 인벤토리의 네트워크 동기화
- DB 저장, 불러오기 및 플레이어 영속 데이터
- `NetworkPlayer.prefab`, `NetworkEnemy.prefab`, `NetworkRuntime.prefab`
- Host/Client 네트워크 테스트 Scene

### 직접 변경하지 않는 항목

- 스킬 데미지 공식
- 스킬 타격 횟수와 판정 시점
- 캐릭터 입력과 이동 방식
- Animation Clip과 Animator State
- 스킬 이펙트의 시각적 동작
- `SkillSpec`의 게임플레이 수치
- `Dummy.prefab`의 캐릭터·스킬 구성

멀티·DB 담당자는 스킬 규칙을 네트워크 코드에 복사해 별도로 구현하지 않는다. 캐릭터·스킬 담당자가 정의한 결과가 서버에서만 적용되고 모든 Client에 동기화되도록 연결한다.

## 5. 공용 계약 영역

두 영역이 함께 사용하는 순수 타입은 다음 경로로 분리한다.

```text
RAW_unity/Assets/_RAW/Scripts/Contracts/
```

예상 항목:

```text
KeyMapping.cs
CastType.cs
TargettingSkillTarget.cs
SkillCastIntent.cs
ISkillDamageReceiver.cs
```

공용 계약에는 다음 의존성을 넣지 않는다.

- Unity Netcode
- 구체적인 DB 구현
- 구체적인 캐릭터 또는 네트워크 컴포넌트

공용 계약을 추가·삭제·변경할 때는 두 담당자의 사전 합의와 리뷰가 필요하다. enum 값은 Unity 직렬화와 네트워크 직렬화에 사용될 수 있으므로 기존 순서를 변경하지 않고 명시적인 숫자 값을 유지한다.

## 6. 현재 경계가 섞인 코드

현재 `Char_Control.cs`는 캐릭터 동작과 네트워크 요청 생성이 함께 들어 있는 임시 상태다.

최종 책임은 다음과 같이 분리한다.

```text
Char_Control
입력, 타겟 선택, 사거리 접근
        ↓ SkillCastIntent
NetworkSkillAdapter
NetworkSkillCastRequest 생성
        ↓
NetworkSkillController
서버 요청, 검증 및 동기화
```

분리 완료 후 `Character`와 `Skill` 경로에서는 다음 namespace를 직접 참조하지 않는다.

```csharp
using Unity.Netcode;
using RAW.Network;
```

## 7. 스킬 데미지 책임

### 캐릭터·스킬 담당자

- 타격 대상 선정
- 데미지 계산식
- 공격력과 방어력 계수
- 치명타 및 상태 효과
- 타격 횟수와 간격
- 충돌 모양과 범위
- 판정 발생 시점

### 멀티·DB 담당자

- 서버에서만 판정 결과 적용
- 조작되거나 잘못된 Client 요청 거절
- `NetworkEnemyState` 또는 `NetworkCharacterState`에 결과 반영
- NetworkVariable을 통한 결과 동기화
- 동일 판정의 중복 적용 방지

Client에서 생성되는 애니메이션과 이펙트 오브젝트는 HP를 직접 변경하지 않는다.

## 8. 프리팹과 Unity 에셋 소유권

| 에셋 | 담당자 |
| --- | --- |
| `Dummy.prefab` | 캐릭터·스킬 담당자 |
| 스킬 이펙트 프리팹 | 캐릭터·스킬 담당자 |
| 인디케이터 프리팹 | 캐릭터·스킬 담당자 |
| Animator Controller 및 Animation Clip | 캐릭터·스킬 담당자 |
| `NetworkPlayer.prefab` | 멀티·DB 담당자 |
| `NetworkEnemy.prefab` | 멀티·DB 담당자 |
| `NetworkRuntime.prefab` | 멀티·DB 담당자 |
| NetworkAnimator 동기화 설정 | 멀티·DB 담당자 |

Unity 에셋의 `.meta` 파일은 원본 에셋과 같은 담당자가 소유한다. 파일 이동은 Unity Editor의 Project 창에서 수행해 GUID를 유지한다.

## 9. Scene 소유권

| Scene | 담당자 |
| --- | --- |
| `CharacterTest.unity` | 캐릭터·스킬 담당자 |
| `NetworkCharacterTest.unity` | 멀티·DB 담당자 |
| `NetworkTest.unity` | 멀티·DB 담당자 |
| 통합 또는 실제 게임 Scene | 작업 전 담당자 지정 |

같은 Scene을 두 담당자가 동시에 수정하지 않는다. 통합 Scene 변경이 필요하면 한 명만 편집하고 다른 담당자는 Prefab 단위로 변경 사항을 전달한다.

## 10. 스킬 ID와 DB 규칙

- 멀티·DB 담당자가 영속 데이터에 사용할 `SkillId`를 확정한다.
- 캐릭터·스킬 담당자는 확정된 `SkillId`를 `SkillSpec`에 입력한다.
- DB에는 Unity 에셋 경로나 표시 이름 대신 `SkillId`를 저장한다.
- DB에 저장되기 시작한 `SkillId`는 변경하지 않는다.
- `SkillId` 변경이 반드시 필요하면 데이터 마이그레이션 계획을 먼저 작성한다.

권장 형식:

```text
{character}_{skill_name}
```

예:

```text
warrior_sword_smash
archer_arrow_rain
mage_magic_heal
```

## 11. 새 스킬 개발 및 통합 절차

1. 멀티·DB 담당자가 `SkillId`를 확정한다.
2. 캐릭터·스킬 담당자가 스킬 동작, 데이터, 애니메이션, 이펙트와 로컬 게임플레이 테스트를 완료한다.
3. 캐릭터·스킬 담당자가 스킬을 `SkillCatalog`에 등록한다.
4. 아래 네트워크 요구사항을 멀티·DB 담당자에게 전달한다.
5. 멀티·DB 담당자가 기존 네트워크 계약으로 지원 가능한지 확인한다.
6. 새 계약이 필요하면 공용 계약을 먼저 별도 Pull Request로 변경한다.
7. 멀티·DB 담당자가 서버 검증, 상태 적용, 동기화와 DB 연결을 구현한다.
8. Host와 Client에서 공동 통합 테스트를 수행한다.

전달 정보:

```text
SkillId:
사용 슬롯:
CastType:
대상 타입:
사거리:
판정 시점:
타격 횟수:
데미지 계산 방식:
이펙트 생성 위치:
투사체 또는 캐릭터 이동 여부:
추가 동기화가 필요한 상태:
```

## 12. Git 작업 규칙

- `main` 브랜치에 직접 Push하지 않는다.
- 기능별 브랜치와 Pull Request를 사용한다.
- 가능하면 자신의 소유 경로만 변경한다.
- 다른 담당자 경로 변경이 포함되면 해당 담당자의 승인을 받는다.
- 공용 계약 변경은 구현 변경과 분리된 작은 Pull Request로 먼저 처리한다.
- Unity Scene, Prefab, Animator Controller는 동시에 편집하지 않는다.
- 자동 생성된 `.meta` 파일을 누락하거나 임의로 다른 에셋에 재사용하지 않는다.
- 하나의 커밋에 담당 영역이 다른 변경을 섞지 않는다.

권장 브랜치 예시:

```text
feature/skill-warrior-whirlwind
fix/skill-targeting
feature/network-skill-damage
feature/db-skill-loadout
refactor/code-ownership-boundaries
```

## 13. 다른 담당자 영역 변경 절차

다른 담당자의 파일을 변경해야 할 때는 다음 순서를 따른다.

1. 변경 목적과 필요한 공용 데이터 설명
2. 파일 소유자와 구현 위치 합의
3. 가능하면 소유자가 자신의 영역을 직접 수정
4. 불가피하게 대신 수정하면 별도 커밋으로 분리
5. Pull Request에서 파일 소유자의 승인 후 병합

긴급 수정도 이 절차를 생략하지 않는다. 긴급 상황에서는 담당자가 빠르게 리뷰하고 후속 정리 작업을 별도 Issue로 등록한다.

## 14. 경계 분리 완료 기준

- `Character`와 `Skill` 코드가 Unity Netcode를 직접 참조하지 않는다.
- `Char_Control`이 `NetworkSkillController`를 직접 참조하지 않는다.
- 네트워크 코드는 캐릭터 입력을 직접 읽지 않는다.
- 네트워크 코드는 스킬 공식과 애니메이션을 다시 구현하지 않는다.
- `Dummy.prefab`에는 네트워크 전용 컴포넌트가 없다.
- 네트워크 컴포넌트는 `NetworkPlayer.prefab`과 네트워크 프리팹에만 존재한다.
- 공용 enum과 스킬 요청 정보가 `Contracts`에 분리되어 있다.
- DB는 `SkillId`를 기준으로 저장하고 복원한다.
- 기존 형태의 일반 스킬은 네트워크 코드를 수정하지 않고 추가할 수 있다.
- 특수 스킬은 공용 계약 합의 후 각 담당자가 자신의 영역에서 구현한다.

## 15. 문서 변경

담당 영역이나 협업 방식이 변경되면 코드 변경 전에 이 문서를 먼저 수정한다. 이 문서의 변경은 두 담당자가 모두 확인한 뒤 병합한다.

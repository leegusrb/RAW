# RAW 개발 영역 및 코드 소유권

## 1. 목적

이 문서는 두 개발자가 같은 기능을 중복 구현하거나 같은 Unity 파일을 동시에 수정하지 않도록 책임과 작업 경계를 정한다.

소유권은 해당 영역의 설계, 구현, 수정과 최종 리뷰 책임을 의미한다.

## 2. 핵심 원칙

> 게임플레이 담당자는 **무슨 일이 일어나는지** 구현하고, 멀티·DB 담당자는 **그 일이 서버에서 확정되어 모든 사용자에게 동기화되고 저장되도록** 구현한다.

담당자가 헷갈릴 때는 다음 기준을 사용한다.

- 네트워크를 제거해도 필요한 기능은 게임플레이 담당 영역이다.
- 여러 사용자, 서버 권한, 재접속 또는 저장 때문에 필요한 기능은 멀티·DB 담당 영역이다.
- 양쪽 모두 필요한 기능은 공용 요청·결과 계약을 경계로 나눈다.
- 게임 규칙을 Client용과 서버용으로 각각 구현하지 않는다.

## 3. 담당자

| 담당자 | 담당 영역 |
| --- | --- |
| 이진수 | 캐릭터, 전투, 스킬, 적, 아이템 규칙, UI, 애니메이션, 이펙트와 전체 게임플레이 |
| 이현규 | 멀티플레이, 서버 권한, 인증, 네트워크 동기화, DB와 영속 데이터 |

## 4. 기능별 책임

| 기능 | 게임플레이 담당자 | 멀티·DB 담당자 |
| --- | --- | --- |
| 입력·이동 | 입력, 이동 알고리즘, 충돌, 이동 규칙과 애니메이션 | 소유 캐릭터의 입력 활성화, 위치 전송, 검증과 보정 |
| 캐릭터 상태 | 기본 스탯, 성장, 버프·디버프와 사망 규칙 | 서버 권한 HP·MP 상태, 동기화와 저장·복원 |
| 스킬 | 타겟, 사거리, 시전, 타격 시점·횟수와 효과 계산 | 요청 전송, 소유권·중복 검사, 서버 실행과 결과 동기화 |
| 데미지·회복 | 공격력, 방어력, 치명타, 회복과 상태 효과 계산 | 계산 결과를 서버 상태에 한 번만 적용 |
| 애니메이션·이펙트 | Animator, Animation Clip, VFX와 사운드 | 실행 이벤트 전송과 NetworkAnimator 설정 |
| 적·보스 | AI, 패턴, 전투, 사망과 드롭 규칙 | 서버 실행, Spawn·Despawn과 상태 동기화 |
| 아이템·인벤토리 | 아이템 정의, 슬롯, 용량, 스택 규칙과 UI | 서버 권한 변경 요청, snapshot 동기화와 저장 |
| 장비 | 착용 규칙, 스탯 효과, 외형과 UI | 소유 여부 검증, 장착 상태 동기화와 저장 |
| 상점·제작 | 가격, 레시피, 성공 확률과 UI | 서버 거래, 재화 변조 방지와 DB 트랜잭션 |
| 교환·파티·채팅 | 관련 UI와 상태 표시 | 요청, 참여자 상태, 승인 절차와 네트워크 처리 |
| 던전 | 맵, 몬스터 배치, 전투와 클리어 규칙 | 파티 입장, 네트워크 Scene 전환과 인스턴스 관리 |
| 로그인·캐릭터 선택 | 화면, 연출과 입력 | 인증, 세션, 계정과 캐릭터 데이터 |
| 테스트 | 로컬 게임플레이와 규칙 테스트 | Host·Client, 권한, 재접속과 DB 통합 테스트 |

오른쪽 담당자는 왼쪽의 게임 규칙을 다시 구현하지 않고, 왼쪽 담당자는 오른쪽의 네트워크·DB 처리를 구현하지 않는다.

## 5. 파일 소유권

### 5.1 게임플레이 담당자

```text
RAW_unity/Assets/_RAW/Scripts/Character/
RAW_unity/Assets/_RAW/Scripts/Skill/
RAW_unity/Assets/_RAW/Scripts/GameplayData/
RAW_unity/Assets/_RAW/Art/
RAW_unity/Assets/_RAW/Data/
RAW_unity/Assets/_RAW/Prefabs/Character/
RAW_unity/Assets/_RAW/Prefabs/Indicators/
RAW_unity/Assets/_RAW/Scenes/Tests/CharacterTest.unity
```

게임플레이 영역에는 다음 항목을 추가하지 않는다.

- `NetworkBehaviour`, RPC, `NetworkVariable`, `NetworkObjectReference`
- ClientId, OwnerClientId 또는 연결 상태를 이용한 분기
- 실제 DB 연결, SQL, schema 또는 migration
- 네트워크 전용 Prefab과 설정

### 5.2 멀티·DB 담당자

```text
RAW_unity/Assets/_RAW/Scripts/Network/
RAW_unity/Assets/_RAW/Scripts/Persistence/
RAW_unity/Assets/_RAW/Prefabs/Network/
RAW_unity/Assets/_RAW/Settings/Network/
RAW_unity/Assets/_RAW/Scenes/Tests/NetworkCharacterTest.unity
RAW_unity/Assets/_RAW/Scenes/Tests/NetworkTest.unity
supabase/migrations/
supabase/seed.sql
```

멀티·DB 영역에서는 다음 항목을 별도로 구현하지 않는다.

- 데미지, 방어력, 치명타와 회복 공식
- 스킬 타격 횟수, 간격과 충돌 규칙
- 캐릭터 이동 알고리즘과 적 AI
- 아이템 효과, 가격, 장비 효과와 제작 확률
- Animation Clip, Animator State와 VFX 동작

에셋의 `.meta` 파일은 원본 에셋과 같은 담당자가 소유한다.

## 6. 검증 책임

“서버 검증”은 게임 규칙 검증과 네트워크·보안 검증으로 구분한다.

### 게임플레이 담당자

- 대상이 사거리 안에 있는가
- 공격할 수 있는 대상인가
- 장애물 너머를 공격할 수 있는가
- 해당 아이템을 요청한 슬롯에 장착할 수 있는가
- 행동 결과로 몇 번, 언제, 얼마의 효과가 발생하는가

### 멀티·DB 담당자

- 인증된 사용자의 요청인가
- 요청자가 해당 NetworkObject의 소유자인가
- 요청한 ID와 대상이 실제로 존재하는가
- 서버가 가진 MP, 쿨다운, 재화와 아이템 수량이 충분한가
- 요청 값과 빈도가 허용 범위인가
- 이미 처리한 요청이 아닌가
- DB 변경이 원자적으로 처리되는가

멀티·DB 담당자는 서버에서 게임플레이 담당자가 만든 규칙 검증과 계산 함수를 호출한다.

## 7. 공용 계약

두 영역이 공유하는 ID, enum, 요청과 결과 타입은 다음 경로에 둔다.

```text
RAW_unity/Assets/_RAW/Scripts/Contracts/
```

공용 계약의 작업 규칙은 다음과 같다.

- 실제 파일 작성자는 멀티·DB 담당자로 고정한다.
- 게임플레이 담당자는 필요한 입력, 결과와 변경 이유를 전달한다.
- 변경 전 두 담당자가 구조에 합의한다.
- 변경 Pull Request는 두 담당자가 리뷰한다.
- 두 담당자가 같은 계약 파일을 동시에 수정하지 않는다.
- 계약에는 Netcode, 구체적인 DB 구현, UI, 애니메이션과 VFX를 넣지 않는다.
- enum 값은 기존 순서를 바꾸지 않고 명시적인 숫자를 유지한다.

## 8. 주요 기능의 연결 방식

### 8.1 스킬과 전투

```text
게임플레이 담당
입력 → 대상 선택 → 스킬 요청 데이터 생성
                       ↓
멀티·DB 담당
네트워크 전송 → 서버 권한·중복·상태 검사
                       ↓
게임플레이 담당
사거리·대상·데미지와 타격 결과 계산
                       ↓
멀티·DB 담당
서버 상태 적용 → 결과 동기화
                       ↓
게임플레이 담당
애니메이션·이펙트·사운드 표시
```

Client에서 생성한 스킬 이펙트와 충돌 오브젝트는 HP를 직접 변경하지 않는다.

### 8.2 인벤토리·장비·거래

- 게임플레이 담당자는 아이템, 슬롯, 장착, 가격과 제작 규칙을 구현한다.
- 멀티·DB 담당자는 변경 요청, 보유 수량 검증, 원자적 처리, 동기화와 저장을 구현한다.
- 게임플레이 UI는 NetworkBehaviour를 직접 호출하지 않고 제공된 명령 인터페이스 또는 Adapter를 사용한다.

## 9. ID와 저장 규칙

- 멀티·DB 담당자가 `SkillId`, `ItemId`, `CharacterId`의 형식과 최종 값을 관리한다.
- 게임플레이 담당자는 확정된 ID를 자신이 소유한 ScriptableObject에 입력한다.
- 멀티·DB 담당자는 ID 입력을 위해 게임플레이 ScriptableObject를 직접 수정하지 않는다.
- DB에는 Unity 에셋 경로나 표시 이름 대신 안정적인 ID를 저장한다.
- 저장에 사용되기 시작한 ID를 변경하려면 먼저 migration을 작성한다.

스킬 ID 형식:

```text
{character}_{skill_name}
```

## 10. Prefab과 Scene

| 파일 | 담당자 |
| --- | --- |
| `Dummy.prefab`과 게임플레이 Prefab | 게임플레이 담당자 |
| 스킬 이펙트, 인디케이터와 Animator | 게임플레이 담당자 |
| `NetworkPlayer.prefab`과 네트워크 Prefab | 멀티·DB 담당자 |
| NetworkAnimator와 NetworkTransform 설정 | 멀티·DB 담당자 |
| `CharacterTest.unity` | 게임플레이 담당자 |
| `NetworkCharacterTest.unity`, `NetworkTest.unity` | 멀티·DB 담당자 |
| 실제 게임 Scene의 맵과 게임플레이 구성 | 게임플레이 담당자 |
| 통합 Scene의 최종 편집 | 작업 전에 지정한 한 명 |

`NetworkPlayer.prefab`은 `Dummy.prefab`의 Prefab Variant 관계를 유지한다. 게임플레이 구성은 `Dummy.prefab`에서 변경하고 Network Prefab에는 네트워크 전용 컴포넌트와 override만 추가한다.

같은 Scene을 두 담당자가 동시에 수정하지 않는다. 파일 이동과 이름 변경은 Unity Editor에서 수행해 GUID를 유지한다.

## 11. 공용 프로젝트 파일

다음 경로의 실제 작성자는 멀티·DB 담당자로 고정하고 두 담당자가 리뷰한다.

```text
RAW_unity/Packages/manifest.json
RAW_unity/Packages/packages-lock.json
RAW_unity/ProjectSettings/
RAW_unity/Assets/AddressableAssetsData/
```

게임플레이 담당자가 Package, Layer, Tag, Input 또는 Addressable 설정 변경이 필요하면 요구사항을 전달한다. 공용 설정은 기능 변경과 분리된 Pull Request로 반영한다.

## 12. 기능 인계 절차

1. 게임플레이 담당자가 로컬 규칙, 데이터와 시각적 표현을 구현한다.
2. 네트워크 없이 게임플레이 테스트를 완료한다.
3. 필요한 입력, 결과와 동기화 요구사항을 멀티·DB 담당자에게 전달한다.
4. 공용 계약 변경이 필요하면 먼저 계약을 합의하고 별도 Pull Request로 반영한다.
5. 멀티·DB 담당자가 서버 실행, 동기화와 저장을 연결한다.
6. Host와 Client에서 함께 통합 테스트한다.

기능을 전달할 때 최소한 다음 내용을 작성한다.

```text
기능 ID:
입력:
실행 가능 조건:
게임플레이 결과:
실행 시점과 횟수:
동기화할 상태:
저장할 상태:
시각적 표현:
```

## 13. 완료 기준

### 게임플레이 담당자

- 네트워크 없이 로컬 테스트에서 정상 동작한다.
- Netcode와 DB 구현을 참조하지 않는다.
- 규칙과 계산을 외부에서 호출할 수 있다.
- 필요한 입력과 결과가 정의되어 있다.
- 게임플레이 테스트 또는 재현 절차가 있다.

### 멀티·DB 담당자

- Host와 Client에서 동일한 결과를 확인할 수 있다.
- Client가 서버 권한 상태를 임의 변경할 수 없다.
- 중복 요청이 결과를 중복 적용하지 않는다.
- 늦게 입장한 Client가 현재 상태를 받는다.
- 종료와 재접속 후 저장 상태가 복원된다.
- 네트워크·저장 통합 테스트 또는 재현 절차가 있다.

## 14. Git과 리뷰 규칙

- `main`에 직접 Push하지 않고 기능별 브랜치와 Pull Request를 사용한다.
- 자신의 소유 경로만 수정하는 것을 원칙으로 한다.
- 다른 담당자의 변경이 필요하면 파일 소유자에게 요청한다.
- 공용 계약과 프로젝트 설정은 별도 Pull Request로 먼저 변경한다.
- Scene, Prefab, Animator와 공용 설정은 동시에 편집하지 않는다.
- 하나의 커밋에 서로 다른 담당 영역의 변경을 섞지 않는다.
- 다른 담당자 영역이 포함된 Pull Request는 해당 담당자의 승인을 받아야 한다.

## 15. 예외 처리

다른 담당자의 파일을 직접 수정해야 한다면 다음 순서를 따른다.

1. 변경 목적과 필요한 결과를 설명한다.
2. 파일 소유자와 수정 위치를 합의한다.
3. 사전 동의를 받은 후 별도 커밋으로 수정한다.
4. 파일 소유자의 리뷰와 승인을 받은 뒤 병합한다.

담당 영역이나 협업 방식이 바뀌면 코드 변경 전에 이 문서를 먼저 수정한다.

# RAW 코딩 규칙

## 기본 원칙

- 임시 전역 싱글톤 사용을 최소화한다.
- 기능이 커지면 하나의 MonoBehaviour에 입력, 이동, 전투, UI 책임을 섞지 않는다.
- Runtime 코드와 Editor 코드는 분리한다.
- 테스트 가능한 데이터 구조를 우선한다.

## 네이밍

### 클래스

PascalCase를 사용한다.

```csharp
PlayerController
SkillTargetingController
EquipmentDefinition
````

### 필드

private serialized field는 camelCase를 사용한다.

```csharp
[SerializeField] private float moveSpeed;
[SerializeField] private SkillDefinition skillDefinition;
```

### Enum

Enum 값은 PascalCase를 사용한다.

```csharp
public enum CastType
{
    Bar,
    Area,
    Target
}
```

## 폴더 기준

* 게임 실행 코드는 `Assets/_RAW/Runtime`에 둔다.
* 에디터 확장 코드는 `Assets/_RAW/Editor`에 둔다.
* 테스트 코드는 `Assets/_RAW/Tests`에 둔다.
* 외부 에셋은 `Assets/ThirdParty`에 둔다.
* 기존 외부 에셋 이동은 별도 브랜치에서 처리한다.

## ScriptableObject 기준

* 스킬 정의는 `SkillDefinition`을 사용한다.
* 아이템 정의는 `ItemDefinition`을 기반으로 한다.
* 장비 정의는 `EquipmentDefinition`을 사용한다.
* 임시 데이터 컨테이너에 여러 시스템 데이터를 섞지 않는다.

## 금지 또는 지양

* `DataBase.Instance` 같은 임시 전역 접근을 새 코드에 추가하지 않는다.
* 문자열 기반 스킬 슬롯 키 `"q"`, `"w"`, `"e"`, `"r"` 사용을 늘리지 않는다.
* 장비 슬롯과 외형 파츠를 같은 enum에 계속 섞지 않는다.
* 하나의 클래스가 입력, 이동, 스킬, 애니메이션을 모두 처리하지 않도록 한다.

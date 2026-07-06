# RAW 폴더 구조

## Unity 프로젝트 루트

```text
RAW_unity/
├── Assets/
├── Packages/
└── ProjectSettings/
````

## RAW 전용 코드/리소스

```text
Assets/_RAW/
├── Runtime/
├── Presentation/
├── Data/
├── Editor/
├── Tests/
└── Addressables/
```

## Runtime

게임 실행 중 사용하는 코드를 배치합니다.

```text
Runtime/
├── Core/
├── Characters/
├── Combat/
├── Skills/
├── Items/
├── Inventory/
├── Dungeons/
├── Party/
├── Guild/
├── Network/
├── SaveLoad/
└── UI/
```

## Presentation

화면에 보이는 리소스를 배치합니다.

```text
Presentation/
├── Scenes/
├── Prefabs/
├── UI/
├── Sprites/
├── Animation/
├── VFX/
└── SFX/
```

## Data

ScriptableObject, 밸런스 데이터, 테이블 데이터를 배치합니다.

```text
Data/
├── ScriptableObjects/
├── Tables/
└── Balance/
```

## ThirdParty

외부 에셋 또는 패키지성 리소스를 배치합니다.

```text
Assets/ThirdParty/
├── Cainos/
└── SPUM/
```

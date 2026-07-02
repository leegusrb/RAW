# RAW 본격 개발 전 정리 체크리스트

기준 브랜치: `Folder`

목표: 본격 개발 전에 문서, 폴더, 임시 코드, 중복 시스템을 정리해 안정적인 개발 베이스를 만든다.

## 체크 규칙

- `[ ]` 미진행
- `[x]` 완료
- 한 단계가 끝날 때마다 Unity Console Error, 참조 깨짐, Git 변경 파일을 확인한다.

---

## 1. docs: update README and project documentation

- [x] README의 Unity 버전을 실제 프로젝트 버전과 맞추기
- [x] `Docs/FolderStructure.md` 추가
- [x] `Docs/CodingRules.md` 추가
- [x] `Docs/UnityVersion.md` 추가

## 2. chore: normalize gitignore and gitattributes

- [x] 루트 `.gitignore`와 `RAW_unity/.gitignore` 중복 확인
- [x] ignore 기준 통합 여부 결정
- [x] Unity 생성 폴더가 추적되지 않는지 확인
- [x] `.gitattributes` 유지 확인
- [x] Addressables 빌드 산출물이 추적되지 않는지 확인

## 3. chore: add asmdef files for RAW runtime/editor/tests

- [x] `Assets/_RAW/Runtime/RAW.Runtime.asmdef` 추가
- [x] `Assets/_RAW/Editor/RAW.Editor.asmdef` 추가
- [x] `Assets/_RAW/Tests/EditMode/RAW.Tests.EditMode.asmdef` 추가
- [x] `Assets/_RAW/Tests/PlayMode/RAW.Tests.PlayMode.asmdef` 추가
- [x] Unity 스크립트 컴파일 성공 확인

## 4. refactor: replace SkillSpec with SkillDefinition

- [ ] `SkillSpec` 사용처 목록화
- [ ] 테스트용 `SkillSpec` 에셋을 `SkillDefinition` 에셋으로 변환
- [ ] `SkillTargetingController`가 `SkillDefinition`을 받도록 수정
- [ ] `SkillSpec.cs` 삭제
- [ ] `SkillSpec` 검색 결과 없음 확인

## 5. refactor: introduce SkillLoadout and remove database skill key map

- [ ] `SkillSlotAssignment` 추가
- [ ] `SkillLoadout` 추가
- [ ] Q/W/E/R 슬롯을 `SkillSlotKey`로 처리
- [ ] `DataBase.mySkillKeyMap` 제거
- [ ] `SkillTargetingController.BeginIndicate(string)` 제거

## 6. refactor: split player input from player controller

- [ ] `Char_Control` 책임 목록 확인
- [ ] `PlayerInputReader` 추가
- [ ] `PlayerController` 추가 또는 `Char_Control` 이름 변경
- [ ] 이동 입력과 스킬 입력 분리
- [ ] 우클릭 이동, S 정지, Q/W/E/R 표시 정상 작동 확인

## 7. refactor: split equipment slot and appearance part

- [ ] 현재 `EquipmentSlot` 값 분류
- [ ] 실제 장비 슬롯용 `EquipmentSlot` 정리
- [ ] 외형 파츠용 `AppearancePart` 추가
- [ ] `Char_Appearance`의 Renderer Map을 `AppearancePart` 기준으로 수정
- [ ] `Char_Inventory`는 실제 장비 슬롯만 다루게 수정

## 8. refactor: make EquipmentDefinition inherit ItemDefinition

- [ ] `ItemDefinition` 공통 필드 정리
- [ ] `EquipmentDefinition : ItemDefinition` 구조로 변경
- [ ] `EquipmentStatBlock` 추가
- [ ] 상태이상 옵션 구조 추가 여부 결정
- [ ] 기존 장비 에셋 참조 확인

## 9. refactor: remove temporary DataBase service

- [ ] `equipmentAddress` 대체 위치 결정
- [ ] `maxInventoryCapacity`를 `InventoryConfig`로 이동
- [ ] 스킬 참조를 `SkillLoadout` 또는 `SkillDatabase`로 대체
- [ ] `RAW_Services.prefab`에서 `DataBase` 제거
- [ ] `DataBase.cs` 삭제
- [ ] `DataBase.Instance` 검색 결과 없음 확인

## 10. test: add validation tests for skill and item definitions

- [ ] 스킬 ID 중복 검사 테스트 추가
- [ ] 스킬 필수 필드 검사 테스트 추가
- [ ] 아이템 ID 중복 검사 테스트 추가
- [ ] 장비 필수 필드 검사 테스트 추가
- [ ] Unity Test Runner에서 EditMode 테스트 실행 확인

## 11. chore: clean legacy scenes and readme images

- [ ] `SampleScene` 제거 또는 Legacy/Test로 이동
- [ ] `CharacterTest`는 Test 씬으로 유지
- [ ] Boot 씬 추가 여부 결정
- [ ] README 이미지 폴더 유지/이동/삭제 결정
- [ ] SPUM 이동은 별도 브랜치로 분리

---

## 최종 완료 조건

- [ ] README, Docs, Unity 버전이 일치함
- [ ] `SkillSpec` 제거 완료
- [ ] `DataBase` 제거 완료
- [ ] `EquipmentSlot`과 `AppearancePart` 분리 완료
- [ ] 입력/이동/스킬 책임 분리 완료
- [ ] Unity Console Error 없음
- [ ] 주요 테스트 씬 정상 실행
- [ ] 본격 개발 시작용 브랜치로 병합 가능

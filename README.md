🎒 C# Inventory System

C# 콘솔을 활용한 RPG 스타일 인벤토리 + 전투 + 합성 시스템
C# 인터페이스와 디자인 패턴 적용을 연습하기 위함.


📌 핵심 기능

아이템 추가 / 삭제: List<Item> 기반의 인벤토리 슬롯 관리
장비 장착 / 해제: 무기(Weapon) · 방어구(Armor) 슬롯에 장착 시 플레이어 스탯(ATK / DEF) 실시간 반영
포션 사용: HP / MP 포션 사용 시 플레이어 수치 즉시 회복
장비 분해: 장비를 재료 아이템으로 분해 (IDisassemblable 인터페이스)
정렬 시스템: 등급순 · 무게순 오름차순 / 내림차순 정렬 (ISortStrategy 전략 패턴)
등급 필터: 특정 등급 아이템만 필터링 출력
전투 시스템: 몬스터 스폰 → 전투 → 경험치 획득 → 레벨업 → 아이템 드롭
드롭 시스템: 몬스터 레벨·등급 기반 장비 / 포션 / 재료 확률 드롭 (IDropStrategy 전략 패턴)
합성 시스템: 재료 조건 충족 시 최종 목표 아이템(전설의 용사검) 합성 → 게임 클리어
무게 제한: 인벤토리 최대 무게(100) 초과 시 경고 출력
재료 스택: 같은 재료는 한 슬롯에 최대 5개까지 자동 합산 (IStackable 인터페이스)


🛠 사용 기술

Language: C# 8.0
Framework: .NET Core
Data Structure: List<T>, Dictionary<K,V>, event Action<T>


🧠 학습 포인트

객체 지향 프로그래밍(OOP)의 상속을 이용해 Item → Weapon / Armor / Potion / Material 타입 계층을 구현함.
인터페이스(IDamageable, IDropStrategy, ISortStrategy 등)를 사용하여 확장성과 교체 가능성을 확보함.
전략 패턴으로 드롭 방식·정렬 기준을 Battle / Inventory 수정 없이 외부에서 주입할 수 있도록 설계함.
MonsterTable / ItemTable에 데이터 한 줄 추가만으로 새 몬스터·장비를 등록할 수 있도록 함.
이벤트(event Action<T>)를 활용해 Inventory 내부 로직과 UI 출력을 분리함.


📁 파일 구조
csharp_prac_interface/
│
├── Program.cs              # 진입점 — GameManager.Run() 호출
├── GameManager.cs          # 메인 루프, 메뉴 UI, 게임 클리어 처리
│
├── 전투
│   ├── Battle.cs           # 전투 진행 / 승패 처리 / 드롭 반환
│   ├── Unit.cs             # Player / Monster 공통 스탯 베이스 클래스
│   ├── Player.cs           # 플레이어 — 레벨업, 포션 회복, 장비 장착/해제
│   ├── Monster.cs          # 몬스터 — MonsterData 기반 인스턴스
│   └── MonsterTable.cs     # 몬스터 원본 데이터 테이블 + 스폰
│
├── 아이템
│   ├── Item.cs             # 아이템 추상 베이스 클래스 (Name, Weight, Rarity)
│   ├── Weapon.cs           # 무기 — ATK 증가, IDisassemblable 구현
│   ├── Armor.cs            # 방어구 — DEF 증가, IDisassemblable 구현
│   ├── Potion.cs           # HP / MP 포션 (크기별 4종 × 2)
│   ├── Material.cs         # 재료 — IStackable 구현 (낡은 가죽, 철 조각)
│   ├── EquipmentData.cs    # WeaponData / ArmorData 순수 데이터 클래스
│   └── ItemTable.cs        # 장비 원본 데이터 테이블
│
├── 인벤토리
│   ├── Inventory.cs        # 아이템 목록 관리, 정렬, 필터, 합성 지원 메서드
│   └── CraftingRecipe.cs   # Ingredient / CraftingRecipe / CraftingTable
│
└── 인터페이스
    ├── IDamageable.cs      # 피격 처리 (TakeDamage, IsAlive)
    ├── IDropStrategy.cs    # 드롭 시스템 (DefaultDropStrategy / NoDropStrategy)
    ├── ISortStrategy.cs    # 정렬 방법 (등급 / 무게 오름차순·내림차순)
    ├── IStackable.cs       # 스택 아이템 (AddStack, ConsumeStack)
    └── IDisassemblable.cs  # 장비 분해 (Disassemble → List<Material>)

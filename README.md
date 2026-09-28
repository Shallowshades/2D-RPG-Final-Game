# 2D RPG Final Game

一个基于 Unity 6 的 2D 横版动作 RPG 工程。玩家拥有连段近战、反击、冲刺、掷剑、时间残影与领域展开等技能；敌人具备巡逻/追击/攻击/眩晕/死亡的行为状态；另有一套完整的分组数值系统、元素状态效果、装备背包与技能树 UI。

本文档说明工程的**代码架构**与各子系统的协作方式，面向将要阅读或扩展本项目代码的开发者。

---

## 目录

- [1. 技术栈与工程结构](#1-技术栈与工程结构)
- [2. 架构总览](#2-架构总览)
- [3. 核心基座：Entity 与状态机](#3-核心基座entity-与状态机)
- [4. 玩家子系统](#4-玩家子系统)
- [5. 敌人子系统](#5-敌人子系统)
- [6. 数值系统 Stats](#6-数值系统-stats)
- [7. 战斗与伤害管线](#7-战斗与伤害管线)
- [8. 技能系统](#8-技能系统)
- [9. 物品与背包](#9-物品与背包)
- [10. UI 层](#10-ui-层)
- [11. 数据层（ScriptableObject）](#11-数据层scriptableobject)
- [12. 其他子系统](#12-其他子系统)
- [13. 扩展指南](#13-扩展指南)
- [14. 已知问题与技术债](#14-已知问题与技术债)
- [15. 命名与代码约定](#15-命名与代码约定)

---

## 1. 技术栈与工程结构

| 项目 | 版本 / 说明 |
|---|---|
| Unity | `6000.0.40f1`（Unity 6） |
| 渲染管线 | Universal RP `17.0.4`（2D Renderer） |
| 输入 | Input System `1.13.1`（Action Asset 驱动） |
| 相机 | Cinemachine `3.1.6` |
| 物理 | `Rigidbody2D` + 射线/重叠圆检测（未使用 Tilemap Collider 逻辑） |
| 其他包 | 2D Feature Set、Timeline、Visual Scripting、uGUI（TMP） |

### 目录结构

```
Assets/
├─ Scripts/                   ← 本文档描述的全部代码（101 个 .cs，约 5.7k 行）
│  ├─ StateMachine/           状态机骨架 + 三层状态基类
│  ├─ Entity/                 实体通用组件（战斗/血量/数值/特效/动画事件）
│  ├─ Player/                 玩家本体 + 15 个玩家状态
│  ├─ Enemy/                  敌人本体 + 7 个敌人状态
│  ├─ StatsSystem/            分组数值容器与修饰符
│  ├─ SkillSystem/            技能逻辑（Skill_*）与技能实体（SkillObject_*）
│  ├─ ItemSystem/             背包与装备
│  ├─ InteractiveObjects/     场景交互物（掉落物/宝箱/Buff）
│  ├─ UI/                     背包、提示框、技能树、血条
│  ├─ Data/                   ScriptableObject 与运行时数据类
│  ├─ Enum/                   全局枚举
│  ├─ Interface/              IDamagable / ICounterable
│  ├─ Parallax/               视差背景
│  └─ VFX/                    通用特效控制器
├─ InputSystem/               PlayerInputSet.inputactions（及生成的 C# 包装类）
└─ ...
```

> 注意：`PlayerInputSet`（输入动作包装类）位于 `Assets/InputSystem/` 而非 `Scripts/`，由 `.inputactions` 资源自动生成，**不要手工修改**。

### 输入动作

`Player.cs:165-177` 绑定全局动作，状态类在 `Update` 中轮询其余动作：

| 动作 | 绑定方式 | 用途 |
|---|---|---|
| `Movement` | 事件（performed/canceled） | 写入 `moveInput` |
| `Mouse` | 事件（performed） | 写入 `mousePosition`（掷剑瞄准） |
| `Jump` | 状态内轮询 | 跳跃 / 蹬墙跳 |
| `Attack` | 状态内轮询 | 连段 / 空中攻击 / 蓄力掷剑 |
| `Dash` | `PlayerState.Update` 轮询 | 冲刺 |
| `CounterAttack` | 状态内轮询 | 反击 |
| `RangeAttack` | 状态内轮询 | 掷剑（按住蓄力，松开释放） |
| `Spell` | 事件 | 同时尝试触发 `shard` 与 `timeEcho` |
| `UltimateSpell` | `PlayerState.Update` 轮询 | 领域展开 |
| `SkillTreeBoard` | 事件 | 开关技能树 UI |

---

## 2. 架构总览

工程采用 **"实体 + 组件 + 状态机"** 的组合式设计，没有使用继承树堆叠玩法。分三层：

```
┌─────────────────────────────────────────────────────────────┐
│  数据层 (Data / ScriptableObject)                            │
│  StatsSetupData · SkillData · ItemData · EquipmentData       │
│  —— 策划配置，运行时只读                                      │
└──────────────────────────┬──────────────────────────────────┘
                           │ 读取 / 应用
┌──────────────────────────▼──────────────────────────────────┐
│  逻辑层 (Entity 组件 + 状态机 + 技能/背包)                    │
│  Entity_Stats · Entity_Health · Entity_Combat ·               │
│  Entity_StatusHandler · Skill_* · Inventory_*                │
└──────────────────────────┬──────────────────────────────────┘
                           │ 事件 / 直接调用
┌──────────────────────────▼──────────────────────────────────┐
│  表现层 (UI / VFX / Parallax)                                │
│  UI_Inventory · UI_SkillTree · Entity_VFX · ParallaxLayer     │
└─────────────────────────────────────────────────────────────┘
```

### 关键设计要点

1. **组件按职责切分**。同一个 GameObject 上挂多个 `Entity_*` 组件，各管一摊：`Entity_Stats` 只算数，`Entity_Health` 只管血，`Entity_Combat` 只管打人，`Entity_StatusHandler` 只管元素状态。彼此通过 `GetComponent` 直接互相引用，没有事件总线。
2. **状态机是纯 C# 类**，不是 MonoBehaviour。状态对象在实体 `Awake` 中一次性 `new` 出来并常驻，`ChangeState` 只切换引用并调用 `Enter/Exit`。
3. **动画事件是玩法触发的主要载体**。攻击判定、技能生成、状态结束都由 Animator 上的动画事件驱动，而非时间戳。**动画与代码强耦合**（见 [14. 已知问题](#14-已知问题与技术债)）。
4. **ScriptableObject 只作配置，不存运行时状态**。玩家当前血量、背包内容等都是普通字段。
5. **UI 由事件驱动刷新**，不做逐帧轮询（唯一例外是血量 Slider，见 `Entity_Health.UpdateHealthBar`）。

---

## 3. 核心基座：Entity 与状态机

### 3.1 StateMachine（`StateMachine/StateMachine.cs`）

最小实现，约 35 行：

```csharp
public EntityState currentState { get; private set; }
public bool canChangeState;

void Initialize(EntityState startState);   // 进入初始状态，开启状态切换
void ChangeState(EntityState newState);     // Exit 旧 → 赋值 → Enter 新
void UpdateActiveState();                   // 每帧由 Entity.Update 调用
void SwitchOffStateMachine();               // 置 canChangeState = false，冻结状态机
```

`SwitchOffStateMachine()` 用于死亡：`Enemy_DeadState.Enter` 与 `Player_DeadState` 都会冻结状态机，进入终态。

### 3.2 状态基类三层结构

```
EntityState (abstract)            ← StateMachine/EntityState.cs
   │  持有 stateMachine / animator / rb / stats
   │  持有 animationBoolName、stateTimer、triggerCalled
   │  Enter/Exit 自动开关同名 animator bool
   ├── EnemyState                  ← StateMachine/EnemyState.cs
   │     持有 enemy；推送 moveSpeedMultiplier 等动画参数
   └── PlayerState                 ← StateMachine/PlayerState.cs
         持有 player / input / skillManager
         统一处理 Dash 与 UltimateSpell 两个全局输入
```

`EntityState` 的两个关键机制：

- **`animationBoolName`**：`Enter()` 时 `animator.SetBool(name, true)`，`Exit()` 时置 `false`。因此**每个状态必须对应一个 Animator bool 参数**，否则状态切换会抛异常。多个状态可共用同一 bool（如 `jumpFall` 被跳跃/下落/蹬墙跳/领域展开共用）。
- **`triggerCalled`**：由动画事件 `CurrentStateTrigger()` 置位，是绝大多数状态"动作播完 → 回到上一状态"的判据。

### 3.3 Entity（`Entity/Entity.cs`）

所有可移动实体的基类，提供：

| 成员 | 说明 |
|---|---|
| `rb` / `animator` / `stats` / `stateMachine` | `Awake` 中优先获取，供子类使用 |
| `groundDetected` / `wallDetected` | 每帧 `Update` 中射线检测 |
| `SetVelocity(x, y)` | 设置速度并自动翻转朝向（**击退期间调用无效**） |
| `Flip()` / `facingDir` / `OnFlipped` 事件 | 朝向管理，供 UI 血条订阅 |
| `ReciveKnockback(v, t)` | 击退协程，期间 `isKnocked = true` |
| `SlowDownEntity(...)` / `SlowDownEntityCoroutine` | 减速钩子，子类重写以实现不同减速语义 |
| `CurrentStateAnimationTrigger()` | 转发给当前状态，配合动画事件 |
| `EntityDeath()` | 虚方法，子类重写死亡表现 |

**碰撞检测细节**：`wallDetected` 在同时配置了 `primaryWallCheck` 与 `seconderyWallCheck` 时要求**两条射线同时命中**才为真（用于仅在上半身与下半身都贴墙时才算撞墙）。

---

## 4. 玩家子系统

### 4.1 Player（`Player/Player.cs`）

`Player : Entity`，是整个工程的**装配中心**：

- `Awake` 中 `news` 出 13 个状态实例（`Grounded`/`Aired` 是基类，不实例化），并 `GetComponent` 抓取 `skillManager` / `playerVfx` / `statusHandler` / `health`。
- `Start` 中 `stateMachine.Initialize(idleState)`。
- `OnEnable` 绑定输入动作（见 [1. 输入动作](#输入动作)），`OnDisable` 解绑。
- 暴露 `moveSpeed`、`jumpForce`、`dashSpeed`、`attackVelocity[3]` 等大量可调参数，**全部走 Inspector 序列化，没有配置文件**。
- `static event OnPlayerDeath`：玩家死亡时广播，`Enemy` 订阅后回到 Idle。

### 4.2 玩家状态图

状态分四层组织：`PlayerState` → `Player_GroundedState` / `Player_AiredState` → 具体状态。**父状态的 `Update` 会先执行再执行子状态的 `Update`**，因此跳跃/攻击等输入在基类统一分流。

`PlayerState.Update()`（所有玩家状态的公共前置逻辑）：

| 条件 | 转移 |
|---|---|
| `Dash` 按下 且 `CanDash()` | → `DashState`（并进入冷却） |
| `UltimateSpell` 按下 且 领域可用 且 `InstantDomain()` 为真 | 原地生成领域，**不切换状态** |
| 同上，但 `InstantDomain()` 为假 | → `DomainExpansionState` |

`CanDash()` 的否决条件：技能在冷却 / 贴墙 / 已处于 `DashState` / 已处于 `DomainExpansionState`。

**地面层 `Player_GroundedState`**：

| 条件 | 转移 |
|---|---|
| `y < 0 && !groundDetected`（走下平台） | → `FallState` |
| `Jump` 按下 | → `JumpState` |
| `Attack` 按下 | → `BasicAttackState` |
| `CounterAttack` 按下 | → `CounterAttackState` |
| `RangeAttack` 按下 且 掷剑可用 | → `SwordThrowState` |

**完整转移表**：

| 当前状态 | 条件 | 目标状态 |
|---|---|---|
| `Idle` | `moveInput.x != 0` | `Move` |
| `Idle` | `moveInput.x == facingDir && wallDetected` | 保持（禁止贴墙推进） |
| `Move` | `moveInput.x == 0` 或 `wallDetected` | `Idle` |
| `Jump` | `y < 0` 且 非 `JumpAttack` | `Fall` |
| `Fall` | `groundDetected` | `Idle` |
| `Fall` | `wallDetected` | `WallSlide` |
| `WallSlide` | `Jump` 按下 | `WallJump` |
| `WallSlide` | `!wallDetected` | `Fall` |
| `WallSlide` | `groundDetected` | `Idle` |
| `WallJump` | `y < 0` | `Fall` |
| `WallJump` | `wallDetected` | `WallSlide` |
| `AiredState`（空中基类） | `Attack` 按下 | `JumpAttack` |
| `Dash` | `stateTimer < 0` | 落地→`Idle` / 贴墙→`WallSlide` / 否则→`Fall` |
| `Dash` | `wallDetected` | 落地→`Idle` / 否则→`WallJump` |
| `BasicAttack` | 连段事件触发 且 有排队 | `BasicAttack`（下一段，隔帧） |
| `BasicAttack` | 连段事件触发 且 无排队 | `Idle` |
| `JumpAttack` | 落地 + 动画事件 | `Idle` |
| `CounterAttack` | 动画事件 / `stateTimer < 0` | `Idle` |
| `SwordThrow` | `RangeAttack` 松开 或 动画事件 | `Idle` |
| `DomainExpansion` | 悬停计时结束 | `Idle` |
| 任意 | `health.Die()` → `EntityDeath()` | `Dead`（冻结状态机） |

### 4.3 值得注意的状态细节

- **连段（Combo）**：`BasicAttackState` 用 `comboIndex` 记录 0→1→2 循环，通过 animator int 参数 `basicAttackIndex` 驱动不同段的动画。由于 `Attack` 按键的"进入攻击"判断复用同一字段，同帧切换不会生效，因此下一段必须经过 `Player.EnterAttackStateWithDelay()` **等待 `WaitForEndOfFrame`** 后进入（`Player.cs:144-163` 有对应注释）。
- **冲刺**：`Enter` 时触发残影生成、重力归零、`SetCanTakeDamage(false)`；**这些副作用全部在 `Exit` 里还原**，若异常中断会残留无敌或零重力。
- **掷剑**：`SwordThrowState` 只负责瞄准与轨迹预测，**实际飞剑实体由动画事件 `ThrowSword()` 生成**（`Player_AnimationTriggers.cs`），不是按键分支直接 `Instantiate`。
- **领域展开**：先垂直上升至上限，再悬停并周期性施法，结束时还原重力与受伤开关。`InstantDomain()`（减速升级）则跳过整个状态。

### 4.4 动画事件桥（`Player_AnimationTriggers.cs`）

挂在玩家物体上的 MonoBehaviour，Animator 上配置的动画事件指向以下方法：

| 方法 | 效果 | 典型挂载动画 |
|---|---|---|
| `CurrentStateTrigger()` | 置 `triggerCalled = true` | 所有攻击/技能动画末帧 |
| `AttackTrigger()` | `Entity_Combat.PerformAttack()` | 近战/空中/反击的判定帧 |
| `ThrowSword()` | `skillManager.swordThrow.ThrowSword()` | 掷剑释放帧 |

---

## 5. 敌人子系统

### 5.1 类结构

| 类 | 基类 | 说明 |
|---|---|---|
| `Enemy` | `Entity` | 配置中心：玩家检测、战斗/眩晕参数、状态引用 |
| `Enemy_Skeleton` | `Enemy`, `ICounterable` | 完整实现，**唯一可被弹反的敌人**，构造 6 个状态 |
| `Enemy_Archer` | `Enemy` | ⚠️ 半成品，仅构造 `idle`/`move` 两状态且未 `Initialize`，检测到玩家会 `ChangeState(null)` 抛空 |
| `Enemy_Health` | `Entity_Health` | 仅重写 `TakeDamage`：受击后拉入战斗状态 |
| `Enemy_AnimationTriggers` | `Entity_AnimationTriggers` | 增加弹反窗口 / 攻击预警开关 |
| `Enemy_VFX` | `Entity_VFX` | 增加头顶攻击预警图标 |

### 5.2 敌人状态图

```
        ┌──────────────────────────────┐
        │  Idle ──(idleTime 到)──> Move │
        │   ▲   ◄──(撞墙/悬空)─────┘    │   ← Enemy_GroundedState 父层
        └──────┬───────────────────────┘
               │ PlayerDetected()
               ▼
        ┌──────────┐  dist < attackDistance   ┌────────┐
        │  Battle  ├─────────────────────────>│ Attack │
        │          │<─── triggerCalled ───────┤        │
        └────┬─────┘                          └────────┘
             │ battleTimeDuration 内无目标
             ▼
           Idle

  外部入口： Enemy_Health.TakeDamage → Battle
            Enemy_Skeleton.HandleCounter → Stunned ──> Idle
            Enemy.EntityDeath() → Dead（冻结状态机，终态）
```

| 状态 | 关键行为 |
|---|---|
| `Enemy_GroundedState` | 非抽象，是 Idle/Move 的父层，负责"发现玩家 → Battle"的公共转移 |
| `Idle` | 计时 `idleTime` 后转 Move |
| `Move` | 撞墙或悬空时 `Flip()` 并转 Idle；否则按 `GetMoveSpeed()` 巡游 |
| `Battle` | 目标过近时后跳（`retreatVelocity`），否则以 `battleMoveSpeed` 贴脸；超过 `battleTimeDuration` 未更新目标则转 Idle |
| `Attack` | `Enter` 时 `SyncAttackSpeed()` 写入 animator 的 `attackSpeedMultiplier`；动画事件后转 Battle |
| `Stunned` | 仅由 `HandleCounter()` 进入；受击弹飞，计时结束转 Idle |
| `Dead` | 关闭 Animator 与 Collider2D、重力调大 12、上抛速度 15、冻结状态机 |

### 5.3 AI 检测

`PlayerDetected()` 是一条从 `playerCheck.position` 沿 `facingDir` 出发、长度 10 的射线，遮罩为 `whatIsPlayer | whatIsGround`。命中层不是 `Player` 则返回 false。

**这意味着敌人只有正面直线视野，被从背后偷袭不会进入战斗状态。**`GetPlayerReference()` 会把命中结果缓存到 `player` 字段。

### 5.4 敌人死亡

`Enemy_Health` **没有**掉落表、没有 ragdoll。死亡表现完全由 `Enemy_DeadState.Enter` 实现（关组件 + 物理抛飞）。拾取物需要在场景中预先摆放 `Object_ItemPickup`。

---

## 6. 数值系统 Stats

### 6.1 对象模型

```
Entity_Stats (MonoBehaviour)          ← 挂在实体上，作为全实体的数值门面
├── resources : Stats_ResourceGroup   { maxHealth, healthRegen }
├── major     : Stats_MajorGroup      { strength, agility, intelligence, vitality }
├── offense   : Stats_OffenseGroup    { attackSpeed, damage, critChance, critPower,
│                                       armorReduction, fireDamage, iceDamage, lightningDamage }
└── defense   : Stats_DefenseGroup    { armor, evasion, fireResistance,
                                        iceResistance, lightningResistance }

Stats (可序列化类)                     ← 每个叶子属性都是一个独立实例
├── baseValue : float
├── modifiers : List<StatsModifier>   ← { value, source }
└── GetValue() / AddModifier() / RemoveModifier() / SetBaseValue()
```

### 6.2 数值计算管线

`Stats.GetValue()` 是**纯加法 + 脏标记缓存**，没有乘区、没有百分比修饰符：

```csharp
finalValue = baseValue;
foreach (mod in modifiers) finalValue += mod.value;   // 扁平加成
```

缓存由 `needToBeReCalculated` 控制，`AddModifier` / `RemoveModifier` 会置脏。

**基础值来源**：`Entity_Stats.ApplyDefaultStatsSetup()` 从 `StatsSetupData` 资产逐字段写入。该方法标了 `[ContextMenu]`，**只能在 Inspector 中右键手动触发，不会在 Play 时自动执行**。

**修饰符来源**（全部通过 `source` 字符串做增删配对）：

| 来源 | 写入点 |
|---|---|
| 装备 | `Inventory_Item.AddModifiers(stats)`，`source = itemId` |
| Buff 道具 | `Object_Buff.ApplyBuff(true)` |

### 6.3 派生计算（不走 `GetValue()`）

`Entity_Stats` 提供了一组带公式的派生方法，是数值系统的核心：

| 方法 | 公式 |
|---|---|
| `GetMaxHealth()` | `maxHealth + vitality × 5` |
| `GetPhysicalDamage(out isCrit, scale)` | `(damage + strength) × (暴击 ? (critPower + strength×0.5)/100 : 1) × scale`；暴击率 = `critChance + agility × 0.3` |
| `GetElementalDamage(out element, scale)` | 取三系中最高的作为主元素；`主元素 + intelligence + 其余两系各 ×0.5`，再 `× scale` |
| `GetArmorMitigation(armorReduction)` | `armor + vitality` 经 `(1 - 攻击方减甲率)` 衰减后 → `effArmor / (effArmor + 100)`，上限 0.85 |
| `GetArmorReduction()` | `armorReduction / 100`（攻击方视角的穿甲率） |
| `GetEvasion()` | `evasion + agility × 0.5`，上限 85 |
| `GetElementalResistance(e)` | `对应抗性 + intelligence × 0.5`，上限 75 |

> 主属性（力量/敏捷/智力/体力）不是独立生效的，而是**通过上述公式渗透进各项战斗数值**——这是本工程数值设计的核心思路。

### 6.4 最大生命值的特殊处理

`Entity_Health` **不缓存最大生命值**，每次需要时实时调用 `entityStats.GetMaxHealth()`。因此体力变化会立即反映到血量百分比上，但当前血量不会自动按比例调整。

---

## 7. 战斗与伤害管线

### 7.1 完整伤害流程

```
① 输入：Attack 按下
        │
② Player_GroundedState.Update → stateMachine.ChangeState(basicAttackState)
        │
③ 动画播放到判定帧 → 动画事件 AttackTrigger()
        │
④ Player_AnimationTriggers.AttackTrigger() → Entity_Combat.PerformAttack()
        │
⑤ Physics2D.OverlapCircleAll(targetCheck.position, targetCheckRadius, whatIsTarget)
        │  命中物上取 IDamagable 组件
        │
⑥ stats.GetAttackData(basicAttackScale)
        ├── GetPhysicalDamage()  → 物理伤害 + 是否暴击
        └── GetElementalDamage() → 元素伤害 + 元素类型
        │
⑦ IDamagable.TakeDamage(物理, 元素, 元素类型, attacker.transform)
        │
⑧ Entity_Health.TakeDamage  ← 受击方
        ├── 死亡 / canTakeDamage 为假 → 直接 return false
        ├── AttackEvaded()  闪避判定（Random < GetEvasion()）
        ├── 取攻击方 GetArmorReduction() → 己方 GetArmorMitigation()  ← 物理减伤
        ├── 己方 GetElementalResistance(element)                        ← 元素减伤
        ├── TakeKnockback()  按 damage/maxHealth > 0.3 区分轻重击退
        └── ReduceHp(物理实伤 + 元素实伤)
                │
⑨ HP ≤ 0 → Die() → isDead = true → entity.EntityDeath()
        │
⑩ 玩家 → DeadState（冻结状态机）
   敌人 → Enemy_DeadState；Enemy_Health 额外通知进入 Battle
        │
⑪ 若元素 ≠ None → Entity_StatusHandler.ApplyStatusEffect()（第 ⑥ 步之后由 Entity_Combat 调用）
```

### 7.2 元素状态效果（`Entity_StatusHandler.cs`）

只支持三种，映射关系固定：

| 元素 | 效果 | 结算方式 |
|---|---|---|
| `Ice` | **Chill** 减速 | `duration × (1 - 冰抗)` 秒内 `SlowDownEntity()` |
| `Fire` | **Burn** 持续伤害 | 总伤 `× (1 - 火抗)`，每秒 2 tick 平摊 |
| `Lightning` | **Shock** 充能 | 累积 `charge × (1 - 电抗)`，满值触发落雷并清零 |

`currentEffect` 是**单槽的**：`CanBeApplied()` 仅在当前无状态时返回真（Lightning 例外，可叠加充能）。`RemoveAllNegativeEffects()` 供"净化 wisp"调用。

### 7.3 受击特效（`Entity_VFX.cs`）

基类提供：`PlayOnDamageVfx()`（受击材质闪白 0.2s）、`PlayOnStatusVfs()`（按元素交替闪色）、`CreateOnHitVFX(target, isCrit, element)`（命中特效并按元素染色）。

### 7.4 反击（Counter）

`ICounterable` 接口只有两个成员：`bool CanBeCountered { get; }` 与 `void HandleCounter()`。目前唯一实现者是 `Enemy_Skeleton`（`CanBeCountered => canBeStunned`）。

流程：`Player_Combat.CounterAttackPerformed()` 遍历检测区内的 `ICounterable`，对可反击者调用 `HandleCounter()`，敌人切到 `Stunned`。反击窗口由 `Enemy_AnimationTriggers` 的动画事件 `EnableCounterWindow()` / `DisableCounterWindow()` 开启关闭。

### 7.5 伤害接口

```csharp
public interface IDamagable {
    bool TakeDamage(float damage, float elementalDamage,
                    ElementType element, Transform damageDealer);
}
```

实现者：`Entity_Health`（基类）、`Enemy_Health`（重写）、`Object_Chest`（宝箱也挨打）。
调用者：`Entity_Combat.PerformAttack()`、`SkillObject_Base.DamageEnemiesInRadius()`。

**注意：技能伤害走 `SkillObject_Base`，不走 `Entity_Combat`。**

---

## 8. 技能系统

### 8.1 两层结构

工程把技能拆成**逻辑层**与**实体层**：

| 层 | 基类 | 挂载位置 | 职责 |
|---|---|---|---|
| 逻辑层 | `Skill_Base : MonoBehaviour` | Player 的子物体 | 冷却、解锁状态、生成实体、升级分支 |
| 实体层 | `SkillObject_Base : MonoBehaviour` | 预制体 | 飞行/爆炸/范围伤害、生命周期 |

`Skill_Base` 由 `Player_SkillManager` 通过 `GetComponentInChildren<Skill_Base>()` 收集；技能实体通过 `GetComponentInParent` 反查技能逻辑层。

### 8.2 冷却模型

**时间戳式，不是协程**：

```csharp
bool OnCooldown() => Time.time < lastTimeUsed + cooldown;
void SetSkillOnCooldown() => lastTimeUsed = Time.time;
void ResetCooldown() => lastTimeUsed = Time.time - cooldown;   // 立即就绪
void ReduceCooldownBy(float t) => lastTimeUsed -= t;            // 减少冷却 = 回拨时间戳
```

`Awake` 中 `lastTimeUsed -= cooldown` 实现"初始即就绪"。唯一例外是 `Skill_Shard` 的多发升级，它使用**充能数 + 充能协程**（`currentCharges` / `maxCharges`）。

### 8.3 解锁与升级

解锁由技能树的 `UI_TreeNode.Unlock()` 驱动：

```
UI_TreeNode.Unlock()
  → skillTree.skillManager.GetSkillByType(skillData.skillType)
  → skill.SetSkillUpgrade(skillData.upgradeData)
        → 覆写 upgradeType / cooldown / damageScaleData
```

**`upgradeType == SkillUpgradeType.None` 即视为未解锁**，`Skill_Base.CanUseSkill()` 会直接返回 false。`skillType` 字段仅作分类标签（供 `GetSkillByType` 查找），运行时逻辑只看 `upgradeType`。

### 8.4 五个技能

| 技能 | 类 | 生成物 | 生命周期管理 |
|---|---|---|---|
| 冲刺 | `Skill_Dash` | 无（由升级决定生成残影/碎片） | 依赖 `Enter`/`End` 动画事件调用 `OnStartEffect()` / `OnEndEffect()` |
| 时间碎片 | `Skill_Shard` | `shardPrefab` | 实体内部 `Invoke(Explode)` 自毁；传送类延长至 10s |
| 掷剑 | `Skill_SwordThrow` | 四种剑预制体之一 | **无自毁定时器**，靠再次按键召回（`CanUseSkill()` 检查 `currentSword != null`） |
| 时间残影 | `Skill_TimeEcho` | `timeEchoPrefab` | 实体内部 `Invoke(HandleDeath, duration)` |
| 领域展开 | `Skill_DomainExpansion` | `domainPrefab` | 实体内部 `Invoke(ShrinkDomain, duration)` |

### 8.5 技能实体行为

| 实体 | 行为要点 |
|---|---|
| `SkillObject_Sword` | 速度方向决定朝向；距玩家 > 25 自动召回，`MoveTowards` 接近到 0.5 内销毁；碰撞时 `rb.simulated = false` 并**把剑挂到被撞物体下** |
| `SkillObject_SwordPeirce` | 穿透：每次命中自减 `amountToPierce`，归零或撞地面才停 |
| `SkillObject_SwordBounce` | 首次触发缓存 10 半径内全部敌人并关物理，逐个弹射，`bounceCount` 递减后召回 |
| `SkillObject_SwordSpin` | 按 `attacksPerSecond` 周期 AOE；超时或碰撞即停 |
| `SkillObject_Shard` | 有目标则追踪；`Invoke(Explode)` 到点爆炸并触发 `OnExplode` 事件（供传送升级强制冷却） |
| `SkillObject_TimeEcho` | 按动画事件索引计数攻击；命中后有 `duplicateChance` 概率**再生成一个残影**（可递归）；wisp 型到达玩家处治疗/减 CD/净化 |
| `SkillObject_DomainExpansion` | Lerp 缩放至目标大小；`OnTriggerEnter2D` 只对 `Enemy` 生效，入场减速、出场恢复 |

> ⚠️ `SkillObject_SwordBounce` 继承的 `StopSword()` 会把剑 parent 到敌人身上，敌人移动或销毁时会产生非预期行为。

### 8.6 技能升级树

`SkillUpgradeType` 按技能分为五棵子树，节点名即升级名：

- **Dash**：`Dash` → `Dash_CloneOnStart` / `Dash_ShardOnStart` → `...OnStartAndArrival`
- **Shard**：`Shard` → `Shard_MoveToEnemy` / `Shard_Multicast` / `Shard_Teleport` → `Shard_TeleportHpRewind`
- **SwordThrow**：`SwordThrow` → `SwordThrow_Spin` / `SwordThrow_Pierce` / `SwordThrow_Bounce`
- **TimeEcho**：`TimeEcho` → `TimeEcho_SingleAttack` / `TimeEcho_MultiAttack` / `TimeEcho_ChanceToDuplicate` / `TimeEcho_HealWisp` / `TimeEcho_CleanseWisp` / `TimeEcho_CooldownWisp`
- **Domain**：`Domain_SlowingDown` / `Domain_EchoSpam` / `Domain_ShardSpam`

---

## 9. 物品与背包

### 9.1 对象模型

```
ItemData (ScriptableObject)              ← 静态定义
└── EquipmentData : ItemData             ← 额外带 ItemModifier[] modifiers
        ItemModifier { StatsType statsType, float value }

Inventory_Item ([Serializable])          ← 运行时实例
├── itemData : ItemData
├── stackSize : int
├── itemId : string   ("物品名 - GUID"，用作修饰符 source)
└── modifiers : ItemModifier[]           ← 非装备物品为 null

Inventory_Base (MonoBehaviour)           ← 容器
├── itemList : List<Inventory_Item>
├── maxInventorySize = 10
├── event Action onInventoryChange
└── AddItem / RemoveItem / CanAddItem / FindItem / FindItemCanStack

Inventory_Player : Inventory_Base        ← 玩家背包
├── equipList : List<Inventory_EquipmentSlot>   （Inspector 中配置）
├── playerStats : Entity_Stats
└── TryEquipItem / EquipItem / UnequipItem

Inventory_EquipmentSlot ([Serializable])  ← { ItemType slotType, Inventory_Item equippedItem }
```

### 9.2 装备流程

```
点击 UI_ItemSlot
  → Inventory_Player.TryEquipItem(item)
      → 按 itemData.itemType == slot.slotType 匹配槽位
      → 有空槽则用空槽；否则无条件替换 matchingSlots[0]（先卸下再穿上）
      → EquipItem: 写入槽位 + item.AddModifiers(playerStats) + 从背包 RemoveItem
```

卸下流程相反：先检查 `CanAddItem()`，然后清空槽位 → `item.RemoveModifiers(playerStats)` → 加回背包。

**修饰符的增删靠 `itemId` 字符串配对**，`Stats.RemoveModifier(source)` 会删除该 `itemId` 下的所有修饰符。这意味着**同一个物品实例不能同时装备两次**（否则卸下一次会清空全部）。

### 9.3 已知边界行为

- `RemoveItem` 是**整堆删除**，不递减 `stackSize`，也不做部分移除。
- `AddItem` 只查找"可叠加的已有条目"，**不会填补未满的堆叠**。
- `UnequipItem` 空间不足时只 `Debug.Log("No space")`，没有 UI 反馈。

---

## 10. UI 层

### 10.1 类结构

| 类 | 基类 | 职责 |
|---|---|---|
| `UI` | MonoBehaviour | 根 UI 聚合器，持有 tooltip / skillTree 引用，提供 `ToggleSkillTreeUI()` |
| `UI_ToolTip` | MonoBehaviour | 提示框基类：定位算法 + 隐藏 |
| `UI_ItemToolTip` | `UI_ToolTip` | 物品提示：名称 / 类型 / 词条 |
| `UI_SkillToolTip` | `UI_ToolTip` | 技能提示：名称 / 冷却 / 描述 / 前置需求 |
| `UI_Inventory` | MonoBehaviour | 把背包与装备数据刷到槽位 |
| `UI_ItemSlot` | MonoBehaviour + 三个指针接口 | 背包格，点击装备 |
| `UI_EquipSlot` | `UI_ItemSlot` | 装备格，点击卸下 |
| `UI_SkillTree` | MonoBehaviour | 技能树根：技能点、连线刷新、洗点 |
| `UI_TreeNode` | MonoBehaviour + 三个指针接口 | 技能节点：解锁 / 冲突锁定 / 悬停提示 |
| `UI_TreeConnection` | MonoBehaviour | 单条连线的几何（角度 / 长度 / 取色） |
| `UI_TreeConnectionHandler` | MonoBehaviour | 某节点的出边集合，递归布局 |
| `UI_MiniHealthBar` | MonoBehaviour | 头顶血条的翻转矫正 |

### 10.2 数据刷新机制

**纯事件驱动，不轮询**：

```csharp
// Inventory_Base.cs
public event Action onInventoryChange;
// AddItem / RemoveItem 末尾 Invoke()

// UI_Inventory.Awake
inventory.onInventoryChange += UpdateInventorySlots;
inventory.onInventoryChange += UpdateEquipmentSlots;
// Awake 末尾再手动各调一次做初始刷新
```

**槽位不是运行时生成的**，`UI_Inventory` 用 `GetComponentsInChildren<UI_ItemSlot>()` 收集预先摆好的子物体，然后**按索引**与 `inventory.itemList` / `equipList` 一一对应。

> ⚠️ **背包格子数量必须与 `maxInventorySize` 一致，装备槽数量必须与 `equipList` 长度一致**，否则索引越界。

### 10.3 提示框定位

`UI_ToolTip.UpdatePosition()` 是一个手工算法：

- **水平**：目标 x 超过屏幕中线则向左偏移 `offset.x`(300)，否则向右。
- **垂直**：用 `rect.sizeDelta.y / 2` 夹取上下边界，留 `offset.y`(20) 边距。
- **隐藏**：把 `rect.position` 直接丢到 `(9999, 9999)`（哨兵坐标，而非 `SetActive(false)`）。

### 10.4 技能树

**节点与连线是手工摆放的场景/Prefab 内容，不会从数据自动生成。**

- `UI_SkillTree` 持有序列化的 `parentNodes: UI_TreeConnectionHandler[]`，`Start` 时调 `UpdateAllConnections()` 递归向下刷新。
- 每个 `UI_TreeNode` 上挂 `UI_TreeConnectionHandler`，内含 `connections[]` 与 `connectionDetails[]`（**两者索引必须一一对应**），每条记录含 `childNode` / `direction`（枚举）/ `length` / `rotation`。
- `UpdateConnections()` 会把子节点**自动定位**到父节点的连接点上（`childNode.SetPosition()`），所以子树位置是由数据推导的，根节点位置才是手工摆的。
- `UI_TreeConnection.DirectionConnetion` 按方向枚举查表得出角度（`Up = 90`、`Right = 0`、`DownLeft = -135` 等），再设 `sizeDelta.x = length`。

**节点解锁校验**（`UI_TreeNode.CanBeUnLocked()`）：未锁定 + 未解锁 + 技能点足够 + `neededNodes` 全部已解锁 + `conflictNodes` 均未解锁。解锁后会 `LockConflictNodes()` 并**递归锁定所有子节点**。

`skillPoints` 是普通的序列化字段，**没有存档系统**。

### 10.5 血量条

`Entity_Health.Awake` 通过 `GetComponentInChildren<Slider>()` 取到血条，在 `UpdateHealthBar()` 中直接写 `healthBar.value`。

`UI_MiniHealthBar` 的职责很窄：订阅 `Entity.OnFlipped`，在实体翻转时把 `transform.rotation` 重置为 `identity`，抵消血条被镜像的问题。**它不订阅血量变化，也不处理朝向相机**。

---

## 11. 数据层（ScriptableObject）

| 资产类型 | 菜单路径 | 序列化字段 |
|---|---|---|
| `StatsSetupData` | `RPG Setup/Stats Setup` | `maxHealth, healthRegen, attackSpeed, damage, critChance, critPower, armorReduction, fireDamage, iceDamage, lightningDamage, armor, evasion, fireResistance, iceResistance, lightningResistance, strength, agility, intelligence, vitality` |
| `SkillData` | `RPG Setup/Skill Data` | `displayName, description, icon, cost(int), unlockedByDefault(bool), skillType, upgradeData` |
| `ItemData` | `RPG Setup/Item Data/Regular Item` | `itemName, itemIcon, itemType, maxStackSize = 1` |
| `EquipmentData` | `RPG Setup/Item Data/Equipment Item` | 继承 `ItemData` + `ItemModifier[] modifiers` |

嵌套的可序列化结构：

- **`UpgradeData`**（在 `SkillData` 内）：`upgradeType, cooldown, damageScaleData`
- **`ItemModifier`**（在 `EquipmentData` 内）：`statsType, value`
- **`DamageScaleData`**（`AttackData` 与 `UpgradeData` 共用）：物理/元素倍率，以及 Chill 时长与减速比、Burn 时长与伤害倍率、Shock 时长/伤害/充能
- **`AttackData`**（运行时快照，非资产）：`physicalDamage, elementalDamage, isCrit, elementType, effectData`，构造函数 `AttackData(Entity_Stats, DamageScaleData)`
- **`ElementalEffectData`**（运行时，普通 class）：火/冰/电三种状态的结算参数

### 全局枚举

| 枚举 | 成员 |
|---|---|
| `ElementType` | `None, Fire, Ice, Lightning` |
| `ItemType` | `Material, Weapon, Armor, Trinket` |
| `SkillType` | `Dash, TimeEcho, TimeShard, SwordThrow, DomainExpansion` |
| `StatsType` | 19 项，与 `Stats` 各组的字段一一对应 |
| `SkillUpgradeType` | 见 [8.6 技能升级树](#86-技能升级树) |

---

## 12. 其他子系统

### 12.1 视差背景（`Parallax/`）

`ParallaxBackground`（MonoBehaviour）持有 `ParallaxLayer[]`，`ParallaxLayer` 是**普通可序列化类**（不是组件），配置直接写在 `ParallaxBackground` 的数组里。

- `Awake` 缓存 `Camera.main`，按 `orthographicSize × aspect` 算出相机半宽。
- `LateUpdate` 只取**相机 x 的帧间位移**，逐层调用 `layer.Move(distance)` + `layer.LoopBackground(left, right)`。
- 跟随公式：`position += Vector3.right * (distanceToMove * parallaxMultiplier)`
  - `parallaxMultiplier = 1` → 完全跟随相机（视觉上静止，等于锁定屏幕）
  - `parallaxMultiplier = 0` → 静止于世界
  - 典型远景取 `0.1 ~ 0.5`
- **无限平铺**：当图片右边缘越过相机左边缘时整体 `+imageFullWidth`，反之 `-imageFullWidth`，即**同一张图无条件循环复用**，因此美术资源必须可无缝平铺。

### 12.2 特效（`VFX/VFX_AutoController.cs`）

挂在特效预制体上的通用控制器，三个可开关的行为：

- `canFade`：逐帧把 `SpriteRenderer.color` 的 alpha 按 `fadeSpeed × dt` 减到 0
- `ApplyRandomOffset()`：x/y 随机 ±0.3
- `ApplyRandomRotation()`：z 轴 0–360 随机
- `autoDestory`：`Destroy(gameObject, destoryDelay)`

> ⚠️ 淡出会**整体覆写 SpriteRenderer 的颜色**，因此按元素染色的特效（如 Shard 爆炸）不要开启 `canFade`。

### 12.3 交互物（`InteractiveObjects/`）

| 类 | 触发 | 行为 |
|---|---|---|
| `Object_ItemPickup` | `OnTriggerEnter2D` | 取碰撞体上的 `Inventory_Base`，`CanAddItem()` 为真则加入并销毁自身 |
| `Object_Buff` | `OnTriggerEnter2D` | 上下浮动；取 `Entity_Stats` 后启动协程：加 modifier → 等待时长 → 移除 → 销毁 |
| `Object_Chest` | 实现 `IDamagable` | 受击播放动画（`open` / `broken`）、随机击退，达到 `hitCountLimit` 后关闭 Collider |

> `Object_Chest` **没有任何掉落逻辑**，目前是个纯反馈道具。

---

## 13. 扩展指南

### 添加一个玩家状态

1. 在 `Player/PlayerStates/` 新建 `Player_XxxState : PlayerState`，构造函数传 `(player, stateMachine, "animatorBoolName")`。
2. 在 `Player.cs` 的 `#region State Variables` 声明属性，并在 `Awake` 中 `new` 出来。
3. 在 Animator 上新建同名 bool 参数。
4. 在现有状态的 `Update` 中添加转入条件。

### 添加一个技能

1. 数据层：在 `SkillUpgradeType` 中加节点名，创建对应的 `SkillData` 资产。
2. 逻辑层：写 `Skill_Xxx : Skill_Base`，重写 `CanUseSkill()` / `TryUseSkill()`，按 `upgradeType` 分支不同行为。
3. 实体层：写 `SkillObject_Xxx : SkillObject_Base`，复用 `DamageEnemiesInRadius()` 做范围伤害。
4. 把 `Skill_Xxx` 挂在 Player 的 `SkillManager` 子层级下（`GetComponentInParent` 依赖此层级）。
5. 在 `Player_SkillManager` 中加字段（若需要按类型查找）。
6. 在技能树 Prefab 中摆放节点并连好 `UI_TreeConnectionHandler`。

### 添加一件装备

1. 在 `StatsType` 中确认需要的属性已存在（19 项已覆盖全部 `Stats` 字段）。
2. `Create → RPG Setup → Item Data → Equipment Item`，填 `itemType` 与 `ItemModifier[]`。
3. 若要新增装备槽类型，需同步修改 `ItemType` 枚举，并在 `Inventory_Player.equipList` 中补上对应 `Inventory_EquipmentSlot`，同时保证 UI 装备格数量一致。

### 添加一个敌人

1. 写 `Enemy_Xxx : Enemy`，在 `Awake` 中 `new` 出所需状态，在 `Start` 中 `Initialize`。
2. 在 Inspector 中配置 `whatIsPlayer` / `playerCheck` / `attackDistance` 等参数。
3. 确保所有状态字段（`battleState` / `attackState` / `deadState` / `stunnedState`）都已赋值——**`Enemy_Archer` 就是漏赋值的反面教材**。

### 添加一个元素状态

在 `ElementType` 加枚举 → `Entity_StatusHandler.ApplyStatusEffect` 加分支 → `Entity_VFX` 加颜色映射 → `DamageScaleData` 加结算参数。

---

## 14. 已知问题与技术债

### 14.1 编译期问题

- **`UI/UI_ItemToolTip.cs:5` 引入了 `using static UnityEditor.Rendering.ShadowCascadeGUI;`**（另有未使用的 `using Unity.VisualScripting;`）。`UnityEditor` 命名空间在运行时脚本中不可用，**打包构建会直接编译失败**，需要删除该行。相关工作请优先处理。

### 14.2 逻辑缺陷

| 位置 | 问题 |
|---|---|
| `Entity_Stats.cs:185-186` | `StatsType.FireResistance` 与 `StatsType.LightningResistance` 都返回 `defense.iceResistance`——装备火抗/电抗实际会修改冰抗 |
| `Stats.cs:50` | `SetBaseValue()` 未置 `needToBeReCalculated = true`，若 `GetValue()` 已缓存过，基础值修改不会立即生效 |
| `Object_Buff.cs` | `ApplyBuff(false)` 首行 `if (enable == false) return;` 导致 Buff **永远不会被移除** |
| `UI_ItemToolTip.cs` | `GetStatsNameByType()` 是死代码，`GetItemInfo()` 直接输出 `StatsType.ToString()`，提示里显示的是枚举原名（如 `CritChance` 而非 `Critical Chance`） |
| `Inventory_Item.modifiers` | 非装备物品为 `null`，而 `UI_ItemToolTip.GetItemInfo()` 会直接 `foreach`——存在非 `Material` 也非装备的 `ItemType` 时会空引用 |
| `Enemy_Archer` | 未调用 `stateMachine.Initialize()`，且 `battleState` 等未赋值，检测到玩家会 `ChangeState(null)` 抛空。属未完成分支 |
| `SkillObject_TimeEcho` | `duplicateChance` 触发时在目标旁再生成残影，新残影同样可复制，**理论上会无限连锁** |
| `SkillObject_SwordBounce` | 继承的 `StopSword()` 会把剑 parent 到敌人身上，敌人移动/销毁会产生非预期行为 |
| `Player_DashState` | 受伤免疫与重力归零的还原只在 `Exit` 中执行，异常中断会残留状态 |

### 14.3 结构性风险

- **强依赖动画事件**：`CurrentStateTrigger` / `AttackTrigger` / `ThrowSword` 等方法名硬编码在 Animator 上，缺失会导致 `triggerCalled` 永不为真，攻击状态**软锁死**。
- **硬编码的 Animator 参数名**：`basicAttackIndex`(int)、`jumpAttackFall` 系列、`jumpAttackTrigger`(trigger)、`counterAttackPerformed`、`attackThrowSword_Perform`、`yVelocity`、`attackSpeedMultiplier`、`open` / `broken`。
- **无存档系统**：血量、背包、技能点、已解锁技能全在内存中，退出即丢失。
- **无对象池**：技能实体全部走 `Instantiate` / `Destroy`。
- **全局查找**：`Player.Awake` 中 `FindAnyObjectByType<UI>()`，场景中不可存在多份 UI。
- **`SkillObject_Base.DamageEnemiesInRadius(Transform transform, ...)`** 的参数名遮蔽了 `MonoBehaviour.transform`，可读性差且易误用。
- **拼写错误**：`Peirce`（应为 Pierce）、`autoDestory`、`destoryDelay`、`seconderyWallCheck`、`ReciveKnockback`、`comboxResetTime`——搜索时需注意。

---

## 15. 命名与代码约定

| 前缀 / 后缀 | 含义 | 示例 |
|---|---|---|
| `Entity_*` | 挂在实体上的通用组件 | `Entity_Stats`, `Entity_Health` |
| `Player_*` / `Enemy_*` | 阵营专属的组件或状态 | `Player_Combat`, `Enemy_MoveState` |
| `Skill_*` | 技能逻辑层（挂在 Player 子物体） | `Skill_Dash` |
| `SkillObject_*` | 技能实体层（预制体） | `SkillObject_Sword` |
| `Inventory_*` | 背包相关 | `Inventory_Player` |
| `UI_*` | 界面相关 | `UI_Inventory` |
| `Object_*` | 场景交互物 | `Object_Chest` |
| `*State` | 状态机的状态节点 | `Player_IdleState` |
| `*Handler` | 事件/状态处理器 | `Entity_StatusHandler` |

**数据类命名**：资产类以 `Data` 结尾（`SkillData` / `ItemData`），运行时快照类不带后缀（`AttackData` 是个例外，也以 `Data` 结尾）。

**语言**：代码中的注释以中文为主，标识符全英文。UI 文案为英文。

---

## 参考：脚本数量分布

| 目录 | 文件数 |
|---|---|
| `SkillSystem/` | 16 |
| `UI/` | 12 |
| `Player/` + `PlayerStates/` | 20 |
| `Enemy/` + `EnemyStates/` | 13 |
| `Entity/` | 7 |
| `StatsSystem/` | 5 |
| `Data/` | 7 |
| `ItemSystem/` | 4 |
| `StateMachine/` | 4 |
| `Enum/` | 5 |
| `Interface/` | 2 |
| `InteractiveObjects/` | 3 |
| `Parallax/` / `VFX/` | 2 / 1 |

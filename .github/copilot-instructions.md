# Copilot Instructions

## 项目指南
- 用户是编程初学者，解释代码时应使用非常简单、短句、少术语，并优先用直白类比；用户要求长话短说。
- 用户希望学习和修改代码时一次只讲一个步骤，使用非常慢、简单、清晰的说明，避免一次提供大量信息。
- 用户无法理解大段代码，后续必须一次只解释一个很小的修改，先说明目的，再给最少代码，不要一次替换整段方法。
- 用户希望先整理代码命名，再修改功能；解释时应尊重这个顺序。

## 角色和动画
- 当前用普通僵尸作为舞王伴舞的临时占位符；后续制作真正的伴舞僵尸和步伐动画后，再把召唤 ID 替换成真正伴舞 ID。

## 协作底线（违反过，务必遵守）
- **不要替用户决定行为细节。** 凡是"这个状态要不要停""这个计时器要不要暂停""这个数值取多少"这类可感知的差异，先问，不要自己定。
  - 反例：把舞王的跳舞间隔计时器写成"只在走路时计时"，啃食/受击时暂停。正确做法是**照常计时**。
- **不要从资料里擅自换算数值。** 中文 wiki 的"每行走四步"不等于"四格"；查不到确切数值就直接说"查不到"，不要编一个能自圆其说的数。
- **不要为了迁就视觉效果去改逻辑层的语义属性。** 例如为了"太空步"去改 `FLIP_X`，会连带改变行走方向、啃食判定、索敌方向。
- 发现某处改动会波及用户不想要的东西（例如顶掉关卡原曲）时，先说明再动手，不要"做完再告知"。

## MVZ2 引擎坑（都踩过）
- `FLIP_X` 会被敌人模板的 `mvz2:faction` 行为（`FactionBehaviour`）每帧用 `PROP_FLIP_X` 覆盖，直接 `SetProperty(EngineEntityProps.FLIP_X, ...)` **无效**。要翻转必须用高优先级 modifier（`VanillaModifierPriorities.FORCE`，参考 `TheGiant`、`WickedHermitWarppedBuff`）。
- 但 `FLIP_X` 会改变 `IsFacingLeft()`，进而影响行走方向（`EnemyWalkBehaviour` 用 `GetFacingX()`）、啃食判定、`Detection` 索敌、出界判定。纯粹想要"镜像外观"时应该只动动画层（在 clip 里 K `SpriteRenderer.flipX`），不要动实体属性。
- 敌人移动速度：`UpdateWalkVelocity` 里实际速度 = `GetSpeed() * 0.4`（`VanillaEnemyExt.WALK_SPEED_FACTOR`）。`mvz2:speed` 默认 1（模板 `<enemy>`），要 2 倍就写 2。
- 护甲 `ignored="true"`（如 `pop_captain_cap`）**不吸收伤害**，其 `maxHealth` 是摆设；它靠 `mvz2:destroy_after_half_hp` 在**本体**血量 ≤ 50% 时掉落。只有去掉 `ignored` 才会走护甲减伤。
- 播放音乐必须走 `level.PlayMusic`（全局唯一音轨，会顶掉关卡原曲）。要叠加且不干扰原曲，用循环音效：`sounds.xml` 里加 `loopVolume`，代码用 `level.AddLoopSoundEntity(id, entity.ID)` / `RemoveLoopSoundEntity`。
- `enemy_state` 可以通过 XML 的 `<behaviour operator="replace" source="mvz2:enemy_state" id="...">` 替换来改状态机，参考 `wicked_hermit_zombie/state`。
- `EntityMeta` 的 `<behaviours includeSelf="false">` 会**注销该实体自身的 AI 类**（`[AutoEntityBehaviourDefinition]` 挂不上）。要在 `<behaviours>` 里加自己的行为，必须 `includeSelf="true"`。

## Context
`football_zombie` 当前撞到目标时会立刻造成高额伤害并直接进入近战，已有的 `football_zombie_impact` 动画没有接入状态机，两把剑也默认同时显示。本次把流程改成：碰撞即停下并播放冲撞动画，第 6 个逻辑帧结束才真正摧毁当时撞到的器械，动画完整结束后永久切换为手持剑。

## 实施步骤
1. **增加冲撞阶段状态与计时**
   - 在 `VanillaEnemyStates.cs` 增加 Football 专用 impact 状态。
   - 在 `FootballZombie.cs` 增加冲撞目标 `EntityID`、6-tick 命中计时和动画结束计时的行为字段；复用撑杆跳的 `FrameTimer` 与 EntityID 缓存模式。
   - 进入冲撞阶段时立刻清除水平移动速度、结束冲刺倍率并缓存当前碰撞目标，避免第 6 tick 时误伤后来切换的 melee target。

2. **第 6 个逻辑帧结束结算**
   - 在 `FootballZombie_States.cs` 新增 `ImpactState`，进入时重置计时器，期间保持停止且不被普通 WALK/MELEE 状态覆盖。
   - 精确推进 6 次逻辑更新后结算一次；若缓存目标仍存在、未死亡且属于器械目标，则通过正常死亡链摧毁，避免 `Remove()` 绕过死亡回调。
   - 目标提前消失或死亡时静默跳过；Football 自身死亡时由死亡状态优先接管，绝不补结算第二次。

3. **接入 impact 动画**
   - 在 `FootballZombie_AnimationBehaviour.cs` 将新增逻辑状态映射为私有 AnimationState。
   - 整理 `football_zombie.controller` 中会抢占状态的无条件 Any State 过渡，建立 impact 的明确进入/退出条件，并保留死亡状态最高优先级。
   - 将 `football_zombie_impact.anim` 改为非循环；按片段的实际结束时间同步逻辑状态结束计时，使动画完整播完后才离开 Impact。

4. **动画结束后切剑**
   - 在 `football_zombie.prefab` 的 `Body/SwordInBack` 与 `Body/InnerArm/Lower/SwordInHand` 上复用项目现有的 `ModelPropertyGameObjectActivatorBoolean` 显隐模式。
   - 初始化为背剑显示、手中剑隐藏；Impact 动画结束时由逻辑设置模型属性，永久变为背剑隐藏、手中剑显示。
   - 不用 Animation Event 执行战斗逻辑，避免动画事件与实体目标状态脱节。

5. **验证**
   - 检查改动 diff，确认未覆盖工作区中其他未提交内容。
   - 编译 Unity C# 工程并报告真实结果。
   - 如 Unity CLI 可用，在编辑器中核查 Controller、clip 循环设置和 prefab 两个显隐组件；最后给出构建版实测步骤：生成 Football、撞普通器械、确认前 5 tick 器械仍在、第 6 tick 后死亡、Impact 期间不移动、动画结束才切换两把剑并恢复行为。
#nullable enable

using MVZ2.GameContent.Damages;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Definitions;
using PVZEngine.Entities;

namespace MVZ2.GameContent.Enemies
{
    /// <summary>
    /// 地上撑杆（撑杆跳僵尸跳跃后留下的杆）：
    /// - 不被子弹/啃咬等普通伤害摧毁（伤害归零）；
    /// - 单次受到的爆炸伤害超过阈值时被摧毁；
    /// - 存在满 maxTimeout 后经 timeout_death 行为自动消失；
    /// - 被拾取时由撑杆跳僵尸写入 claimed 标记，保证同一根杆同一时刻只被一只僵尸吸走。
    /// </summary>
    [AutoEntityBehaviourDefinition(VanillaEnemyNames.pole)]
    public class PoleEntity : EnemyBehaviour
    {
        public PoleEntity(string nsp, string name) : base(nsp, name)
        {
        }

        public override void PreTakeDamage(DamageInput input, CallbackResult<bool> result)
        {
            base.PreTakeDamage(input, result);
            var entity = input.Entity;
            if (input.HasEffect(VanillaDamageEffects.EXPLOSION))
            {
                if (input.Amount > DESTROY_THRESHOLD)
                {
                    // 单次爆炸伤害超过阈值：直接摧毁
                    input.SetAmount(entity.GetMaxHealth() * 10);
                }
                else
                {
                    // 小型爆炸炸不毁
                    input.SetAmount(0);
                }
            }
            else
            {
                // 其余伤害免疫
                input.SetAmount(0);
            }
        }

        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            // 被摧毁/到期后立即消失，不留尸骸
            entity.Remove();
        }

        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            // 拾取冷却递减：刚落地的杆短时间内不能被任何撑杆僵尸吸走（防止丢杆者同帧吸回）
            var cooldown = entity.GetProperty<int>(PROP_PICKUP_COOLDOWN);
            if (cooldown > 0)
            {
                entity.SetProperty(PROP_PICKUP_COOLDOWN, cooldown - 1);
            }
        }

        public const float DESTROY_THRESHOLD = 600;
        public static readonly VanillaEntityPropertyMeta<int> PROP_PICKUP_COOLDOWN = new VanillaEntityPropertyMeta<int>("pickup_cooldown");
    }
}

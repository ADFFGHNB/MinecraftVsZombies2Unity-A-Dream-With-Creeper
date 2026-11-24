#nullable enable

using System.Collections.Generic;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Entities;
using PVZEngine;
using PVZEngine.Buffs;
using PVZEngine.Definitions;
using PVZEngine.Entities;
using PVZEngine.Models;
using UnityEngine;

namespace MVZ2.GameContent.Enemies
{
    [AutoEntityBehaviourDefinition(VanillaEnemyNames.tsCreeper)]
    public class TsCreeper : Creeper
    {
        public TsCreeper(string nsp, string name) : base(nsp, name)
        {
        }

        public override void Init(Entity entity)
        {
            base.Init(entity);
            UpdateReleasedProperty(entity);
        }

        protected override void UpdateAI(Entity entity)
        {
            // 每帧同步解放外观属性
            UpdateReleasedProperty(entity);
            base.UpdateAI(entity);
        }

        // 蓄力完成：释放真化脉冲，不造成直接伤害
        protected override void OnChargeCompleted(Entity entity)
        {
            var released = entity.HasBuff<TrueNameReleaseBuff>();
            var radiusGrids = released ? EFFECT_RADIUS_GRIDS_RELEASED : EFFECT_RADIUS_GRIDS;
            var range = entity.Level.GetGridWidth() * radiusGrids;
            var center = entity.GetCenter();

            var targets = new List<Entity>();
            var detector = new SphereDetector(range)
            {
                factionTarget = FactionTarget.Any,
                mask = EntityCollisionHelper.MASK_PLANT | EntityCollisionHelper.MASK_ENEMY,
                canDetectInvisible = true,
                includeOverlapDisabled = true
            };
            detector.DetectEntities(entity, targets);

            foreach (var detectedTarget in targets)
            {
                var target = detectedTarget;
                if (target.Type == EntityTypes.PLANT)
                {
                    ContraptionInvertedBuff.Apply(target, INVERT_SECONDS);
                    continue;
                }

                if (target.Type != EntityTypes.ENEMY || !target.HasBehaviour<Creeper>())
                    continue;

                // 未解放的真化苦力怕：获得解放，但不叠加高压
                if (target.HasBehaviour<TsCreeper>() && !target.HasBuff<TrueNameReleaseBuff>())
                {
                    target.AddBuff<TrueNameReleaseBuff>();
                    continue;
                }

                // 其余苦力怕（含已解放的真化苦力怕）：获得高压
                if (!target.HasBuff<CreeperChargedBuff>())
                {
                    target.AddBuff<CreeperChargedBuff>();
                }
            }

            Explosion.Spawn(entity, center, range);
            entity.PlaySound(VanillaSoundID.explosion);
            entity.Remove();
        }

        private static void UpdateReleasedProperty(Entity entity)
        {
            entity.SetModelProperty(PROP_RELEASED_MODEL, entity.HasBuff<TrueNameReleaseBuff>());
        }

        public const float EFFECT_RADIUS_GRIDS = 1.5f;
        public const float EFFECT_RADIUS_GRIDS_RELEASED = 2f;
        public const float INVERT_SECONDS = 10f;
        public const string PROP_RELEASED_MODEL = "TrueNameReleased";
    }
}

#nullable enable

using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Enemies;
using MVZ2.GameContent.Models;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Models;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Entities;
using MVZ2Logic.Grids;
using MVZ2Logic.Level;
using MVZ2Logic.Models;
using PVZEngine;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using UnityEngine;
using PVZEngine.Definitions;
using PVZEngine.Entities;
using PVZEngine.Grids;
using PVZEngine.Level;
using PVZEngine.Timing;
using PVZEngine.Tools.Timing;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [AutoBuffDefinition(VanillaBuffNames.Contraption.inverted)]
    public class ContraptionInvertedBuff : BuffDefinition
    {
        public ContraptionInvertedBuff(string nsp, string name) : base(nsp, name)
        {
            AddModelInsertion(
                LogicModelHelper.ANCHOR_CENTER,
                VanillaModelKeys.staticParticles,
                VanillaModelID.staticParticles
            );
            AddTriggerFiltered(
                LevelCallbacks.POST_ENTITY_DEATH,
                PostEntityDeathCallback,
                EntityTypes.PLANT,
                VanillaCallbackPriorities.LATE
            );
        }

        public override void OnCreate(Buff buff)
        {
            base.OnCreate(buff);
            SetTimer(buff, TimerHelper.NewSecondTimer(DURATION_SECONDS));
            SetResolved(buff, false);
        }

        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var timer = GetTimer(buff);
            if (timer.RunToExpiredOrNull())
            {
                buff.Remove();
            }
        }

        private void PostEntityDeathCallback(LevelCallbacks.EntityDeathParams param, CallbackResultVoid result)
        {
            var entity = param.entity;
            var buffs = entity.GetBuffs(this);
            bool shouldResolve = false;

            foreach (var buff in buffs)
            {
                if (IsResolved(buff))
                    continue;
                SetResolved(buff, true);
                shouldResolve = true;
            }

            if (!shouldResolve)
                return;

            Resolve(entity);
        }

        private static void Resolve(Entity entity)
        {
            var level = entity.Level;
            var definition = entity.Definition;
            var position = entity.Position;
            var center = entity.GetCenter();
            var faction = entity.GetFaction();
            var column = entity.GetColumn();
            var lane = entity.GetLane();
            var invertedUpgrade = definition.GetInvertedUpgrade();
            var range = level.GetGridWidth() * EXPLOSION_RADIUS_GRIDS;
            var effects = new DamageEffectList(
                VanillaDamageEffects.EXPLOSION,
                VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN,
                VanillaDamageEffects.MUTE
            );

            entity.ExplodeAgainstFriendly(center, range, faction, EXPLOSION_DAMAGE, effects);
            Explosion.Spawn(entity, center, range);
            entity.PlaySound(VanillaSoundID.explosion);

            if (entity.Exists())
            {
                entity.Remove();
            }

            if (NamespaceID.IsValid(invertedUpgrade))
            {
                var upgradeDef = level.Content.GetEntityDefinition(invertedUpgrade!);
                if (upgradeDef != null)
                {
                    // 进化体直接替换原器械：X/Z 对齐其所在格，Y 保留原高度
                    // （空中被铲则进化体从空中原位出现），清空动量后自由落体
                    var offset = upgradeDef.GetStartingPositionOffset() - upgradeDef.GetGridPivotOffset();
                    var spawnPosition = new Vector3(
                        level.GetEntityColumnX(column) + offset.x,
                        position.y,
                        level.GetEntityLaneZ(lane) + offset.z
                    );

                    var spawnParams = new SpawnParams();
                    spawnParams.SetProperty(EngineEntityProps.FACTION, faction);
                    spawnParams.SetProperty(EngineEntityProps.GRAVITY, 1f);
                    var spawned = level.Spawn(invertedUpgrade!, spawnPosition, entity, spawnParams);
                    if (spawned != null)
                    {
                        // 落地时砸毁落点格上的冲突器械（弹射器同款）
                        spawned.DestroyConflictGridEntitiesOnLand();
                        return;
                    }
                }
            }

            var enemyParams = new SpawnParams();
            enemyParams.SetProperty(EngineEntityProps.FACTION, level.GetRightFaction());
            level.Spawn(VanillaEnemyID.creeper, position, entity, enemyParams);
        }

        public static void Apply(Entity entity, float durationSeconds = DURATION_SECONDS)
        {
            var buff = entity.GetFirstBuff<ContraptionInvertedBuff>();
            if (buff == null)
            {
                buff = entity.AddBuff<ContraptionInvertedBuff>();
            }
            SetTimer(buff, TimerHelper.NewSecondTimer(durationSeconds));
            SetResolved(buff, false);
        }

        private static FrameTimer? GetTimer(Buff buff)
        {
            return buff.GetProperty<FrameTimer>(PROP_TIMER);
        }

        private static void SetTimer(Buff buff, FrameTimer? value)
        {
            buff.SetProperty(PROP_TIMER, value);
        }

        private static bool IsResolved(Buff buff)
        {
            return buff.GetProperty<bool>(PROP_RESOLVED);
        }

        private static void SetResolved(Buff buff, bool value)
        {
            buff.SetProperty(PROP_RESOLVED, value);
        }

        public static readonly VanillaBuffPropertyMeta<FrameTimer> PROP_TIMER =
            new VanillaBuffPropertyMeta<FrameTimer>("timer");

        public static readonly VanillaBuffPropertyMeta<bool> PROP_RESOLVED =
            new VanillaBuffPropertyMeta<bool>("resolved");

        public const float DURATION_SECONDS = 10;
        public const float EXPLOSION_RADIUS_GRIDS = 1.5f;
        public const float EXPLOSION_DAMAGE = 150;
    }
}

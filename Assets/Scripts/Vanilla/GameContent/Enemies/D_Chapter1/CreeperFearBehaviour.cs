#nullable enable

using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Definitions;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Enemies
{
    [AutoEntityBehaviourDefinition("creeper_fear")]
    public class CreeperFearBehaviour : EntityBehaviourDefinition
    {
        public CreeperFearBehaviour(string nsp, string name)
            : base(nsp, name)
        {
        }

        private static readonly VanillaEntityPropertyMeta<int> FIELD_BACK_STATE =
            new VanillaEntityPropertyMeta<int>("CreeperBackState");

        private static readonly VanillaEntityPropertyMeta<bool> FIELD_IS_FEARING =
            new VanillaEntityPropertyMeta<bool>("CreeperIsFearing");

        private static readonly VanillaEntityPropertyMeta<int> FIELD_FEAR_TIME =
            new VanillaEntityPropertyMeta<int>("CreeperFearTime");

        private static readonly VanillaEntityPropertyMeta<int> FIELD_FEAR_COOLDOWN =
            new VanillaEntityPropertyMeta<int>("CreeperFearCooldown");

        private static readonly VanillaEntityPropertyMeta<float> FIELD_FEAR_DIRECTION =
            new VanillaEntityPropertyMeta<float>("CreeperFearDirection");

        private static readonly Detector catDetector = new SameRowDetector();

        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetBackState(entity, 0);
            SetFearing(entity, false);
            SetFearTime(entity, 0);
            SetFearCooldown(entity, 0);
        }

        public override void Update(Entity entity)
        {
            base.Update(entity);

            if (entity.IsDead)
                return;

            if (IsFearing(entity))
            {
                int fearTime = GetFearTime(entity) - 1;
                SetFearTime(entity, fearTime);

                if (fearTime <= 0)
                {
                    StopFear(entity);
                    return;
                }

                var cat = FindCat(entity);
                if (cat != null)
                {
                    SetFearDirection(entity, GetAwayDirection(entity, cat));
                }
                UpdateFearVelocity(entity);
                return;
            }

            int cooldown = GetFearCooldown(entity);
            if (cooldown > 0)
            {
                cooldown--;
                SetFearCooldown(entity, cooldown);
                if (cooldown > 0)
                    return;
            }

            var detectedCat = FindCat(entity);
            if (detectedCat != null)
            {
                StartFear(entity, detectedCat);
            }
        }

        private static Entity? FindCat(Entity entity)
        {
            return catDetector.DetectEntityWithTheLeast(
                entity,
                (self, target) => Mathf.Abs(self.Position.x - target.Position.x)
            );
        }

        private static void StartFear(Entity entity, Entity cat)
        {
            SetFearing(entity, true);
            SetFearTime(entity, FEAR_DURATION);
            SetFearDirection(entity, GetAwayDirection(entity, cat));
            UpdateFearVelocity(entity);
        }

        private static void StopFear(Entity entity)
        {
            SetFearing(entity, false);
            SetFearTime(entity, 0);
            SetFearCooldown(entity, FEAR_COOLDOWN);
            SetBackState(entity, 0);

            var velocity = entity.Velocity;
            velocity.x = entity.GetSpeed() * VanillaEnemyExt.WALK_SPEED_FACTOR * entity.GetFacingX();
            entity.Velocity = velocity;
        }

        private static float GetAwayDirection(Entity entity, Entity cat)
        {
            float catOffset = cat.Position.x - entity.Position.x;
            if (catOffset == 0)
                return -entity.GetFacingX();
            return catOffset > 0 ? -1 : 1;
        }

        private static void UpdateFearVelocity(Entity entity)
        {
            float direction = GetFearDirection(entity);
            float facing = entity.GetFacingX();

            SetBackState(entity, facing * direction < 0 ? 1 : 0);

            var velocity = entity.Velocity;
            velocity.x = entity.GetSpeed() * VanillaEnemyExt.WALK_SPEED_FACTOR * 3 * direction;
            entity.Velocity = velocity;
        }

        public static int GetBackState(Entity entity)
        {
            return entity.GetBehaviourField<int>(FIELD_BACK_STATE);
        }

        public static void SetBackState(Entity entity, int value)
        {
            entity.SetBehaviourField(FIELD_BACK_STATE, value);
        }

        public static bool IsFearing(Entity entity)
        {
            return entity.GetBehaviourField<bool>(FIELD_IS_FEARING);
        }

        public static void SetFearing(Entity entity, bool value)
        {
            entity.SetBehaviourField(FIELD_IS_FEARING, value);
        }

        private static int GetFearTime(Entity entity)
        {
            return entity.GetBehaviourField<int>(FIELD_FEAR_TIME);
        }

        private static void SetFearTime(Entity entity, int value)
        {
            entity.SetBehaviourField(FIELD_FEAR_TIME, value);
        }

        private static int GetFearCooldown(Entity entity)
        {
            return entity.GetBehaviourField<int>(FIELD_FEAR_COOLDOWN);
        }

        private static void SetFearCooldown(Entity entity, int value)
        {
            entity.SetBehaviourField(FIELD_FEAR_COOLDOWN, value);
        }

        private static float GetFearDirection(Entity entity)
        {
            return entity.GetBehaviourField<float>(FIELD_FEAR_DIRECTION);
        }

        private static void SetFearDirection(Entity entity, float value)
        {
            entity.SetBehaviourField(FIELD_FEAR_DIRECTION, value);
        }

        private class SameRowDetector : Detector
        {
            public SameRowDetector()
            {
                factionTarget = FactionTarget.Any;
            }

            protected override Bounds GetDetectionBounds(Entity self)
            {
                return new Bounds(
                    self.Position,
                    new Vector3(10000f, 10000f, 80f)
                );
            }

            public override bool ValidateTarget(DetectionParams self, Entity target)
            {
                return base.ValidateTarget(self, target)
                    && target.IsEntityOf(VanillaEnemyID.zombieCat)
                    && Detection.IsInSameRow(self.entity, target);
            }
        }

        public const int FEAR_DURATION = 90;
        public const int FEAR_COOLDOWN = 90;
    }
}
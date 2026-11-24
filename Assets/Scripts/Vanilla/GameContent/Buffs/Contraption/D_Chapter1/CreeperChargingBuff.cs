#nullable enable

using MVZ2.Vanilla.Properties;

using PVZEngine.Buffs;
using PVZEngine.Definitions;
using PVZEngine.Entities;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [AutoBuffDefinition(VanillaBuffNames.Contraption.creeperCharging)]
    public class CreeperChargingBuff : BuffDefinition
    {
        private const int FLASH_INTERVAL = 5;

        public CreeperChargingBuff(string nsp, string name)
            : base(nsp, name)
        {
            AddModifier(
                new ColorModifier(
                    EngineEntityProps.COLOR_OFFSET,
                    PROP_COLOR
                )
            );
        }

        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);

            buff.SetProperty(PROP_TIME, 0);
            UpdateColor(buff);
        }

        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);

            var time = buff.GetProperty<int>(PROP_TIME);

            time++;
            buff.SetProperty(PROP_TIME, time);

            UpdateColor(buff);
        }

        private void UpdateColor(Buff buff)
        {
            var time = buff.GetProperty<int>(PROP_TIME);

            bool flash = (time / FLASH_INTERVAL) % 2 == 0;

            float intensity = flash ? 1f : 0f;

            buff.SetProperty(
                PROP_COLOR,
                new Color(
                    intensity,
                    intensity,
                    intensity,
                    1f
                )
            );
        }

        public static readonly VanillaBuffPropertyMeta<int>
            PROP_TIME = new VanillaBuffPropertyMeta<int>("Time");

        public static readonly VanillaBuffPropertyMeta<Color>
            PROP_COLOR = new VanillaBuffPropertyMeta<Color>("Color");
    }
}

#nullable enable

using PVZEngine.Buffs;
using PVZEngine.Definitions;
using PVZEngine.Entities;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [AutoBuffDefinition("creeperFear")]
    public class CreeperFearBuff : BuffDefinition
    {
        public CreeperFearBuff(string nsp, string name) : base(nsp, name)
        {
            // 反转实体朝向
            AddModifier(
                new BooleanModifier(EngineEntityProps.FLIP_X, true)
            );
        }
    }
}
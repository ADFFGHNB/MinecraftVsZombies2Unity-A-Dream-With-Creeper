#nullable enable

using PVZEngine.Buffs;
using PVZEngine.Definitions;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [AutoBuffDefinition(VanillaBuffNames.Enemy.trueNameRelease)]
    public class TrueNameReleaseBuff : BuffDefinition
    {
        public TrueNameReleaseBuff(string nsp, string name) : base(nsp, name)
        {
        }
    }
}

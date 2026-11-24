#nullable enable

using MVZ2.Vanilla.Entities;
using PVZEngine.Definitions;
using PVZEngine.Entities;

namespace MVZ2.GameContent.Enemies
{
    [AutoEntityBehaviourDefinition("creeper_animation")]
    public class CreeperAnimationBehaviour : HumanoidAnimationBehaviour
    {
        public CreeperAnimationBehaviour(string nsp, string name)
            : base(nsp, name)
        {
        }

        protected override void ModifyAnimationParameters(
            Entity entity,
            int state,
            ref AnimationParameters parameters)
        {
            base.ModifyAnimationParameters(entity, state, ref parameters);

            parameters.specialState =
                CreeperFearBehaviour.GetBackState(entity);
        }
    }
}
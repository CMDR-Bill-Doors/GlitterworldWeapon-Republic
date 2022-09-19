using Verse;
using RimWorld;

namespace BDsPlasmaWeapon
{
    public class CompUseEffect_LizionPopper : CompUseEffect
    {
        CompProximityLizionPopper compProximityLizionPopper;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            compProximityLizionPopper = parent.TryGetComp<CompProximityLizionPopper>();
        }

        public override void DoEffect(Pawn usedBy)
        {
            compProximityLizionPopper.trigger();
        }

        public override bool CanBeUsedBy(Pawn p, out string failReason)
        {
            if (compProximityLizionPopper == null || !compProximityLizionPopper.isAvailableForPop)
            {
                failReason = "unavailable".Translate();
                return false;
            }
            return base.CanBeUsedBy(p, out failReason);
        }
    }
}

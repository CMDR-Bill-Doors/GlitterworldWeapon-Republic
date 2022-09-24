using Verse;
using RimWorld;

namespace BDsPlasmaWeapon
{
    public class CompUseEffect_ReplacementAccelerator : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            ThingWithComps weapon = usedBy.equipment.Primary;
            weapon.HitPoints = (int)(weapon.MaxHitPoints * 0.75);
            parent.Destroy();
        }

        public override bool CanBeUsedBy(Pawn p, out string failReason)
        {
            ThingWithComps weapon = p.equipment.Primary;
            if (weapon == null)
            {
                failReason = "BDP_RepairFailNoWeapon".Translate();
                return false;
            }
            if ((weapon.HitPoints / (float)weapon.MaxHitPoints) > 0.75)
            {
                failReason = "BDP_RepairFailHitpoint".Translate();
                return false;
            }
            if (weapon.TryGetComp<CompCasingReturn>() == null)
            {
                failReason = "BDP_RepairFailWrongWeapon".Translate();
                return false;
            }
            return base.CanBeUsedBy(p, out failReason);
        }
    }
}

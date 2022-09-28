using Verse;
using RimWorld;

namespace BDsPlasmaWeaponVanilla
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
            if (weapon.TryGetComp<CompTankFeedWeapon>() == null)
            {
                failReason = "BDP_RepairFailWrongWeapon".Translate();
                return false;
            }
            return base.CanBeUsedBy(p, out failReason);
        }
    }
}

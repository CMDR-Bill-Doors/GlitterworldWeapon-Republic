using Verse;
using CombatExtended;
using RimWorld;
using System.Collections.Generic;

namespace BDsPlasmaWeapon
{
    public class Verb_ShootWithCasingReturn : Verb_ShootCE
    {

        public CompCasingReturn CompCasing
        {
            get
            {
                return EquipmentSource.TryGetComp<CompCasingReturn>();
            }
        }

        public CompTankFeedWeapon compTankSwitch
        {
            get
            {
                return EquipmentSource.TryGetComp<CompTankFeedWeapon>();
            }
        }

        public CompReloadableFromFiller compTank;

        public override CompAmmoUser CompAmmo
        {
            get
            {
                if (compTankSwitch != null && compTankSwitch.isOn)
                {
                    compAmmo = null;
                    return null;
                }
                else
                {
                    if (compAmmo == null && EquipmentSource != null)
                    {
                        compAmmo = EquipmentSource.TryGetComp<CompAmmoUser>();
                    }
                    return compAmmo;
                }
            }
        }

        public override bool Available()
        {
            if (compTankSwitch != null && compTankSwitch.compReloadableFromFiller != null && compTankSwitch.isOn)
            {
                return true;
            }
            return base.Available();
        }

        public override bool TryCastShot()
        {
            if (compTankSwitch != null && compTankSwitch.compReloadableFromFiller != null && compTankSwitch.isOn)
            {
                compTank = compTankSwitch.compReloadableFromFiller;
                if (compTank.remainingCharges < VerbPropsCE.ammoConsumedPerShotCount)
                {
                    compTankSwitch.searchTank(base.VerbPropsCE.ammoConsumedPerShotCount);
                    compTank = compTankSwitch.compReloadableFromFiller;
                }
                if (base.TryCastShot())
                {
                    compTank.DrawGas(VerbPropsCE.ammoConsumedPerShotCount);
                    return true;
                }
            }
            else
            {
                if (base.TryCastShot())
                {
                    if (CompCasing != null && CompAmmo != null && compAmmo.CurrentAmmo != AmmoDefOf.Ammo_LizionCellOvercharged)
                    {
                        if (CasterIsPawn && ShooterPawn.Faction == Faction.OfPlayer)
                        {
                            CompCasing.DropCasing(ShooterPawn);
                        }
                        else
                        {
                            CompCasing.DropCasing(Caster.Position, Caster.Map);
                        }
                    }
                    return true;
                }
            }
            return false;
        }
    }
}

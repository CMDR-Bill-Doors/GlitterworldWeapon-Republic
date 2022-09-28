﻿using Verse;
using RimWorld;
using PipeSystem;
using System.Runtime.Remoting.Messaging;
using Verse.AI;

namespace BDsPlasmaWeaponVanilla
{
    public class Verb_ShootFromBackpackTank : Verb_Shoot
    {

        public CompTankFeedWeapon compTankFeedWeapon => EquipmentSource.TryGetComp<CompTankFeedWeapon>();

        private CompSecondaryVerb compSecondaryVerb => EquipmentSource.TryGetComp<CompSecondaryVerb>();

        private bool isOvercharged => compSecondaryVerb != null && compSecondaryVerb.IsSecondaryVerbSelected;

        public CompReloadableFromFiller compTank
        {
            get
            {
                CompReloadableFromFiller comp = EquipmentSource.TryGetComp<CompReloadableFromFiller>();
                if (comp != null && (compTankFeedWeapon != null && comp.remainingCharges > compTankFeedWeapon.Props.ammoConsumption))
                {
                    return comp;
                }
                else if (compTankFeedWeapon != null)
                {
                    return compTankFeedWeapon.compReloadableFromFiller;
                }
                return null;
            }
        }

        private int ammoConsumption => isOvercharged ? compTankFeedWeapon.Props.ammoConsumption * 2 : compTankFeedWeapon.Props.ammoConsumption;

        public override bool Available()
        {
            if (base.Available())
            {
                if (CasterIsPawn && CasterPawn.Faction != Faction.OfPlayer)
                {
                    return true;
                }
                if (compTank == null)
                {
                    return false;
                }
                if (compTank.remainingCharges < ammoConsumption)
                {
                    compTankFeedWeapon?.searchTank(ammoConsumption, false);
                    return false;
                }
                int storedGas = compTank.remainingCharges;
                return storedGas >= ammoConsumption;
            }
            return false;
        }

        protected override bool TryCastShot()
        {
            Log.Message(ammoConsumption.ToString());
            if (base.TryCastShot())
            {
                if (!(CasterIsPawn && CasterPawn.Faction != Faction.OfPlayer) && (compTank == null || compTank.remainingCharges < ammoConsumption))
                {
                    compTankFeedWeapon?.searchTank();
                    return false;
                }
                if (isOvercharged)
                {
                    if (Caster is Building turret)
                    {
                        compTankFeedWeapon.OverchargedDamage(turret);
                    }
                    else
                    {
                        compTankFeedWeapon.OverchargedDamage(EquipmentSource); Log.Message("1");
                    }
                }
                compTank.DrawGas(ammoConsumption);
                return true;
            }
            return false;
        }
    }
}

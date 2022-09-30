using Verse;
using RimWorld;
using PipeSystem;
using System.Runtime.Remoting.Messaging;
using Verse.AI;
using static HarmonyLib.Code;
using System;
using System.Security.Cryptography;

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
                if (comp != null && (compTankFeedWeapon != null && comp.remainingCharges >= compTankFeedWeapon.Props.ammoConsumption))
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
                if (compTank == null || compTankFeedWeapon == null)
                {
                    return false;
                }
                if (compTank.remainingCharges < ammoConsumption)
                {
                    return compTankFeedWeapon.searchTank(ammoConsumption, false);
                }
                return true;
            }
            return false;
        }

        protected override bool TryCastShot()
        {
            if (base.TryCastShot())
            {
                if (!(CasterIsPawn && CasterPawn.Faction != Faction.OfPlayer) && (compTank == null || compTank.remainingCharges < ammoConsumption))
                {
                    compTankFeedWeapon?.searchTank();
                    return false;
                }
                if (isOvercharged && BDPMod.OverchargeDamageWeapon)
                {
                    if (Caster is Building turret)
                    {
                        compTankFeedWeapon.OverchargedDamage(turret);
                    }
                    else
                    {
                        compTankFeedWeapon.OverchargedDamage(EquipmentSource);
                    }
                }
                compTank.DrawGas(ammoConsumption);
                return true;
            }
            return false;
        }
    }


    public class Verb_ShootOverchargeDamage : Verb_Shoot
    {

        public DefModExtension_VerbOverchargeDamage Data
        {
            get
            {
                return EquipmentSource.def.GetModExtension<DefModExtension_VerbOverchargeDamage>();
            }
        }

        private CompSecondaryVerb compSecondaryVerb => EquipmentSource.TryGetComp<CompSecondaryVerb>();

        private bool isOvercharged => compSecondaryVerb != null && compSecondaryVerb.IsSecondaryVerbSelected;

        protected override bool TryCastShot()
        {
            if (base.TryCastShot())
            {
                if (Data != null && isOvercharged && BDPMod.OverchargeDamageWeapon)
                {
                    if (Caster is Building turret)
                    {
                        OverchargedDamage(turret);
                    }
                    else
                    {
                        OverchargedDamage(EquipmentSource);
                    }
                }
                return true;
            }
            return false;
        }

        public void OverchargedDamage(ThingWithComps weapon)
        {
            if (Rand.Chance(Data.overchargeDamageChance))
            {
                float HPcache = (float)weapon.HitPoints / weapon.MaxHitPoints;
                weapon.HitPoints -= (int)Math.Round(Rand.Value * Data.overchargeDamageMultiplier);
                float HPnow = (float)weapon.HitPoints / weapon.MaxHitPoints;
                if (EquipmentSource.ParentHolder is Pawn pawn && pawn.Faction == Faction.OfPlayer)
                {
                    if (HPcache > 0.5 && HPnow <= 0.5)
                    {
                        Messages.Message(string.Format("BDP_WeaponFailingPawn".Translate(), pawn, EquipmentSource.LabelCap), EquipmentSource, MessageTypeDefOf.RejectInput, historical: false);
                    }
                    else if (HPcache > 0.25 && HPnow <= 0.25)
                    {
                        Messages.Message(string.Format("BDP_WeaponFailingUrgentPawn".Translate(), pawn, EquipmentSource.LabelCap), EquipmentSource, MessageTypeDefOf.ThreatSmall, historical: false);
                    }
                }
                else
                {
                    if (HPcache > 0.5 && HPnow <= 0.5)
                    {
                        Messages.Message(string.Format("BDP_WeaponFailing".Translate(), EquipmentSource.LabelCap), EquipmentSource, MessageTypeDefOf.RejectInput, historical: false);
                    }
                    else if (HPcache > 0.25 && HPnow <= 0.25)
                    {
                        Messages.Message(string.Format("BDP_WeaponFailingUrgent".Translate(), EquipmentSource.LabelCap), EquipmentSource, MessageTypeDefOf.ThreatSmall, historical: false);
                    }
                }
            }
        }
    }

    public class DefModExtension_VerbOverchargeDamage : DefModExtension
    {
        public float overchargeDamageChance = 0;
        public float overchargeDamageMultiplier = 1;
    }
}

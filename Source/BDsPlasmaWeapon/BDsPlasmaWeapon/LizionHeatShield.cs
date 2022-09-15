using System.Collections.Generic;
using PipeSystem;
using RimWorld;
using UnityEngine;
using Verse;
using System;
using static HarmonyLib.Code;
using System.Drawing;

namespace BDsPlasmaWeapon
{
    public class LizionHeatShield : Apparel
    {
        private Vector3 impactAngleVect;

        private float efficiency => this.GetStatValue(StatDefOf.BDP_LizionHeatShieldEfficiency);

        private float hiccupChance => this.GetStatValue(StatDefOf.BDP_LizionHeatShieldHiccupChance);

        protected CompReloadableFromFiller compReloadableFromFiller
        {
            get
            {
                return GetComp<CompReloadableFromFiller>();
            }
        }

        protected CompLizionHeatShieldDataInterface compGizmo
        {
            get
            {
                return GetComp<CompLizionHeatShieldDataInterface>();
            }
        }

        public FloatRange hiccupDamageMultiplierRange = new FloatRange(0, 0.5f);

        private bool currentMode;

        System.Random random = new System.Random();

        public override void PostPostMake()
        {
            base.PostPostMake();
            currentMode = true;
            if (compReloadableFromFiller == null)
            {
                Log.Warning("LizionHeatShield is meant to be used in conjunction with CompReloadableFromFiller!");
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                currentMode = true;
            }
            if (compReloadableFromFiller == null)
            {
                Log.Warning("LizionHeatShield is meant to be used in conjunction with CompReloadableFromFiller!");
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentMode, "currentMode", true);
        }

        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            foreach (Gizmo wornGizmo in base.GetWornGizmos())
            {
                yield return wornGizmo;
            }
            if (Wearer == null)
            {
                yield break;
            }
            if (compReloadableFromFiller == null)
            {
                yield break;
            }
            if (compGizmo == null)
            {
                yield break;
            }
            if (Wearer.Faction.Equals(Faction.OfPlayer))
            {
                string commandIcon = currentMode ? compGizmo.Props.onIcon : compGizmo.Props.offIcon;

                if (commandIcon == "")
                {
                    commandIcon = "UI/Buttons/Reload";
                }

                Command_Action switchSecondaryLauncher = new Command_Action
                {
                    action = new Action(toggle),
                    defaultLabel = currentMode ? compGizmo.Props.onLabel : compGizmo.Props.offLabel,
                    defaultDesc = compGizmo.Props.description,
                    icon = ContentFinder<Texture2D>.Get(commandIcon, false),
                };
                yield return switchSecondaryLauncher;
            }
        }

        public void toggle()
        {
            currentMode = !currentMode;
        }

        public override void Tick()
        {
            base.Tick();
            Fire fire = (Fire)Wearer.GetAttachment(RimWorld.ThingDefOf.Fire);
            if (fire != null && currentMode && compReloadableFromFiller != null)
            {
                compReloadableFromFiller.DrawGas(Math.Max(fire.fireSize / efficiency, 1));
                fire.Destroy();
            }
        }

        public override void TickRare()
        {
            base.TickRare(); base.TickRare();
            Fire fire = (Fire)Wearer.GetAttachment(RimWorld.ThingDefOf.Fire);
            if (fire != null && currentMode && compReloadableFromFiller != null)
            {
                compReloadableFromFiller.DrawGas(Math.Max(fire.fireSize / efficiency, 1));
                fire.Destroy();
            }
        }

        public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
        {
            Log.Message(dinfo.ToString());
            if (currentMode && dinfo.Def.armorCategory == DamageArmorCategoryDefOf.Heat && compReloadableFromFiller != null)
            {
                float damageCache = dinfo.Amount;
                float equivalentDamageAbsorbtion = compReloadableFromFiller.remainingCharges * efficiency;
                if (equivalentDamageAbsorbtion > 0)
                {
                    if (equivalentDamageAbsorbtion >= damageCache)
                    {
                        compReloadableFromFiller.DrawGas(Math.Max(damageCache / efficiency, 1));
                        if (hiccupChance >= 1 || (hiccupChance > 0 && random.NextDouble() < hiccupChance))
                        {
                            dinfo.SetAmount(damageCache * hiccupDamageMultiplierRange.RandomInRange);
                            return false;
                        }
                        return true;

                    }
                    else
                    {
                        dinfo.SetAmount(damageCache - (compReloadableFromFiller.remainingCharges * efficiency));
                        compReloadableFromFiller.Empty();
                    }
                }
            }
            return false;
        }
    }

    public class CompLizionHeatShieldDataInterface : ThingComp
    {
        public CompProperties_LizionHeatShieldDataInterface Props
        {
            get
            {
                return (CompProperties_LizionHeatShieldDataInterface)props;
            }
        }
    }


    public class CompProperties_LizionHeatShieldDataInterface : CompProperties
    {
        public string onIcon = "UI/Commands/DesirePower";
        public string onLabel = "shield on";
        public string offIcon = "UI/Commands/DesirePower";
        public string offLabel = "shield off";
        public string description = "";

        public CompProperties_LizionHeatShieldDataInterface()
        {
            compClass = typeof(CompLizionHeatShieldDataInterface);
        }
    }
}

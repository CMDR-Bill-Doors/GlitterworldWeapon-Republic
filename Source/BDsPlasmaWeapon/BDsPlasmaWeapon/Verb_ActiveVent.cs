using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace BDsPlasmaWeapon
{
    public class Verb_ActiveVent : Verb
    {
        public new CompReloadableFromFiller ReloadableCompSource => DirectOwner as CompReloadableFromFiller;

        protected override bool TryCastShot()
        {
            Pop(ReloadableCompSource);
            return true;
        }

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            float radius = ReloadableCompSource.compActiveVentData.Props.radius;
            int consumption = ReloadableCompSource.compActiveVentData.Props.maxConsumption;
            if (ReloadableCompSource.remainingCharges < consumption)
            {
                radius = radius * ((float)ReloadableCompSource.remainingCharges / (float)consumption);
            }
            return radius;
        }

        public override void DrawHighlight(LocalTargetInfo target)
        {
            DrawHighlightFieldRadiusAroundTarget(caster);
        }

        public void Pop(CompReloadableFromFiller comp)
        {
            if (comp != null && comp.CanBeUsed && ReloadableCompSource.compActiveVentData != null)
            {
                float radius = ReloadableCompSource.compActiveVentData.Props.radius;
                int consumption = ReloadableCompSource.compActiveVentData.Props.maxConsumption;
                if (comp.remainingCharges < consumption)
                {
                    radius = radius * ((float)comp.remainingCharges / (float)consumption);
                    consumption = comp.remainingCharges;
                }
                Pawn wearer = comp.Wearer;
                GenExplosion.DoExplosion(wearer.Position, wearer.Map, radius, RimWorld.DamageDefOf.Extinguish, null, -1, -1f, null, null, null, null, RimWorld.ThingDefOf.Gas_Smoke, 1f);
                comp.DrawGas(consumption);
            }
        }
    }

    public class CompActiveVentDataInterface : ThingComp
    {
        public CompProperties_ActiveVentDataInterface Props
        {
            get
            {
                return (CompProperties_ActiveVentDataInterface)props;
            }
        }
    }


    public class CompProperties_ActiveVentDataInterface : CompProperties
    {
        public string Icon = "UI/Commands/DesirePower";
        public string Label = "shield on";
        public string description = "";
        public float radius = 5;
        public int maxConsumption = 100;
        public float heatPushPerUnit = -1;

        public CompProperties_ActiveVentDataInterface()
        {
            compClass = typeof(CompActiveVentDataInterface);
        }
    }
}

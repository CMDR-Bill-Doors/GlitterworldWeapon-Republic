using UnityEngine;
using RimWorld;
using Verse;

namespace BDsPlasmaWeapon
{
    public class Verb_GasJump : Verb_Jump
    {
        public new CompReloadableFromFiller ReloadableCompSource => DirectOwner as CompReloadableFromFiller;

        public override bool MultiSelect => true;
        protected override float EffectiveRange
        {
            get
            {
                float radius = ReloadableCompSource.compGasJumpData.Props.radius;
                int consumption = ReloadableCompSource.compGasJumpData.Props.maxConsumption;
                if (ReloadableCompSource.remainingCharges < consumption)
                {
                    radius = radius * ((float)ReloadableCompSource.remainingCharges / (float)consumption);
                }
                return radius;
            }
        }

        protected override bool TryCastShot()
        {
            if (!ModLister.CheckRoyalty("Jumping"))
            {
                return false;
            }
            if (ReloadableCompSource == null || !ReloadableCompSource.CanBeUsed || ReloadableCompSource.compGasJumpData == null)
            {
                return false;
            }
            Pawn casterPawn = CasterPawn;
            if (casterPawn == null || ReloadableCompSource == null || !ReloadableCompSource.CanBeUsed)
            {
                return false;
            }
            IntVec3 cell = currentTarget.Cell;
            Map map = casterPawn.Map;
            GenExplosion.DoExplosion(casterPawn.Position, casterPawn.Map, ReloadableCompSource.compGasJumpData.Props.BlastCloudRadius, RimWorld.DamageDefOf.Extinguish, null, -1, -1f, null, null, null, null, RimWorld.ThingDefOf.Gas_Smoke, 1f);
            ReloadableCompSource.DrawGas(ReloadableCompSource.compGasJumpData.Props.maxConsumption);
            PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(RimWorld.ThingDefOf.PawnJumper, casterPawn, cell);
            if (pawnFlyer != null)
            {
                GenSpawn.Spawn(pawnFlyer, cell, map);
                return true;
            }
            return false;
        }
        public override void DrawHighlight(LocalTargetInfo target)
        {
            if (target.IsValid && ValidJumpTarget(caster.Map, target.Cell))
            {
                GenDraw.DrawTargetHighlightWithLayer(target.CenterVector3, AltitudeLayer.MetaOverlays);
            }
            GenDraw.DrawRadiusRing(caster.Position, EffectiveRange, Color.white, (IntVec3 c) => GenSight.LineOfSight(caster.Position, c, caster.Map) && ValidJumpTarget(caster.Map, c));
        }
    }

    public class CompGasJumpDataInterface : ThingComp
    {
        public CompProperties_GasJumpDataInterface Props
        {
            get
            {
                return (CompProperties_GasJumpDataInterface)props;
            }
        }
    }


    public class CompProperties_GasJumpDataInterface : CompProperties
    {
        public string Icon = "UI/Commands/DesirePower";
        public string Label = "shield on";
        public string description = "";
        public float radius = 5;
        public int maxConsumption = 100;
        public float BlastCloudRadius = 2;

        public CompProperties_GasJumpDataInterface()
        {
            compClass = typeof(CompGasJumpDataInterface);
        }
    }
}

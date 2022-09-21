using UnityEngine;
using RimWorld;
using Verse;
using System.Security.Cryptography;

namespace BDsPlasmaWeapon
{
    public class Verb_GasJump : Verb_Jump
    {
        public DefModExtension_GasJump Data
        {
            get
            {
                return ReloadableCompSource.parent.def.GetModExtension<DefModExtension_GasJump>();
            }
        }

        public new CompReloadableFromFiller ReloadableCompSource => DirectOwner as CompReloadableFromFiller;

        public override bool MultiSelect => true;
        protected override float EffectiveRange
        {
            get
            {
                float radius = Data.radius;
                int consumption = Data.maxConsumption;
                if (ReloadableCompSource.remainingCharges < consumption)
                {
                    radius *= ReloadableCompSource.remainingCharges / (float)consumption;
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
            GenExplosion.DoExplosion(casterPawn.Position, casterPawn.Map, Data.blastCloudRadius, RimWorld.DamageDefOf.Extinguish, null, -1, -1f, null, null, null, null, RimWorld.ThingDefOf.Gas_Smoke, 1f);
            ReloadableCompSource.DrawGas(Data.maxConsumption);
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

    public class DefModExtension_GasJump : DefModExtension
    {
        public string icon = "UI/Commands/DesirePower";
        public string label = "BDP_GasJumpLabel";
        public string description = "BDP_GasJumpDesc";
        public float radius = 5;
        public int maxConsumption = 100;
        public float blastCloudRadius = 2;
    }
}

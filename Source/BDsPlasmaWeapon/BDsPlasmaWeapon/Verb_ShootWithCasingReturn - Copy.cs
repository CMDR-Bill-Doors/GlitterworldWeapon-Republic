using Verse;
using CombatExtended;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;
using Verse.Sound;

namespace BDsPlasmaWeapon
{
    public class Verb_ShootWithCasingReturnALT : Verb_LaunchProjectileCE
    {
        private const int AimTicksMin = 30;

        private const int AimTicksMax = 240;

        private const float PawnXp = 20f;

        private const float HostileXp = 170f;

        private const float SuppressionSwayFactor = 1.5f;

        private bool _isAiming;

        public bool isBipodGun => (base.EquipmentSource?.TryGetComp<BipodComp>() ?? null) != null;

        public override int ShotsPerBurst
        {
            get
            {
                return (CompFireModes != null) ? ShotsPerBurstFor(CompFireModes.CurrentFireMode) : base.VerbPropsCE.burstShotCount;
            }
        }

        private bool ShouldAim
        {
            get
            {
                if (CompFireModes != null)
                {
                    if (base.ShooterPawn != null)
                    {
                        if (base.ShooterPawn.CurJob != null && base.ShooterPawn.CurJob.def == RimWorld.JobDefOf.Hunt)
                        {
                            return true;
                        }
                        if (IsSuppressed)
                        {
                            return false;
                        }
                        Pawn_PathFollower pather = base.ShooterPawn.pather;
                        if (pather != null && pather.Moving)
                        {
                            return false;
                        }
                    }
                    return CompFireModes.CurrentAimMode == AimMode.AimedShot;
                }
                return false;
            }
        }

        public override float SwayAmplitude
        {
            get
            {
                float swayAmplitude = base.SwayAmplitude;
                float b = base.SightsEfficiency;
                if (base.ShooterPawn != null && !base.ShooterPawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
                {
                    b = 0f;
                }
                if (ShouldAim)
                {
                    return swayAmplitude * Mathf.Max(0f, 1f - base.AimingAccuracy) / Mathf.Max(1f, b);
                }
                if (IsSuppressed)
                {
                    return swayAmplitude * 1.5f;
                }
                return swayAmplitude;
            }
        }

        public float SpreadDegrees => (base.EquipmentSource?.GetStatValue(StatDef.Named("ShotSpread")) ?? 0f) * ((base.projectilePropsCE != null) ? base.projectilePropsCE.spreadMult : 0f);

        private bool IsSuppressed => (base.ShooterPawn?.TryGetComp<CompSuppressable>()?.isSuppressed).GetValueOrDefault();

        public float SwayAmplitudeFor(AimMode mode)
        {
            float swayAmplitude = base.SwayAmplitude;
            float b = base.SightsEfficiency;
            if (base.ShooterPawn != null && !base.ShooterPawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
            {
                b = 0f;
            }
            if (ShouldAimFor(mode))
            {
                return swayAmplitude * Mathf.Max(0f, 1f - base.AimingAccuracy) / Mathf.Max(1f, b);
            }
            if (IsSuppressed)
            {
                return swayAmplitude * 1.5f;
            }
            return swayAmplitude;
        }

        public bool ShouldAimFor(AimMode mode)
        {
            if (base.ShooterPawn != null)
            {
                if (base.ShooterPawn.CurJob != null && base.ShooterPawn.CurJob.def == RimWorld.JobDefOf.Hunt)
                {
                    return true;
                }
                if (IsSuppressed)
                {
                    return false;
                }
                Pawn_PathFollower pather = base.ShooterPawn.pather;
                if (pather != null && pather.Moving)
                {
                    return false;
                }
            }
            return mode == AimMode.AimedShot;
        }

        public virtual int ShotsPerBurstFor(FireMode mode)
        {
            if (CompFireModes != null)
            {
                switch (mode)
                {
                    case FireMode.SingleFire:
                        return 1;
                    case FireMode.BurstFire:
                        if (CompFireModes.Props.aimedBurstShotCount > 0)
                        {
                            return CompFireModes.Props.aimedBurstShotCount;
                        }
                        break;
                }
            }
            float num = base.VerbPropsCE.burstShotCount;
            if (base.EquipmentSource != null)
            {
                float statValue = base.EquipmentSource.GetStatValue(CE_StatDefOf.BurstShotCount);
                if (statValue > 0f)
                {
                    num = statValue;
                }
            }
            return (int)num;
        }

        public override void WarmupComplete()
        {
            float lengthHorizontal = (currentTarget.Cell - caster.Position).LengthHorizontal;
            int num = (int)Mathf.Lerp(30f, 240f, lengthHorizontal / 100f);
            if (ShouldAim && !_isAiming)
            {
                if (caster is Building_TurretGunCE building_TurretGunCE)
                {
                    building_TurretGunCE.burstWarmupTicksLeft += num;
                    _isAiming = true;
                    return;
                }
                if (base.ShooterPawn != null)
                {
                    base.ShooterPawn.stances.SetStance(new Stance_Warmup(num, currentTarget, this));
                    _isAiming = true;
                    return;
                }
            }
            base.WarmupComplete();
            _isAiming = false;
            if (base.ShooterPawn?.skills != null && currentTarget.Thing is Pawn)
            {
                float num2 = verbProps.AdjustedFullCycleTime(this, base.ShooterPawn);
                num2 += num.TicksToSeconds();
                float num3 = (currentTarget.Thing.HostileTo(base.ShooterPawn) ? 170f : 20f);
                num3 *= num2;
                base.ShooterPawn.skills.Learn(SkillDefOf.Shooting, num3);
            }
        }

        public override void VerbTickCE()
        {
            if (_isAiming)
            {
                if (!ShouldAim)
                {
                    WarmupComplete();
                }
                if (!(caster is Building_TurretGunCE) && base.ShooterPawn?.stances?.curStance?.GetType() != typeof(Stance_Warmup))
                {
                    _isAiming = false;
                }
            }
            if (isBipodGun && Controller.settings.BipodMechanics)
            {
                base.EquipmentSource.TryGetComp<BipodComp>().SetUpStart(CasterPawn);
            }
        }

        public virtual ShiftVecReport SimulateShiftVecReportFor(LocalTargetInfo target, AimMode aimMode)
        {
            IntVec3 cell = target.Cell;
            ShiftVecReport shiftVecReport = new ShiftVecReport();
            shiftVecReport.target = target;
            shiftVecReport.aimingAccuracy = base.AimingAccuracy;
            shiftVecReport.sightsEfficiency = base.SightsEfficiency;
            if (base.ShooterPawn != null && !base.ShooterPawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
            {
                shiftVecReport.sightsEfficiency = 0f;
            }
            shiftVecReport.shotDist = (cell - caster.Position).LengthHorizontal;
            shiftVecReport.maxRange = EffectiveRange;
            if (!caster.Position.Roofed(caster.Map) || !cell.Roofed(caster.Map))
            {
                shiftVecReport.weatherShift = 1f - caster.Map.weatherManager.CurWeatherAccuracyMultiplier;
            }
            shiftVecReport.shotSpeed = base.ShotSpeed;
            shiftVecReport.swayDegrees = SwayAmplitudeFor(aimMode);
            float num = ((base.projectilePropsCE != null) ? base.projectilePropsCE.spreadMult : 0f);
            shiftVecReport.spreadDegrees = (base.EquipmentSource?.GetStatValue(StatDef.Named("ShotSpread")) ?? 0f) * num;
            return shiftVecReport;
        }

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            if (base.ShooterPawn != null && !base.ShooterPawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
            {
                if (!base.ShooterPawn.health.capacities.CapableOf(PawnCapacityDefOf.Hearing))
                {
                    return false;
                }
                float num = targ.Cell.DistanceTo(root);
                if (num < 5f)
                {
                    return base.CanHitTargetFrom(root, targ);
                }
                Map map = base.ShooterPawn.Map;
            }
            return base.CanHitTargetFrom(root, targ);
        }






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



        public override bool TryCastShot()
        {
            if (compTankSwitch.isOn)
            {
                compTank = compTankSwitch.compReloadableFromFiller;

                int gasConsumptionMultiplier = 1;

                if (compAmmo.CurrentAmmo != AmmoDefOf.Ammo_LizionCellNormal && compAmmo.CurrentAmmo != AmmoDefOf.Ammo_LizionCellOvercharged)
                {
                    compAmmo.SelectedAmmo = AmmoDefOf.Ammo_LizionCellNormal;
                    compAmmo.TryStartReload();
                    return false;
                }
                if (compAmmo.CurrentAmmo != AmmoDefOf.Ammo_LizionCellOvercharged)
                {
                    gasConsumptionMultiplier = 2;
                }
                if (compTank.remainingCharges < VerbPropsCE.ammoConsumedPerShotCount * gasConsumptionMultiplier)
                {
                    compTankSwitch.searchTank(base.VerbPropsCE.ammoConsumedPerShotCount * gasConsumptionMultiplier);
                    compTank = compTankSwitch.compReloadableFromFiller;
                }


                if (!Retarget())
                {
                    return false;
                }
                if (base.TryCastShot())
                {
                    if (base.ShooterPawn != null)
                    {
                        base.ShooterPawn.records.Increment(RecordDefOf.ShotsFired);
                    }
                    if (base.VerbPropsCE.muzzleFlashScale > 0.01f)
                    {
                        FleckMakerCE.Static(caster.Position, caster.Map, FleckDefOf.ShotFlash, base.VerbPropsCE.muzzleFlashScale);
                    }
                    if (base.VerbPropsCE.soundCast != null)
                    {
                        base.VerbPropsCE.soundCast.PlayOneShot(new TargetInfo(caster.Position, caster.Map));
                    }
                    if (base.VerbPropsCE.soundCastTail != null)
                    {
                        base.VerbPropsCE.soundCastTail.PlayOneShotOnCamera();
                    }
                    if (base.ShooterPawn != null && base.ShooterPawn.thinker != null)
                    {
                        base.ShooterPawn.mindState.lastEngageTargetTick = Find.TickManager.TicksGame;
                    }
                    compTank.DrawGas(VerbPropsCE.ammoConsumedPerShotCount * gasConsumptionMultiplier);
                    return true;
                }
                return false;
            }
            else
            {
                if (!Retarget())
                {
                    return false;
                }
                if (CompAmmo != null && !CompAmmo.TryReduceAmmoCount(base.VerbPropsCE.ammoConsumedPerShotCount))
                {
                    return false;
                }
                if (base.TryCastShot())
                {
                    if (base.ShooterPawn != null)
                    {
                        base.ShooterPawn.records.Increment(RecordDefOf.ShotsFired);
                    }
                    if (CompAmmo != null && !CompAmmo.HasMagazine && CompAmmo.UseAmmo)
                    {
                        if (!CompAmmo.Notify_ShotFired())
                        {
                            if (base.VerbPropsCE.muzzleFlashScale > 0.01f)
                            {
                                FleckMakerCE.Static(caster.Position, caster.Map, FleckDefOf.ShotFlash, base.VerbPropsCE.muzzleFlashScale);
                            }
                            if (base.VerbPropsCE.soundCast != null)
                            {
                                base.VerbPropsCE.soundCast.PlayOneShot(new TargetInfo(caster.Position, caster.Map));
                            }
                            if (base.VerbPropsCE.soundCastTail != null)
                            {
                                base.VerbPropsCE.soundCastTail.PlayOneShotOnCamera();
                            }
                            if (base.ShooterPawn != null && base.ShooterPawn.thinker != null)
                            {
                                base.ShooterPawn.mindState.lastEngageTargetTick = Find.TickManager.TicksGame;
                            }
                            if (CompCasing != null)
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
                        }
                        return CompAmmo.Notify_PostShotFired();
                    }
                    return true;
                }
                return false;
            }
        }
    }
}

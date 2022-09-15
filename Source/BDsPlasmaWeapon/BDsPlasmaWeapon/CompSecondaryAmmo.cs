using System;
using System.Collections.Generic;
using Verse;
using UnityEngine;
using CombatExtended;
using Verse.AI;
using RimWorld;

namespace BDsPlasmaWeapon
{
    public class CompSecondaryAmmo : CompRangedGizmoGiver
    {
        public CompProperties_SecondaryAmmo Props
        {
            get
            {
                return (CompProperties_SecondaryAmmo)this.props;
            }
        }

        public Verb_LaunchProjectileCE AttackVerb
        {
            get
            {
                if (this.CompEquippable.PrimaryVerb == null)
                {
                    return null;
                }

                Verb_LaunchProjectileCE verb = this.CompEquippable.PrimaryVerb as Verb_LaunchProjectileCE;
                return verb;
            }
        }


        //----------------------------- Comp cache ------------------

        //get current CompAmmoUser
        public CompAmmoUser CompAmmo
        {
            get
            {
                if (this.compAmmo == null && base.parent != null)
                {
                    this.compAmmo = base.parent.TryGetComp<CompAmmoUser>();
                }
                return this.compAmmo;
            }
        }

        public CompEquippable CompEquippable
        {
            get
            {
                if (this.compEquippable == null && base.parent != null)
                {
                    this.compEquippable = base.parent.TryGetComp<CompEquippable>();
                }
                if (this.compEquippable == null)
                {
                    Log.Error("Can not get CompEquippable.");
                }
                return compEquippable;
            }
        }

        public CompInventory CompInventory
        {
            get
            {
                if (this.compInventory == null && this.Holder != null)
                {
                    this.compInventory = this.Holder.TryGetComp<CompInventory>();
                }
                if (this.compInventory == null)
                {
                    Log.Error("Can not get CompInventory.");
                }
                return this.compInventory;
            }
        }

        //Pawn cache
        public Pawn Holder
        {
            get
            {
                if (CompEquippable == null
                    || CompEquippable.PrimaryVerb == null
                    || CompEquippable.PrimaryVerb.caster == null
                    || ((CompEquippable?.parent?.ParentHolder as Pawn_InventoryTracker)?.pawn is Pawn holderPawn && holderPawn != CompEquippable?.PrimaryVerb?.CasterPawn))
                {
                    return null;
                }
                return CompEquippable.PrimaryVerb.CasterPawn ?? (CompEquippable.parent.ParentHolder as Pawn_InventoryTracker)?.pawn;
            }
        }

        //------------------------ CompProperties_AmmoUser Cache -----------------------

        public CompProperties_AmmoUser OriginAmmoSetData
        {
            get
            {
                if (this.originAmmoProps == null)
                {
                    Log.Error("CompProperties_AmmoUser 'originAmmoProps' not initialized");
                }
                return this.originAmmoProps;
            }
        }

        public CompProperties_AmmoUser SecondaryAmmoSetData
        {
            get
            {
                if (this.secondaryAmmoProps == null)
                {
                    Log.Error("CompProperties_AmmoUser 'secondaryAmmoProps' not initialized");
                }
                return this.secondaryAmmoProps;
            }
        }

        //---------------- public status-------------------

        public bool IsSecondaryAmmoSelected
        {
            get
            {
                return useSecondaryAmmo;
            }
        }

        public bool IsSharedAmmo
        {
            get
            {
                return isSharedAmmo;
            }
        }

        public bool HasAmmoUser
        {
            get
            {
                return CompAmmo != null;
            }
        }

        public string MainAmmoName
        {
            get
            {
                return this.OriginAmmoSetData.ammoSet.label;
            }
        }

        public string SecondaryAmmoName
        {
            get
            {
                return this.SecondaryAmmoSetData.ammoSet.label;
            }
        }

        public int ScondaryMagSize
        {
            get
            {
                return IsSecondaryAmmoSelected ? OriginAmmoSetData.magazineSize : SecondaryAmmoSetData.magazineSize;

            }
        }

        public AmmoData MainAmmoData
        {
            get
            {
                if (mainAmmo == null)
                {
                    this.mainAmmo = new AmmoData(0, this.CompAmmo.Props.ammoSet.ammoTypes[0].ammo);
                }
                return this.mainAmmo;
            }
        }

        public AmmoData SecondaryAmmoData
        {
            get
            {
                if (secondaryAmmo == null)
                {
                    this.secondaryAmmo = new AmmoData(0, this.Props.secondaryAmmoProps.ammoSet.ammoTypes[0].ammo);
                }
                return this.secondaryAmmo;
            }
        }

        public AmmoDef CurrentSecondaryAmmo
        {
            get
            {
                return IsSecondaryAmmoSelected ? MainAmmoData.selectedAmmo : SecondaryAmmoData.selectedAmmo;
            }
        }

        public int ScondaryMagCount
        {
            get
            {
                return IsSecondaryAmmoSelected ? MainAmmoData.curMagCount : SecondaryAmmoData.curMagCount;

            }
        }

        //----------------- private vars -----------------

        private CompAmmoUser compAmmo;
        private CompEquippable compEquippable;
        private CompInventory compInventory;

        private AmmoData mainAmmo;//exposed
        private AmmoData secondaryAmmo;//exposed

        private CompProperties_AmmoUser originAmmoProps = null;
        private CompProperties_AmmoUser secondaryAmmoProps = null;

        private VerbProperties mainVerbProps;
        private VerbProperties secondaryVerbProps;

        private bool useSecondaryAmmo;
        private bool isSharedAmmo;

        private CompCharges secondaryAmmoCharges = null;

        //----------------------- Methods ---------------------------------

        private void SwitchLauncher()
        {
            if (!useSecondaryAmmo)
            {
                SaveAmmo(MainAmmoData);
                LoadAmmo(SecondaryAmmoData);
                LoadAmmoProps(SecondaryAmmoSetData);
                this.CompEquippable.PrimaryVerb.verbProps = secondaryVerbProps;
                useSecondaryAmmo = true;
            }
            else
            {
                SaveAmmo(SecondaryAmmoData);
                LoadAmmo(MainAmmoData);
                LoadAmmoProps(OriginAmmoSetData);
                this.CompEquippable.PrimaryVerb.verbProps = mainVerbProps;
                useSecondaryAmmo = false;
            }

            if (compAmmo.CurMagCount == 0 && compAmmo.IsEquippedGun)
            {
                TryReload();
            }

            if (this.Holder != null)
            {
                CompInventory.UpdateInventory();
            }

            return;
        }

        private void LoadAmmo(AmmoData data)
        {
            if (!HasAmmoUser || isSharedAmmo)
            {
                return;
            }
            this.CompAmmo.ResetAmmoCount(data.selectedAmmo);
            this.CompAmmo.CurMagCount = data.curMagCount;
            this.CompAmmo.SelectedAmmo = data.selectedAmmo;
        }

        private void SaveAmmo(AmmoData data)
        {
            if (!HasAmmoUser)
            {
                return;
            }
            data.curMagCount = this.CompAmmo.CurMagCount;
            data.selectedAmmo = this.CompAmmo.SelectedAmmo;
        }

        private void LoadAmmoProps(CompProperties_AmmoUser ammoProps)
        {
            if (!HasAmmoUser)
            {
                return;
            }
            this.CompAmmo.props = ammoProps;
        }


        public override void PostExposeData()
        {
            base.PostExposeData();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                this.PreSaveData();
            }

            Scribe_Values.Look<bool>(ref this.useSecondaryAmmo, "CE_useSecondaryAmmo", false);
            Scribe_Deep.Look<AmmoData>(ref this.mainAmmo, "CE_mainAmmoData", null);
            Scribe_Deep.Look<AmmoData>(ref this.secondaryAmmo, "CE_secondaryAmmoProps", null);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.PostAmmoDataLoaded();
            }
        }

        private void PreSaveData()
        {
            bool check = this.CompAmmo.Props == this.secondaryAmmoProps;

            if (!check)
            {
                this.MainAmmoData.curMagCount = this.CompAmmo.CurMagCount;
                this.MainAmmoData.selectedAmmo = this.CompAmmo.SelectedAmmo;

                return;
            }

            this.SecondaryAmmoData.curMagCount = this.CompAmmo.CurMagCount;
            this.SecondaryAmmoData.selectedAmmo = this.CompAmmo.SelectedAmmo;

            return;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (this.Props.secondaryAmmoProps == null)
            {
                Log.Error("Prop secondaryAmmoProps not initialized");
            }

            if (this.Props.secondaryVerb == null)
            {
                Log.Error("Prop secondaryVerb not initialized");
            }

            InitData();
            Log.Message(IsSharedAmmo.ToString());
        }

        private void PostAmmoDataLoaded()
        {

            InitData();

            if (useSecondaryAmmo)
            {
                LoadAmmo(this.SecondaryAmmoData);
                LoadAmmoProps(this.secondaryAmmoProps);
                this.CompEquippable.PrimaryVerb.verbProps = secondaryVerbProps;
            }
        }

        public void InitData()
        {
            mainVerbProps = this.CompEquippable.PrimaryVerb.verbProps;
            secondaryVerbProps = this.Props.secondaryVerb;

            if (this.secondaryAmmoCharges == null && this.Props.secondaryWeaponChargeSpeeds != null)
            {
                CompProperties_Charges chargesProps = new CompProperties_Charges();
                chargesProps.chargeSpeeds = this.Props.secondaryWeaponChargeSpeeds;
                this.secondaryAmmoCharges = new CompCharges();
                this.secondaryAmmoCharges.props = chargesProps;
            }

            if (HasAmmoUser)
            {
                if (this.originAmmoProps == null)
                {
                    this.originAmmoProps = this.CompAmmo.Props;
                }

                if (this.secondaryAmmoProps == null)
                {
                    this.secondaryAmmoProps = this.Props.secondaryAmmoProps;
                }
            }

            //Command Icon

            if (this.Props.mainCommandIcon == "")
            {
                this.Props.mainCommandIcon = "UI/Buttons/Reload";
            }

            if (this.Props.secondaryCommandIcon == "")
            {
                this.Props.secondaryCommandIcon = "UI/Buttons/Reload";
            }

            //Is shared ammo

            CompProperties_AmmoUser compProps = null;
            HashSet<AmmoDef> ammoCheck = new HashSet<AmmoDef>();
            isSharedAmmo = true;

            foreach (var comp in this.parent.def.comps)
            {
                compProps = comp as CompProperties_AmmoUser;
                if (compProps != null)
                {
                    break;
                }
            }

            if (compProps == null || this.Props.secondaryAmmoProps.ammoSet.ammoTypes.Count != compProps.ammoSet.ammoTypes.Count)
            {
                isSharedAmmo = false;
                return;
            }

            foreach (var ammo in Props.secondaryAmmoProps.ammoSet.ammoTypes)
            {
                ammoCheck.Add(ammo.ammo);
            }

            foreach (var ammo in compProps.ammoSet.ammoTypes)
            {
                if (!ammoCheck.Contains(ammo.ammo))
                {
                    isSharedAmmo = false;
                }
            }
        }


        private void TryReload()
        {
            Job job = compAmmo.TryMakeReloadJob();
            if (job == null)
            {
                return;
            }
            job.playerForced = true;
            Pawn_JobTracker jobs = compAmmo.Holder.jobs;
            Job newJob = job;
            JobCondition lastJobEndCondition = JobCondition.InterruptForced;
            ThinkNode jobGiver = null;
            Job curJob = compAmmo.Holder.CurJob;
            jobs.StartJob(newJob, lastJobEndCondition, jobGiver, ((curJob != null) ? curJob.def : null) != job.def, true, null, null, false, false);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (this.CompAmmo != null && !this.CompAmmo.UseAmmo)
            {
                yield break;
            }

            if (this.Holder != null && !this.Holder.Faction.Equals(Faction.OfPlayer))
            {
                yield break;
            }



            yield return new Command_Action
            {
                action = new Action(this.SwitchLauncher),
                defaultDesc = this.Props.description,
                icon = ContentFinder<Texture2D>.Get(IsSecondaryAmmoSelected ? this.Props.secondaryCommandIcon : this.Props.mainCommandIcon, false),
                defaultLabel = IsSecondaryAmmoSelected ? this.Props.secondaryWeaponLabel : this.Props.mainWeaponLabel,
            };

            if (IsSharedAmmo)
            {
                yield break;
            }

            yield return new GizmoSecondaryAmmoStatus
            {
                compAmmo = this
            };

            yield break;
        }

        private void EnableChargeSpeed()
        {
            //Log.Message("Enabling: has charges: " + (this.secondaryAmmoCharges != null) + ", has attack verb: " + (this.AttackVerb != null));
            if (this.secondaryAmmoCharges == null || this.AttackVerb == null)
            {
                return;
            }

            this.parent.AllComps.Add(this.secondaryAmmoCharges);
            this.AttackVerb.compCharges = this.secondaryAmmoCharges;

            //Log.Message("enabled");
        }

        private void DisableChargeSpeed()
        {
            //Log.Message("Disabling: has charges: " + (this.secondaryAmmoCharges != null) + ", has attack verb: " + (this.AttackVerb != null));
            if (this.secondaryAmmoCharges == null || this.AttackVerb == null)
            {
                return;
            }
            this.parent.AllComps.Remove(this.secondaryAmmoCharges);
            this.AttackVerb.compCharges = null;

            //Log.Message("disabled");
        }
    }
}

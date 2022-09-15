using System;
using UnityEngine;
using RimWorld;
using Verse;
using System.Collections.Generic;
using CombatExtended;

namespace BDsPlasmaWeapon
{
    public class CompSecondaryVerb : CompRangedGizmoGiver
    {
        public CompProperties_SecondaryVerb Props
        {
            get
            {
                return (CompProperties_SecondaryVerb)this.props;
            }
        }

        public bool IsSecondaryVerbSelected
        {
            get
            {
                return this.isSecondaryVerbSelected;
            }
        }

        public Verb_LaunchProjectileCE AttackVerb
        {
            get
            {
                if (this.EquipmentSource.PrimaryVerb == null)
                {
                    return null;
                }

                Verb_LaunchProjectileCE verb = this.EquipmentSource.PrimaryVerb as Verb_LaunchProjectileCE;
                return verb;
            }
        }

        private CompEquippable EquipmentSource
        {
            get
            {
                if (compEquippableInt != null)
                {
                    return this.compEquippableInt;
                }
                this.compEquippableInt = this.parent.TryGetComp<CompEquippable>();
                if (compEquippableInt == null)
                {
                    Log.ErrorOnce(this.parent.LabelCap + " has CompSecondaryVerb but no CompEquippable", 50020);

                }
                return this.compEquippableInt;
            }
        }

        public Pawn CasterPawn
        {
            get
            {
                return this.Verb.caster as Pawn;
            }
        }


        private Verb Verb
        {
            get
            {
                if (this.verbInt == null)
                {
                    this.verbInt = this.EquipmentSource.PrimaryVerb;
                }
                return this.verbInt;
            }
        }

        private Verb mainVerb;
        private Verb secondaryVerb;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            InitData();
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            InitData();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            if (this.CasterPawn != null && !this.CasterPawn.Faction.Equals(Faction.OfPlayer))
            {
                yield break;
            }


            string commandIcon = IsSecondaryVerbSelected ? this.Props.secondaryCommandIcon : this.Props.mainCommandIcon;

            if (commandIcon == "")
            {
                commandIcon = "UI/Buttons/Reload";
            }

            Command_Action switchSecondaryLauncher = new Command_Action
            {
                action = new Action(this.SwitchVerb),
                defaultLabel = IsSecondaryVerbSelected ? this.Props.secondaryWeaponLabel : this.Props.mainWeaponLabel,
                defaultDesc = this.Props.description,
                icon = ContentFinder<Texture2D>.Get(commandIcon, false),
                //tutorTag = "Switch between rifle and grenade launcher"
            };
            yield return switchSecondaryLauncher;

            yield break;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look<bool>(ref this.isSecondaryVerbSelected, "PLA_useSecondaryVerb", false);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.PostAmmoDataLoaded();
            }
        }

        public void SwitchVerb()
        {
            if (!IsSecondaryVerbSelected)
            {

                this.EquipmentSource.verbTracker.AllVerbs.Replace(this.EquipmentSource.PrimaryVerb, secondaryVerb);
                this.isSecondaryVerbSelected = true;
                return;
            }
            this.EquipmentSource.verbTracker.AllVerbs.Replace(this.EquipmentSource.PrimaryVerb, mainVerb);
            this.isSecondaryVerbSelected = false;
        }

        private void PostAmmoDataLoaded()
        {

            InitData();

            if (isSecondaryVerbSelected)
            {
                this.EquipmentSource.verbTracker.AllVerbs.Replace(this.EquipmentSource.PrimaryVerb, secondaryVerb);
            }
        }

        public void InitData()
        {
            mainVerb = this.EquipmentSource.PrimaryVerb;

            Log.Message("Has caster: " + (mainVerb.caster != null));

            secondaryVerb = (Verb)Activator.CreateInstance(this.Props.verbProps.verbClass);
            secondaryVerb.verbProps = this.Props.verbProps;
            secondaryVerb.verbTracker = new VerbTracker(this.EquipmentSource);
            secondaryVerb.caster = this.mainVerb.Caster;
            secondaryVerb.castCompleteCallback = this.mainVerb.castCompleteCallback;
        }



        private Verb verbInt = null;
        private CompEquippable compEquippableInt;
        private bool isSecondaryVerbSelected;
    }
}

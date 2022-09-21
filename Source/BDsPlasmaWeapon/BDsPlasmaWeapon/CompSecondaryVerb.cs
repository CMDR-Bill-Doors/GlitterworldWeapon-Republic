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
                return (CompProperties_SecondaryVerb)props;
            }
        }

        public bool IsSecondaryVerbSelected
        {
            get
            {
                return isSecondaryVerbSelected;
            }
        }

        public Verb_LaunchProjectileCE AttackVerb
        {
            get
            {
                if (EquipmentSource.PrimaryVerb == null)
                {
                    return null;
                }

                Verb_LaunchProjectileCE verb = EquipmentSource.PrimaryVerb as Verb_LaunchProjectileCE;
                return verb;
            }
        }

        private CompEquippable EquipmentSource
        {
            get
            {
                if (compEquippableInt != null)
                {
                    return compEquippableInt;
                }
                compEquippableInt = parent.TryGetComp<CompEquippable>();
                if (compEquippableInt == null)
                {
                    Log.ErrorOnce(parent.LabelCap + " has CompSecondaryVerb but no CompEquippable", 50020);

                }
                return compEquippableInt;
            }
        }

        public Pawn CasterPawn
        {
            get
            {
                return Verb.caster as Pawn;
            }
        }


        private Verb Verb
        {
            get
            {
                if (verbInt == null)
                {
                    verbInt = EquipmentSource.PrimaryVerb;
                }
                return verbInt;
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

            if (CasterPawn != null && !CasterPawn.Faction.Equals(Faction.OfPlayer))
            {
                yield break;
            }


            string commandIcon = IsSecondaryVerbSelected ? Props.secondaryCommandIcon : Props.mainCommandIcon;

            if (commandIcon == "")
            {
                commandIcon = "UI/Buttons/Reload";
            }

            Command_Action switchSecondaryLauncher = new Command_Action
            {
                action = new Action(SwitchVerb),
                defaultLabel = (IsSecondaryVerbSelected ? Props.secondaryWeaponLabel : Props.mainWeaponLabel).Translate(),
                defaultDesc = Props.description.Translate(),
                icon = ContentFinder<Texture2D>.Get(commandIcon, false),
                //tutorTag = "Switch between rifle and grenade launcher"
            };
            yield return switchSecondaryLauncher;

            yield break;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look<bool>(ref isSecondaryVerbSelected, "PLA_useSecondaryVerb", false);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                PostAmmoDataLoaded();
            }
        }

        public void SwitchVerb()
        {
            if (!IsSecondaryVerbSelected)
            {

                EquipmentSource.PrimaryVerb.verbProps = Props.verbProps;
                isSecondaryVerbSelected = true;
                return;
            }
            EquipmentSource.PrimaryVerb.verbProps = parent.def.Verbs[0];
            isSecondaryVerbSelected = false;
        }

        private void PostAmmoDataLoaded()
        {

            InitData();

            if (isSecondaryVerbSelected)
            {
                EquipmentSource.verbTracker.AllVerbs.Replace(EquipmentSource.PrimaryVerb, secondaryVerb);
            }
        }

        public void InitData()
        {
            mainVerb = EquipmentSource.PrimaryVerb;

            secondaryVerb = (Verb)Activator.CreateInstance(Props.verbProps.verbClass);
            secondaryVerb.verbProps = Props.verbProps;
            secondaryVerb.verbTracker = new VerbTracker(EquipmentSource);
            secondaryVerb.caster = mainVerb.Caster;
            secondaryVerb.castCompleteCallback = mainVerb.castCompleteCallback;
        }



        private Verb verbInt = null;
        private CompEquippable compEquippableInt;
        private bool isSecondaryVerbSelected;
    }
}

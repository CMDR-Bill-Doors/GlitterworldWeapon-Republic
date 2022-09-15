using Verse;
using RimWorld;
using PipeSystem;
using CombatExtended;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace BDsPlasmaWeapon
{
    public class CompTankFeedWeapon : CompRangedGizmoGiver
    {
        public CompProperties_TankFeedWeapon Props
        {
            get
            {
                return (CompProperties_TankFeedWeapon)props;
            }
        }

        public CompReloadableFromFiller compReloadableFromFiller;

        public bool isOn;
        public override void PostPostMake()
        {
            base.PostPostMake();
            searchTank();
            isOn = false;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            searchTank();
            if (!respawningAfterLoad)
            {
                isOn = false;
            }

        }

        public override void Notify_Equipped(Pawn pawn)
        {
            searchTank();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isOn, "isOn", false);
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
                return EquipmentSource.PrimaryVerb;
            }
        }


        private CompEquippable EquipmentSource
        {
            get
            {
                return parent.TryGetComp<CompEquippable>();
            }
        }

        public bool searchTank(int t)
        {
            if (CasterPawn != null)
            {
                List<Apparel> apparels = CasterPawn.apparel.WornApparel;
                foreach (Apparel apparel in apparels)
                {
                    CompReloadableFromFiller a = apparel.TryGetComp<CompReloadableFromFiller>();
                    if (a != null && a.remainingCharges >= t)
                    {
                        compReloadableFromFiller = a;
                        return true;
                    }
                }
            }
            isOn = false;
            return false;
        }

        public bool searchTank()
        {
            return searchTank(1);
        }



        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            if (CasterPawn != null && !CasterPawn.Faction.Equals(Faction.OfPlayer))
            {
                yield break;
            }


            string commandIcon = isOn ? Props.onIcon : Props.offIcon;

            if (commandIcon == "")
            {
                commandIcon = "UI/Buttons/Reload";
            }

            Command_Action switchSecondaryLauncher = new Command_Action
            {
                action = new Action(toggle),
                defaultLabel = isOn ? this.Props.onLabel : Props.offLabel,
                defaultDesc = this.Props.description,
                icon = ContentFinder<Texture2D>.Get(commandIcon, false),
                //tutorTag = "Switch between rifle and grenade launcher"
            };
            yield return switchSecondaryLauncher;

            if (Prefs.DevMode)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "Debug: search tank";
                command_Action.action = delegate
                {
                    searchTank();
                    Log.Message("tank found " + (compReloadableFromFiller != null).ToString());
                };
                yield return command_Action;
            }

            yield break;
        }

        public void toggle()
        {
            if (compReloadableFromFiller == null)
            {
                if (!searchTank())
                {
                    isOn = false;
                }
                else
                {
                    isOn = !isOn;
                }
            }
            else
            {
                isOn = !isOn;
            }
        }
    }

    public class CompProperties_TankFeedWeapon : CompProperties
    {
        public string onIcon = "UI/Commands/DesirePower";
        public string onLabel = "tank mode on";
        public string offIcon = "UI/Commands/DesirePower";
        public string offLabel = "take mode off";
        public string description = "";

        public CompProperties_TankFeedWeapon()
        {
            compClass = typeof(CompTankFeedWeapon);
        }
    }
}

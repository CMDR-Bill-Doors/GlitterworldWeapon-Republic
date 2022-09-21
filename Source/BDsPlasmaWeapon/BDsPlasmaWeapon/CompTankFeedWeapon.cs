using Verse;
using RimWorld;
using PipeSystem;
using CombatExtended;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Drawing;

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

        public bool alwaysTrue => Props.alwaysTrue;

        public bool isOn;
        public override void PostPostMake()
        {
            base.PostPostMake();
            searchTank(1, false);
            isOn = alwaysTrue;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            searchTank(1, false);
            if (!respawningAfterLoad)
            {
                isOn = alwaysTrue;
            }

        }

        public override void Notify_Equipped(Pawn pawn)
        {
            if (alwaysTrue || isOn)
            {
                searchTank();
            }
            else
            {
                searchTank(1, false);
            }
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
                return Verb.caster as Pawn;
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

        public bool searchTank(int t = 1, bool notify = true)
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
            isOn = alwaysTrue;
            if (notify)
            {
                if (CasterPawn != null)
                {
                    Messages.Message(string.Format("BDP_LizionTankSearchFailedWithPawn".Translate(), parent.LabelCap, CasterPawn), parent, MessageTypeDefOf.RejectInput, historical: false);
                }
                else
                {
                    Messages.Message(string.Format("BDP_LizionTankSearchFailed".Translate(), parent.LabelCap), parent, MessageTypeDefOf.RejectInput, historical: false);
                }
            }
            return false;
        }



        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            if (CasterPawn != null && !CasterPawn.Faction.Equals(Faction.OfPlayer))
            {
                yield break;
            }

            if (!alwaysTrue)
            {
                string commandIcon = (compReloadableFromFiller == null ? Props.disabledIcon : (isOn ? Props.onIcon : Props.offIcon));

                if (commandIcon == "")
                {
                    commandIcon = "UI/Buttons/Reload";
                }

                Command_Action switchSecondaryLauncher = new Command_Action
                {
                    action = new Action(toggle),
                    defaultLabel = (compReloadableFromFiller == null ? Props.disabledLabel : (isOn ? Props.onLabel : Props.offLabel)).Translate(),
                    defaultDesc = (compReloadableFromFiller == null ? Props.disabledDescription.Translate() + "\n\n" : "".Translate()) + Props.description.Translate(),
                    icon = ContentFinder<Texture2D>.Get(commandIcon, false),
                };
                yield return switchSecondaryLauncher;
            }

            Command_Action command_Action = new Command_Action
            {
                defaultLabel = Props.reconnect.Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Icons/PlasmaBackpack_Reconnect", false),
                defaultDesc = Props.reconnectdescription.Translate(),
                action = delegate
                {
                    searchTank();
                }
            };
            yield return command_Action;

            yield break;
        }

        public void toggle()
        {
            if (compReloadableFromFiller == null)
            {
                if (!searchTank() && !alwaysTrue)
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
        public string onLabel = "BDP_SiphonOn";
        public string offIcon = "UI/Commands/DesirePower";
        public string offLabel = "BDP_SiphonOff";
        public string disabledIcon = "UI/Commands/DesirePower";
        public string disabledLabel = "BDP_SiphonDisabled";
        public string description = "BDP_SiphonModeDesc";
        public string disabledDescription = "BDP_SiphonDisabledDesc";
        public bool alwaysTrue = false;
        public string reconnect = "BDP_SiphonReconnect";
        public string reconnectdescription = "BDP_SiphonReconnectDesc";
        public CompProperties_TankFeedWeapon()
        {
            compClass = typeof(CompTankFeedWeapon);
        }
    }
}

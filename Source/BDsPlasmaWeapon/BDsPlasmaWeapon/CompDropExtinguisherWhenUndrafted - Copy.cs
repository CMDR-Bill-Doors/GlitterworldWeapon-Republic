using System.Collections.Generic;
using PipeSystem;
using RimWorld;
using UnityEngine;
using Verse;
using System;
using Verse.Sound;
using Verse.AI;
using CombatExtended;
using System.Runtime.Remoting.Messaging;
using CombatExtended.Compatibility;
using System.Linq;
using System.Drawing;

namespace BDsPlasmaWeapon
{
    public class CompDropExtinguisherWhenUndraftedEX : CompRangedGizmoGiver
    {
        CompReloadableFromFiller compTank;

        CompEquippable compEquippable;

        public CompProperties_DropExtinguisherWhenUndraftedEX Props
        {
            get
            {
                return (CompProperties_DropExtinguisherWhenUndraftedEX)props;
            }
        }

        public bool isEquipment => compEquippable != null;

        public bool isApparel => parent is Apparel;

        public Pawn pawn
        {
            get
            {
                if (isEquipment)
                {
                    return compEquippable.PrimaryVerb.CasterPawn;
                }
                if (isApparel)
                {
                    return (parent as Apparel).Wearer;
                }
                return null;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compTank = parent.TryGetComp<CompReloadableFromFiller>();
            compEquippable = parent.TryGetComp<CompEquippable>();
            if (compTank == null)
            {
                Log.Error("CompDropExtinguisherWhenUndrafted is meant to work with CompReloadableFromFiller!");
            }
        }

        private void toggle()
        {
            shouldDrop = !shouldDrop;
        }

        public bool shouldDrop;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent != null)
            {
                string commandIcon = shouldDrop ? Props.onIcon : Props.offIcon;

                if (commandIcon == "")
                {
                    commandIcon = "UI/Buttons/Reload";
                }

                Command_Action switchSecondaryLauncher = new Command_Action
                {
                    action = new Action(toggle),
                    defaultLabel = shouldDrop ? Props.onLabel : Props.offLabel,
                    defaultDesc = Props.description,
                    icon = ContentFinder<Texture2D>.Get(commandIcon, false),
                };
                yield return switchSecondaryLauncher;
            }
        }
    }

    public class CompProperties_DropExtinguisherWhenUndraftedEX : CompProperties
    {
        public DropLogic defaultDropLogic = DropLogic.DontDrop;
        public string onIcon = "UI/Commands/DesirePower";
        public string offIcon = "UI/Commands/DesirePower";
        public string onLabel = "tank mode on";
        public string offLabel = "take mode off";
        public string description = "UI/Commands/DesirePower";
        public string label = "Drop logic";
        public CompProperties_DropExtinguisherWhenUndraftedEX()
        {
            compClass = typeof(CompDropExtinguisherWhenUndraftedEX);
        }
    }
}

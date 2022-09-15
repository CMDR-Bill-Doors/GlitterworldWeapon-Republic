using System.Collections.Generic;
using PipeSystem;
using RimWorld;
using UnityEngine;
using Verse;
using System;

namespace BDsPlasmaWeapon
{
    public class CompLizionCooler : CompResource
    {
        public CompProperties_LizionCooler Props
        {
            get
            {
                return (CompProperties_LizionCooler)props;
            }
        }

        protected CompPowerTrader powerComp;

        protected CompBreakdownable breakdownableComp;

        protected CompFlickable flickableComp;

        private int currentMode = 0;

        private PipeNet pipeNet;

        public override void PostPostMake()
        {
            base.PostPostMake();
            currentMode = 0;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                currentMode = 0;
            }
            powerComp = parent.GetComp<CompPowerTrader>();
            flickableComp = parent.GetComp<CompFlickable>();
            breakdownableComp = parent.GetComp<CompBreakdownable>();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentMode, "currentMode", 0);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            if (parent != null && !parent.Faction.Equals(Faction.OfPlayer))
            {
                yield break;
            }

            Command_Action turnUp = new Command_Action
            {
                action = new Action(this.turnUp),
                defaultLabel = Props.turnUpLabel,
                icon = ContentFinder<Texture2D>.Get(Props.turnUpIcon, false),
            };
            yield return turnUp;

            Command_Action turnDown = new Command_Action
            {
                action = new Action(this.turnDown),
                defaultLabel = Props.turnDownLabel,
                icon = ContentFinder<Texture2D>.Get(Props.turnDownIcon, false),
            };
            yield return turnDown;

            yield break;
        }

        public override string CompInspectStringExtra()
        {
            string inspectStringExtra = "CurrentMode".Translate() + currentMode;
            return inspectStringExtra;
        }

        public void turnUp()
        {
            if (currentMode < Props.maxModes)
            {
                currentMode++;
            }
        }

        public void turnDown()
        {
            if (currentMode > 0)
            {
                currentMode--;
            }
        }

        private bool ShouldPushHeatNow
        {
            get
            {
                PipeNet pipeNet = this.PipeNet;
                if ((powerComp != null && !powerComp.PowerOn) || (breakdownableComp != null && breakdownableComp.BrokenDown) || currentMode <= 0 || (pipeNet.Stored < Props.consumptionPerMode * currentMode) || (flickableComp != null && !flickableComp.SwitchIsOn))
                {
                    return false;
                }
                return true;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            float currentConsumption = Props.consumptionPerMode * currentMode;
            if (parent.IsHashIntervalTick(60) && ShouldPushHeatNow)
            {
                PipeNet pipeNet = this.PipeNet;
                GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, Props.heatPerMode * currentMode);
                pipeNet.DrawAmongStorage(currentConsumption, pipeNet.storages);
            }
            if (parent.IsHashIntervalTick((int)(30 / currentConsumption)) && ShouldPushHeatNow)
            {
                TargetInfo a = parent;

                if (currentConsumption > 2)
                {
                    Effecter effecter = BDStatDefOf.LizionCoolerHigh.Spawn(parent.Position, parent.Map);
                    effecter.Trigger(a, TargetInfo.Invalid);
                }
                else
                {
                    Effecter effecter = BDStatDefOf.LizionCoolerLow.Spawn(parent.Position, parent.Map);
                    effecter.Trigger(a, TargetInfo.Invalid);
                }
            }
        }
    }

    public class CompProperties_LizionCooler : CompProperties_Resource
    {
        public int maxModes = 10;
        public float heatPerMode = -10;
        public float consumptionPerMode = 1;
        public string turnUpIcon = "UI/Commands/DesirePower";
        public string turnUpLabel = "turn up";
        public string turnDownIcon = "UI/Commands/DesirePower";
        public string turnDownLabel = "turn down";


        public CompProperties_LizionCooler()
        {
            compClass = typeof(CompLizionCooler);
        }
    }
}

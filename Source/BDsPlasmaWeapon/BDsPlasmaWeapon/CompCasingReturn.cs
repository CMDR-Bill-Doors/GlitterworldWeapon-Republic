using System;
using Verse;
using RimWorld;
using CombatExtended;

namespace BDsPlasmaWeapon
{
    public class CompCasingReturn : ThingComp
    {
        public CompProperties_CasingReturn Props
        {
            get
            {
                return (CompProperties_CasingReturn)props;
            }
        }

        private VerbPropertiesCE verbProperties;

        private CompSecondaryAmmo compSecondaryAmmo;

        public override void Initialize(CompProperties props)
        {
            compSecondaryAmmo = parent.TryGetComp<CompSecondaryAmmo>();
            base.Initialize(props);
            verbProperties = parent.TryGetComp<CompEquippable>().verbTracker.PrimaryVerb.verbProps as VerbPropertiesCE; ;
        }

        private int DropedCasingAmount()
        {
            int dropedCasingAmount = 0;
            int actualCasingAmount;
            if (compSecondaryAmmo != null && Props.dontShootInSecondaryMode && compSecondaryAmmo.IsSecondaryAmmoSelected)
            {
                return 0;
            }
            if (Props.casingAmount < 0)
            {
                actualCasingAmount = verbProperties.ammoConsumedPerShotCount;
            }
            else
            {
                actualCasingAmount = Props.casingAmount;
            }
            for (int i = 0; i < actualCasingAmount; i++)
            {
                if (Rand.Chance(ActualCasingRate))
                {
                    dropedCasingAmount++;
                }
            }
            return dropedCasingAmount;
        }

        public float ActualCasingRate
        {
            get
            {
                if (BDStatDefOf.BDP_CasingReturn != null)
                {
                    return parent.GetStatValue(BDStatDefOf.BDP_CasingReturn);
                }
                Log.Error("Found BDsPlasmaWeapon.CompCasingReturn without BDP_CasingReturn in stats");
                return 0;
            }
        }

        public void DropCasing(IntVec3 pos, Map map)
        {
            int DropedCasingAmount = this.DropedCasingAmount();
            if (DropedCasingAmount > 0)
            {
                Thing thing = ThingMaker.MakeThing(Props.casingThingDef, null);
                thing.stackCount = DropedCasingAmount;
                thing.SetForbidden(true, false);
                GenPlace.TryPlaceThing(thing, pos, map, ThingPlaceMode.Near, out _, null, null, default);
            }
        }

        public void DropCasing(Pawn Caster)
        {
            int DropedCasingAmount = this.DropedCasingAmount();
            if (DropedCasingAmount > 0)
            {
                Thing thing = ThingMaker.MakeThing(Props.casingThingDef, null);
                thing.stackCount = DropedCasingAmount;
                Caster.inventory.innerContainer.TryAdd(thing);
            }
        }
    }

    public class CompProperties_CasingReturn : CompProperties
    {
        public CompProperties_CasingReturn()
        {
            compClass = typeof(CompCasingReturn);
        }
        public ThingDef casingThingDef;
        public int casingAmount = -1;
        public bool rateAffectedByQuality = true;
        public bool dontShootInSecondaryMode = true;
    }
}

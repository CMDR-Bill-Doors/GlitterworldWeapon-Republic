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
        Random random = new Random();

        private int dropedCasingAmount()
        {
            int dropedCasingAmount = 0;
            int actualCasingAmount;
            if (Props.casingAmount < 0)
            {
                VerbPropertiesCE verbProperties = parent.TryGetComp<CompEquippable>()?.verbTracker.PrimaryVerb.verbProps as VerbPropertiesCE;
                actualCasingAmount = verbProperties.ammoConsumedPerShotCount;
            }
            else
            {
                actualCasingAmount = Props.casingAmount;
            }
            for (int i = 0; i < actualCasingAmount; i++)
            {
                double Random = random.NextDouble();
                if (Random <= actualCasingRate)
                {
                    dropedCasingAmount++;
                }
            }
            return dropedCasingAmount;
        }

        public float actualCasingRate
        {
            get
            {
                if (BDStatDefOf.BDP_CasingReturn != null)
                {
                    return this.parent.GetStatValue(BDStatDefOf.BDP_CasingReturn);
                }
                Log.Error("Found BDsPlasmaWeapon.CompCasingReturn without BDP_CasingReturn in stats");
                return 0;
            }
        }

        public void DropCasing(IntVec3 pos, Map map)
        {
            int DropedCasingAmount = dropedCasingAmount();
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
            int DropedCasingAmount = dropedCasingAmount();
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
    }
}

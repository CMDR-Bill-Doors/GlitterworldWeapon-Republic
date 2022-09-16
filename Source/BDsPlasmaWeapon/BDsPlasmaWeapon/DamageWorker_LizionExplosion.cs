﻿using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;


namespace BDsPlasmaWeapon
{
    public class DamageWorker_LizionExplosion : DamageWorker_AddInjury
    {
        public override void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
        {
            base.ExplosionStart(explosion, cellsToAffect);
            GenTemperature.PushHeat(explosion.Position, explosion.Map, def.explosionHeatEnergyPerCell * (float)cellsToAffect.Count);
        }

        public override void ExplosionAffectCell(Explosion explosion, IntVec3 c, List<Thing> damagedThings, List<Thing> ignoredThings, bool canThrowMotes)
        {
            base.ExplosionAffectCell(explosion, c, damagedThings, ignoredThings, canThrowMotes);
            float lengthHorizontal = (c - explosion.Position).LengthHorizontal;
            float num2 = 1f - lengthHorizontal / explosion.radius;
            if (num2 > 0f)
            {
                explosion.Map.snowGrid.AddDepth(c, (0f - num2) * def.explosionSnowMeltAmount);
            }
        }
    }
}
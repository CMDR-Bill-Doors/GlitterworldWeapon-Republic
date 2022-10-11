using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;
using static HarmonyLib.Code;

namespace BDsPlasmaWeapon
{
    public class DisintegratingProjectile : BulletCE
    {
        public DefModExtension_DisintegratingProjectile Data
        {
            get
            {
                return def.GetModExtension<DefModExtension_DisintegratingProjectile>();
            }
        }

        public DefModExtension_DisintegratingProjectileSoundExtension SoundData
        {
            get
            {
                return def.GetModExtension<DefModExtension_DisintegratingProjectileSoundExtension>();
            }
        }

        public CompColorableFaction compColorableFaction;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compColorableFaction = this.TryGetComp<CompColorableFaction>();
        }

        System.Random random = new System.Random();

        private float FadeOutStartPercentage
        {
            get
            {
                if (Data != null && Data.fadeOutStartPercentage > 0 && Data.fadeOutStartPercentage < 1)
                {
                    return Data.fadeOutStartPercentage;
                }
                return (2 / 3f);
            }
        }

        private float FadeOutExpandMultiplier
        {
            get
            {
                if (Data != null)
                {
                    return Data.fadeOutExpandMultiplier;
                }
                return 1;
            }
        }

        public PropertyInfo P_ShadowMaterial
        {
            get
            {
                if (P_material == null)
                {
                    Reflect();
                }
                return P_material;
            }
        }
        public PropertyInfo P_material = null;
        public PropertyInfo P_LastPos
        {
            get
            {
                if (P_lastPos == null)
                {
                    Reflect();
                }
                return P_lastPos;
            }
        }
        public PropertyInfo P_lastPos = null;
        private void Reflect()
        {
            foreach (PropertyInfo x in typeof(ProjectileCE).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (x.Name == "ShadowMaterial")
                {
                    P_material = x;
                    continue;
                }
                if (x.Name == "LastPos")
                {
                    P_lastPos = x;
                    continue;
                }
            }
        }
        public float DistancePercent
        {
            get
            {
                Vector3 v3Pos = ExactPosition;
                float distance = (origin - new Vector2(v3Pos.x, v3Pos.z)).magnitude;
                return distance / equipmentDef.Verbs[0].range;
            }
        }
        public float FadeOutPercent => Math.Max(0f, (float)(DistancePercent - FadeOutStartPercentage) / ((4 / 3f) - FadeOutStartPercentage));
        public override void Tick()
        {
            if (DistancePercent <= 1f)
            {
                base.Tick();
                return;
            }
            if (DistancePercent > (4 / 3f))
            {
                Destroy();
                return;
            }
            if (!landed)
            {
                P_LastPos.SetValue(this, ExactPosition);
                ticksToImpact--;
                if (!ExactPosition.InBounds(base.Map))
                {
                    Position = ((Vector3)P_LastPos.GetValue(this)).ToIntVec3();
                    Destroy();
                }
                Position = ExactPosition.ToIntVec3();
                if (ticksToImpact <= 0)
                {
                    Destroy();
                }
            }
        }

        public override void Impact(Thing hitThing)
        {
            Map map = base.Map;
            base.Impact(hitThing);
            if (Data != null && Data.shouldStartFire)
            {
                if (landed)
                {
                    startFire(map);
                }
                else
                {
                    startFire(hitThing, map);
                }
            }
            if (SoundData != null)
            {
                if (hitThing != null)
                {
                    if (hitThing is Pawn pawn)
                    {
                        if (pawn.IsNotFresh())
                        {
                            SoundData.metalImpactSound?.PlayOneShot(new TargetInfo(base.Position, map));
                        }
                        else
                        {
                            SoundData.fleshImpactSound?.PlayOneShot(new TargetInfo(base.Position, map));
                        }
                    }
                    if (hitThing is Plant plant && ((plant.def.ingestible != null && plant.def.ingestible.foodType == FoodTypeFlags.Tree) || plant.def.defName == "BurnedTree"))
                    {
                        SoundData.woodImpactSound?.PlayOneShot(new TargetInfo(base.Position, map));
                    }
                    if (hitThing is Building building && building.Stuff != null)
                    {
                        List<StuffCategoryDef> stuffs = building.Stuff.stuffProps.categories;

                        if (stuffs.Contains(StuffCategoryDefOf.Stony))
                        {
                            SoundData.stoneImpactSound?.PlayOneShot(new TargetInfo(base.Position, map));
                        }
                        if (stuffs.Contains(StuffCategoryDefOf.Woody))
                        {
                            SoundData.woodImpactSound?.PlayOneShot(new TargetInfo(base.Position, map));
                        }
                        if (stuffs.Contains(StuffCategoryDefOf.Metallic))
                        {
                            SoundData.metalImpactSound?.PlayOneShot(new TargetInfo(base.Position, map));
                        }
                    }
                    if (hitThing is Mineable)
                    {
                        SoundData.stoneImpactSound?.PlayOneShot(new TargetInfo(base.Position, map));
                    }
                }
                else if (base.Position.GetTerrain(map).takeSplashes)
                {
                    SoundData.waterImpactSound?.PlayOneShot(new TargetInfo(base.Position, map));
                }
                else
                {
                    SoundData.terrainImpactSound?.PlayOneShot(new TargetInfo(base.Position, map));
                }
            }
        }

        private void startFire(Map map)
        {
            if (Rand.Chance(Data.chanceOfFire))
            {
                float fireSize = Data.minFireSize + (float)(random.NextDouble() * (Data.maxFireSize - Data.minFireSize));
                FireUtility.TryStartFireIn(ExactPosition.ToIntVec3(), map, fireSize);
            }
        }

        private void startFire(Thing thing, Map map)
        {
            if (Rand.Chance(Data.chanceOfFire))
            {
                float fireSize = Data.minFireSize + (float)(random.NextDouble() * (Data.maxFireSize - Data.minFireSize));

                if (thing is Pawn)
                {
                    FireUtility.TryAttachFire(thing, fireSize);
                }
                else if (thing.FlammableNow)
                {
                    FireUtility.TryStartFireIn(thing.Position, map, fireSize);
                }
            }
        }

        public override void Draw()
        {
            if (!(FlightTicks == 0 && launcher != null && launcher is Pawn))
            {
                Material material = new Material(def.DrawMatSingle);
                Color color = material.color;
                if (Data != null && !Data.shouldIgnoreColorable && compColorableFaction != null && BDPMod.enableProjectileColoring)
                {
                    color = compColorableFaction.FactionColor();
                }
                color.a *= 1 - FadeOutPercent;
                material.color = color;
                float drawSize = 1 + (FadeOutPercent * FadeOutExpandMultiplier);
                Graphics.DrawMesh(MeshPool.GridPlane(def.graphicData.drawSize * drawSize), DrawPos, DrawRotation, material, 0);
                if (castShadow)
                {
                    Vector3 position = new Vector3(ExactPosition.x, def.Altitude - 0.01f, ExactPosition.z - Mathf.Lerp(shotHeight, 0f, fTicks / StartingTicksToImpact));
                    Material shadowMaterial = P_ShadowMaterial.GetValue(this, null) as Material;
                    color = shadowMaterial.color;
                    color.a *= 1 - FadeOutPercent;
                    shadowMaterial.color = color;
                    Graphics.DrawMesh(MeshPool.GridPlane(def.graphicData.drawSize * drawSize), position, DrawRotation, shadowMaterial, 0);
                }
            }
        }
    }

    public class DefModExtension_DisintegratingProjectile : DefModExtension
    {
        public float fadeOutStartPercentage = (2 / 3f);
        public float fadeOutExpandMultiplier = 1;
        public bool shouldStartFire = false;
        public float chanceOfFire = 1f;
        public float minFireSize = 0.1f;
        public float maxFireSize = 1;
        public bool shouldIgnoreColorable = true;
    }

    public class DefModExtension_DisintegratingProjectileSoundExtension : DefModExtension
    {
        public SoundDef fleshImpactSound;
        public SoundDef metalImpactSound;
        public SoundDef stoneImpactSound;
        public SoundDef terrainImpactSound;
        public SoundDef waterImpactSound;
        public SoundDef woodImpactSound;
    }
}

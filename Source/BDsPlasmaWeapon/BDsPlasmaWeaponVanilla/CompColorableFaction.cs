
using Verse;
using RimWorld;
using UnityEngine;

namespace BDsPlasmaWeaponVanilla
{
    public class CompColorableFaction : ThingComp
    {
        public CompProperties_ColorableFaction Props
        {
            get
            {
                return (CompProperties_ColorableFaction)props;
            }
        }

        public Color playerProjectileColor = new Color(BDPMod.CustomPlayerProjectileColorR, BDPMod.CustomPlayerProjectileColorG, BDPMod.CustomPlayerProjectileColorB);

        public Color colorCache;

        private bool colorGetted;

        public Color FactionColor()
        {
            if (Props.discoLightMode || BDPMod.discoLightMode)
            {
                if (!colorGetted)
                {
                    colorCache = Color.HSVToRGB(Rand.Value, Rand.Range(8, 10) / 10f, Rand.Range(8, 10) / 10f);
                    Log.Message(colorCache.ToString());
                    colorGetted = true;
                }
                return colorCache;
            }
            else
            {
                Faction faction = parent.Faction;
                if (parent is Projectile projectile)
                {
                    faction = projectile.Launcher?.Faction;
                }
                if (faction != null)
                {
                    if (BDPMod.useFactionColor)
                    {
                        return (faction.Color);
                    }
                    else
                    {
                        if (faction == Faction.OfPlayer)
                        {
                            return BDPMod.CustomPlayerProjectileColor ? playerProjectileColor : (Props.colorPlayer);
                        }
                        if (faction == Faction.OfPirates)
                        {
                            return (Props.colorPirate);
                        }
                        if (faction == Faction.OfEmpire)
                        {
                            return (Props.colorEmpire);
                        }
                        if (faction.HostileTo(Faction.OfPlayer))
                        {
                            return (Props.colorHostile);
                        }
                        if (faction.AllyOrNeutralTo(Faction.OfPlayer))
                        {
                            return (Props.colorNeutualOrAlly);
                        }
                    }
                }
            }
            return Color.white;
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if ((parent.def.graphic.color == Color.white || Props.overrideExistingColoring) && parent is Projectile projectile)
            {
                Vector3 drawPos = projectile.DrawPos;
                Material material = projectile.def.DrawMatSingle;
                material.color = FactionColor();
                Graphics.DrawMesh(MeshPool.GridPlane(projectile.def.graphicData.drawSize), drawPos, projectile.ExactRotation, material, 0);
            }
        }
    }

    public class CompProperties_ColorableFaction : CompProperties
    {
        public Color colorPlayer = Color.cyan;

        public Color colorPirate = Color.red;

        public Color colorEmpire = Color.green;

        public Color colorHostile = Color.red;

        public Color colorNeutualOrAlly = Color.cyan;

        public bool overrideExistingColoring = false;

        public bool discoLightMode = false;

        public CompProperties_ColorableFaction()
        {
            compClass = typeof(CompColorableFaction);
        }
    }
}

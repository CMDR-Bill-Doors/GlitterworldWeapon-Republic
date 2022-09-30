
using Verse;
using RimWorld;
using UnityEngine;
namespace BDsPlasmaWeapon
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

        public Color FactionColor()
        {
            Faction faction = parent.Faction;
            if (parent is Projectile projectile)
            {
                faction = projectile.Launcher?.Faction;
            }
            if (faction != null)
            {
                Log.Message(parent.def.drawerType.ToString());
                if (Props.useFactionColor)
                {
                    return (faction.Color);
                }
                else
                {
                    if (faction == Faction.OfPlayer)
                    {
                        return (Props.colorPlayer);
                    }
                    if (faction.HostileTo(Faction.OfPlayer))
                    {
                        return (Props.colorHostile);
                    }
                    if (faction.AllyOrNeutralTo(Faction.OfPlayer))
                    {
                        return (Props.colorNeutualOrAlly);
                    }
                    if (faction == Faction.OfPirates)
                    {
                        return (Props.colorPirate);
                    }
                    if (faction == Faction.OfEmpire)
                    {
                        return (Props.colorEmpire);
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
        public Color colorPlayer = Color.blue;

        public Color colorPirate = Color.red;

        public Color colorEmpire = Color.green;

        public Color colorHostile = Color.red;

        public Color colorNeutualOrAlly = Color.cyan;

        public bool useFactionColor = false;

        public bool overrideExistingColoring = false;

        public CompProperties_ColorableFaction()
        {
            compClass = typeof(CompColorableFaction);
        }
    }
}

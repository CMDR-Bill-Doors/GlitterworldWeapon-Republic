using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BDsPlasmaWeaponVanilla
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatch
    {
        private static readonly Type patchType = typeof(HarmonyPatch);
        static HarmonyPatch()
        {
            Harmony harmony = new Harmony("BDsPlasmaWeapon");

            harmony.Patch(AccessTools.Method(typeof(PawnGenerator), "PostProcessGeneratedGear"), postfix: new HarmonyMethod(patchType, nameof(PostProcessGeneratedGear_Postfix)));
        }

        public static void PostProcessGeneratedGear_Postfix(Thing gear)
        {
            CompReloadableFromFiller comp = gear.TryGetComp<CompReloadableFromFiller>();
            if (comp != null)
            {
                comp.remainingCharges = comp.MaxCharges;
            }
        }

        public static void DrawEquipmentAiming_postfix(Thing eq, Vector3 drawLoc, Mesh mesh, float num)
        {
            DefModExtension_WeaponGlowRender renderExtension = eq.def.GetModExtension<DefModExtension_WeaponGlowRender>();
            if (renderExtension != null)
            {
                Graphics.DrawMesh(material: renderExtension.graphicData.Graphic.MatSingle, mesh: mesh, position: drawLoc, rotation: Quaternion.AngleAxis(num, Vector3.up), layer: 0);
            }
        }
    }


    public class DefModExtension_WeaponGlowRender : DefModExtension
    {
        public GraphicData graphicData;
    }
}

using CombatExtended;
using System;
using System.Reflection;
using UnityEngine;
using Verse;

namespace BDsPlasmaWeapon
{
    public class DisintegratingProjectile : BulletCE
    {
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
        public float FadeOutPercent => Math.Max(0f, (float)(DistancePercent - (2 / 3f)) / (2 / 3f));
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
                Position = this.ExactPosition.ToIntVec3();
                if (ticksToImpact <= 0)
                {
                    Destroy();
                }
            }
        }
        public override void Draw()
        {
            if (!(FlightTicks == 0 && launcher != null && launcher is Pawn))
            {
                Material material = new Material(def.DrawMatSingle);
                Color color = material.color;
                color.a *= 1 - FadeOutPercent;
                material.color = color;
                float drawSize = 1 + FadeOutPercent;
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
}

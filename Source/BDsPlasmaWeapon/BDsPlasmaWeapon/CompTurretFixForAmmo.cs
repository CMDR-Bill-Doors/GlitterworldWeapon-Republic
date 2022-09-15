using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;
using UnityEngine;

using CombatExtended;

namespace BDsPlasmaWeapon
{
    public class CompTurretFixForAmmo : ThingComp
    {
        public ThingWithComps TurretGun
        {
            get
            {
                if (turretGunInt == null)
                {
                    turretGunInt = (this.parent as Building_TurretGunCE)?.Gun as ThingWithComps;
                }
                return turretGunInt;
            }
        }

        private ThingWithComps turretGunInt;
        private bool initialized = false;
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (this.turretGunInt == null) yield break;
            foreach (var comp in this.TurretGun.AllComps)
            {
                foreach (var gizmo in comp.CompGetGizmosExtra())
                {
                    yield return gizmo;
                }
            }
        }

        public override void CompTick()
        {
            if (!initialized)
            {
                initialized = true;
                this.TurretGun.TryGetComp<CompSecondaryAmmo>()?.InitData();
                var compAmmo = this.TurretGun.TryGetComp<CompAmmoUser>();
                if (compAmmo != null)
                {
                    compAmmo.turret = this.parent as Building_Turret; ;
                }

            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;
using UnityEngine;

namespace BDsPlasmaWeaponVanilla
{
    public class CompTurretFixForVerb : ThingComp
    {
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            turretGunInt = (parent as Building_TurretGun).gun as ThingWithComps;
            Log.Message((parent as Building_TurretGun).gun.ToString());
        }

        public ThingWithComps TurretGun
        {
            get
            {
                if (turretGunInt == null)
                {
                    turretGunInt = (parent as Building_TurretGun).gun as ThingWithComps;
                }
                return turretGunInt;
            }
        }

        private ThingWithComps turretGunInt;


        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Log.Message("1");
            if (turretGunInt == null) yield break;
            Log.Message("2");
            foreach (var comp in TurretGun.AllComps)
            {
                Log.Message("3");
                foreach (var gizmo in comp.CompGetGizmosExtra())
                {
                    Log.Message("4");
                    yield return gizmo;
                }
            }
        }
    }

    public class Building_TurretGunWithGizmo : Building_TurretGun
    {
        /*
        private CompSecondaryVerb compSecondaryVerb
        {
            get
            {
                if (compSecondaryVerbCache == null)
                {
                    compSecondaryVerbCache = gun.TryGetComp<CompSecondaryVerb>();
                }
                return compSecondaryVerbCache;
            }
        }

        private CompSecondaryVerb compSecondaryVerbCache;

        /*
        public override Verb AttackVerb
        {
            get
            {
                if (compSecondaryVerb != null && compSecondaryVerb.IsSecondaryVerbSelected)
                {
                    return compSecondaryVerb.secondaryVerb;
                }
                return base.AttackVerb;
            }
        }
        */

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (gun == null) yield break;
            foreach (var comp in (gun as ThingWithComps).AllComps)
            {
                foreach (var gizmo in comp.CompGetGizmosExtra())
                {
                    yield return gizmo;
                }
            }
        }


    }
}

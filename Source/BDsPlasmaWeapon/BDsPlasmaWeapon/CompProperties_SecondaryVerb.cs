using System;
using UnityEngine;
using RimWorld;
using Verse;
using CombatExtended;
using System.Collections.Generic;

namespace BDsPlasmaWeapon
{
    public class CompProperties_SecondaryVerb : CompProperties
    {
        public CompProperties_SecondaryVerb()
        {
            this.compClass = typeof(CompSecondaryVerb);
        }

        public VerbPropertiesCE verbProps = new VerbPropertiesCE();

        public string mainCommandIcon = "";
        public string mainWeaponLabel = "";
        public string secondaryCommandIcon = "";
        public string secondaryWeaponLabel = "";
        public string description = "";

        public List<int> secondaryWeaponChargeSpeeds = new List<int>();
    }
}

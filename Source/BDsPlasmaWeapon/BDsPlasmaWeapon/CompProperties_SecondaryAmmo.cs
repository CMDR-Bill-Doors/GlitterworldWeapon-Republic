using Verse;
using CombatExtended;
using System.Collections.Generic;

namespace BDsPlasmaWeapon
{
    public class CompProperties_SecondaryAmmo : CompProperties
    {
        public CompProperties_SecondaryAmmo()
        {
            this.compClass = typeof(CompSecondaryAmmo);
        }

        public CompProperties_AmmoUser secondaryAmmoProps;
        public VerbPropertiesCE secondaryVerb;
        public float loadedAmmoBulkFactor = 0f;
        public bool showSecondaryAmmoStat = true;

        public string mainCommandIcon = "";
        public string secondaryCommandIcon = "";
        public string mainWeaponLabel = "";
        public string secondaryWeaponLabel = "";
        public string description = "";

        public List<int> secondaryWeaponChargeSpeeds = new List<int>();
    }
}

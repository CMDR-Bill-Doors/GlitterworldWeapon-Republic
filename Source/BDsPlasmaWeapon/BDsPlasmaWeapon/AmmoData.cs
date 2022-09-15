using System;
using CombatExtended;
using Verse;

namespace BDsPlasmaWeapon
{
    public class AmmoData : IExposable
    {
        public int curMagCount;
        public AmmoDef selectedAmmo;

        public AmmoData()
        {

        }

        public AmmoData(int curMagCount, AmmoDef selectedAmmo)
        {
            this.curMagCount = curMagCount;
            this.selectedAmmo = selectedAmmo;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.curMagCount, "magCount", 0);
            Scribe_Defs.Look<AmmoDef>(ref this.selectedAmmo, "selectedAmmo");
        }


        public override string ToString()
        {
            return "Magazine: " + this.curMagCount + ", Ammo: " + this.selectedAmmo.defName;
        }
    }
}

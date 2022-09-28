using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BDsPlasmaWeaponVanilla
{
    internal class CompPawnEquipmentGizmo : ThingComp
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            ThingWithComps thingWithComps = ((parent is Pawn pawn) ? pawn.equipment.Primary : null);
            if (thingWithComps == null || thingWithComps.AllComps.NullOrEmpty())
            {
                yield break;
            }
            foreach (ThingComp allComp in thingWithComps.AllComps)
            {
                if (allComp is CompSecondaryVerb || allComp is CompTankFeedWeapon || allComp is CompReloadableFromFiller)
                {
                    foreach (Gizmo item in allComp.CompGetGizmosExtra())
                    {
                        yield return item;
                    }
                }
            }
        }
    }
}

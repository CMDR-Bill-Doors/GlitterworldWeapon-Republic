using Verse;
using CombatExtended;
using RimWorld;
using PipeSystem;

namespace BDsPlasmaWeapon
{
    public class Comp_TurretPipedDualMode : CompResource
    {
        public override void CompTick()
        {
            base.CompTick();
            if (parent is Building_TurretGunCE turret)
            {
                ThingWithComps Gun = turret.Gun as ThingWithComps;
                CompAmmoUser compAmmoUser = Gun.TryGetComp<CompAmmoUser>();
                CompSecondaryAmmo compSecondaryAmmoUser = Gun.TryGetComp<CompSecondaryAmmo>();
                int ammoDifference = compSecondaryAmmoUser.CompAmmo.CurMagCount - compSecondaryAmmoUser.CompAmmo.CurMagCount;
                if (compSecondaryAmmoUser.IsSecondaryAmmoSelected && ammoDifference > 0)
                {
                    PipeNet pipeNet = PipeNet;
                    if (pipeNet != null && pipeNet.Stored >= ammoDifference)
                    {
                        pipeNet.DrawAmongStorage(ammoDifference, pipeNet.storages);
                        compSecondaryAmmoUser.CompAmmo.CurMagCount += ammoDifference;
                    }
                    else if (pipeNet != null && pipeNet.Stored > 1)
                    {
                        pipeNet.DrawAmongStorage(pipeNet.Stored, pipeNet.storages);
                        compSecondaryAmmoUser.CompAmmo.CurMagCount += (int)pipeNet.Stored;
                    }
                }
            }
        }
    }
}

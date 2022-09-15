using Verse;
using CombatExtended;
using RimWorld;
using PipeSystem;

namespace BDsPlasmaWeapon
{
    public class Verb_ShootFromPipeSystem : Verb_ShootCE
    {

        public CompShootFromPipeNet compShootFromPipeNet
        {
            get
            {
                if (caster is Building_TurretGunCE turret)
                {
                    return turret.TryGetComp<CompShootFromPipeNet>();
                }
                return null;
            }
        }

        public override bool Available()
        {

            if (base.Available())
            {
                if (compShootFromPipeNet == null)
                {
                    Log.Error("You forgot to add CompShootFromPipeNet to a Verb_ShootFromPipeSystem using turret");
                    return false;
                }
                else
                {
                    PipeNet pipeNet = compShootFromPipeNet.PipeNet;
                    if (pipeNet != null && pipeNet.Stored >= compShootFromPipeNet.Props.resourceConsumption)
                    {
                        pipeNet.DrawAmongStorage(compShootFromPipeNet.Props.resourceConsumption, pipeNet.storages);
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }
    }
}

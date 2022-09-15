using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using RimWorld;
using UnityEngine;


namespace BDsPlasmaWeapon
{
    public class StatPart_AmmoConsumedSecondaryVerb : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            return "";
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            CompSecondaryAmmo compVerb = req.Thing?.TryGetComp<CompSecondaryAmmo>();
            Log.Message("1");
            if (compVerb != null && compVerb.IsSecondaryAmmoSelected)
            {
                val = compVerb.Props.secondaryVerb.ammoConsumedPerShotCount;
            }
            Log.Message("ammoConsumedPerShotCount" + val.ToString());
        }
    }
}

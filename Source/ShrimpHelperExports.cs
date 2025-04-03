using Celeste.Mod.ShrimpHelper.Entities;
using MonoMod.ModInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.ShrimpHelper;

[ModExportName("ShrimpHelper")]
public static class ShrimpHelperExports
{
    public static Entity TransformIntoShreo(Actor from)
    {
        Holdable hFrom = from.Get<Holdable>();
        if (hFrom == null) return null;
        Entity shreo = new ShreoCrystal(from.Position);
        (shreo as ShreoCrystal).Hold.SetSpeed(hFrom.GetSpeed());
        (shreo as ShreoCrystal).antiGrav = true;
        return shreo;
    }
    public static Component GetShreoCollider(Action<Entity> callback)
    {
        Component collider = new EntityCollider<ShreoCrystal>(callback);
        return collider;
    }
}


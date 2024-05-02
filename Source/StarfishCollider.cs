using Celeste.Mod.ShrimpHelper.Entities;
using Monocle;
using System;

namespace Celeste.Mod.ShrimpHelper.Components;

[Tracked(false)]
public class StarfishCollider : Component
{
    public Action<StarfishGuy> OnCollide;

    public Collider Collider;

    public StarfishCollider(Action<StarfishGuy> onCollide, Collider collider = null)
        : base(active: false, visible: false)
    {
        OnCollide = onCollide;
        Collider = null;
    }

    public void Check(StarfishGuy guy)
    {
        if (OnCollide != null)
        {
            Collider collider = Entity.Collider;
            if (Collider != null)
            {
                Entity.Collider = Collider;
            }
            if (guy.CollideCheck(Entity))
            {
                OnCollide(guy);
            }
            Entity.Collider = collider;
        }
    }
}


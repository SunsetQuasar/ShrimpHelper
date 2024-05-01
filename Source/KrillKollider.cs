using Celeste.Mod.ShrimpHelper.Entities;
using Monocle;
using System;

namespace Celeste.Mod.ShrimpHelper.Components
{
	[Tracked(false)]
	public class KrillKollider : Component
	{
		public Action<BonkKrill> OnCollide;

		public Collider Collider;

		public KrillKollider(Action<BonkKrill> onCollide, Collider collider = null)
			: base(active: false, visible: false)
		{
			OnCollide = onCollide;
			Collider = null;
		}

		public void Check(BonkKrill krill)
		{
			if (OnCollide != null)
			{
				Collider collider = Entity.Collider;
				if (Collider != null)
				{
					Entity.Collider = Collider;
				}
				if (krill.CollideCheck(Entity))
				{
					OnCollide(krill);
				}
				Entity.Collider = collider;
			}
		}
	}
}

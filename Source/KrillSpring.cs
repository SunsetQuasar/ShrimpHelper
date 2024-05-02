using System;
using Monocle;
using Microsoft.Xna.Framework;
using System.Reflection;
using Celeste.Mod.Entities;
using Celeste.Mod.ShrimpHelper.Components;

namespace Celeste.Mod.ShrimpHelper.Entities
{
	[CustomEntity("ShrimpHelper/KrillSpring")]
	[Tracked]
	public class KrillSpring : Entity
	{
		public enum KrillOrientations
		{
			Floor = 0,
			WallLeft = 1,
			WallRight = 2,
			Ceiling = 3
		}

		private Sprite sprite;

		private Wiggler wiggler;

		private StaticMover staticMover;

		public KrillOrientations Orientation;

		private bool playerCanUse;

		public Color DisabledColor = Color.White;

		public bool VisibleWhenDisabled;

		public KrillSpring(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			int ori = data.Int("orientation", 0);
			KrillOrientations orientation = KrillOrientations.Floor;
			orientation = (KrillOrientations)ori;
			Orientation = orientation;
			playerCanUse = data.Bool("playerCanUse", true);
			KrillKollider krillCollider = new KrillKollider(OnKrill);
			StarfishCollider starfishCollider = new StarfishCollider(OnStarfishGuy);
			Add(krillCollider);
            Add(starfishCollider);
            Add(sprite = new Sprite(GFX.Game, "objects/spring/"));
			sprite.Add("idle", "", 0f, default(int));
			sprite.Add("bounce", "", 0.07f, "idle", 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 4, 5);
			sprite.Add("disabled", "white", 0.07f);
			sprite.Play("idle");
			sprite.Origin.X = sprite.Width / 2f;
			sprite.Origin.Y = sprite.Height;
			base.Depth = -8501;
			staticMover = new StaticMover();
			staticMover.OnAttach = delegate (Platform p)
			{
				base.Depth = p.Depth + 1;
			};
			switch (orientation)
			{
				case KrillOrientations.Floor:
					staticMover.SolidChecker = (Solid s) => CollideCheck(s, Position + Vector2.UnitY);
					staticMover.JumpThruChecker = (JumpThru jt) => CollideCheck(jt, Position + Vector2.UnitY);
					Add(staticMover);
					break;
				case KrillOrientations.WallLeft:
					staticMover.SolidChecker = (Solid s) => CollideCheck(s, Position - Vector2.UnitX);
					staticMover.JumpThruChecker = (JumpThru jt) => CollideCheck(jt, Position - Vector2.UnitX);
					Add(staticMover);
					break;
				case KrillOrientations.WallRight:
					staticMover.SolidChecker = (Solid s) => CollideCheck(s, Position + Vector2.UnitX);
					staticMover.JumpThruChecker = (JumpThru jt) => CollideCheck(jt, Position + Vector2.UnitX);
					Add(staticMover);
					break;
                case KrillOrientations.Ceiling:
                    staticMover.SolidChecker = (Solid s) => CollideCheck(s, Position - Vector2.UnitY);
                    staticMover.JumpThruChecker = (JumpThru jt) => CollideCheck(jt, Position - Vector2.UnitY);
                    Add(staticMover);
                    break;
			}
			Add(wiggler = Wiggler.Create(1f, 4f, delegate (float v)
			{
				sprite.Scale.Y = 1f + v * 0.2f;
			}));
			switch (orientation)
			{
				case KrillOrientations.Floor:
					base.Collider = new Hitbox(16f, 6f, -8f, -6f);
					krillCollider.Collider = new Hitbox(16f, 10f, -8f, -10f);
					break;
				case KrillOrientations.WallLeft:
					base.Collider = new Hitbox(6f, 16f, 0f, -8f);
					krillCollider.Collider = new Hitbox(12f, 16f, 0f, -8f);
					sprite.Rotation = (float)Math.PI / 2f;
					break;
				case KrillOrientations.WallRight:
					base.Collider = new Hitbox(6f, 16f, -6f, -8f);
					krillCollider.Collider = new Hitbox(12f, 16f, -12f, -8f);
					sprite.Rotation = -(float)Math.PI / 2f;
					break;
				case KrillOrientations.Ceiling:
					base.Collider = new Hitbox(16f, 6f, -8f);
					krillCollider.Collider = new Hitbox(16f, 10f, -8f, -4f);
					sprite.Rotation = (float)Math.PI;
					break;
				default:
					throw new Exception("Orientation not supported!");
			}
			staticMover.OnEnable = OnEnable;
			staticMover.OnDisable = OnDisable;
		}

		private void OnEnable()
		{
			Visible = (Collidable = true);
			sprite.Color = Color.White;
			sprite.Play("idle");
		}

		private void OnDisable()
		{
			Collidable = false;
			if (VisibleWhenDisabled)
			{
				sprite.Play("disabled");
				sprite.Color = DisabledColor;
			}
			else
			{
				Visible = false;
			}
		}

		private void OnCollide(Player player)
		{
			if (player.StateMachine.State == 9 || !playerCanUse)
			{
				return;
			}
			if (Orientation == KrillOrientations.Floor)
			{
				if (player.Speed.Y >= 0f)
				{
					BounceAnimate();
					player.SuperBounce(base.Top);
				}
				return;
			}
			if (Orientation == KrillOrientations.WallLeft)
			{
				if (player.SideBounce(1, base.Right, base.CenterY))
				{
					BounceAnimate();
				}
				return;
			}
			if (Orientation == KrillOrientations.WallRight)
			{
				if (player.SideBounce(-1, base.Left, base.CenterY))
				{
					BounceAnimate();
				}
				return;
			}
			if (Orientation == KrillOrientations.Ceiling)
			{
				if (player.Speed.Y <= 0f)
				{
					BounceAnimate();
					player.SuperBounce(base.Top);
					player.Speed.Y *= -1;
				}
				return;
			}
			throw new Exception("Orientation not supported!");
		}

		private void BounceAnimate()
		{
			Audio.Play("event:/game/general/spring", base.BottomCenter);
			staticMover.TriggerPlatform();
			sprite.Play("bounce", restart: true);
			wiggler.Start();
		}

		private void OnKrill(BonkKrill k)
		{
			if (k.HitKrillSpring(this))
			{
				BounceAnimate();
			}
		}
        private void OnStarfishGuy(StarfishGuy s)
        {
            if (s.HitKrillSpring(this))
            {
                BounceAnimate();
            }
        }

        public override void Render()
		{
			if (Collidable)
			{
				sprite.DrawOutline();
			}
			base.Render();
		}
	}
}

using Celeste.Mod.Entities;
using Celeste.Mod.ShrimpHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.ShrimpHelper.Entities;

[Tracked]
[CustomEntity("ShrimpHelper/StarfishGuy")]
public class StarfishGuy : Actor
{

    [Tracked]
    public class StarfishPlatform : JumpThru
    {

        StarfishGuy parent;

        public StarfishPlatform(StarfishGuy parent) : base(parent.TopLeft - (Vector2.UnitX * 2f), 24, false)
        {
            Collidable = false;
            Visible = false;
            this.parent = parent;
            OnDashCollide = onDashed;
        }

        public DashCollisionResults onDashed(Player player, Vector2 direction)
        {
            parent.Speed = player.Speed;
            parent.ChangeState(StarfishStates.Rising);
            return DashCollisionResults.Rebound;
        }

        public override void Update()
        {
            base.Update();
            MoveTo(parent.TopLeft - (Vector2.UnitX * 2f));
        }

        public override void Render()
        {
            base.Render();
            Draw.HollowRect(Collider, Color.MistyRose);
        }

        public static void Load()
        {
            On.Celeste.Player.RefillDash += RefillDashHook;
        }
        public static void Unload()
        {
            On.Celeste.Player.RefillDash -= RefillDashHook;
        }

        public static bool RefillDashHook(On.Celeste.Player.orig_RefillDash orig, Player self)
        {
            if (self.Scene == null || self.Dead)
            {
                return orig(self);
            }
            Level level = self.Scene as Level;
            if (level.Transitioning)
            {
                return orig(self);
            }
            if (self.CollideCheck<StarfishPlatform>(self.Position + Vector2.UnitY))
            {
                return false;
            }
            return orig(self);
        }
    }

    public enum StarfishStates
    {
        Idle = 0,
        Platform = 1,
        Rising = 2
    }

    private Vector2 Speed;

    private Collision onCollideH;

    private Collision onCollideV;


    private StarfishPlatform child;

    private StarfishStates state;

    public float fall2;

    public float fall2counter;

    public StarfishGuy(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        onCollideH = OnCollideH;
        onCollideV = OnCollideV;
        child = new StarfishPlatform(this);
        Add(new PlayerCollider(onPlayer, Collider));
        Collider = new Hitbox(20, 20, 10, 10);
        Depth = -120;
        if (data.Bool("startPlatform", false)) ChangeState(StarfishStates.Platform, false);
        Center = data.Position + offset;
        Add(new WindMover(WindMode));
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        scene.Add(child);
    }

    public override void Update()
    {
        base.Update();

        Level level = Scene as Level;

        MoveH(Speed.X * Engine.DeltaTime, onCollideH);
        MoveV(Speed.Y * Engine.DeltaTime, onCollideV);

        switch (state)
        {
            case StarfishStates.Platform:

                MoveV(fall2 * Engine.DeltaTime, onCollideV);
                fall2counter += fall2 * Engine.DeltaTime;

                if (fall2counter >= 24)
                {
                    Speed.Y = 100;
                    ChangeState(StarfishStates.Rising);
                    break;
                }

                Speed.X = Calc.Approach(Speed.X, 0, Engine.DeltaTime * 320);
                Speed.Y = Calc.Approach(Speed.Y, 0, Engine.DeltaTime * 320);

                fall2 = Calc.Approach(fall2, child.HasRider() ? 60 : 0, Engine.DeltaTime * 70);

                break;
            case StarfishStates.Idle:

                Speed.X = Calc.Approach(Speed.X, 0, Engine.DeltaTime * 320);
                Speed.Y = Calc.Approach(Speed.Y, 0, Engine.DeltaTime * 320);

                break;
            case StarfishStates.Rising:

                Speed.X = Calc.Approach(Speed.X, 0, Engine.DeltaTime * 120);
                Speed.Y = Calc.Approach(Speed.Y, -480, Engine.DeltaTime * 320);

                break;
            default:

                break;
        }

        if (base.Left < level.Bounds.Left)
        {
            base.Left = level.Bounds.Left;
            OnCollideH(new CollisionData
            {
                Direction = -Vector2.UnitX
            });
        }
        else if (base.Right > level.Bounds.Right)
        {
            base.Right = level.Bounds.Right;
            OnCollideH(new CollisionData
            {
                Direction = Vector2.UnitX
            });
        }
        if (base.Top < level.Bounds.Top)
        {
            base.Top = level.Bounds.Top;
            OnCollideV(new CollisionData
            {
                Direction = -Vector2.UnitY
            });
        }
        else if (base.Top > (level.Bounds.Bottom + 16))
        {
            RemoveSelf();
            return;
        }

        foreach (StarfishCollider component in base.Scene.Tracker.GetComponents<StarfishCollider>())
        {
            component.Check(this);
        }

        child.MoveTo(TopLeft - (Vector2.UnitX * 2f));
    }

    private void OnCollideH(CollisionData data)
    {
        if (Math.Abs(Speed.X) > 90)
        {
            Speed.X *= -0.5f;
        }
        else
        {
            Speed.X = 0;
        }
    }

    private void OnCollideV(CollisionData data)
    {
        if (state == StarfishStates.Rising && Speed.Y <= -90)
        {
            Explode();
            child.RemoveSelf();
            RemoveSelf();
            return;
        }
        if (Math.Abs(Speed.Y) > 90)
        {
            Speed.Y *= -0.5f;
        }
        else
        {
            Speed.Y = 0;
        }
    }

    private void WindMode(Vector2 wind)
    {
        if (wind.X != 0f)
        {
            MoveH(wind.X);
        }
        if (wind.Y != 0f)
        {
            MoveV(wind.Y);
        }
    }

    public bool HitSpring(Spring spring)
    {
        if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
        {
            Speed.X *= 0.25f;
            Speed.Y = -160f;
            return true;
        }
        if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
        {
            MoveTowardsY(spring.CenterY + 5f, 4f);
            Speed.X = 220f;
            Speed.Y += -20f;
            return true;
        }
        if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
        {
            MoveTowardsY(spring.CenterY + 5f, 4f);
            Speed.X = -220f;
            Speed.Y += -20f;
            return true;
        }
        return false;
    }
    public static void Load()
    {
        On.Celeste.Spring.ctor_Vector2_Orientations_bool += Spring_ctor_Vector2_Orientations_bool;
        StarfishPlatform.Load();
    }

    public static void Unload()
    {
        On.Celeste.Spring.ctor_Vector2_Orientations_bool -= Spring_ctor_Vector2_Orientations_bool;
        StarfishPlatform.Unload();
    }
    private static void Spring_ctor_Vector2_Orientations_bool(On.Celeste.Spring.orig_ctor_Vector2_Orientations_bool orig, Spring self, Vector2 position, Spring.Orientations orientation, bool playerCanUse)
    {
        orig(self, position, orientation, playerCanUse);
        var collider = new StarfishCollider((p) =>
        {
            if (p.HitSpring(self))
            {
                self.BounceAnimate();
            }
        });
        switch (self.Orientation)
        {
            case Spring.Orientations.Floor:
                collider.Collider = new Hitbox(16f, 10f, -8f, -10f); break;
            case Spring.Orientations.WallLeft:
                collider.Collider = new Hitbox(12f, 16f, 0f, -8f); break;
            case Spring.Orientations.WallRight:
                collider.Collider = new Hitbox(12f, 16f, -12f, -8f); break;
        }
        self.Add(collider);
    }

    public bool HitKrillSpring(KrillSpring spring)
    {
        if (spring.Orientation == KrillSpring.KrillOrientations.Floor && Speed.Y >= 0f)
        {
            Speed.X *= 0.25f;
            Speed.Y = -160f;
            return true;
        }
        if (spring.Orientation == KrillSpring.KrillOrientations.WallLeft && Speed.X <= 0f)
        {
            MoveTowardsY(spring.CenterY + 5f, 4f);
            Speed.X = 220f;
            Speed.Y += -20f;
            return true;
        }
        if (spring.Orientation == KrillSpring.KrillOrientations.WallRight && Speed.X >= 0f)
        {
            MoveTowardsY(spring.CenterY + 5f, 4f);
            Speed.X = -220f;
            Speed.Y += -20f;
            return true;
        }
        if (spring.Orientation == KrillSpring.KrillOrientations.Ceiling && Speed.Y <= 0f)
        {
            Speed.X *= 0.25f;
            Speed.Y = 160f;
            return true;
        }
        return false;
    }

    public void Explode()
    {
        Collider collider = base.Collider;
        base.Collider = new Circle(40f);
        Audio.Play("event:/new_content/game/10_farewell/puffer_splode", Position);
        Player player = CollideFirst<Player>();
        if (player != null && !base.Scene.CollideCheck<Solid>(Position, player.Center))
        {
            player.ExplodeLaunch(Position, snapUp: false, sidesOnly: true);
        }
        TheoCrystal theoCrystal = CollideFirst<TheoCrystal>();
        if (theoCrystal != null && !base.Scene.CollideCheck<Solid>(Position, theoCrystal.Center))
        {
            theoCrystal.ExplodeLaunch(Position);
        }
        foreach (TempleCrackedBlock entity in base.Scene.Tracker.GetEntities<TempleCrackedBlock>())
        {
            if (CollideCheck(entity))
            {
                entity.Break(Position);
            }
        }
        foreach (TouchSwitch entity2 in base.Scene.Tracker.GetEntities<TouchSwitch>())
        {
            if (CollideCheck(entity2))
            {
                entity2.TurnOn();
            }
        }
        foreach (FloatingDebris entity3 in base.Scene.Tracker.GetEntities<FloatingDebris>())
        {
            if (CollideCheck(entity3))
            {
                entity3.OnExplode(Position);
            }
        }
        base.Collider = collider;
        Level level = SceneAs<Level>();
        level.Shake();
        level.Displacement.AddBurst(Position, 0.4f, 12f, 36f, 0.5f);
        level.Displacement.AddBurst(Position, 0.4f, 24f, 48f, 0.5f);
        level.Displacement.AddBurst(Position, 0.4f, 36f, 60f, 0.5f);
        for (float num = 0f; num < (float)Math.PI * 2f; num += 0.17453292f)
        {
            Vector2 position = base.Center + Calc.AngleToVector(num + Calc.Random.Range(-(float)Math.PI / 90f, (float)Math.PI / 90f), Calc.Random.Range(12, 18));
            level.Particles.Emit(Seeker.P_Regen, position, num);
        }
    }

    public void onPlayer(Player player)
    {
        if (player.DashAttacking)
        {
            if (state == StarfishStates.Idle && player.Speed.LengthSquared() > 90 * 90)
            {
                Speed += player.Speed;
                ChangeState(StarfishStates.Platform);
                player.StateMachine.State = Player.StNormal;
                player.Speed *= -1;
            }
        }
    }

    public void ChangeState(StarfishStates state, bool move = true)
    {
        this.state = state;
        switch (state)
        {
            case StarfishStates.Platform:
                Collider.Height = 10;
                if (move) MoveV(5, onCollideV);
                child.Collidable = true;
                child.Visible = true;
                break;
            case StarfishStates.Idle:
                Collider.Height = 20;
                break;
            case StarfishStates.Rising:
                child.Collidable = false;
                child.Visible = false;
                Collider.Width = 10;
                if (move) MoveH(5, onCollideH);
                break;
            default:
                break;
        }
    }

    public override void Render()
    {
        base.Render();
        Draw.Rect(Collider, Color.Red);
    }
}

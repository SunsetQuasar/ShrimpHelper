using System;
using Monocle;
using Microsoft.Xna.Framework;
using System.Reflection;
using Celeste.Mod.Entities;
using Celeste.Mod.ShrimpHelper.Components;
using Celeste.Mod.ShrimpHelper;

namespace Celeste.Mod.ShrimpHelper.Entities;

[CustomEntity("ShrimpHelper/BonkKrill")]
[Tracked]
public class BonkKrill : Actor
{
    private Sprite sprite;
    private Collider bonkCollider;
    private int state = 0;
    private float timer;

    public Vector2 Speed;

    private Collision onCollideH;

    private Collision onCollideV;

    public bool newhits;

    public bool phaseThru;

    private ParticleType platformP = new ParticleType
    {
        Acceleration = Vector2.UnitY * 60f,
        SpeedMin = 5f,
        SpeedMax = 20f,
        Direction = -(float)Math.PI / 2f,
        LifeMin = 0.6f,
        LifeMax = 1.4f,
        FadeMode = ParticleType.FadeModes.Late,
        Size = 1f
    };
    private ParticleType popper = new ParticleType
    {
        Color = Color.White * 0.6f,
        Source = GFX.Game["particles/bubble"],
        FadeMode = ParticleType.FadeModes.Late,
        LifeMin = 0.4f,
        LifeMax = 1f,
        Size = 1f,
        SizeRange = 0.5f,
        SpeedMin = 8f,
        SpeedMax = 16f,
        Acceleration = new Vector2(0f, -24f),
        DirectionRange = (float)Math.PI * 2f
    };


    public BonkKrill(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        Add(sprite = ShrimpHelperModule.ShrimpSpriteBank.Create("krill"));
        Collider = new Hitbox(12, 12, -6, -6);
        phaseThru = data.Bool("walkThrough", false);
        bonkCollider = new Hitbox(16, 12, -8, -8);
        Add(new PlayerCollider(OnPlayerBounce, bonkCollider));
        Add(new PlayerCollider(OnPlayer, new Hitbox(12, 12, -6, -6)));

        onCollideH = OnCollideH;
        onCollideV = OnCollideV;

        timer = Calc.Random.NextFloat() * (float)Math.PI * 2;
        sprite.FlipX = !data.Bool("left", false);
        state = data.Bool("alreadyHitOnce", false) ? 1 : 0;
        sprite.Play(state == 1 ? "idle_fall" : "idle");

        newhits = data.Bool("moreInteractions", false);
    }

    public override void Update()
    {
        Level level = Scene as Level;
        base.Update();
        if (sprite.Scale.Y < 1)
        {
            sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 1, (1 - sprite.Scale.Y) / 6);
        }
        if (sprite.Scale.Y > 1)
        {
            sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 1, -(1 - sprite.Scale.Y) / 6);
        }

        if (sprite.Scale.X > 1)
        {
            sprite.Scale.X = Calc.Approach(sprite.Scale.X, 1, -(1 - sprite.Scale.X) / 8);
        }
        if (sprite.Scale.X < 1)
        {
            sprite.Scale.X = Calc.Approach(sprite.Scale.X, 1, (1 - sprite.Scale.X) / 8);
        }
        timer += Engine.DeltaTime;

        Speed.X = Calc.Approach(Speed.X, 0, 4 * Engine.DeltaTime * 60);

        if (state != 1)
        {
            Speed.Y = Calc.Approach(Speed.Y, 0, 4 * Engine.DeltaTime * 60);
        }

        if (Speed.X != 0)
        {
            MoveH(Speed.X * Engine.DeltaTime, onCollideH);
        }
        if (Speed.Y != 0)
        {
            MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
        }


        if (state == 1)
        {
            if (!OnGround())
            {
                Speed.Y = Speed.Y >= 200 ? 200 : Speed.Y + (4 * Engine.DeltaTime * 60);
            }
            else
            {
                if (Math.Abs(Speed.Y) < 8f)
                {
                    Speed.Y = 0;
                }
            }

            if (base.Top > (float)(level.Bounds.Bottom + 16))
            {
                RemoveSelf();
                return;
            }
        }
        foreach (KrillKollider component in base.Scene.Tracker.GetComponents<KrillKollider>())
        {
            component.Check(this);
        }

        if (base.Top > (float)(level.Bounds.Bottom + 16))
        {
            RemoveSelf();
            return;
        }
    }

    public float[] getSpeedNoXNAFNABullshit()
    {
        return [Speed.X, Speed.Y];
    }

    private void OnCollideH(CollisionData data)
    {
        int dirSign = Math.Sign(Speed.X);

        Level level = Scene as Level;


        if (data.Hit is DashSwitch)
        {
            (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * dirSign);
        }

        if (newhits)
        {
            if (data.Hit is CrushBlock)
            {
                if ((data.Hit as CrushBlock).CanActivate(-Vector2.UnitX * dirSign))
                {
                    if (Math.Abs(Speed.X) >= 120) (data.Hit as CrushBlock).Attack(-Vector2.UnitX * dirSign);
                }
            }

            if (data.Hit is DashBlock)
            {
                if (Math.Abs(Speed.X) >= 120) (data.Hit as DashBlock).Break(Center, Vector2.UnitX * dirSign, true, true);
            }
        }

        if (Speed.X < 0f)
        {
            Audio.Play("event:/new_content/game/10_farewell/glider_wallbounce_left", Position);
        }
        else
        {
            Audio.Play("event:/new_content/game/10_farewell/glider_wallbounce_right", Position);
        }
        Speed.X *= -0.5f;
        sprite.Scale = new Vector2(0.8f, 1.2f);
        float where = Speed.X > 0 ? base.Left : Speed.X < 0 ? base.Right : Position.X;
        for (int i = 0; i < 6; i++)
        {
            level.Particles.Emit(popper, new Vector2(where, Position.Y + Calc.Random.Range(-4, 4)), Color.White * 0.5f);
        }
    }

    private void OnCollideV(CollisionData data)
    {
        Level level = Scene as Level;

        int dirSign = Math.Sign(Speed.Y);

        if (data.Hit is DashSwitch)
        {
            (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
        }

        if (newhits)
        {
            if (data.Hit is CrushBlock)
            {
                if ((data.Hit as CrushBlock).CanActivate(-Vector2.UnitY * dirSign))
                {
                    if (Math.Abs(Speed.Y) >= 120) (data.Hit as CrushBlock).Attack(-Vector2.UnitY * dirSign);
                }
            }

            if (data.Hit is DashBlock)
            {
                if (Math.Abs(Speed.Y) >= 120) (data.Hit as DashBlock).Break(Center, Vector2.UnitY * dirSign, true, true);
            }
        }

        if (Math.Abs(Speed.Y) > 8f)
        {
            sprite.Scale = new Vector2(1.2f, 0.8f);
            Audio.Play("event:/new_content/game/10_farewell/glider_land", Position);
            for (int i = 0; i < 6; i++)
            {
                level.Particles.Emit(popper, new Vector2(Position.X + Calc.Random.Range(-4, 4), base.Bottom), Color.White * 0.5f);
            }

        }
        if (Speed.Y < 0f)
        {
            Speed.Y *= -0.2f;
        }
        else
        {
            Speed.Y = 0f;
        }
    }

    public bool HitSpring(Spring spring)
    {
        if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
        {
            Speed.X *= 0.5f;
            Speed.Y = -160f;
            return true;
        }
        if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
        {
            MoveTowardsY(spring.CenterY + 5f, 4f);
            Speed.X = 220f;
            Speed.Y = -80f;
            sprite.FlipX = true;
            return true;
        }
        if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
        {
            MoveTowardsY(spring.CenterY + 5f, 4f);
            Speed.X = -220f;
            Speed.Y = -80f;
            sprite.FlipX = false;
            return true;
        }
        return false;
    }
    public bool HitKrillSpring(KrillSpring spring)
    {
        if (spring.Orientation == KrillSpring.KrillOrientations.Floor && Speed.Y >= 0f)
        {
            Speed.X *= 0.5f;
            Speed.Y = -160f;
            return true;
        }
        if (spring.Orientation == KrillSpring.KrillOrientations.WallLeft && Speed.X <= 0f)
        {
            MoveTowardsY(spring.CenterY + 5f, 4f);
            Speed.X = 220f;
            Speed.Y = -80f;
            sprite.FlipX = true;
            return true;
        }
        if (spring.Orientation == KrillSpring.KrillOrientations.WallRight && Speed.X >= 0f)
        {
            MoveTowardsY(spring.CenterY + 5f, 4f);
            Speed.X = -220f;
            Speed.Y = -80f;
            sprite.FlipX = false;
            return true;
        }
        if (spring.Orientation == KrillSpring.KrillOrientations.Ceiling && Speed.Y <= 0f)
        {
            Speed.X *= 0.5f;
            Speed.Y = 160f;
            return true;
        }
        return false;
    }
    public override void OnSquish(CollisionData data)
    {
        Explode();
        RemoveSelf();
    }

    private void Explode()
    {
        Collider collider = base.Collider;
        base.Collider = new Circle(40f);
        Audio.Play("event:/new_content/game/10_farewell/puffer_splode", Position);
        Player player = CollideFirst<Player>();
        if (player != null && !base.Scene.CollideCheck<Solid>(Position, player.Center))
        {
            player.ExplodeLaunch(Position, snapUp: false, sidesOnly: false);
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
        for (float num = 0f; num < (float)Math.PI * 2f; num += 0.17453292f) // increments of pi/18, 36 particles
        {
            Vector2 position = base.Center + Calc.AngleToVector(num + Calc.Random.Range(-(float)Math.PI / 90f, (float)Math.PI / 90f), Calc.Random.Range(12, 18));
            level.Particles.Emit(Seeker.P_Regen, position, num);
        }
    }

    public override bool IsRiding(JumpThru jumpThru)
    {
        return false;
    }

    public override bool IsRiding(Solid solid)
    {
        return false;
    }

    private void OnPlayerBounce(Player player)
    {
        Level level = Scene as Level;
        if (player.Bottom <= base.Top + 6f)
        {
            Audio.Play("event:/game/general/thing_booped", Position);
            global::Celeste.Celeste.Freeze(0.1f);
            sprite.Scale.Y = 0.5f;
            sprite.Scale.X = 1.4f;
            player.Bounce(base.Top + 2f);
            state++;
            if (state == 2)
            {
                RemoveSelf();
                for (int i = 0; i < 8; i++)
                {
                    level.Particles.Emit(popper, Position + new Vector2(Calc.Random.Range(-8, 8), Calc.Random.Range(-4, 4)), Color.White, Calc.Random.Range(0, (float)Math.PI * 2));
                }
            }
            else
            {
                sprite.Play("idle_fall");
                for (float i = 0; i < 24; i++)
                {
                    Color col = Calc.HsvToColor(((i / 25) + (timer / 8)) % 1, 0.4f, 1);
                    Vector2 pos = new Vector2(Position.X - 12 + i, Position.Y + (float)Math.Sin((timer * 1.5) + (i / 8)) * 2);
                    float alph = (float)(((i < 1 || i > 22) ? 0.4 : 0.8) + Math.Sin((timer * 8) + (i / 4.5)) * 0.2);
                    level.Particles.Emit(platformP, pos, col * alph);
                }
            }
        }
    }
    private void OnPlayer(Player player)
    {
        if (phaseThru) return;
        player.Die((player.Center - base.Center).SafeNormalize(Vector2.UnitX));
    }
    public override void Render()
    {
        //sprite.DrawOutline();
        base.Render();
        if (state == 0)
        {
            for (float i = 0; i < 24; i++)
            {
                Color col = Calc.HsvToColor(((i / 25) + (timer / 8)) % 1, 0.4f, 1);
                Vector2 pos = new Vector2(Position.X - 12 + i, Position.Y + (float)Math.Sin((timer * 1.5) + (i / 8)) * 2);
                float alph = (float)(((i < 1 || i > 22) ? 0.4 : 0.8) + Math.Sin((timer * 8) + (i / 4.5)) * 0.2);
                Draw.Point(pos, col * alph);
            };
        }
    }

    public static void Load()
    {
        On.Celeste.Spring.ctor_Vector2_Orientations_bool += Spring_ctor_Vector2_Orientations_bool;
    }

    public static void Unload()
    {
        On.Celeste.Spring.ctor_Vector2_Orientations_bool -= Spring_ctor_Vector2_Orientations_bool;
    }

    private static void Spring_ctor_Vector2_Orientations_bool(On.Celeste.Spring.orig_ctor_Vector2_Orientations_bool orig, Spring self, Vector2 position, Spring.Orientations orientation, bool playerCanUse)
    {
        orig(self, position, orientation, playerCanUse);
        var collider = new KrillKollider((p) =>
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

}

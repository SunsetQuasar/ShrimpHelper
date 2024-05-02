using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod;
using System.Collections;
using System.Reflection;
using Celeste.Mod.Entities;

namespace Celeste.Mod.ShrimpHelper.Entities;

[CustomEntity("ShrimpHelper/Sprimp")]
[Tracked]
public class Sprimp : Entity
{
    private Sprite sprite;
    private bool grabbed;
    private bool left;
    private float cooldownTimer;
    private bool oneUse;
    private float timer;
    public Sprimp(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        left = data.Bool("left");
        oneUse = data.Bool("oneUse", false);
        Add(sprite = ShrimpHelperModule.ShrimpSpriteBank.Create("sprimp"));
        if (left)
        {
            sprite.FlipX = true;
        }
        Collider = new Hitbox(32f, 4f, -16f, 12f);
        Add(new PlayerCollider(OnPlayer, Collider));
        Depth = 100;
    }

    public override void Update()
    {
        base.Update();

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Engine.DeltaTime;
        }

        timer += Engine.DeltaTime;
        if (oneUse)
        {
            sprite.Color = Color.Lerp(Color.White, Calc.HexToColor("A09797"), (float)(Math.Sin(timer * 8) + 1) / 2);

            if (Scene.OnInterval(0.02f))
            {
                (Scene as Level).ParticlesBG.Emit(Seeker.P_Attack, 1, Position, new Vector2(2, 2), Color.White, Calc.Random.Range(0, (float)Math.PI * 2));
                if (Calc.Random.Chance(0.2f))
                {
                    sprite.RenderPosition = Position;
                }
                else
                {
                    sprite.RenderPosition = Position + new Vector2(Calc.Random.Range(-1f, 1f), Calc.Random.Range(-1f, 1f));
                }

            }

        }

    }
    private void OnPlayer(Player player)
    {

        if (player != null && Input.Grab.Check && !grabbed && cooldownTimer <= 0f && player.Stamina > 20)
        {
            grabbed = true;
            sprite.Play("grab");
            player.MoveToX(CenterX);
            player.MoveToY(Bottom + 10f);
            player.Speed = Vector2.Zero;
            player.Sprite.Play("fallSlow_carry");
            player.StateMachine.State = 11;
            player.DummyMoving = false;
            player.DummyMaxspeed = false;
            player.DummyGravity = false;
            player.DummyFriction = false;
            player.ForceCameraUpdate = true;
            player.DummyAutoAnimate = false;
            Add(new Coroutine(PlayerLaunchRoutine(player)));
        }
        else if (grabbed && player != null && Input.Grab.Check && Input.Jump.Pressed)
        {
            player.Jump();
            grabbed = false;
            sprite.Play("idle");
            if (player.Facing == Facings.Right)
            {
                Audio.Play("event:/char/madeline/jump_climb_right");
                Dust.Burst(base.Center + Vector2.UnitX * 2f, (float)Math.PI * -3f / 4f, 4, ParticleTypes.Dust);
            }
            else
            {
                Audio.Play("event:/char/madeline/jump_climb_left");
                Dust.Burst(base.Center + Vector2.UnitX * -2f, -(float)Math.PI / 4f, 4, ParticleTypes.Dust);
            }
            player.Stamina -= 27.5f;
            player.StateMachine.State = 0;
            cooldownTimer = 0.7f;
        }
        else if (grabbed && player != null && !Input.Grab.Check)
        {
            grabbed = false;
            sprite.Play("idle");
            player.StateMachine.State = 0;
        }
    }

    private IEnumerator PlayerLaunchRoutine(Player player)
    {
        yield return 0.2f;
        sprite.Play("launch");
        while (grabbed && sprite.CurrentAnimationID == "launch")
        {
            player.Speed.X = Calc.Approach(player.Speed.X, -200f * (left ? -1 : 1), 4000f * Engine.DeltaTime);
            player.Speed.Y = Calc.Approach(player.Speed.Y, -300f, 6000f * Engine.DeltaTime);
            yield return null;
        }
        if (grabbed != false)
        {
            Audio.Play("event:/char/madeline/jump_superwall");
            SceneAs<Level>().Add(Engine.Pooler.Create<SpeedRing>().Init(player.Center, player.Speed.Angle(), Color.White));
        }
        if (oneUse && grabbed != false)
        {
            FuckingDie();
        }
        cooldownTimer = 0.7f;
        grabbed = false;
        sprite.Play("idle");
        player.StateMachine.State = 0;

    }

    private void FuckingDie()
    {
        Audio.Play("event:/char/badeline/temple_move_chats", Position);
        Level level = Scene as Level;
        level.Add(new ShrisperseImage(Position, new Vector2(Calc.Random.Range(-0.2f, 0.2f), Calc.Random.Range(-0.2f, 0.2f)), sprite.Origin, left ? (sprite.Scale * new Vector2(-1, 1)) : sprite.Scale, GFX.Game["sprimp/SC2023/ShrimpHelper/asset/dissipate"], sprite.Color));
        RemoveSelf();
    }
    public override void Render()
    {
        sprite.DrawOutline();
        base.Render();
    }
}

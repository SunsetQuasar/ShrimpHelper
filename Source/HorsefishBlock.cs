using Celeste.Mod.Entities;
using Iced.Intel;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.ShrimpHelper;

[CustomEntity("ShrimpHelper/HorsefishBlock")]
public class HorsefishBlock : Solid
{
    public bool destroyStaticMovers;

    public HorsefishBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
    {
        OnDashCollide = OnDashed;
        destroyStaticMovers = data.Bool("destroyStaticMovers", true);
        Add(new Coroutine(Sequence()));
    }

    public override void Update()
    {
        base.Update();
    }

    public IEnumerator Sequence()
    {
        bool impact = false;
        float speed = 0f;
        while (true)
        {
            while (!FallingCheck())
            {
                speed = Calc.Approach(speed, 0, 500f * Engine.DeltaTime);
                if (MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true))
                {
                    impact = true;
                    break;
                }
                yield return null;
            }
            if (!impact)
            {
                Audio.Play("event:/game/06_reflection/fallblock_boss_shake", base.Center);
                StartShaking();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                yield return 0.6f;
                StopShaking();
                while (true)
                {
                    speed = Calc.Approach(speed, 130, 500f * Engine.DeltaTime);
                    if (!FallingCheck())
                    {
                        break;
                    }
                    if (MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true))
                    {
                        impact = true;
                        break;
                    }
                    yield return null;
                }
            }
            if (impact)
            {
                Audio.Play("event:/game/06_reflection/fallblock_boss_impact", base.BottomCenter);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                SceneAs<Level>().DirectionalShake(Vector2.UnitY, 0.2f);
                StartShaking();
                LandParticles();
                yield return 0.2f;
                StopShaking();
                Safe = true;
                break;
            }
        }
    }

    public DashCollisionResults OnDashed(Player player, Vector2 direction)
    {
        DashCollisionResults result;
        if (direction.Y > 0)
        {
            player.StateMachine.State = Player.StNormal;
            player.Speed.Y *= -1f;
            Celeste.Freeze(0.03f);
            if (Input.Jump.Check)
            {
                player.varJumpSpeed = player.Speed.Y;
                player.varJumpTimer = 0.3f;
                player.Play("event:/char/madeline/jump_assisted");
                player.launched = true;
            }
            result = DashCollisionResults.Ignore;
        }
        else
        {
            result = DashCollisionResults.Rebound;
        }
        Break(player.Center, direction);
        DestroyStaticMovers();
        player.RefillDash();
        return result;
    }

    public void Break(Vector2 from, Vector2 direction)
    {
        Audio.Play("event:/game/general/wall_break_stone", Position);

        for (int i = 1; (float)i < (base.Width / 8f) - 1; i++)
        {
            for (int j = 1; (float)j < (base.Height / 8f) - 1; j++)
            {
                base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), 'n', true).BlastFrom(from));
            }
        }
        RemoveSelf();
    }

    public bool FallingCheck()
    {
        return (HasPlayerOnTop() || HasPlayerClimbing() || HasRider());
    }

    private void LandParticles()
    {
        for (int i = 2; (float)i <= base.Width; i += 4)
        {
            if (base.Scene.CollideCheck<Solid>(base.BottomLeft + new Vector2(i, 3f)))
            {
                SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_FallDustA, 1, new Vector2(base.X + (float)i, base.Bottom), Vector2.One * 4f, -MathF.PI / 2f);
                float direction = ((!((float)i < base.Width / 2f)) ? 0f : MathF.PI);
                SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_LandDust, 1, new Vector2(base.X + (float)i, base.Bottom), Vector2.One * 4f, direction);
            }
        }
    }


    public override void Render()
    {
        base.Render();
        Position += shakeAmount;
        Draw.HollowRect(Collider, Color.Plum);
        Position -= shakeAmount;
    }
}

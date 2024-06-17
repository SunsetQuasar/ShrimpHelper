// Celeste.TempleGate
using System;
using System.Collections;
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;


namespace Celeste.Mod.ShrimpHelper.Entities;

[Tracked(false)]
[CustomEntity("ShrimpHelper/ShreoGate")]
public class ShreoGate : Solid
{
    public struct Dust
    {
        public Vector2 Position;
        public float percent;
        public float ageSpeed;
    }

    public Dust[] dust;

    public bool open;

    public float opacity;

    public bool ignoreShreoToTheRight;

    public bool openByDefault;

    public ShreoGate(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, true)
    {
        SurfaceSoundIndex = 11;
        open = false;
        opacity = 0.9f;
        Depth = -5000;
        dust = new Dust[(data.Width / 4) * (data.Height / 4)];
        Add(new DisplacementRenderHook(OnDisplacementRender));
        for (int i = 0; i < dust.Length; i++)
        {
            dust[i].Position = new Vector2(X + Calc.Random.Range(0, data.Width), Y + Calc.Random.Range(0, data.Height));
            dust[i].percent = 0;
            dust[i].ageSpeed = Calc.Random.Range(1f, 2f);
        }
        ignoreShreoToTheRight = data.Bool("ignoreRightShreo", false);
        openByDefault = data.Bool("openByDefault", true);
    }
    public override void Update()
    {
        base.Update();
        if (TheoIsNearby() && !open)
        {
            open = true;
            Collidable = false;
        }
        else if (!TheoIsNearby() && open)
        {
            open = false;
            Collidable = true;
        }

        if (open)
        {
            opacity = Calc.Approach(opacity, 0, Engine.DeltaTime * 4);
        }
        else
        {
            opacity = Calc.Approach(opacity, 1, Engine.DeltaTime * 4);
        }
        for (int i = 0; i < dust.Length; i++)
        {
            dust[i].percent += Engine.DeltaTime * dust[i].ageSpeed;
            if (dust[i].percent >= 1)
            {
                dust[i].Position = new Vector2(X + Calc.Random.Range(0, Width), Y + Calc.Random.Range(0, Height));
                dust[i].percent = 0;
                dust[i].ageSpeed = Calc.Random.Range(1f, 2f);
            }
        }
    }

    public override void Render()
    {
        base.Render();

        float opacity2 = Calc.ClampedMap(Ease.CubeInOut(opacity), 0, 1, 0.3f, 0.9f);

        Color barrier = Calc.HexToColor("BBBB77") * opacity2;
        barrier.A /= 3;
        Draw.Rect(X, Y, Width, Height, barrier);
        Color dustColor;
        for (int i = 0; i < dust.Length; i++)
        {
            dustColor = Calc.HexToColor("FFFFFF") * (float)Math.Sin(dust[i].percent * Math.PI);
            dustColor.A = 0;
            Draw.Rect(dust[i].Position, 1, 1, dustColor);
        }
        MTexture tex = GFX.Game["characters/theoCrystal/SC2023/shrimphelper/gateIcon"];
        tex.DrawCentered(Position + new Vector2(base.Width / 2f, Height / 2), Color.White * opacity2);
    }

    public bool TheoIsNearby()
    {
        ShreoCrystal entity = base.Scene.Tracker.GetEntity<ShreoCrystal>();
        if (entity != null)
        {
            if (!ignoreShreoToTheRight)
            {
                if (!(entity.X > base.X + 10f)) return Vector2.DistanceSquared(Position + new Vector2(base.Width / 2f, Height / 2), entity.Center) < (open ? 6400f : 4096f);
            }
            else
            {
                return Vector2.DistanceSquared(Position + new Vector2(base.Width / 2f, Height / 2), entity.Center) < (open ? 6400f : 4096f);
            }
        } else return openByDefault;
        return true;
    }
    private void OnDisplacementRender()
    {
        Draw.Rect(X, Y, Width, Height, new Color(0.5f, 0.5f, 0.2f, 1f));

    }
}

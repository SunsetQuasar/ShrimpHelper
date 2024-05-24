using Celeste.Mod.ShrimpHelper.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.ShrimpHelper.Entities.StarfishGuy;
using static Celeste.Spring;

namespace Celeste.Mod.ShrimpHelper;

[Tracked]
[CustomEntity("ShrimpHelper/LeapPrawn")]
public class LeapPrawn : Actor
{
    public Vector2 GravDir;

    public Vector2 Speed;

    public float JumpSpeed;
    public float VarJumpTime;

    public Vector2 VarJumpSpeed;

    public bool launched;

    public float launchedTimer;

    public float GravAccel => (Speed * GravDir).LengthSquared() <= 40*40 ? 450f : 900f;
    public const float MaxFall = 160f;

    public VertexLight Light;

    public class PrawnPlatform : JumpThru
    {
        public LeapPrawn parent;

        public Vector2 Target => parent.TopLeft - (Vector2.UnitY * 4);
        public PrawnPlatform(LeapPrawn parent) : base(parent.TopLeft - (Vector2.UnitY * 4), 16, false)
        {
            this.parent = parent;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            MoveTo(Target, parent.Speed);
        }

        public override void Update()
        {
            base.Update();
            MoveTo(Target, parent.Speed);
        }

        public override void Render()
        {
            base.Render();
            Draw.Rect(Target, 16, 4, Color.IndianRed);
        }
    }

    public class PrawnSolid : Solid
    {
        public LeapPrawn parent;

        public static Vector2 Target(LeapPrawn parent) 
        {
            if (parent.GravDir == new Vector2(0, 1))
            {
                return parent.TopLeft - (Vector2.UnitY * 8);
            }
            if (parent.GravDir == new Vector2(0, -1))
            {
                return parent.BottomLeft + (Vector2.UnitY * 2);
            }
            if (parent.GravDir == new Vector2(1, 0))
            {
                return parent.TopLeft - (Vector2.UnitX * 8);
            }
            return parent.TopRight + (Vector2.UnitX * 2);
        }

        public PrawnSolid(LeapPrawn parent) : base(Target(parent), 0, 0, false)
        {
            this.parent = parent;

            if (parent.GravDir == new Vector2(1, 0) || parent.GravDir == new Vector2(-1, 0))
            {
                Collider = new Hitbox(6, 16);
            } else
            {
                Collider = new Hitbox(16, 6);
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            MoveToNaive(Target(parent));
        }

        public override void Update()
        {
            base.Update();
            MoveTo(Target(parent), parent.Speed);
        }

        public override void Render()
        {
            base.Render();
            Draw.Rect(Collider, Color.IndianRed);
        }
    }

    public class PrawnSpring : Spring
    {
        public LeapPrawn parent;
        public Vector2 Target => Orientation switch
        {
            Orientations.WallLeft => parent.CenterRight,
            Orientations.WallRight => parent.CenterLeft,
            _ => parent.TopCenter
        };

        public PrawnSpring(LeapPrawn parent) : base(parent.CenterLeft, GravToOrientation(parent), true)
        {
            this.parent = parent;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Position = Target;
        }

        public override void Update()
        {
            base.Update();
            Position = Target;
        }
    }
    public class PrawnSpringDown : KrillSpring
    {
        public LeapPrawn parent;
        public Vector2 Target => parent.BottomCenter;

        public PrawnSpringDown(LeapPrawn parent) : base(parent.BottomCenter, KrillOrientations.Ceiling, true)
        {
            this.parent = parent;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Position = Target;
        }
        public override void Update()
        {
            base.Update();
            Position = Target;
        }
    }

    public enum Types
    {
        Normal = 0,
        Spiky = 1,
        Platform = 2,
        Spring = 3,
        Solid = 4
    }

    public Types PrawnType;

    public PrawnPlatform PlatformChild;
    public PrawnSolid SolidChild;
    public PrawnSpring SpringChild;
    public PrawnSpringDown SpringDownChild;

    public PlayerCollider SpikyOrBounce;

    private Collision onCollideH;

    private Collision onCollideV;

    public float Delay;
    public bool Jumping;

    public static Spring.Orientations GravToOrientation(LeapPrawn prawn)
    {
        if (prawn.GravDir == Vector2.UnitX) return Spring.Orientations.WallRight;
        if (prawn.GravDir == Vector2.UnitX * -1) return Spring.Orientations.WallLeft;
        return Spring.Orientations.Floor;
    }

    public bool Grounded()
    {
        if (!CollideCheck<Solid>(Position + GravDir))
        {
            if (!IgnoreJumpThrus)
            {
                return CollideCheckOutside<JumpThru>(Position + GravDir * 2);
            }
            return false;
        }
        return true;
    }

    public LeapPrawn(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Collider = new Hitbox(16, 16, -8, -8);
        Add(Light = new VertexLight(Calc.HexToColor("FF0000"), 1f, 24, 32));
        Add(new PlayerCollider(OnPlayer, Collider));
        PrawnType = (Types)data.Int("type", 0);
        JumpSpeed = data.Float("jumpSpeed", 105);
        VarJumpTime = data.Float("varJumpTime", 0.2f);
        GravDir = data.Int("gravityDirection", 0) switch {
            0 => new Vector2(0, 1),  //down

            1 => new Vector2(1, 0),  //right

            2 => new Vector2(-1, 0), //left

            _ => new Vector2(0, -1), //up
        };
        onCollideH = OnCollideH;
        onCollideV = OnCollideV;

        Delay = data.Float("delay", 0f);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        switch (PrawnType)
        {
            case Types.Spring:
                if (GravDir == Vector2.UnitY * -1)
                {
                    Scene.Add(new PrawnSpringDown(this));
                } else
                {
                    Scene.Add(new PrawnSpring(this));
                }
                break;
            case Types.Platform:
                Scene.Add(new PrawnPlatform(this));
                break;
            case Types.Solid:
                Scene.Add(new PrawnSolid(this));
                break;
            default:
                Add(new PlayerCollider(OnPlayerSpecial, new Hitbox(16, 4, -8, -12)));
                break;
        }
    }

    public override void Update()
    {

        Level level = Scene as Level;

        if (launched)
        {
            if (Speed.LengthSquared() < 19600f)
            {
                launched = false;
            }
            else
            {
                float prevVal = launchedTimer;
                launchedTimer += Engine.DeltaTime;
                if (launchedTimer >= 0.5f)
                {
                    launched = false;
                    launchedTimer = 0f;
                }
                else if (Calc.OnInterval(launchedTimer, prevVal, 0.15f))
                {
                    level.Add(Engine.Pooler.Create<SpeedRing>().Init(base.Center, Speed.Angle(), Color.White));
                }
            }
        }
        else
        {
            launchedTimer = 0f;
        }

        Speed = Calc.Approach(Speed, GravDir * MaxFall, GravAccel * Engine.DeltaTime);

        MoveH(Speed.X * Engine.DeltaTime, onCollideH);
        MoveV(Speed.Y * Engine.DeltaTime, onCollideV);

        base.Update();

        if (PlatformChild != null)
        {
            PlatformChild.MoveTo(PlatformChild.Target, Speed);
        }

        if (SpringChild != null)
        {
            SpringChild.Position = Position;
        }

        if (SpringDownChild != null)
        {
            SpringDownChild.Position = Position;
        }
    }
    private void OnCollideH(CollisionData data)
    {
        Speed.X = 0;
    }

    private void OnCollideV(CollisionData data)
    {
        Speed.Y = 0;
    }

    public void OnPlayer(Player player)
    {
        player.Die((Position - player.Position).SafeNormalize());
    }

    public void OnPlayerSpecial(Player player)
    {

    }

    private bool LaunchedBoostCheck()
    {
        if (LiftSpeed.LengthSquared() >= 10000f && Speed.LengthSquared() >= 48400f)
        {
            launched = true;
            return true;
        }
        launched = false;
        return false;
    }

    public void Jump()
    {
        Add(new Coroutine(JumpRoutine()));
    }

    public IEnumerator JumpRoutine()
    {
        Jumping = true;
        float wait = Delay;
        yield return wait;
        Speed = -JumpSpeed * GravDir;
        Speed += LiftSpeed;
        VarJumpSpeed = Speed;

        LaunchedBoostCheck();

        if (launched)
        {
            Audio.Play("event:/char/madeline/jump_assisted", Position).setPitch(Calc.Random.Range(0.9f, 1.1f));
        }
        else
        {
            Audio.Play("event:/char/madeline/jump", Position).setPitch(Calc.Random.Range(0.9f, 1.1f));
        }

        float time = 0.2f;
        while (time > 0)
        {
            Speed = VarJumpSpeed;
            time -= Engine.DeltaTime;
            yield return null;
        }
        Jumping = false;
    }

    public override void Render()
    {
        base.Render();
        Draw.HollowRect(Collider, Color.Coral * 0.9f);
    }

    public static void Load()
    {
        On.Celeste.Player.Jump += Player_Jump;
        On.Celeste.Player.SuperJump += Player_SuperJump;
        On.Celeste.Player.WallJump += Player_WallJump;
        On.Celeste.Player.SuperWallJump += Player_SuperWallJump;
        On.Celeste.Actor.MoveH += Actor_MoveH;
        On.Celeste.Actor.MoveV += Actor_MoveV;
    }

    private static bool Actor_MoveV(On.Celeste.Actor.orig_MoveV orig, Actor self, float moveV, Collision onCollide, Solid pusher)
    {
        LeapPrawn l = self as LeapPrawn;
        if (l == null)
        {
            return orig(self, moveV, onCollide, pusher);
        } else if (l.SolidChild != null)
        {
            l.SolidChild.Collidable = false;
            bool result = orig(self, moveV, onCollide, pusher);
            l.SolidChild.Collidable = true;
            return result;
        }
        return orig(self, moveV, onCollide, pusher);
    }

    private static bool Actor_MoveH(On.Celeste.Actor.orig_MoveH orig, Actor self, float moveH, Collision onCollide, Solid pusher)
    {
        LeapPrawn l = self as LeapPrawn;
        if (l == null)
        {
            return orig(self, moveH, onCollide, pusher);
        }
        else if (l.SolidChild != null)
        {
            l.SolidChild.Collidable = false;
            bool result = orig(self, moveH, onCollide, pusher);
            l.SolidChild.Collidable = true;
            return result;
        }
        return orig(self, moveH, onCollide, pusher);
    }

    public static void Unload()
    {
        On.Celeste.Player.Jump -= Player_Jump;
        On.Celeste.Player.SuperJump -= Player_SuperJump;
        On.Celeste.Player.WallJump -= Player_WallJump;
        On.Celeste.Player.SuperWallJump -= Player_SuperWallJump;
        On.Celeste.Actor.MoveH -= Actor_MoveH;
        On.Celeste.Actor.MoveV -= Actor_MoveV;
    }
    private static void Player_Jump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx)
    {
        orig(self, particles, playSfx);

        foreach(LeapPrawn p in self.Scene.Tracker.GetEntities<LeapPrawn>())
        {
            if(p.Grounded() && !p.Jumping) p.Jump();
        }
    }
    private static void Player_SuperJump(On.Celeste.Player.orig_SuperJump orig, Player self)
    {
        orig(self);

        foreach (LeapPrawn p in self.Scene.Tracker.GetEntities<LeapPrawn>())
        {
            if (p.Grounded() && !p.Jumping) p.Jump();
        }
    }

    private static void Player_SuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir)
    {
        orig(self, dir);

        foreach (LeapPrawn p in self.Scene.Tracker.GetEntities<LeapPrawn>())
        {
            if (p.Grounded() && !p.Jumping) p.Jump();
        }
    }

    private static void Player_WallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir)
    {
        orig(self, dir);

        foreach (LeapPrawn p in self.Scene.Tracker.GetEntities<LeapPrawn>())
        {
            if (p.Grounded() && !p.Jumping) p.Jump();
        }
    }
}

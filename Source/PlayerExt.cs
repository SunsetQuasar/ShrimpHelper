using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.ShrimpHelper;
public class PlayerExt : Component
{
    public Player player;

    public bool HasPearlDash = false;

    public PlayerExt(Player player) : base(true, false)
    {
        this.player = player;
    }

    public override void Update()
    {
        if (HasPearlDash)
        {
            player.dashAttackTimer = 1f;
        }
    }

    public static void Load()
    {
        On.Celeste.Player.Added += Player_Added;
        On.Celeste.Player.DashBegin += Player_DashBegin;
    }

    public static void Unload()
    {
        On.Celeste.Player.Added -= Player_Added;
        On.Celeste.Player.DashBegin -= Player_DashBegin;
    }



    private static void Player_Added(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
    {
        orig(self, scene);
        self.Add(new PlayerExt(self));
    }
    private static void Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        orig(self);
        PlayerExt e = self.Get<PlayerExt>();
        if (e != null && e.HasPearlDash)
        {
            e.HasPearlDash = false;
        }
    }
}

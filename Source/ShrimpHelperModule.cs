using Celeste.Mod.ShrimpHelper.Entities;
using Monocle;
using System;

namespace Celeste.Mod.ShrimpHelper;

public class ShrimpHelperModule : EverestModule {
    public static ShrimpHelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(ShrimpHelperModuleSettings);
    public static ShrimpHelperModuleSettings Settings => (ShrimpHelperModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(ShrimpHelperModuleSession);
    public static ShrimpHelperModuleSession Session => (ShrimpHelperModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(ShrimpHelperModuleSaveData);
    public static ShrimpHelperModuleSaveData SaveData => (ShrimpHelperModuleSaveData) Instance._SaveData;

    public static SpriteBank ShrimpSpriteBank;

    public ShrimpHelperModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(ShrimpHelperModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(ShrimpHelperModule), LogLevel.Info);
#endif
    }
    public override void LoadContent(bool firstLoad)
    {
        ShrimpSpriteBank = new SpriteBank(GFX.Game, "Graphics/SC2023xmls/ShrimpHelper/CustomSprites.xml");
    }

    public override void Load() {
        BonkKrill.Load();
    }

    public override void Unload() {
        BonkKrill.Unload();
    }
}
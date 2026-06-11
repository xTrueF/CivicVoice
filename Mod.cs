// ============================================================
// CivicVoice — Democracy & Governance Mod for Cities: Skylines II
// Created by xTrueF | github.com/xTrueF/CivicVoice
// Licensed under MIT License
// ============================================================
using CivicVoice.Systems;
using CivicVoice.UI;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;


namespace CivicVoice
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(CivicVoice)}.{nameof(Mod)}")
            .SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");

            // Register our systems with CS2's update loop
            updateSystem.UpdateAt<CivicVoiceSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<CivicVoiceUISystem>(SystemUpdatePhase.UIUpdate);

            log.Info("CivicVoice systems registered.");
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
        }
    }
}
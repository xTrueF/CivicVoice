// ============================================================
// CivicVoice — Democracy & Governance Mod for Cities: Skylines II
// Created by xTrueF | github.com/xTrueF/CivicVoice
// Licensed under MIT License
// ============================================================
using CivicVoice.Systems;
using CivicVoice.UI;
using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Game.Settings;
using System.Collections.Generic;

namespace CivicVoice
{
    [FileLocation("CivicVoice")]
    [SettingsUITabOrder(CivicVoiceSettings.kGeneralTab, CivicVoiceSettings.kProposalsTab, CivicVoiceSettings.kElectionsTab)]
    [SettingsUIGroupOrder(CivicVoiceSettings.kUIGroup, CivicVoiceSettings.kElectionGroup, CivicVoiceSettings.kResetGroup, CivicVoiceSettings.kThresholdsGroup, CivicVoiceSettings.kProjectsGroup, CivicVoiceSettings.kElectionsActionGroup)]
    [SettingsUIShowGroupName(CivicVoiceSettings.kUIGroup, CivicVoiceSettings.kElectionGroup, CivicVoiceSettings.kResetGroup, CivicVoiceSettings.kThresholdsGroup, CivicVoiceSettings.kProjectsGroup, CivicVoiceSettings.kElectionsActionGroup)]
    public class CivicVoiceSettings : ModSetting
    {
        // Tabs
        public const string kGeneralTab = "General";
        public const string kProposalsTab = "Proposals";
        public const string kElectionsTab = "Elections";

        // Groups
        public const string kUIGroup = "UI";
        public const string kElectionGroup = "Elections";
        public const string kThresholdsGroup = "Thresholds";
        public const string kProjectsGroup = "Projects";
        public const string kElectionsActionGroup = "ElectionsActions";
        public const string kResetGroup = "Reset";

        public CivicVoiceSettings(IMod mod) : base(mod) { }

        // ── General Tab ───────────────────────────────────────────────────────

        [SettingsUISection(kGeneralTab, kUIGroup)]
        public bool UseUniversalModMenu { get; set; } = false;

        [SettingsUISection(kGeneralTab, kUIGroup)]
        public bool ShowNotifications { get; set; } = true;

        [SettingsUISection(kGeneralTab, kResetGroup)]
        [SettingsUIButton]
        [SettingsUIConfirmation]
        public bool ResetToDefaults
        {
            get => false;
            set { if (value) SetDefaults(); }
        }


        // ── Proposals Tab ─────────────────────────────────────────────────────

        [SettingsUISection(kProposalsTab, kThresholdsGroup)]
        [SettingsUISlider(min = 1, max = 30, step = 1)]
        public int UnemploymentThreshold { get; set; } = 10;

        [SettingsUISection(kProposalsTab, kThresholdsGroup)]
        [SettingsUISlider(min = 1, max = 50, step = 1)]
        public int HomelessThreshold { get; set; } = 10;

        [SettingsUISection(kProposalsTab, kThresholdsGroup)]
        [SettingsUISlider(min = 10, max = 100, step = 5)]
        public int CrimeRateThreshold { get; set; } = 50;

        [SettingsUISection(kProposalsTab, kThresholdsGroup)]
        [SettingsUISlider(min = 10, max = 90, step = 5)]
        public int HousingDemandThreshold { get; set; } = 85;

        [SettingsUISection(kProposalsTab, kThresholdsGroup)]
        [SettingsUISlider(min = 10, max = 80, step = 5)]
        public int HealthThreshold { get; set; } = 45;

        [SettingsUISection(kProposalsTab, kThresholdsGroup)]
        [SettingsUISlider(min = 10, max = 80, step = 5)]
        public int WellbeingThreshold { get; set; } = 50;

        [SettingsUISection(kProposalsTab, kProjectsGroup)]
        [SettingsUISlider(min = 1, max = 6, step = 1)]
        public int MaxActiveMetricProposals { get; set; } = 3;

        [SettingsUISection(kProposalsTab, kProjectsGroup)]
        [SettingsUISlider(min = 1, max = 6, step = 1)]
        public int MaxActiveAdHocProposals { get; set; } = 2;

        [SettingsUISection(kProposalsTab, kProjectsGroup)]
        [SettingsUISlider(min = 1, max = 4, step = 1)]
        public int MaxActiveMajorProposals { get; set; } = 2;

        [SettingsUISection(kProposalsTab, kProjectsGroup)]
        [SettingsUISlider(min = 100, max = 2000, step = 100)]
        public int MajorProjectMinPopulation { get; set; } = 500;

        [SettingsUISection(kProposalsTab, kProjectsGroup)]
        [SettingsUISlider(min = 1, max = 24, step = 1)]
        public int AdHocCooldownMonths { get; set; } = 6;

        [SettingsUISection(kProposalsTab, kProjectsGroup)]
        [SettingsUISlider(min = 1, max = 24, step = 1)]
        public int RejectedCooldownMonths { get; set; } = 6;

        [SettingsUISection(kProposalsTab, kProjectsGroup)]
        [SettingsUISlider(min = 1, max = 12, step = 1)]
        public int MetricProposalCooldownMonths { get; set; } = 2;

        // ── Election Tab ─────────────────────────────────────────────────────────

        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(kElectionsTab, kElectionsActionGroup)]
        public bool ForceElection
        {
            get => false;
            set { if (value) Mod.ForceElectionRequested = true; }
        }

        [SettingsUISection(kElectionsTab, kElectionGroup)]
        [SettingsUISlider(min = 1, max = 24, step = 1)]
        public int ElectionFrequencyMonths { get; set; } = 12;

        [SettingsUISection(kElectionsTab, kElectionGroup)]
        [SettingsUISlider(min = 50, max = 1000, step = 50)]
        public int MinPopulationForElection { get; set; } = 100;

        [SettingsUISection(kElectionsTab, kElectionGroup)]
        [SettingsUISlider(min = 1, max = 20, step = 1)]
        public int EndorsementInfluencePercent { get; set; } = 5;

        [SettingsUIButton]
        [SettingsUIConfirmation]
        [SettingsUISection(kElectionsTab, kElectionsActionGroup)]
        public bool ConcludeElection
        {
            get => false;
            set { if (value) Mod.ConcludeElectionRequested = true; }
        }

        public override void SetDefaults()
        {
            UseUniversalModMenu = false;
            ShowNotifications = true;
            ElectionFrequencyMonths = 12;
            MinPopulationForElection = 100;
            UnemploymentThreshold = 10;
            HomelessThreshold = 10;
            CrimeRateThreshold = 50;
            HousingDemandThreshold = 70;
            HealthThreshold = 45;
            WellbeingThreshold = 50;
            MaxActiveMetricProposals = 3;
            MaxActiveAdHocProposals = 3;
            MaxActiveMajorProposals = 2;
            MajorProjectMinPopulation = 500;
            AdHocCooldownMonths = 3;
            RejectedCooldownMonths = 3;
            EndorsementInfluencePercent = 5;
            MetricProposalCooldownMonths = 2;
        }
    }

    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(CivicVoice)}.{nameof(Mod)}")
            .SetShowsErrorsInUI(false);

        public static CivicVoiceSettings? Settings { get; private set; }

        public static bool ForceElectionRequested { get; set; } = false;

        public static bool ConcludeElectionRequested { get; set; } = false;

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");

            Settings = new CivicVoiceSettings(this);
            Settings.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(Settings));
            AssetDatabase.global.LoadSettings("CivicVoiceSettings", Settings, new CivicVoiceSettings(this));

            updateSystem.UpdateAt<CivicVoiceSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAt<CivicVoiceUISystem>(SystemUpdatePhase.UIUpdate);

            log.Info("CivicVoice systems registered.");
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
            if (Settings != null)
                Settings.UnregisterInOptionsUI();
        }
    }
}
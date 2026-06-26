using Colossal;
using Colossal.IO.AssetDatabase.Internal;
using System.Collections.Generic;

namespace CivicVoice
{
    public class LocaleEN : IDictionarySource
    {
        private readonly CivicVoiceSettings _settings;

        public LocaleEN(CivicVoiceSettings settings)
        {
            _settings = settings;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
{
    { _settings.GetSettingsLocaleID(), "Civic Voice" },

    { _settings.GetOptionTabLocaleID(CivicVoiceSettings.kGeneralTab), "General" },
    { _settings.GetOptionTabLocaleID(CivicVoiceSettings.kProposalsTab), "Proposals" },
    { _settings.GetOptionTabLocaleID(CivicVoiceSettings.kElectionsTab), "Elections" },

    { _settings.GetOptionGroupLocaleID(CivicVoiceSettings.kUIGroup), "Interface" },
    { _settings.GetOptionGroupLocaleID(CivicVoiceSettings.kElectionGroup), "Elections" },
    { _settings.GetOptionGroupLocaleID(CivicVoiceSettings.kThresholdsGroup), "Proposal Thresholds" },
    { _settings.GetOptionGroupLocaleID(CivicVoiceSettings.kProjectsGroup), "Project Limits" },
    { _settings.GetOptionGroupLocaleID(CivicVoiceSettings.kApprovalWeightsGroup), "Approval Weights" },
    { _settings.GetOptionGroupLocaleID(CivicVoiceSettings.kElectionsActionGroup), "Election Actions" },
    { _settings.GetOptionGroupLocaleID(CivicVoiceSettings.kResetGroup), "Reset" },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.UseUniversalModMenu)), "Use Universal Mod Menu" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.UseUniversalModMenu)), "Show Civic Voice in the Universal Mod Menu instead of the top right toolbar." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.ShowNotifications)), "Show Notifications" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.ShowNotifications)), "Show notifications when proposals are added or elections begin." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.ElectionFrequencyMonths)), "Election Frequency (months)" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.ElectionFrequencyMonths)), "How many in-game months between elections." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.MinPopulationForElection)), "Minimum Population for Election" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.MinPopulationForElection)), "Minimum population required before the first election." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.EndorsementInfluencePercent)), "Endorsement Influence (%)" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.EndorsementInfluencePercent)), "How much your endorsement affects the election result. 5% is default." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.UnemploymentThreshold)), "Unemployment Threshold (%)" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.UnemploymentThreshold)), "Unemployment % that triggers a proposal." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.HomelessThreshold)), "Homeless Threshold" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.HomelessThreshold)), "Number of homeless citizens that triggers a proposal." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.CrimeRateThreshold)), "Crime Rate Threshold" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.CrimeRateThreshold)), "Crime probability level that triggers a proposal. Higher = less sensitive." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.HousingDemandThreshold)), "Housing Demand Threshold (%)" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.HousingDemandThreshold)), "Housing demand % that triggers a proposal." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.HealthThreshold)), "Health Threshold" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.HealthThreshold)), "Health level that triggers a proposal." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.WellbeingThreshold)), "Wellbeing Threshold" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.WellbeingThreshold)), "Wellbeing level that triggers a proposal." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.MaxActiveMetricProposals)), "Max Active Urgent Proposals" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.MaxActiveMetricProposals)), "Maximum number of active urgent metric proposals." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.MaxActiveAdHocProposals)), "Max Active Citizen Proposals" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.MaxActiveAdHocProposals)), "Maximum number of active citizen proposals." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.MaxActiveMajorProposals)), "Max Active Major Projects" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.MaxActiveMajorProposals)), "Maximum number of active major projects." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.MajorProjectMinPopulation)), "Major Project Min Population" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.MajorProjectMinPopulation)), "Minimum population required before major projects appear." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.AdHocCooldownMonths)), "Citizen Proposal Cooldown (months)" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.AdHocCooldownMonths)), "Months between new citizen proposals." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.RejectedCooldownMonths)), "Rejected Proposal Cooldown (months)" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.RejectedCooldownMonths)), "Months before a rejected proposal can reappear." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.MetricProposalCooldownMonths)), "Metric Proposal Cooldown (months)" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.MetricProposalCooldownMonths)), "Months before the same metric proposal can be generated again." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.MajorProjectApprovalWeight)), "Major Project Weight" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.MajorProjectApprovalWeight)), "Multiplier applied to mayor approval when a major project is completed or failed." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.AdHocProjectApprovalWeight)), "Citizen Project Weight" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.AdHocProjectApprovalWeight)), "Multiplier applied to mayor approval when a citizen proposal is completed or failed." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.UrgentProjectApprovalWeight)), "Urgent Project Weight" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.UrgentProjectApprovalWeight)), "Multiplier applied to mayor approval when an urgent proposal is completed or failed." },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.ForceElection)), "Force Election" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.ForceElection)), "Trigger a mayoral election immediately." },
    { _settings.GetOptionWarningLocaleID(nameof(CivicVoiceSettings.ForceElection)), "Trigger an election now?" },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.ConcludeElection)), "Conclude Election" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.ConcludeElection)), "Immediately resolve the active election, electing the current leading candidate." },
    { _settings.GetOptionWarningLocaleID(nameof(CivicVoiceSettings.ConcludeElection)), "Conclude the active election now?" },

    { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.ResetToDefaults)), "Reset to Defaults" },
    { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.ResetToDefaults)), "Reset all Civic Voice settings to their default values." },
    { _settings.GetOptionWarningLocaleID(nameof(CivicVoiceSettings.ResetToDefaults)), "Reset all settings to defaults?" },
};
        }

        public void Unload() { }
    }
}
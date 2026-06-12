using Colossal;
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
                { _settings.GetOptionTabLocaleID(CivicVoiceSettings.kSection), "Main" },
                { _settings.GetOptionGroupLocaleID(CivicVoiceSettings.kGeneralGroup), "General" },

                { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.UseUniversalModMenu)), "Use Universal Mod Menu" },
                { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.UseUniversalModMenu)), "Show Civic Voice in the Universal Mod Menu instead of the top right toolbar." },

                { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.ShowNotifications)), "Show Notifications" },
                { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.ShowNotifications)), "Show notifications when proposals are added or elections begin." },

                { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.EndorsementInfluencePercent)), "Endorsement Influence (%)" },
                { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.EndorsementInfluencePercent)), "How much your endorsement affects the election result. 5% is default." },

                { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.ForceElection)), "Force Election" },
                { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.ForceElection)), "Trigger a mayoral election immediately." },
                { _settings.GetOptionWarningLocaleID(nameof(CivicVoiceSettings.ForceElection)), "Trigger an election now?" },

                { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.ElectionFrequencyMonths)), "Election Frequency (months)" },
                { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.ElectionFrequencyMonths)), "How many in-game months between elections." },

                { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.MinPopulationForElection)), "Minimum Population for Election" },
                { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.MinPopulationForElection)), "Minimum population required before the first election." },

                { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.UnemploymentThreshold)), "Unemployment Threshold (%)" },
                { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.UnemploymentThreshold)), "Unemployment % that triggers a proposal." },

                { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.HomelessThreshold)), "Homeless Threshold" },
                { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.HomelessThreshold)), "Number of homeless citizens that triggers a proposal." },

                { _settings.GetOptionLabelLocaleID(nameof(CivicVoiceSettings.CrimeRateThreshold)), "Crime Rate Threshold" },
                { _settings.GetOptionDescLocaleID(nameof(CivicVoiceSettings.CrimeRateThreshold)), "Crime rate that triggers a proposal." },

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
            };
        }

        public void Unload() { }
    }
}
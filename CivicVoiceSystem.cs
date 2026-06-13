// ============================================================
// CivicVoice — Democracy & Governance Mod for Cities: Skylines II
// Created by xTrueF | github.com/xTrueF/CivicVoice
// Licensed under MIT License
// ============================================================
using CivicMod = CivicVoice.Mod;
using CivicVoice.Models;
using Colossal.Serialization.Entities;
using Game;
using Game.City;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

namespace CivicVoice.Systems
{
    public partial class CivicVoiceSystem : GameSystemBase, IDefaultSerializable
    {
        // ── Settings-driven limits ────────────────────────────────────────
        private int MaxActiveMetric => Mod.Settings?.MaxActiveMetricProposals ?? 3;
        private int MaxActiveAdHoc => Mod.Settings?.MaxActiveAdHocProposals ?? 2;
        private int MaxActiveMajor => Mod.Settings?.MaxActiveMajorProposals ?? 2;
        private int MaxActiveProjects => MaxActiveMetric + MaxActiveAdHoc + MaxActiveMajor;
        private int MaxProposedAdHoc => Mod.Settings?.MaxActiveAdHocProposals ?? 2;
        private int MaxProposedMajor => Mod.Settings?.MaxActiveMajorProposals ?? 2;

        // ── Static data ───────────────────────────────────────────────────
        private static readonly string[] s_FirstNames = {
            "James", "Sarah", "Michael", "Emma", "David", "Claire", "Robert", "Helen",
            "William", "Catherine", "Thomas", "Jessica", "Richard", "Laura", "John",
            "Amanda", "Peter", "Rachel", "Andrew", "Samantha", "Daniel", "Rebecca",
            "Christopher", "Michelle", "Matthew", "Stephanie", "Anthony", "Christine",
            "Mark", "Patricia", "Paul", "Jennifer", "Steven", "Angela", "Kevin",
            "Deborah", "Brian", "Sharon", "George", "Karen", "Edward", "Lisa",
            "Benjamin", "Olivia", "Charles", "Sophie", "Jonathan", "Charlotte",
            "Alexander", "Grace", "Patrick", "Victoria", "Simon", "Elizabeth",
            "Marcus", "Diana", "Oliver", "Hannah", "Lucas", "Natalie"
        };
        private static readonly string[] s_LastNames = {
            "Harrison", "Bennett", "Fletcher", "Thornton", "Coleman", "Webb",
            "Morrison", "Griffiths", "Adeyemi", "Patel", "Singh", "Khan",
            "Williams", "Johnson", "Brown", "Taylor", "Davies", "Evans",
            "Wilson", "Thomas", "Roberts", "Walker", "Wright", "Thompson",
            "White", "Hughes", "Martin", "Lewis", "Clarke", "Robinson",
            "Hall", "Jackson", "Wood", "Turner", "Collins", "Edwards",
            "Mitchell", "Cooper", "Morris", "Ward", "Watson", "Brooks",
            "Kelly", "Murray", "Reid", "Campbell", "Stewart", "Anderson",
            "MacDonald", "Ahmed", "Ali", "Shah", "Hussain", "Malik",
            "Okafor", "Mensah", "Diallo", "Nkosi", "Fernandez", "Garcia"
        };
        private static readonly string[] s_Parties = {
            "Citizens First Party", "Progressive City Alliance", "Conservative Growth Party",
            "Green Future Movement", "People's Reform Party", "Urban Renewal Coalition"
        };

        // ── Instance fields ───────────────────────────────────────────────
        public CivicVoiceSaveData Data { get; private set; } = new CivicVoiceSaveData();
        public List<string> Notifications { get; } = new List<string>();

        private int _lastTickDay = -1;
        private int _tickCounter = 0;
        private bool _loaded = false;
        private bool _forceElection = false;
        private static readonly Random _rng = new Random();

        public Game.Simulation.TimeSystem? _timeSystem;
        private Game.Simulation.CityStatisticsSystem? _statsSystem;
        private Game.Simulation.ResidentialDemandSystem? _residentialDemandSystem;
        private Game.Simulation.CommercialDemandSystem? _commercialDemandSystem;
        private Game.Simulation.IndustrialDemandSystem? _industrialDemandSystem;

        // ── Cached stats ──────────────────────────────────────────────────
        public float Happiness { get; private set; } = 50f;
        public int Population { get; private set; } = 0;
        public float Unemployment { get; private set; } = 5f;
        public int TeenCount { get; private set; } = 0;
        public int AdultCount { get; private set; } = 0;
        public int SeniorCount { get; private set; } = 0;
        public int HomelessCount { get; private set; } = 0;
        public float Health { get; private set; } = 50f;
        public float Wellbeing { get; private set; } = 50f;
        public int CrimeRate { get; private set; } = 0;
        public float Income { get; private set; } = 0f;
        public float Expense { get; private set; } = 0f;
        public float LowDensityDemand { get; private set; } = 0f;
        public float MedDensityDemand { get; private set; } = 0f;
        public float HighDensityDemand { get; private set; } = 0f;
        public float CommercialDemand { get; private set; } = 0f;
        public float GarbageProcessingRate { get; private set; } = 0f;
        public int GarbageCapacity { get; private set; } = 0;

        // ── Lifecycle ─────────────────────────────────────────────────────
        protected override void OnCreate()
        {
            base.OnCreate();
            _timeSystem = World.GetOrCreateSystemManaged<Game.Simulation.TimeSystem>();
            _statsSystem = World.GetOrCreateSystemManaged<Game.Simulation.CityStatisticsSystem>();
            _residentialDemandSystem = World.GetOrCreateSystemManaged<Game.Simulation.ResidentialDemandSystem>();
            _commercialDemandSystem = World.GetOrCreateSystemManaged<Game.Simulation.CommercialDemandSystem>();
            _industrialDemandSystem = World.GetOrCreateSystemManaged<Game.Simulation.IndustrialDemandSystem>();
            CivicMod.log.Info("CivicVoiceSystem created.");
        }

        protected override void OnUpdate()
        {
            if (!_loaded)
            {
                _loaded = true;
                Data = new CivicVoiceSaveData();
            }

            _tickCounter++;
            if (_tickCounter < 60) return;
            _tickCounter = 0;

            DoUpdateStats();

            _lastTickDay++;
            Data.LastTickDay = _lastTickDay;

            DoUpdateActiveProjects(_lastTickDay);
            DoCheckForMetricProposals();
            DoCheckForAdHocProposals();
            DoCheckForMajorProposals();
            DoFluctuateProposalVotes();
            DoCheckForElection();

            foreach (var p in Data.ProposedProjects)
                if (p.Type == ProjectType.Metric && p.GoalType.HasValue)
                    p.CurrentValue = DoGetMetricValue(p.GoalType.Value);

            Data.ProposedProjects.RemoveAll(p => IsAlreadyAchieved(p));
        }

        // ── Stats ─────────────────────────────────────────────────────────
        private void DoUpdateStats()
        {
            try
            {
                var cs = World.GetOrCreateSystemManaged<Game.Simulation.CountHouseholdDataSystem>();
                Population = cs.MovedInCitizenCount;
                Unemployment = cs.UnemploymentRate;
                Happiness = cs.AverageCitizenHappiness;
                HomelessCount = cs.HomelessHouseholdCount;
                TeenCount = cs.TeenCount;
                AdultCount = cs.AdultCount;
                SeniorCount = cs.SeniorCount;
            }
            catch (Exception ex) { CivicMod.log.Warn($"Stats update failed (household): {ex.Message}"); }

            try
            {
                if (_statsSystem != null)
                {
                    Health = _statsSystem.GetStatisticValue(StatisticType.Health);
                    Wellbeing = _statsSystem.GetStatisticValue(StatisticType.Wellbeing);
                    CrimeRate = _statsSystem.GetStatisticValue(StatisticType.CrimeRate);
                    Income = _statsSystem.GetStatisticValue(StatisticType.Income);
                    Expense = _statsSystem.GetStatisticValue(StatisticType.Expense);
                }
            }
            catch (Exception ex) { CivicMod.log.Warn($"Stats update failed (city stats): {ex.Message}"); }

            try
            {
                if (_residentialDemandSystem != null)
                {
                    LowDensityDemand = _residentialDemandSystem.buildingDemand.x;
                    MedDensityDemand = _residentialDemandSystem.buildingDemand.y;
                    HighDensityDemand = _residentialDemandSystem.buildingDemand.z;
                }
                if (_commercialDemandSystem != null)
                    CommercialDemand = _commercialDemandSystem.buildingDemand;
            }
            catch (Exception ex) { CivicMod.log.Warn($"Stats update failed (demand): {ex.Message}"); }

            try
            {
                var gs = World.GetOrCreateSystemManaged<Game.UI.InGame.GarbageInfoviewUISystem>();
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var rateM = gs?.GetType().GetMethod("GetProcessingRate", flags);
                var capM = gs?.GetType().GetMethod("GetGarbageCapacity", flags);
                if (rateM != null) GarbageProcessingRate = (float)rateM.Invoke(gs, null);
                if (capM != null) GarbageCapacity = (int)capM.Invoke(gs, null);
            }
            catch (Exception ex) { CivicMod.log.Warn($"Stats update failed (garbage): {ex.Message}"); }
        }

        // ── Metric triggered proposals ────────────────────────────────────
        private void DoCheckForMetricProposals()
        {
            if (_timeSystem == null || Population < 100) return;
            DateTime now = _timeSystem.GetCurrentDateTime();

            int crimeThreshold = Mod.Settings?.CrimeRateThreshold ?? 20;
            int homelessThreshold = Mod.Settings?.HomelessThreshold ?? 10;
            float unempThreshold = Mod.Settings?.UnemploymentThreshold ?? 10f;
            float healthThreshold = Mod.Settings?.HealthThreshold ?? 50f;
            float wellbThreshold = Mod.Settings?.WellbeingThreshold ?? 50f;
            float housingThreshold = Mod.Settings?.HousingDemandThreshold ?? 70f;

            DoTryAddMetricProject("crime", CrimeRate > crimeThreshold, now, new CivicProject
            {
                Title = "Expand police presence",
                Description = $"Crime is rising. Residents feel unsafe with a crime rate of {CrimeRate}. Citizens are demanding more police coverage.",
                Tier = ProjectTier.MetricTriggered,
                Type = ProjectType.Metric,
                Category = ProjectCategory.PublicSafety,
                GoalType = MetricGoalType.CrimeRateBelow,
                GoalTarget = Math.Max(CrimeRate * 0.7f, 5f),
                DeadlineGameDays = 3
            }, now);

            DoTryAddMetricProject("homeless", HomelessCount > homelessThreshold, now, new CivicProject
            {
                Title = "Address the homelessness crisis",
                Description = $"There are {HomelessCount} homeless citizens on the streets. Residents are demanding action to house the vulnerable.",
                Tier = ProjectTier.MetricTriggered,
                Type = ProjectType.Metric,
                Category = ProjectCategory.Housing,
                GoalType = MetricGoalType.HomelessBelow,
                GoalTarget = Math.Max(HomelessCount * 0.5f, 5f),
                DeadlineGameDays = 4
            }, now);

            DoTryAddMetricProject("unemployment", Unemployment > unempThreshold, now, new CivicProject
            {
                Title = "Create more jobs",
                Description = $"Unemployment is at {Unemployment:F1}%. Citizens are struggling to find work and demanding economic action.",
                Tier = ProjectTier.MetricTriggered,
                Type = ProjectType.Metric,
                Category = ProjectCategory.Economy,
                GoalType = MetricGoalType.UnemploymentBelow,
                GoalTarget = Math.Max(Unemployment - 4f, 2f),
                DeadlineGameDays = 6
            }, now);

            DoTryAddMetricProject("health", Health > 0 && Health < healthThreshold, now, new CivicProject
            {
                Title = "Improve healthcare services",
                Description = $"Citizen health levels are at {Health:F0}. Residents are demanding better access to healthcare facilities.",
                Tier = ProjectTier.MetricTriggered,
                Type = ProjectType.Metric,
                Category = ProjectCategory.Healthcare,
                GoalType = MetricGoalType.HealthAbove,
                GoalTarget = Math.Min(Health + 15f, 70f),
                DeadlineGameDays = 4
            }, now);

            DoTryAddMetricProject("wellbeing", Wellbeing > 0 && Wellbeing < wellbThreshold, now, new CivicProject
            {
                Title = "Improve citizen wellbeing",
                Description = $"Resident wellbeing is at {Wellbeing:F0}. Citizens are calling for improvements across city services.",
                Tier = ProjectTier.MetricTriggered,
                Type = ProjectType.Metric,
                Category = ProjectCategory.Leisure,
                GoalType = MetricGoalType.WellbeingAbove,
                GoalTarget = Math.Min(Wellbeing + 15f, 70f),
                DeadlineGameDays = 4
            }, now);

            DoTryAddMetricProject("budget", Expense > Income && Income > 0, now, new CivicProject
            {
                Title = "Address the city budget deficit",
                Description = $"The city is spending more than it earns. Income: £{Income:N0} Expenses: £{Expense:N0}.",
                Tier = ProjectTier.MetricTriggered,
                Type = ProjectType.Metric,
                Category = ProjectCategory.Economy,
                GoalType = MetricGoalType.BudgetSurplus,
                GoalTarget = 0,
                DeadlineGameDays = 3
            }, now);

            DoTryAddMetricProject("garbage", GarbageProcessingRate > 0 && GarbageProcessingRate < 0.5f && GarbageCapacity > 0, now, new CivicProject
            {
                Title = "Expand garbage processing capacity",
                Description = $"Garbage is piling up with only {GarbageProcessingRate * 100:F0}% processing capacity.",
                Tier = ProjectTier.MetricTriggered,
                Type = ProjectType.Metric,
                Category = ProjectCategory.Environment,
                GoalType = MetricGoalType.WellbeingAbove,
                GoalTarget = Math.Min(Wellbeing + 10f, 80f),
                DeadlineGameDays = 4
            }, now);

            DoTryAddMetricProject("housing_low", LowDensityDemand > housingThreshold, now, new CivicProject
            {
                Title = "Zone more low density housing",
                Description = "Demand for low density housing is high. Residents want more space to build family homes.",
                Tier = ProjectTier.MetricTriggered,
                Type = ProjectType.Metric,
                Category = ProjectCategory.Housing,
                GoalType = MetricGoalType.LowDensityDemandBelow,
                GoalTarget = 50f,
                DeadlineGameDays = 6
            }, now);

            DoTryAddMetricProject("housing_med", MedDensityDemand > housingThreshold, now, new CivicProject
            {
                Title = "Develop medium density neighbourhoods",
                Description = "Demand for medium density housing is high. Citizens want more apartment blocks and townhouses.",
                Tier = ProjectTier.MetricTriggered,
                Type = ProjectType.Metric,
                Category = ProjectCategory.Housing,
                GoalType = MetricGoalType.MedDensityDemandBelow,
                GoalTarget = 50f,
                DeadlineGameDays = 6
            }, now);

            DoTryAddMetricProject("housing_high", HighDensityDemand > housingThreshold, now, new CivicProject
            {
                Title = "Build high density housing",
                Description = "Demand for high density housing is high. Some residents want tower blocks but others are concerned about neighbourhood character.",
                Tier = ProjectTier.MetricTriggered,
                Type = ProjectType.Metric,
                Category = ProjectCategory.Housing,
                GoalType = MetricGoalType.HighDensityDemandBelow,
                GoalTarget = 50f,
                DeadlineGameDays = 6
            }, now);
        }

        private string GetMetricKey(CivicProject p) => p.GoalType switch
        {
            MetricGoalType.CrimeRateBelow => "crime",
            MetricGoalType.HomelessBelow => "homeless",
            MetricGoalType.UnemploymentBelow => "unemployment",
            MetricGoalType.HealthAbove => "health",
            MetricGoalType.WellbeingAbove => "wellbeing",
            MetricGoalType.BudgetSurplus => "budget",
            MetricGoalType.LowDensityDemandBelow => "housing_low",
            MetricGoalType.MedDensityDemandBelow => "housing_med",
            MetricGoalType.HighDensityDemandBelow => "housing_high",
            _ => null
        };

        private void DoTryAddMetricProject(string key, bool condition, DateTime now, CivicProject project, DateTime currentTime)
        {
            if (!condition) return;
            if (Data.MetricProjectCooldowns.TryGetValue(key, out DateTime cooldownEnd) && currentTime < cooldownEnd) return;
            if (Data.ProposedProjects.Any(p => p.Title == project.Title)) return;
            if (Data.ActiveProjects.Any(p => p.Title == project.Title)) return;

            DoAssignVotes(project);
            Data.ProposedProjects.Add(project);
            CivicMod.log.Info($"[CivicVoice] Metric proposal added: {project.Title} (key: {key})");
            DoNotify($"Citizens are demanding action: \"{project.Title}\"");
        }

        // ── Ad-hoc proposals ──────────────────────────────────────────────
        private void DoCheckForAdHocProposals()
        {
            if (_timeSystem == null || Population < 100) return;

            DateTime now = _timeSystem.GetCurrentDateTime();
            int proposed = Data.ProposedProjects.Count(p => p.Tier == ProjectTier.AdHoc);
            int active = Data.ActiveProjects.Count(p => p.Tier == ProjectTier.AdHoc);
            if (proposed + active >= MaxProposedAdHoc) return;

            int cooldownDays = Mod.Settings?.AdHocCooldownMonths ?? 6;
            if (Data.LastAdHocProjectDate != DateTime.MinValue && (now - Data.LastAdHocProjectDate).TotalDays < cooldownDays) return;

            Data.LastAdHocProjectDate = now;

            var pool = DoBuildAdHocPool();
            pool.RemoveAll(p => IsAlreadyAchieved(p));
            pool.RemoveAll(p => Data.ProposedProjects.Any(x => x.Title == p.Title));
            pool.RemoveAll(p => Data.ActiveProjects.Any(x => x.Title == p.Title));

            pool = pool.OrderByDescending(p => GetMayorWeight(p, Data.CurrentMayor?.Specialty))
                       .ThenBy(_ => _rng.Next())
                       .ToList();

            int needed = MaxProposedAdHoc - proposed - active;
            int added = 0;
            foreach (var p in pool.Take(needed))
            {
                DoAssignVotes(p);
                Data.ProposedProjects.Add(p);
                added++;
            }
            CivicMod.log.Info($"[CivicVoice] AdHoc proposals added: {added} (mayor specialty: {Data.CurrentMayor?.Specialty})");
            DoNotify("Citizens have new ideas for the city.");
        }

        private int GetMayorWeight(CivicProject p, MayorSpecialty? specialty)
        {
            if (specialty == null) return 1;
            var favoured = specialty switch
            {
                MayorSpecialty.Healthcare => new[] { ProjectCategory.Healthcare },
                MayorSpecialty.Economy => new[] { ProjectCategory.Economy },
                MayorSpecialty.Environment => new[] { ProjectCategory.Environment, ProjectCategory.Leisure },
                MayorSpecialty.Infrastructure => new[] { ProjectCategory.Infrastructure, ProjectCategory.Transport },
                MayorSpecialty.Education => new[] { ProjectCategory.Education },
                MayorSpecialty.PublicSafety => new[] { ProjectCategory.PublicSafety },
                _ => new ProjectCategory[] { }
            };
            return favoured.Contains(p.Category) ? 3 : 1;
        }

        private List<CivicProject> DoBuildAdHocPool() => new List<CivicProject>
{
    new CivicProject {
        Title = "Build a public park",
        Description = "Residents want green spaces to enjoy and relax in. A new park would improve quality of life.",
        Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Leisure,
        GoalType = MetricGoalType.HappinessAbove, GoalTarget = Math.Min(Happiness + 5f, 85f),
        DeadlineGameDays = 6, ManualCompletion = true },
    new CivicProject {
        Title = "Improve leisure facilities",
        Description = "Citizens want more recreational options. Expanding parks and leisure areas would improve quality of life.",
        Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Leisure,
        GoalType = MetricGoalType.WellbeingAbove, GoalTarget = Math.Min(Wellbeing + 5f, 85f),
        DeadlineGameDays = 6, ManualCompletion = true },
    new CivicProject {
        Title = "Build a fire station",
        Description = "Residents want better fire protection across the city. A new fire station would improve safety.",
        Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.PublicSafety,
        GoalType = MetricGoalType.HappinessAbove, GoalTarget = Math.Min(Happiness + 4f, 84f),
        DeadlineGameDays = 6, ManualCompletion = true },
    new CivicProject {
        Title = "Develop a new shopping strip",
        Description = $"Citizens want more retail options. Commercial demand is at {CommercialDemand:F0}%. A new commercial area would boost the local economy.",
        Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Economy,
        GoalType = MetricGoalType.CommercialDemandBelow, GoalTarget = 40f,
        DeadlineGameDays = 6 },
    new CivicProject {
        Title = "Build a new school",
        Description = "Parents are concerned about education access. A new school would improve opportunities for children.",
        Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Education,
        GoalType = MetricGoalType.WellbeingAbove, GoalTarget = Math.Min(Wellbeing + 5f, 85f),
        DeadlineGameDays = 6, ManualCompletion = true },
    new CivicProject {
        Title = "Improve public transport",
        Description = "Citizens want better bus and transit connections across the city.",
        Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Transport,
        GoalType = MetricGoalType.HappinessAbove, GoalTarget = Math.Min(Happiness + 6f, 88f),
        DeadlineGameDays = 6, ManualCompletion = true },
    new CivicProject {
        Title = "Expand the electricity network",
        Description = "More power infrastructure is needed to support city growth.",
        Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Infrastructure,
        GoalType = MetricGoalType.PopulationAbove, GoalTarget = (int)(Population * 1.2f),
        DeadlineGameDays = 6, ManualCompletion = true },
    new CivicProject {
        Title = "Build a new suburb",
        Description = "Citizens want new residential areas. Developing a new suburb would ease housing demand.",
        Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Housing,
        GoalType = MetricGoalType.LowDensityDemandBelow, GoalTarget = 40f,
        DeadlineGameDays = 6 },
    new CivicProject {
        Title = "Reduce industrial pollution",
        Description = "Residents near industrial areas are complaining about air and ground pollution affecting their quality of life.",
        Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Environment,
        GoalType = MetricGoalType.HappinessAbove, GoalTarget = Math.Min(Happiness + 6f, 88f),
        DeadlineGameDays = 6, ManualCompletion = true },
    new CivicProject {
        Title = "Plant street trees",
        Description = "Citizens want greener streets. A tree planting initiative would improve air quality and city aesthetics.",
        Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Environment,
        GoalType = MetricGoalType.WellbeingAbove, GoalTarget = Math.Min(Wellbeing + 5f, 85f),
        DeadlineGameDays = 6, ManualCompletion = true },
};

        // ── Major proposals ───────────────────────────────────────────────
        private void DoCheckForMajorProposals()
        {
            if (_timeSystem == null) return;

            int minPop = Mod.Settings?.MajorProjectMinPopulation ?? 500;
            if (Population < minPop) return;

            DateTime now = _timeSystem.GetCurrentDateTime();
            int proposedCount = Data.ProposedProjects.Count(p => p.Tier == ProjectTier.Major);
            int activeCount = Data.ActiveProjects.Count(p => p.Tier == ProjectTier.Major);
            if (proposedCount + activeCount >= MaxProposedMajor) return;
            if (Data.LastMajorProjectDate != DateTime.MinValue && (now - Data.LastMajorProjectDate).TotalDays < 12) return;

            Data.LastMajorProjectDate = now;

            var pool = DoBuildMajorPool();
            DoShuffle(pool);

            int needed = MaxProposedMajor - proposedCount - activeCount;
            int added = 0;
            foreach (var p in pool.Take(needed))
            {
                if (Data.ProposedProjects.Any(x => x.Title == p.Title)) continue;
                if (Data.ActiveProjects.Any(x => x.Title == p.Title)) continue;
                DoAssignVotes(p);
                Data.ProposedProjects.Add(p);
                added++;
            }
            CivicMod.log.Info($"[CivicVoice] Major proposals added: {added}");
            DoNotify("Citizens are proposing a major city project.");
        }

        private List<CivicProject> DoBuildMajorPool() => new List<CivicProject>
        {
            new CivicProject {
                Title = "Develop a new town district",
                Description = "Citizens want to expand the city with an entirely new district. This is a major undertaking that will take a full year.",
                Tier = ProjectTier.Major, Type = ProjectType.Metric, Category = ProjectCategory.Housing,
                GoalType = MetricGoalType.PopulationAbove, GoalTarget = (int)(Population * 1.5f),
                DeadlineGameDays = 12, ManualCompletion = true },
            new CivicProject {
                Title = "Build a new port",
                Description = "A port would open up shipping trade and bring economic growth. A major infrastructure investment for the city.",
                Tier = ProjectTier.Major, Type = ProjectType.Metric, Category = ProjectCategory.Infrastructure,
                GoalType = MetricGoalType.PopulationAbove, GoalTarget = (int)(Population * 1.3f),
                DeadlineGameDays = 12, ManualCompletion = true },
            new CivicProject {
                Title = "Establish a new industrial complex",
                Description = "Citizens want major job creation through a large industrial development. This will take significant planning.",
                Tier = ProjectTier.Major, Type = ProjectType.Metric, Category = ProjectCategory.Economy,
                GoalType = MetricGoalType.UnemploymentBelow, GoalTarget = Math.Max(Unemployment - 5f, 2f),
                DeadlineGameDays = 12, ManualCompletion = true },
            new CivicProject {
                Title = "Build a signature landmark",
                Description = "Citizens want an iconic building that puts the city on the map and attracts tourists.",
                Tier = ProjectTier.Major, Type = ProjectType.Metric, Category = ProjectCategory.Leisure,
                GoalType = MetricGoalType.HappinessAbove, GoalTarget = Math.Min(Happiness + 15f, 90f),
                DeadlineGameDays = 12, ManualCompletion = true },
            new CivicProject {
                Title = "Develop a new commercial hub",
                Description = "A major commercial district to attract businesses and boost the city economy.",
                Tier = ProjectTier.Major, Type = ProjectType.Metric, Category = ProjectCategory.Economy,
                GoalType = MetricGoalType.CommercialDemandBelow, GoalTarget = 30f,
                DeadlineGameDays = 12, ManualCompletion = true },
            new CivicProject {
                Title = "Build a university campus",
                Description = "Citizens want world class education facilities. A university would attract talent and boost the city's reputation.",
                Tier = ProjectTier.Major, Type = ProjectType.Metric, Category = ProjectCategory.Education,
                GoalType = MetricGoalType.WellbeingAbove, GoalTarget = Math.Min(Wellbeing + 20f, 90f),
                DeadlineGameDays = 12, ManualCompletion = true },
        };

        // ── Vote assignment ───────────────────────────────────────────────
        private void DoAssignVotes(CivicProject p)
        {
            int eligibleVoters = Math.Max(100, TeenCount + AdultCount + SeniorCount);
            float forShare = Math.Max(0.40f, Math.Min(0.90f, 0.55f + DoGetUrgency(p) * 0.30f + (float)(_rng.NextDouble() - 0.5) * 0.1f));
            int totalStartVotes = p.Tier == ProjectTier.Major
                ? Math.Max(10, (int)(eligibleVoters * 0.4f))
                : Math.Max(10, (int)(eligibleVoters * 0.35f));
            p.VotesFor = (int)(totalStartVotes * forShare);
            p.VotesAgainst = totalStartVotes - p.VotesFor;
        }

        private void DoFluctuateProposalVotes()
        {
            int eligible = Math.Max(10, TeenCount + AdultCount + SeniorCount);
            foreach (var p in Data.ProposedProjects)
            {
                if (p.VotesAgainst > p.VotesFor * 1.5f)
                {
                    float share = DoGetUrgency(p);
                    int total = p.VotesFor + p.VotesAgainst;
                    p.VotesFor = (int)(total * share);
                    p.VotesAgainst = total - p.VotesFor;
                }

                if (p.VotesFor + p.VotesAgainst >= eligible) continue;

                int newVotes = _rng.Next(1, 4);
                float lean = p.Category switch
                {
                    ProjectCategory.Healthcare => 0.60f,
                    ProjectCategory.PublicSafety => 0.58f,
                    ProjectCategory.Environment => 0.57f,
                    ProjectCategory.Leisure => 0.57f,
                    ProjectCategory.Education => 0.55f,
                    ProjectCategory.Housing when p.Title.Contains("high density") => 0.45f,
                    ProjectCategory.Housing when p.Title.Contains("medium") => 0.50f,
                    ProjectCategory.Housing => 0.54f,
                    ProjectCategory.Transport => 0.52f,
                    ProjectCategory.Economy => 0.50f,
                    ProjectCategory.Infrastructure => 0.48f,
                    _ => 0.52f
                };
                float forBias = Math.Max(0.35f, Math.Min(0.85f, lean + (float)(_rng.NextDouble() - 0.3f) * 0.2f));
                p.VotesFor += (int)(newVotes * forBias);
                p.VotesAgainst += newVotes - (int)(newVotes * forBias);
            }
        }

        private float DoGetUrgency(CivicProject p)
        {
            float tierBase = p.Tier switch
            {
                ProjectTier.MetricTriggered => 0.75f,
                ProjectTier.Major => 0.55f,
                _ => 0.55f
            };
            float categoryMod = p.Category switch
            {
                ProjectCategory.Healthcare => 0.15f,
                ProjectCategory.PublicSafety => 0.12f,
                ProjectCategory.Environment => 0.10f,
                ProjectCategory.Leisure => 0.10f,
                ProjectCategory.Education => 0.08f,
                ProjectCategory.Housing when p.Title.Contains("high density") => 0.02f,
                ProjectCategory.Housing when p.Title.Contains("medium") => 0.05f,
                ProjectCategory.Housing => 0.08f,
                ProjectCategory.Transport => 0.05f,
                ProjectCategory.Economy => 0.03f,
                ProjectCategory.Infrastructure => 0.02f,
                _ => 0.05f
            };
            return Math.Max(0.35f, Math.Min(0.90f, tierBase + categoryMod + (float)(_rng.NextDouble() - 0.35f) * 0.15f));
        }

        // ── Project actions ───────────────────────────────────────────────
        public bool AcceptProject(string projectId)
        {
            var p = Data.ProposedProjects.FirstOrDefault(x => x.Id == projectId);
            if (p == null) return false;

            int maxForTier = p.Tier == ProjectTier.MetricTriggered ? MaxActiveMetric
                           : p.Tier == ProjectTier.Major ? MaxActiveMajor
                           : MaxActiveAdHoc;

            if (Data.ActiveProjects.Count(x => x.Tier == p.Tier) >= maxForTier)
            {
                DoNotify($"You already have the maximum active {p.Tier} projects.");
                return false;
            }

            p.Status = ProjectStatus.Active;
            p.StartGameDay = _lastTickDay;
            if (_timeSystem != null) p.StartDate = _timeSystem.GetCurrentDateTime();
            Data.ProposedProjects.Remove(p);
            Data.ActiveProjects.Add(p);
            CivicMod.log.Info($"[CivicVoice] Project accepted: {p.Title} (tier: {p.Tier})");
            DoNotify($"Project accepted: \"{p.Title}\"");
            return true;
        }

        public void RejectProject(string projectId)
        {
            var p = Data.ProposedProjects.FirstOrDefault(x => x.Id == projectId);
            if (p == null) return;
            p.Status = ProjectStatus.Rejected;
            Data.ProposedProjects.Remove(p);
            CivicMod.log.Info($"[CivicVoice] Project rejected: {p.Title}");

            if (p.Tier == ProjectTier.MetricTriggered && _timeSystem != null)
            {
                string key = GetMetricKey(p);
                if (key != null)
                {
                    int cooldownDays = Mod.Settings?.RejectedCooldownMonths ?? 6;
                    DateTime now = _timeSystem.GetCurrentDateTime();
                    Data.MetricProjectCooldowns[key] = now.AddDays(cooldownDays);
                    CivicMod.log.Info($"[CivicVoice] Cooldown set for {key} until {now.AddDays(cooldownDays)}");
                    DoNotify($"Proposal rejected: \"{p.Title}\".");
                }
            }
        }

        public void AbandonProject(string projectId)
        {
            var p = Data.ActiveProjects.FirstOrDefault(x => x.Id == projectId);
            if (p == null) return;
            Data.ActiveProjects.Remove(p);
            Data.TotalProjectsFailed++;
            CivicMod.log.Info($"[CivicVoice] Project abandoned: {p.Title}");
            DoNotify($"Project abandoned: \"{p.Title}\".");
        }

        public void MarkProjectComplete(string projectId)
        {
            var p = Data.ActiveProjects.FirstOrDefault(x => x.Id == projectId && x.ManualCompletion);
            if (p == null) return;
            p.MarkedComplete = true;
            CivicMod.log.Info($"[CivicVoice] Major project marked complete: {p.Title}");
        }

        public void TriggerForceElection()
        {
            _forceElection = true;
            CivicMod.log.Info("[CivicVoice] Force election triggered.");
        }

        // ── Progress tracking ─────────────────────────────────────────────
        private void DoUpdateActiveProjects(int today)
        {
            var toComplete = new List<CivicProject>();
            var toFail = new List<CivicProject>();

            foreach (var p in Data.ActiveProjects)
            {
                if (!p.ManualCompletion && p.Type == ProjectType.Metric && p.GoalType.HasValue)
                    p.CurrentValue = DoGetMetricValue(p.GoalType.Value);

                if (p.IsComplete())
                    toComplete.Add(p);
                else if (p.StartDate != DateTime.MinValue && _timeSystem != null)
                {
                    if ((int)(_timeSystem.GetCurrentDateTime() - p.StartDate).TotalDays >= p.DeadlineGameDays)
                        toFail.Add(p);
                }
                else if (today - p.StartGameDay >= p.DeadlineGameDays)
                    toFail.Add(p);
            }

            foreach (var p in toComplete) DoCompleteProject(p);
            foreach (var p in toFail) DoFailProject(p);
        }

        private float DoGetMetricValue(MetricGoalType goalType) => goalType switch
        {
            MetricGoalType.UnemploymentBelow => Unemployment,
            MetricGoalType.HappinessAbove => Happiness,
            MetricGoalType.PopulationAbove => Population,
            MetricGoalType.CrimeRateBelow => CrimeRate,
            MetricGoalType.HomelessBelow => HomelessCount,
            MetricGoalType.HealthAbove => Health,
            MetricGoalType.WellbeingAbove => Wellbeing,
            MetricGoalType.BudgetSurplus => Income - Expense,
            MetricGoalType.CommercialDemandBelow => CommercialDemand,
            MetricGoalType.LowDensityDemandBelow => LowDensityDemand,
            MetricGoalType.MedDensityDemandBelow => MedDensityDemand,
            MetricGoalType.HighDensityDemandBelow => HighDensityDemand,
            _ => 0f
        };

        private void DoCompleteProject(CivicProject p)
        {
            p.Status = ProjectStatus.Completed;
            Data.ActiveProjects.Remove(p);
            Data.CompletedProjects.Add(p);
            Data.TotalProjectsCompleted++;
            CivicMod.log.Info($"[CivicVoice] Project completed: {p.Title}");
            DoNotify($"Project complete: \"{p.Title}\"!");
        }

        private void DoFailProject(CivicProject p)
        {
            p.Status = ProjectStatus.Failed;
            Data.ActiveProjects.Remove(p);
            Data.TotalProjectsFailed++;
            CivicMod.log.Info($"[CivicVoice] Project failed: {p.Title}");
            DoNotify($"Project failed: \"{p.Title}\".");

            if (p.Tier == ProjectTier.MetricTriggered && _timeSystem != null)
            {
                string key = GetMetricKey(p);
                if (key != null)
                {
                    DateTime now = _timeSystem.GetCurrentDateTime();
                    Data.MetricProjectCooldowns[key] = now.AddDays(3);
                    CivicMod.log.Info($"[CivicVoice] Cooldown set for {key} until {now.AddDays(3)}");
                }
            }
        }

        private bool IsAlreadyAchieved(CivicProject p)
        {
            if (p.ManualCompletion || p.Type != ProjectType.Metric) return false;
            float current = DoGetMetricValue(p.GoalType ?? MetricGoalType.HappinessAbove);
            return p.GoalType switch
            {
                MetricGoalType.UnemploymentBelow => current <= p.GoalTarget,
                MetricGoalType.CrimeRateBelow => current <= p.GoalTarget,
                MetricGoalType.HomelessBelow => current <= p.GoalTarget,
                MetricGoalType.CommercialDemandBelow => current <= p.GoalTarget,
                MetricGoalType.LowDensityDemandBelow => current <= p.GoalTarget,
                MetricGoalType.MedDensityDemandBelow => current <= p.GoalTarget,
                MetricGoalType.HighDensityDemandBelow => current <= p.GoalTarget,
                MetricGoalType.HappinessAbove => current >= p.GoalTarget,
                MetricGoalType.PopulationAbove => current >= p.GoalTarget,
                MetricGoalType.BudgetSurplus => current >= p.GoalTarget,
                MetricGoalType.HealthAbove => current >= p.GoalTarget,
                MetricGoalType.WellbeingAbove => current >= p.GoalTarget,
                _ => false
            };
        }

        public string GetLiveDescription(CivicProject p)
        {
            if (p.Tier == ProjectTier.AdHoc || p.Tier == ProjectTier.Major)
                return p.Description;

            return p.GoalType switch
            {
                MetricGoalType.CrimeRateBelow => $"Crime is rising. Residents feel unsafe with a crime rate of {CrimeRate}. Citizens are demanding more police coverage.",
                MetricGoalType.HomelessBelow => $"There are {HomelessCount} homeless citizens on the streets. Residents are demanding action to house the vulnerable.",
                MetricGoalType.UnemploymentBelow => $"Unemployment is at {Unemployment:F1}%. Citizens are struggling to find work and demanding economic action.",
                MetricGoalType.HealthAbove => $"Citizen health levels are at {Health:F0}. Residents are demanding better access to healthcare facilities.",
                MetricGoalType.WellbeingAbove when p.Title.Contains("garbage") => $"Garbage is piling up with only {GarbageProcessingRate * 100:F0}% processing capacity. Citizens are demanding better waste management.",
                MetricGoalType.WellbeingAbove => $"Resident wellbeing is at {Wellbeing:F0}. Citizens are calling for improvements across city services.",
                MetricGoalType.BudgetSurplus => $"The city is spending more than it earns. Income: £{Income:N0} Expenses: £{Expense:N0}.",
                MetricGoalType.CommercialDemandBelow => $"Commercial demand is at {CommercialDemand:F0}%. Citizens want more retail and business options.",
                MetricGoalType.LowDensityDemandBelow => "Demand for low density housing is high. Residents want more space to build family homes.",
                MetricGoalType.MedDensityDemandBelow => "Demand for medium density housing is high. Citizens want more apartment blocks and townhouses.",
                MetricGoalType.HighDensityDemandBelow => "Demand for high density housing is high. Some residents want tower blocks but others are concerned about neighbourhood character.",
                MetricGoalType.PopulationAbove => $"The city has {Population:N0} citizens. Growing to {p.GoalTarget:N0} would show the city is thriving.",
                MetricGoalType.HappinessAbove => $"Citizen happiness is at {Happiness:F1}%. {p.Description}",
                _ => p.Description
            };
        }

        // ── Elections ─────────────────────────────────────────────────────
        private void DoCheckForElection()
        {
            if (_timeSystem == null) return;

            int minPop = Mod.Settings?.MinPopulationForElection ?? 100;
            try
            {
                if (World.GetOrCreateSystemManaged<Game.Simulation.CountHouseholdDataSystem>().MovedInCitizenCount < minPop) return;
            }
            catch { return; }

            DateTime now = _timeSystem.GetCurrentDateTime();

            if (Data.CurrentElection != null && Data.CurrentElection.IsActive)
            {
                int eligible = Math.Max(10, TeenCount + AdultCount + SeniorCount);
                int totalVotes = Data.CurrentElection.Candidates.Sum(c => c.Votes);
                if (totalVotes < eligible)
                    foreach (var c in Data.CurrentElection.Candidates)
                        c.Votes += _rng.Next(0, 4);

                if (Data.ElectionStartDate != DateTime.MinValue && (now - Data.ElectionStartDate).TotalDays >= 1)
                {
                    var winner = Data.CurrentElection.Candidates.OrderByDescending(c => c.Votes).First();
                    Data.CurrentElection.Winner = winner;
                    Data.CurrentElection.IsActive = false;
                    Data.CurrentMayor = winner;
                    int electionFreq = Mod.Settings?.ElectionFrequencyMonths ?? 12;
                    Data.NextElectionDate = now.AddDays(electionFreq);
                    CivicMod.log.Info($"[CivicVoice] Election won by: {winner.Name} ({winner.PartyName})");
                    DoNotify($"{winner.Name} ({winner.PartyName}) has won the election!");
                }
                return;
            }

            if (_forceElection)
            {
                _forceElection = false;
                Data.LastElectionDate = now;
                Data.ElectionStartDate = now;
                int electionFreq = Mod.Settings?.ElectionFrequencyMonths ?? 12;
                Data.NextElectionDate = now.AddDays(electionFreq);
                Data.CurrentElection = DoGenerateElection();
                var fc1 = Data.CurrentElection.Candidates[0].Name;
                var fc2 = Data.CurrentElection.Candidates[1].Name;
                CivicMod.log.Info($"[CivicVoice] Forced election started: {fc1} vs {fc2}");
                DoNotify($"A special election has been called! {fc1} vs {fc2}");
                return;
            }

            if (Data.NextElectionDate == DateTime.MinValue)
            {
                Data.NextElectionDate = now;
                if (!Data.FirstElectionAnnounced)
                {
                    Data.FirstElectionAnnounced = true;
                    CivicMod.log.Info("[CivicVoice] First election triggered.");
                    DoNotify($"Your city has reached {minPop} citizens! The first mayoral election has begun!");
                }
            }

            if (now < Data.NextElectionDate || (Data.CurrentElection != null && Data.CurrentElection.IsActive)) return;

            Data.LastElectionDate = now;
            Data.ElectionStartDate = now;
            int freq = Mod.Settings?.ElectionFrequencyMonths ?? 12;
            Data.NextElectionDate = now.AddDays(freq);
            Data.CurrentElection = DoGenerateElection();
            var c1 = Data.CurrentElection.Candidates[0].Name;
            var c2 = Data.CurrentElection.Candidates[1].Name;
            CivicMod.log.Info($"[CivicVoice] Election started: {c1} vs {c2}");
            DoNotify($"Election time! {c1} vs {c2}");
        }

        public void CastVote(string candidateName)
        {
            if (Data.CurrentElection == null || !Data.CurrentElection.IsActive || Data.CurrentElection.HasVoted) return;
            Data.CurrentElection.HasVoted = true;
            float influence = (Mod.Settings?.EndorsementInfluencePercent ?? 5) / 100f;
            foreach (var c in Data.CurrentElection.Candidates)
            {
                if (c.Name == candidateName)
                    c.Votes += Math.Max(1, (int)(Population * influence));
            }
            CivicMod.log.Info($"[CivicVoice] Vote cast for: {candidateName}");
            DoNotify($"You have endorsed {candidateName}. The election will conclude at the end of the voting period.");
        }

        public DateTime GetCurrentGameDate()
        {
            if (_timeSystem == null) return DateTime.MinValue;
            try { return _timeSystem.GetCurrentDateTime(); }
            catch { return DateTime.MinValue; }
        }

        private MayorElection DoGenerateElection()
        {
            var allSpecialties = (MayorSpecialty[])Enum.GetValues(typeof(MayorSpecialty));
            DoShuffle(allSpecialties);

            var candidate1 = DoMakeCandidate(allSpecialties[0]);
            var candidate2 = DoMakeCandidate(allSpecialties[1]);

            int totalVoters = Math.Max(10, TeenCount + AdultCount + SeniorCount);
            float share = 0.4f + (float)_rng.NextDouble() * 0.2f;
            candidate1.Votes = (int)(totalVoters * share);
            candidate2.Votes = totalVoters - candidate1.Votes;

            return new MayorElection { Candidates = new List<MayorCandidate> { candidate1, candidate2 } };
        }

        private MayorCandidate DoMakeCandidate(MayorSpecialty s) => new MayorCandidate
        {
            Name = s_FirstNames[_rng.Next(s_FirstNames.Length)] + " " + s_LastNames[_rng.Next(s_LastNames.Length)],
            Age = _rng.Next(35, 68),
            Specialty = s,
            PartyName = s_Parties[_rng.Next(s_Parties.Length)],
            Slogan = DoGetSlogan(s)
        };

        private static string DoGetSlogan(MayorSpecialty s) => s switch
        {
            MayorSpecialty.Economy => "Jobs for everyone, growth for all.",
            MayorSpecialty.Environment => "A green city is a healthy city.",
            MayorSpecialty.Infrastructure => "Better connections build better communities.",
            MayorSpecialty.Healthcare => "No citizen left without care.",
            MayorSpecialty.Education => "Invest in minds, invest in our future.",
            MayorSpecialty.PublicSafety => "Safe streets, safe homes.",
            _ => "Building a better city together."
        };

        // ── Serialization ─────────────────────────────────────────────────
        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            try
            {
                writer.Write(JsonConvert.SerializeObject(Data, Formatting.None));
                CivicMod.log.Info("[CivicVoice] Data serialized.");
            }
            catch (Exception ex)
            {
                writer.Write("");
                CivicMod.log.Warn($"[CivicVoice] Serialize failed: {ex.Message}");
            }
        }

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            try
            {
                reader.Read(out string json);
                if (!string.IsNullOrEmpty(json))
                {
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        ObjectCreationHandling = ObjectCreationHandling.Replace
                    };
                    Data = JsonConvert.DeserializeObject<CivicVoiceSaveData>(json, settings) ?? new CivicVoiceSaveData();
                    // Migration: set ManualCompletion for AdHoc projects
                    foreach (var p in Data.ProposedProjects.Where(p => p.Tier == ProjectTier.AdHoc && p.Title != "Develop a new shopping strip" && p.Title != "Build a new suburb"))
                        p.ManualCompletion = true;
                    foreach (var p in Data.ActiveProjects.Where(p => p.Tier == ProjectTier.AdHoc && p.Title != "Develop a new shopping strip" && p.Title != "Build a new suburb"))
                        p.ManualCompletion = true;
                    _lastTickDay = Data.LastTickDay;
                    CivicMod.log.Info($"[CivicVoice] Deserialized. Projects: {Data.ProposedProjects.Count} proposed, {Data.ActiveProjects.Count} active. Mayor: {Data.CurrentMayor?.Name ?? "None"}");
                }
                _loaded = true;
            }
            catch (Exception ex)
            {
                CivicMod.log.Warn($"[CivicVoice] Deserialize failed: {ex.Message}");
                Data = new CivicVoiceSaveData();
                _loaded = true;
            }
        }

        public void SetDefaults(Context context)
        {
            Data = new CivicVoiceSaveData();
            _lastTickDay = -1;
            _loaded = true;
            CivicMod.log.Info("[CivicVoice] SetDefaults called — new city.");
        }

        // ── Helpers ───────────────────────────────────────────────────────
        private void DoNotify(string msg)
        {
            if (Mod.Settings?.ShowNotifications ?? true)
                Notifications.Add(msg);
            CivicMod.log.Info($"[CivicVoice] {msg}");
        }

        private static void DoShuffle<T>(T[] arr)
        {
            for (int i = arr.Length - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
        }

        private static void DoShuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public bool HasProposals => Data.ProposedProjects.Count > 0;
        public bool HasActiveElection => Data.CurrentElection != null && Data.CurrentElection.IsActive;
        public int AvailableSlots => MaxActiveProjects - Data.ActiveProjects.Count;
    }
}
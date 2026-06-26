// ============================================================
// CivicVoice — Democracy & Governance Mod for Cities: Skylines II
// Created by xTrueF | github.com/xTrueF/CivicVoice
// Licensed under MIT License
// ============================================================
using CivicVoice.Models;
using Colossal.Serialization.Entities;
using Game;
using Game.SceneFlow;
using Game.City;
using Game.Simulation;
using Game.UI.InGame;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using static CivicVoice.Models.CivicProject;
using CivicMod = CivicVoice.Mod;

namespace CivicVoice.Systems
{
    public partial class CivicVoiceSystem : GameSystemBase, IDefaultSerializable
    {
        // ── Settings-driven limits ────────────────────────────────────────
        private int MaxActiveMetric => Mod.Settings?.MaxActiveMetricProposals ?? 3;
        private int MaxActiveAdHoc => Mod.Settings?.MaxActiveAdHocProposals ?? 2;
        private int MaxActiveMajor => Mod.Settings?.MaxActiveMajorProposals ?? 1;
        private int MaxActiveProjects => MaxActiveMetric + MaxActiveAdHoc + MaxActiveMajor;
        private int MaxProposedAdHoc => Mod.Settings?.MaxActiveAdHocProposals ?? 2;
        private int MaxProposedMajor => Mod.Settings?.MaxActiveMajorProposals ?? 1;

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
            "Marcus", "Diana", "Oliver", "Hannah", "Lucas", "Natalie",
            "Nathan", "Chloe", "Adam", "Zoe", "Sean", "Fiona", "Ryan", "Leah",
            "Jack", "Amber", "Harry", "Ellie", "Jake", "Harriet", "Owen", "Iris",
            "Ethan", "Alice", "Liam", "Rosie", "Calum", "Imogen", "Kieran", "Molly",
            "Dominic", "Penelope", "Adrian", "Lydia", "Brendan", "Nora", "Colin", "Ruth",
            "Gareth", "Heather", "Nigel", "Judith", "Alistair", "Vivian", "Clive", "Audrey",
            "Rowan", "Sienna", "Felix", "Phoebe", "Leo", "Freya", "Hugo", "Isla",
            "Theo", "Eliza", "Jasper", "Arabella", "Miles", "Cecily", "Edmund", "Wren",
            "Maya", "Aaron", "Priya", "Leon", "Aisha", "Omar", "Layla", "Tariq",
            "Nina", "Stefan", "Elena", "Marco", "Rosa", "Luca", "Petra", "Sven"
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
            "Okafor", "Mensah", "Diallo", "Nkosi", "Fernandez", "Garcia",
            "Barrett", "Chambers", "Dawson", "Fleming", "Goodwin", "Hammond",
            "Hawkins", "Hayward", "Holloway", "Horton", "Houghton", "Ingram",
            "Jennings", "Keane", "Lawson", "Marsden", "Maxwell", "Naylor",
            "Norris", "Osborne", "Pearce", "Pennington", "Preston", "Ramsay",
            "Rawlings", "Sherwood", "Simmons", "Slater", "Sutton", "Sweeney",
            "Tanner", "Underwood", "Vaughan", "Walters", "Warwick", "Whitfield",
            "Wilkins", "Yates", "Cross", "Frost", "Lane", "Park",
            "Stone", "Swift", "Blake", "Chase", "Drake", "Ford",
            "Grant", "Hayes", "Hunt", "Knox", "Nash", "Page",
            "Quinn", "Ross", "Rowe", "Sharp", "Vance", "Wade",
            "Nwosu", "Eze", "Owusu", "Boateng", "Asante", "Conteh",
            "Nakamura", "Yamamoto", "Andersen", "Nielsen", "Kowalski", "Novak",
            "Petrov", "Vasquez", "Reyes", "Santos", "Costa", "Ferreira"
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
        public static bool ElectionsModActive { get; private set; } = false;
        private bool _electionsModChecked = false;
        private bool _gameStartLogged = false;

        public Game.Simulation.TimeSystem? _timeSystem;
        private Game.Simulation.CityStatisticsSystem? _statsSystem;
        private Game.Simulation.ResidentialDemandSystem? _residentialDemandSystem;
        private Game.Simulation.CommercialDemandSystem? _commercialDemandSystem;
        private NaturalResourcesInfoviewUISystem? _naturalResourcesSystem;

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
        public float CrimeRate { get; private set; } = 0f;
        public float Income { get; private set; } = 0f;
        public float Expense { get; private set; } = 0f;
        public float LowDensityDemand { get; private set; } = 0f;
        public float MedDensityDemand { get; private set; } = 0f;
        public float HighDensityDemand { get; private set; } = 0f;
        public float CommercialDemand { get; private set; } = 0f;
        public int UniversityCapacity { get; private set; } = 0;
        public int UniversityStudents { get; private set; } = 0;
        public int UniversityEligible { get; private set; } = 0;
  

        // ── Lifecycle ─────────────────────────────────────────────────────
        protected override void OnCreate()
        {
            base.OnCreate();
            _timeSystem = World.GetOrCreateSystemManaged<Game.Simulation.TimeSystem>();
            _statsSystem = World.GetOrCreateSystemManaged<Game.Simulation.CityStatisticsSystem>();
            _residentialDemandSystem = World.GetOrCreateSystemManaged<Game.Simulation.ResidentialDemandSystem>();
            _commercialDemandSystem = World.GetOrCreateSystemManaged<Game.Simulation.CommercialDemandSystem>();
            ElectionsModActive = AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => a.GetName().Name == "Elections");
            Mod.log.Info("CivicVoiceSystem created.");
        }

        protected override void OnUpdate()
        {
            if (!_electionsModChecked)
            {
                _electionsModChecked = true;
                ElectionsModActive = AppDomain.CurrentDomain.GetAssemblies()
                    .Any(a => a.GetName().Name == "Elections");
                Mod.log.Info($"[CivicVoice] Elections mod detected: {ElectionsModActive}");
            }
            if (!_loaded)
            {
                _loaded = true;
                Data = new CivicVoiceSaveData();
            }

            if (GameManager.instance.gameMode != GameMode.Game) return;

            if (!_gameStartLogged) { _gameStartLogged = true; Mod.log.Info($"[CivicVoice] gameMode=Game confirmed, tick={_lastTickDay}"); }

            _tickCounter++;
            if (_tickCounter < 60) return;
            _tickCounter = 0;

            _naturalResourcesSystem = World.GetOrCreateSystemManaged<NaturalResourcesInfoviewUISystem>();

            DoUpdateStats();

            _lastTickDay++;
            Data.LastTickDay = _lastTickDay;

            DoUpdateActiveProjects(_lastTickDay);
            DoCheckForMetricProposals();
            DoCheckForAdHocProposals();
            DoCheckForMajorProposals();
            DoFluctuateProposalVotes();
            if (!ElectionsModActive)
            {
                DoCheckForElection();
                // DoCheckForTermReview(); // review disabled — happy with election event
            }

            foreach (var p in Data.ProposedProjects)
                if (p.Type == ProjectType.Metric && p.GoalType.HasValue)
                    p.CurrentValue = DoGetMetricValue(p.GoalType.Value);

            Data.ProposedProjects.RemoveAll(p => IsAlreadyAchieved(p));
            if (_timeSystem != null) DoCheckBirthdays(_timeSystem.GetCurrentDateTime());
        }

        // ── Natural Resources ─────────────────────────────────────────────
        private float GetNaturalResourceValue(string fieldName)
        {
            if (_naturalResourcesSystem == null) return 0f;
            var field = typeof(NaturalResourcesInfoviewUISystem).GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null) return 0f;
            var binding = field.GetValue(_naturalResourcesSystem) as Colossal.UI.Binding.ValueBinding<float>;
            return binding?.value ?? 0f;
        }

        public float Oil => GetNaturalResourceValue("m_AvailableOil");
        public float Ore => GetNaturalResourceValue("m_AvailableOre");
        public float Forest => GetNaturalResourceValue("m_AvailableForest");

        private (string resourceName, float amount) GetMostAbundantResource(HashSet<string>? exclude = null)
        {
            exclude ??= new HashSet<string>();
            var resources = new (string name, float amount)[]
            {
        ("oil", Oil),
        ("ore", Ore),
        ("timber", Forest)
            }.Where(r => !exclude.Contains(r.name)).ToArray();

            if (resources.Length == 0) return ("", 0f);

            float maxAmount = resources.Max(r => r.amount);
            if (maxAmount <= 0f) return resources[0];

            var candidates = resources.Where(r => r.amount >= maxAmount * 0.5f).ToList();
            return candidates[_rng.Next(candidates.Count)];
        }

        // ── Candidate details ─────────────────────────────────────────────
        private void DoCheckBirthdays(DateTime now)
        {
            if (Data.LastAgeCheckDate == DateTime.MinValue)
            {
                Data.LastAgeCheckDate = now;
                return;
            }

            // Collect all tracked candidates — current mayor + current election candidates
            var tracked = new List<MayorCandidate>();
            if (Data.CurrentMayor != null) tracked.Add(Data.CurrentMayor);
            if (Data.CurrentElection != null)
                foreach (var c in Data.CurrentElection.Candidates)
                    if (!tracked.Any(x => x.Name == c.Name))
                        tracked.Add(c);

            foreach (var c in tracked)
            {
                // Find the most recent birthday date before or on now
                var birthday = new DateTime(now.Year, c.BirthdayMonth, c.BirthdayDay);
                if (birthday > now) birthday = birthday.AddYears(-1);

                // If birthday falls between last check and now, increment age
                if (birthday > Data.LastAgeCheckDate && birthday <= now)
                {
                    c.Age++;
                    Mod.log.Info($"[CivicVoice] {c.Name} turned {c.Age}");
                }
            }

            Data.LastAgeCheckDate = now;
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
                Wellbeing = cs.AverageCitizenHappiness;
                HomelessCount = cs.HomelessHouseholdCount;
                TeenCount = cs.TeenCount;
                AdultCount = cs.AdultCount;
                SeniorCount = cs.SeniorCount;
            }
            catch (Exception ex) { Mod.log.Warn($"Stats update failed (household): {ex.Message}"); }

            try
            {
                if (_statsSystem != null)
                {
                    Income = _statsSystem.GetStatisticValue(StatisticType.Income);
                    Expense = _statsSystem.GetStatisticValue(StatisticType.Expense);

                }
            }
            catch (Exception ex) { Mod.log.Warn($"Stats update failed (city stats): {ex.Message}"); }

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
            catch (Exception ex) { Mod.log.Warn($"Stats update failed (demand): {ex.Message}"); }


            try
            {
                var es = World.GetOrCreateSystemManaged<Game.UI.InGame.EducationInfoviewUISystem>();
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var capF = es?.GetType().GetField("m_UniversityCapacity", flags);
                var eligF = es?.GetType().GetField("m_UniversityEligible", flags);
                if (capF != null) UniversityCapacity = ((Colossal.UI.Binding.ValueBinding<int>)capF.GetValue(es)).value;
                if (eligF != null) UniversityEligible = ((Colossal.UI.Binding.ValueBinding<int>)eligF.GetValue(es)).value;
            }
            catch (Exception ex) { Mod.log.Warn($"Stats update failed (university): {ex.Message}"); }

            try
            {
                var hs = World.GetOrCreateSystemManaged<Game.UI.InGame.HealthcareInfoviewUISystem>();
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var healthF = hs?.GetType().GetField("m_AverageHealth", flags);
                if (healthF != null) Health = ((Colossal.UI.Binding.ValueBinding<float>)healthF.GetValue(hs)).value;
            }
            catch (Exception ex) { Mod.log.Warn($"Stats update failed (health): {ex.Message}"); }

            try
            {
                var ps = World.GetOrCreateSystemManaged<Game.UI.InGame.PoliceInfoviewUISystem>();
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                var probF = ps?.GetType().GetField("m_CrimeProbability", flags);
                var prodF = ps?.GetType().GetField("m_CrimeProducers", flags);
                if (probF != null && prodF != null)
                {
                    float prob = ((Colossal.UI.Binding.ValueBinding<float>)probF.GetValue(ps)).value;
                    int producers = ((Colossal.UI.Binding.ValueBinding<int>)prodF.GetValue(ps)).value;
                    CrimeRate = producers > 0 ? (prob / producers) / 250f : 0f;
                }
            }
            catch (Exception ex) { Mod.log.Warn($"Stats update failed (crime): {ex.Message}"); }

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
                GoalTarget = Math.Max(CrimeRate * 0.8f, 5f),
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
                GoalTarget = Math.Max(HomelessCount * 0.8f, 2f),
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
                GoalTarget = Math.Max(Unemployment * 0.8f, 2f),
                DeadlineGameDays = 6
            }, now);

            DoTryAddMetricProject("health", Health > 0 && Health < healthThreshold, now, new CivicProject
            {
                Title = "Improve healthcare services",
                Description = $"Average health level is at {Health:F0}. Residents are demanding better access to healthcare facilities.",
                Tier = ProjectTier.MetricTriggered,
                Type = ProjectType.Metric,
                Category = ProjectCategory.Healthcare,
                GoalType = MetricGoalType.HealthAbove,
                GoalTarget = Math.Min(Health + 10f, 60f),
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

            DoTryAddMetricProject("housing_low", LowDensityDemand > housingThreshold && !Data.ProposedProjects.Any(p => p.GoalType == MetricGoalType.LowDensityDemandBelow) && !Data.ActiveProjects.Any(p => p.GoalType == MetricGoalType.LowDensityDemandBelow), now, new CivicProject
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

            DoTryAddMetricProject("housing_med", MedDensityDemand > housingThreshold && !Data.ProposedProjects.Any(p => p.GoalType == MetricGoalType.MedDensityDemandBelow) && !Data.ActiveProjects.Any(p => p.GoalType == MetricGoalType.MedDensityDemandBelow), now, new CivicProject
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

            DoTryAddMetricProject("housing_high", HighDensityDemand > housingThreshold && !Data.ProposedProjects.Any(p => p.GoalType == MetricGoalType.HighDensityDemandBelow) && !Data.ActiveProjects.Any(p => p.GoalType == MetricGoalType.HighDensityDemandBelow), now, new CivicProject
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

        private string? GetMetricKey(CivicProject p) => p.GoalType switch
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
            if (Data.MetricProjectCooldowns.TryGetValue(key, out DateTime cooldownEnd) && currentTime.Date < cooldownEnd.Date)
            {
                return;
            }
            if (Data.ProposedProjects.Any(p => p.Title == project.Title)) return;
            if (Data.ActiveProjects.Any(p => p.Title == project.Title)) return;

            DoAssignVotes(project);
            Data.ProposedProjects.Add(project);
            int addCooldown = Mod.Settings?.MetricProposalCooldownMonths ?? 2;
            Data.MetricProjectCooldowns[key] = (_timeSystem?.GetCurrentDateTime() ?? DateTime.MinValue).AddDays(addCooldown);
            Mod.log.Info($"[CivicVoice] Metric proposal added: {project.Title} (key: {key})");
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
            Mod.log.Info($"[CivicVoice] AdHoc proposals added: {added} (mayor specialty: {Data.CurrentMayor?.Specialty})");
            DoNotify("Citizens have new ideas for the city.");
        }

        private int GetMayorWeight(CivicProject p, MayorSpecialty? specialty)
        {
            int weight = 1;
            if (specialty != null)
            {
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
                if (favoured.Contains(p.Category)) weight += 2;
            }
            float healthThreshold = Mod.Settings?.HealthThreshold ?? 50f;
            if (p.Title == "Build a new hospital" && Health < healthThreshold) weight += 4;
            return weight;
        }

        private List<CivicProject> DoBuildAdHocPool() => new List<CivicProject>
        {
            new CivicProject {
                Title = "Build a public park",
                Description = "Residents want green spaces to enjoy and relax in. A new park would improve quality of life.",
                Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Leisure,
                GoalType = MetricGoalType.None,
                DeadlineGameDays = 6, ManualCompletion = true },
            new CivicProject {
                Title = "Improve leisure facilities",
                Description = "Citizens want more recreational options. Expanding parks and leisure areas would improve quality of life.",
                Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Leisure,
                GoalType = MetricGoalType.None,
                DeadlineGameDays = 6, ManualCompletion = true },
            new CivicProject {
                Title = "Transition to greener facilities",
                Description = "Citizens want the city to invest in cleaner, more sustainable facilities to reduce its environmental footprint.",
                Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Environment,
                GoalType = MetricGoalType.None,
                DeadlineGameDays = 6, ManualCompletion = true },
            new CivicProject {
                Title = "Build a fire station",
                Description = "Residents want better fire protection across the city. A new fire station would improve safety.",
                Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.PublicSafety,
                GoalType = MetricGoalType.None,
                DeadlineGameDays = 6, ManualCompletion = true },
            new CivicProject {
                Title = "Develop a new shopping strip",
                Description = $"Citizens want more retail options. Commercial demand is at {CommercialDemand:F0}%. A new commercial area would boost the local economy.",
                Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Economy,
                GoalType = MetricGoalType.CommercialDemandBelow, GoalTarget = 40f,
                DeadlineGameDays = 6 },
            new CivicProject {
                Title = "Reduce industrial pollution",
                Description = "Residents near industrial areas are suffering from pollution affecting their quality of life.",
                Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Environment,
                GoalType = MetricGoalType.None,
                DeadlineGameDays = 6, ManualCompletion = true },
            new CivicProject {
                Title = "Build a new college",
                Description = "Citizens want better further education options. A new college would improve opportunities and skill levels.",
                Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Education,
                GoalType = MetricGoalType.None,
                DeadlineGameDays = 6, ManualCompletion = true },
            new CivicProject {
                Title = "Build a new hospital",
                Description = "Residents are concerned about healthcare access. A new hospital would improve medical coverage across the city.",
                Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Healthcare,
                GoalType = MetricGoalType.None,
                DeadlineGameDays = 6, ManualCompletion = true },
            new CivicProject {
                Title = "Improve public transport",
                Description = "Citizens want better bus and transit connections across the city.",
                Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Transport,
                GoalType = MetricGoalType.None,
                DeadlineGameDays = 6, ManualCompletion = true },
            new CivicProject {
                Title = "Expand the electricity network",
                Description = "More power infrastructure is needed to support city growth.",
                Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Infrastructure,
                GoalType = MetricGoalType.None,
                DeadlineGameDays = 6, ManualCompletion = true },
            new CivicProject {
                Title = "Build a new suburb",
                Description = "Citizens want new residential areas. Developing a new suburb would ease housing demand.",
                Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Housing,
                GoalType = MetricGoalType.LowDensityDemandBelow, GoalTarget = 40f,
                DeadlineGameDays = 6 },
            new CivicProject {
                Title = "Plant street trees",
                Description = "Citizens want greener streets. A tree planting initiative would improve air quality and city aesthetics.",
                Tier = ProjectTier.AdHoc, Type = ProjectType.Metric, Category = ProjectCategory.Environment,
                GoalType = MetricGoalType.None,
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
            if (UniversityCapacity >= UniversityEligible * 0.5f)
                pool.RemoveAll(p => p.Title == "Build a university campus");
            if (Data.CompletedProjects.Any(p => p.Title == "Construct a regional airport hub") ||
                Data.ActiveProjects.Any(p => p.Title == "Construct a regional airport hub") ||
                Data.ProposedProjects.Any(p => p.Title == "Construct a regional airport hub"))
                pool.RemoveAll(p => p.Title == "Construct a regional airport hub");
            DoShuffle(pool);

            int needed = MaxProposedMajor - proposedCount - activeCount;
            int added = 0;
            foreach (var p in pool.Take(needed))
            {
                if (Data.ProposedProjects.Any(x => x.Title == p.Title)) continue;
                if (Data.ActiveProjects.Any(x => x.Title == p.Title)) continue;
                if (p.Description == "PENDING_RESOURCE_PITCH")
                {
                    var usedResources = Data.ProposedProjects.Concat(Data.ActiveProjects)
                        .Where(x => x.Description != null && x.Description.Contains("environmental trade-offs"))
                        .Select(x => x.Description.Contains("oil") ? "oil" : x.Description.Contains("ore") ? "ore" : x.Description.Contains("timber") ? "timber" : "")
                        .ToHashSet();

                    var company = DoMakeCompanyName();
                    var resource = GetMostAbundantResource(usedResources);
                    if (resource.resourceName == "") continue; // all resources already pitched, skip this proposal entirely

                    p.Title = $"{company} resource investment pitch";
                    p.Description = $"A private investment firm has approached the city with geological survey data showing significant {resource.resourceName} reserves. They're proposing to fund extraction infrastructure in exchange for development rights. Citizens are divided on the environmental trade-offs.";
                }
                DoAssignVotes(p);
                Data.ProposedProjects.Add(p);
                added++;
            }
            Mod.log.Info($"[CivicVoice] Major proposals added: {added}");
            DoNotify("Citizens are proposing a major city project.");
        }

        private List<CivicProject> DoBuildMajorPool() => new List<CivicProject>
        {
            new CivicProject {
                Title = "Develop a new town district",
                Description = "Citizens want to expand the city with an entirely new district. This is a major undertaking that will take a full year.",
                Tier = ProjectTier.Major, Type = ProjectType.Metric, Category = ProjectCategory.Housing,
                GoalType = MetricGoalType.None,
                DeadlineGameDays = 12, ManualCompletion = true },
            new CivicProject {
                Title = "Resource investment pitch",
                Description = "PENDING_RESOURCE_PITCH",
                Tier = ProjectTier.Major, Type = ProjectType.Metric, Category = ProjectCategory.Economy,
                GoalType = MetricGoalType.None,
                DeadlineGameDays = 12, ManualCompletion = true },
            new CivicProject {
                Title = "Improve export capacity",
                Description = "Citizens want better trade infrastructure to grow the city's economy and open up new markets.",
                Tier = ProjectTier.Major, Type = ProjectType.Metric, Category = ProjectCategory.Infrastructure,
                GoalType = MetricGoalType.None,
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
                GoalType = MetricGoalType.None,
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
                GoalType = MetricGoalType.None,
                DeadlineGameDays = 12, ManualCompletion = true },
            new CivicProject {
                Title = "Construct a regional airport hub",
                Description = "Business leaders and residents are pushing for a major airport expansion to connect the city to regional and international destinations. A significant infrastructure commitment.",
                Tier = ProjectTier.Major, Type = ProjectType.Metric, Category = ProjectCategory.Infrastructure,
                GoalType = MetricGoalType.None,
                DeadlineGameDays = 12, ManualCompletion = true },
            new CivicProject {
                Title = "Launch a city regeneration scheme",
                Description = "Older districts are falling behind. Citizens want a coordinated effort to revitalise neglected neighbourhoods with new housing, green space, and services.",
                Tier = ProjectTier.Major, Type = ProjectType.Metric, Category = ProjectCategory.Housing,
                GoalType = MetricGoalType.None,
                DeadlineGameDays = 12, ManualCompletion = true },
        };

        // ── Vote assignment ───────────────────────────────────────────────
        private int DoGetProjectWeight(CivicProject p)
        {
            return p.Tier switch
            {
                ProjectTier.Major => Mod.Settings?.MajorProjectApprovalWeight ?? 3,
                ProjectTier.MetricTriggered => Mod.Settings?.UrgentProjectApprovalWeight ?? 1,
                _ => Mod.Settings?.AdHocProjectApprovalWeight ?? 1
            };
        }
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

                int newVotes = Math.Max(1, (int)(eligible * 0.003f) + _rng.Next(1, 4));
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
                string tierLabel = p.Tier == ProjectTier.MetricTriggered ? "Urgent" : p.Tier.ToString();
                DoNotify($"You already have the maximum active {tierLabel} projects.");
                return false;
            }

            p.Status = ProjectStatus.Active;
            p.StartGameDay = _lastTickDay;
            if (_timeSystem != null) p.StartDate = _timeSystem.GetCurrentDateTime();
            Data.ProposedProjects.Remove(p);
            Data.ActiveProjects.Add(p);
            Mod.log.Info($"[CivicVoice] Project accepted: {p.Title} (tier: {p.Tier})");
            DoNotify($"Project accepted: \"{p.Title}\"");
            return true;
        }

        public void RejectProject(string projectId)
        {
            var p = Data.ProposedProjects.FirstOrDefault(x => x.Id == projectId);
            if (p == null) return;
            p.Status = ProjectStatus.Rejected;
            Data.ProposedProjects.Remove(p);
            Mod.log.Info($"[CivicVoice] Project rejected: {p.Title}");

            if (p.Tier == ProjectTier.MetricTriggered && _timeSystem != null)
            {
                string? key = GetMetricKey(p);
                if (key != null)
                {
                    int cooldownDays = Mod.Settings?.RejectedCooldownMonths ?? 6;
                    DateTime now = _timeSystem.GetCurrentDateTime();
                    Data.MetricProjectCooldowns[key] = now.AddDays(cooldownDays);
                    Mod.log.Info($"[CivicVoice] Cooldown set for {key} until {now.AddDays(cooldownDays)}");
                    DoNotify($"Proposal rejected: \"{p.Title}\".");
                }
            }
            if (p.Title == "Reduce industrial pollution" && _timeSystem != null)
            {
                int cooldownDays = Mod.Settings?.RejectedCooldownMonths ?? 6;
                Data.MetricProjectCooldowns["pollution"] = _timeSystem.GetCurrentDateTime().AddDays(cooldownDays);
            }
        }

        public void AbandonProject(string projectId)
        {
            var p = Data.ActiveProjects.FirstOrDefault(x => x.Id == projectId);
            if (p == null) return;
            Data.ActiveProjects.Remove(p);
            Data.TotalProjectsAbandoned++;
            Data.TermProjectsAbandoned += 1;
            Mod.log.Info($"[CivicVoice] Project abandoned: {p.Title}");
            DoNotify($"Project abandoned: \"{p.Title}\".");
        }

        public void MarkProjectComplete(string projectId)
        {
            var p = Data.ActiveProjects.FirstOrDefault(x => x.Id == projectId && x.ManualCompletion);
            if (p == null) return;
            p.MarkedComplete = true;
            Mod.log.Info($"[CivicVoice] Major project marked complete: {p.Title}");
        }

        public void TriggerForceElection()
        {
            _forceElection = true;
            Mod.log.Info("[CivicVoice] Force election triggered.");
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
            Data.TermProjectsCompleted += 1;
            Mod.log.Info($"[CivicVoice] Project completed: {p.Title}");
            DoNotify($"Project complete: \"{p.Title}\"!");
        }

        private void DoFailProject(CivicProject p)
        {
            p.Status = ProjectStatus.Failed;
            Data.ActiveProjects.Remove(p);
            Data.TotalProjectsFailed++;
            Data.TermProjectsFailed += 1;
            Mod.log.Info($"[CivicVoice] Project failed: {p.Title}");
            DoNotify($"Project failed: \"{p.Title}\".");

            if (p.Tier == ProjectTier.MetricTriggered && _timeSystem != null)
            {
                string? key = GetMetricKey(p);
                if (key != null)
                {
                    DateTime now = _timeSystem.GetCurrentDateTime();
                    Data.MetricProjectCooldowns[key] = now.AddDays(3);
                    Mod.log.Info($"[CivicVoice] Cooldown set for {key} until {now.AddDays(3)}");
                }
            }

            if (p.Title == "Reduce industrial pollution" && _timeSystem != null)
                Data.MetricProjectCooldowns["pollution"] = _timeSystem.GetCurrentDateTime().AddDays(3);
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
                    var previousMayor = Data.CurrentMayor;
                    var winner = Data.CurrentElection.Candidates.OrderByDescending(c => c.Votes).First();
                    DoSnapshotElection(Data.CurrentElection);
                    Data.CurrentElection.Winner = winner;
                    Data.CurrentElection.IsActive = false;
                    int electionFreq = Mod.Settings?.ElectionFrequencyMonths ?? 12;
                    Data.NextElectionDate = now.AddDays(electionFreq);
                    Mod.log.Info($"[CivicVoice] Election won by: {winner.Name} ({winner.PartyName})");
                    DoNotify($"{winner.Name} ({winner.PartyName}) has won the election!");
                    winner.TermsServed++;
                    Data.CurrentMayor = winner;
                    Data.TermProjectsCompleted = 0;
                    Data.TermProjectsFailed = 0;
                    Data.TermProjectsAbandoned = 0;
                    Data.TermReviewIssued = false;
                    Data.MayorElectedDate = now;
                    DoIssueNewspaper(NewspaperEventType.Election, previousMayor);
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
                var fc3 = Data.CurrentElection.Candidates[2].Name;
                Mod.log.Info($"[CivicVoice] Forced election started: {fc1} vs {fc2} vs {fc3}");
                DoNotify($"A special election has been called! {fc1} vs {fc2} vs {fc3}");
                return;
            }

            if (Data.NextElectionDate == DateTime.MinValue)
            {
                Data.NextElectionDate = now;
                if (!Data.FirstElectionAnnounced)
                {
                    Data.FirstElectionAnnounced = true;
                    Mod.log.Info("[CivicVoice] First election triggered.");
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
            var c3 = Data.CurrentElection.Candidates[2].Name;
            Mod.log.Info($"[CivicVoice] Election started: {c1} vs {c2} vs {c3}");
            DoNotify($"Election time! {c1} vs {c2} vs {c3}");
        }

        public void CastVote(string candidateName)
        {
            if (Data.CurrentElection == null || !Data.CurrentElection.IsActive || Data.CurrentElection.HasVoted) return;
            Data.CurrentElection.HasVoted = true;
            float influence = (Mod.Settings?.EndorsementInfluencePercent ?? 5) / 100f;
            int totalVotes = Data.CurrentElection.Candidates.Sum(c => c.Votes);
            foreach (var c in Data.CurrentElection.Candidates)
            {
                if (c.Name == candidateName)
                    c.Votes += Math.Max(1, (int)(totalVotes * influence));
            }
            Mod.log.Info($"[CivicVoice] Vote cast for: {candidateName}");
            DoNotify($"You have endorsed {candidateName}. The election will conclude at the end of the voting period.");
        }

        public void ConcludeElection()
        {
            if (Data.CurrentElection == null || !Data.CurrentElection.IsActive) return;
            var previousMayor = Data.CurrentMayor;
            var winner = Data.CurrentElection.Candidates.OrderByDescending(c => c.Votes).First();
            DoSnapshotElection(Data.CurrentElection);
            Data.CurrentElection.Winner = winner;
            Data.CurrentElection.IsActive = false;
            winner.TermsServed++;
            Data.CurrentMayor = winner;
            Data.TermProjectsCompleted = 0;
            Data.TermProjectsFailed = 0;
            Data.TermProjectsAbandoned = 0;
            Data.TermReviewIssued = false;
            Data.MayorElectedDate = GetCurrentGameDate();
            int electionFreq = Mod.Settings?.ElectionFrequencyMonths ?? 12;
            Data.NextElectionDate = GetCurrentGameDate().AddDays(electionFreq);
            Mod.log.Info($"[CivicVoice] Election concluded manually: {winner.Name}");
            DoNotify($"{winner.Name} ({winner.PartyName}) has won the election!");
            DoIssueNewspaper(NewspaperEventType.Election, previousMayor);
        }

        public DateTime GetCurrentGameDate()
        {
            if (_timeSystem == null) return DateTime.MinValue;
            try { return _timeSystem.GetCurrentDateTime(); }
            catch { return DateTime.MinValue; }
        }

        private int DoGetApprovalScore()
        {
            return Math.Max(0, Math.Min(100,
                (Data.TermProjectsCompleted * 8) - (Data.TermProjectsFailed * 12) - (Data.TermProjectsAbandoned * 12) + 50));
        }

        private MayorElection DoGenerateElection()
        {
            var allSpecialties = (MayorSpecialty[])Enum.GetValues(typeof(MayorSpecialty));
            DoShuffle(allSpecialties);

            int eligibleVoters = Math.Max(10, TeenCount + AdultCount + SeniorCount);
            var candidates = new List<MayorCandidate>();

            // Check if incumbent can run
            bool incumbentRuns = Data.CurrentMayor != null && Data.CurrentMayor.TermsServed < 2;

            // Turnout: 50–75% base, nudged by approval (high approval → more engagement)
            int approval = DoGetApprovalScore();
            float approvalNudge = (approval - 50) / 100f * 0.10f; // -5% to +5%
            float turnoutBase = 0.50f + (float)_rng.NextDouble() * 0.25f;
            float turnoutFraction = Math.Max(0.35f, Math.Min(0.85f, turnoutBase + approvalNudge));
            int totalVoters = Math.Max(5, (int)(eligibleVoters * turnoutFraction));

            if (incumbentRuns)
            {
                // Incumbent runs with approval-weighted votes
                float incumbentShare = approval >= 70 ? 0.45f
                                     : approval >= 40 ? 0.33f
                                     : 0.20f;

                var incumbent = Data.CurrentMayor!;
                incumbent.Votes = (int)(totalVoters * incumbentShare);
                candidates.Add(incumbent);

                // Two fresh challengers split remaining votes
                int remainingVoters = totalVoters - incumbent.Votes;
                var usedSpecialties = new HashSet<MayorSpecialty> { incumbent.Specialty };
                int added = 0;
                foreach (var s in allSpecialties)
                {
                    if (usedSpecialties.Contains(s)) continue;
                    var challenger = DoMakeCandidate(s);
                    float challengerShare = added == 0
                        ? 0.5f + (float)(_rng.NextDouble() - 0.5) * 0.2f
                        : 1f;
                    challenger.Votes = added == 0
                        ? (int)(remainingVoters * challengerShare)
                        : remainingVoters - candidates[1].Votes;
                    candidates.Add(challenger);
                    usedSpecialties.Add(s);
                    added++;
                    if (added >= 2) break;
                }
            }
            else
            {
                // Three fresh challengers
                int idx = 0;
                foreach (var s in allSpecialties)
                {
                    if (idx >= 3) break;
                    candidates.Add(DoMakeCandidate(s));
                    idx++;
                }

                // Distribute votes randomly across three
                float share1 = 0.25f + (float)_rng.NextDouble() * 0.30f;
                float share2 = 0.20f + (float)_rng.NextDouble() * 0.25f;
                if (share1 + share2 > 0.90f) share2 = 0.90f - share1;
                candidates[0].Votes = (int)(totalVoters * share1);
                candidates[1].Votes = (int)(totalVoters * share2);
                candidates[2].Votes = Math.Max(0, totalVoters - candidates[0].Votes - candidates[1].Votes);
            }

            DoShuffle(candidates);
            return new MayorElection { Candidates = candidates };
        }

        private MayorCandidate DoMakeCandidate(MayorSpecialty s)
        {
            int month = _rng.Next(1, 13);
            return new MayorCandidate
            {
                Name = s_FirstNames[_rng.Next(s_FirstNames.Length)] + " " + s_LastNames[_rng.Next(s_LastNames.Length)],
                Age = _rng.Next(35, 68),
                Specialty = s,
                PartyName = s_Parties[_rng.Next(s_Parties.Length)],
                Slogan = DoGetSlogan(s),
                BirthdayMonth = month,
                BirthdayDay = _rng.Next(1, DateTime.DaysInMonth(2000, month) + 1)
            };
        }

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

        private string DoMakeCompanyName()
        {
            return NewspaperContent.CompanyPrefixes[_rng.Next(NewspaperContent.CompanyPrefixes.Length)] + " " +
                   NewspaperContent.CompanySuffixes[_rng.Next(NewspaperContent.CompanySuffixes.Length)];
        }

        private string DoMakeShopName()      => NewspaperContent.ShopNames[_rng.Next(NewspaperContent.ShopNames.Length)];
        private string DoMakeCafeName()      => NewspaperContent.CafeNames[_rng.Next(NewspaperContent.CafeNames.Length)];
        private string DoMakeRestaurantName()=> NewspaperContent.RestaurantNames[_rng.Next(NewspaperContent.RestaurantNames.Length)];
        private string DoMakeSalonName()     => NewspaperContent.SalonNames[_rng.Next(NewspaperContent.SalonNames.Length)];
        private string DoMakeHardwareName()  => NewspaperContent.HardwareNames[_rng.Next(NewspaperContent.HardwareNames.Length)];
        private string DoMakeDeliName()      => NewspaperContent.DeliNames[_rng.Next(NewspaperContent.DeliNames.Length)];
        private string DoMakeGymName()       => NewspaperContent.GymNames[_rng.Next(NewspaperContent.GymNames.Length)];

        // ── Newspaper: tiers ──────────────────────────────────────────────
        private enum ElectionTier { Landslide, Close, IncumbentDefeat }
        // private enum ReviewTier { Good, Mixed, Poor } // review disabled

        private ElectionTier DoGetElectionTier(float winnerPercent, float runnerUpPercent, bool incumbentLost)
        {
            if (incumbentLost) return ElectionTier.IncumbentDefeat;
            float margin = winnerPercent - runnerUpPercent;
            return margin >= 15f ? ElectionTier.Landslide : ElectionTier.Close;
        }

        // review disabled — DoGetReviewTier removed

        // ── Newspaper: headline pools ─────────────────────────────────────
        private static readonly string[] s_HeraldElectionLandslide = {
            "Voters hand {winner} a commanding mandate",
            "{winner} storms to victory in emphatic result",
            "{winner} wins city hall with a dominant share of the vote",
        };
        private static readonly string[] s_HeraldElectionClose = {
            "{winner} edges out rivals in nail-biting finish",
            "Narrow victory for {winner} after a long night",
            "{winner} elected mayor by the slimmest of margins",
        };
        private static readonly string[] s_HeraldElectionIncumbentDefeat = {
            "Voters oust {incumbent} and back {winner}",
            "Change at city hall as {winner} defeats {incumbent}",
            "{incumbent} loses seat as {winner} takes office",
        };

        private static readonly string[] s_UproarElectionLandslide = {
            "{winner} wins so big it's almost embarrassing",
            "Did anyone tell {winner} it was meant to be close?",
            "Election over before it started — {winner} romps home",
        };
        private static readonly string[] s_UproarElectionClose = {
            "{winner} wins by a margin thinner than city hall's coffee",
            "Nail-biter ends with {winner} just about on top",
            "It went to the wire and {winner} scraped through",
        };
        private static readonly string[] s_UproarElectionIncumbentDefeat = {
            "{incumbent} packs desk, {winner} moves straight in",
            "Voters say thanks but no thanks to {incumbent}",
            "{winner} shows {incumbent} the door in shock result",
        };

        private static readonly string[] s_PulseElectionLandslide = {
            "{winner} elected mayor in decisive result",
            "{winner} secures a strong mandate from city voters",
            "Residents back {winner} by a wide margin",
        };
        private static readonly string[] s_PulseElectionClose = {
            "{winner} elected mayor after tight contest",
            "Close race ends with {winner} narrowly ahead",
            "{winner} wins majority in night of counting",
        };
        private static readonly string[] s_PulseElectionIncumbentDefeat = {
            "{winner} unseats incumbent {incumbent}",
            "Voters choose {winner} over sitting mayor {incumbent}",
            "{winner} defeats {incumbent} to claim the mayoralty",
        };

        // ── Splash pools (line1 | line2, separated by "|") ──────────────
        private static readonly string[] s_HeraldSplashLandslide = {
            "{WINNERSURNAME} ELECTED|BY WIDE MARGIN",
            "A MANDATE FOR|{WINNERSURNAME}",
            "{WINNERSURNAME} WINS|IN DOMINANT FASHION",
        };
        private static readonly string[] s_HeraldSplashClose = {
            "{WINNERSURNAME} WINS|IN NARROW CONTEST",
            "CLOSE RACE ENDS|FOR {WINNERSURNAME}",
            "{WINNERSURNAME} ELECTED|BY SLIM MARGIN",
        };
        private static readonly string[] s_HeraldSplashIncumbentDefeat = {
            "{INCUMBENTSURNAME} OUSTED|{WINNERSURNAME} ELECTED",
            "CHANGE AT|CITY HALL",
            "{WINNERSURNAME} DEFEATS|{INCUMBENTSURNAME}",
        };

        private static readonly string[] s_PulseSplashLandslide = {
            "DECISIVE WIN|FOR {WINNERSURNAME}",
            "{WINNERSURNAME}|SWEEPS TO POWER",
            "CITY BACKS|{WINNERSURNAME}",
        };
        private static readonly string[] s_PulseSplashClose = {
            "{WINNERSURNAME}|NARROWLY WINS",
            "A CLOSE RACE|{WINNERSURNAME} AHEAD",
            "TIGHT CONTEST|{WINNERSURNAME} PREVAILS",
        };
        private static readonly string[] s_PulseSplashIncumbentDefeat = {
            "VOTERS CHOOSE|CHANGE",
            "{WINNERSURNAME}|UNSEATS {INCUMBENTSURNAME}",
            "NEW MAYOR|{WINNERSURNAME} ELECTED",
        };

        private static readonly string[] s_UproarSplashLandslide = {
            "{WINNERSURNAME} ROMPS!|CITY BACKS {WINNERSURNAME}",
            "DOMINANT!|{WINNERSURNAME} TAKES IT",
            "{WINNERSURNAME} WINS BIG|LANDSLIDE VERDICT",
        };
        private static readonly string[] s_UproarSplashClose = {
            "{WINNERSURNAME}|JUST AHEAD",
            "TOO CLOSE!|{WINNERSURNAME} SQUEAKS THROUGH",
            "WHAT A NIGHT!|{WINNERSURNAME} INCHES IT",
        };
        private static readonly string[] s_UproarSplashIncumbentDefeat = {
            "{INCUMBENTSURNAME} OUT!|{WINNERSURNAME} STUNNER",
            "SHOCK RESULT!|{INCUMBENTSURNAME} FALLS",
            "{WINNERSURNAME} IN!|{INCUMBENTSURNAME} OUSTED",
        };

        // review headline pools removed — review disabled


        private static string Surname(string fullName)
        {
            var parts = fullName.Trim().Split(' ');
            return parts[parts.Length - 1];
        }

        private string ResolveTemplate(string template, Dictionary<string, string> vars)
        {
            string result = template;
            foreach (var kv in vars)
                result = result.Replace("{" + kv.Key + "}", kv.Value);
            return result;
        }

        private string DoPickBeefyFiller()
        {
            return DoResolveShopPlaceholders(NewspaperContent.BeefyFillerPool[_rng.Next(NewspaperContent.BeefyFillerPool.Length)]);
        }

        private string DoPickFiller()
        {
            var available = Enumerable.Range(0, NewspaperContent.FillerPool.Length)
                .Where(i => !Data.RecentFillerIndices.Contains(i))
                .ToList();

            if (available.Count == 0)
                available = Enumerable.Range(0, NewspaperContent.FillerPool.Length).ToList();

            int chosen = available[_rng.Next(available.Count)];

            Data.RecentFillerIndices.Add(chosen);
            while (Data.RecentFillerIndices.Count > 12)
                Data.RecentFillerIndices.RemoveAt(0);

            return DoResolveShopPlaceholders(NewspaperContent.FillerPool[chosen]);
        }

        private string DoResolveShopPlaceholders(string text)
        {
            if (!text.Contains("{shop") && !text.Contains("{company}") && !text.Contains("{cafe}") &&
                !text.Contains("{restaurant}") && !text.Contains("{salon}") && !text.Contains("{hardware}") &&
                !text.Contains("{deli}") && !text.Contains("{gym}"))
                return text;

            return ResolveTemplate(text, new Dictionary<string, string>
            {
                ["shop"]       = DoMakeShopName(),
                ["company"]    = DoMakeCompanyName(),
                ["cafe"]       = DoMakeCafeName(),
                ["restaurant"] = DoMakeRestaurantName(),
                ["salon"]      = DoMakeSalonName(),
                ["hardware"]   = DoMakeHardwareName(),
                ["deli"]       = DoMakeDeliName(),
                ["gym"]        = DoMakeGymName(),
            });
        }

        private string[] DoPickTeasers()
        {
            var pool = Enumerable.Range(0, NewspaperContent.TeaserPool.Length).ToList();
            var result = new string[4];
            for (int i = 0; i < 4; i++)
            {
                int idx = _rng.Next(pool.Count);
                result[i] = DoResolveShopPlaceholders(NewspaperContent.TeaserPool[pool[idx]]);
                pool.RemoveAt(idx);
            }
            return result;
        }

        // ── Newspaper: style + headline pool selection ────────────────────
        private static readonly string[] s_StylesByWeight = { "herald", "herald", "uproar", "uproar", "pulse", "pulse" };

        private string DoPickStyle() => s_StylesByWeight[_rng.Next(s_StylesByWeight.Length)];

        private string[] DoGetElectionHeadlinePool(string style, ElectionTier tier) => (style, tier) switch
        {
            ("herald", ElectionTier.Landslide) => s_HeraldElectionLandslide,
            ("herald", ElectionTier.Close) => s_HeraldElectionClose,
            ("herald", ElectionTier.IncumbentDefeat) => s_HeraldElectionIncumbentDefeat,
            ("uproar", ElectionTier.Landslide) => s_UproarElectionLandslide,
            ("uproar", ElectionTier.Close) => s_UproarElectionClose,
            ("uproar", ElectionTier.IncumbentDefeat) => s_UproarElectionIncumbentDefeat,
            ("pulse", ElectionTier.Landslide) => s_PulseElectionLandslide,
            ("pulse", ElectionTier.Close) => s_PulseElectionClose,
            _ => s_PulseElectionIncumbentDefeat,
        };

        // DoGetReviewHeadlinePool removed — review disabled

        // ── Newspaper: snapshot + payload builders ────────────────────────
        private void DoSnapshotElection(MayorElection election)
        {
            int total = election.Candidates.Sum(c => c.Votes);
            int eligible = Math.Max(10, TeenCount + AdultCount + SeniorCount);
            election.SnapshotTotalVotes = total;
            election.SnapshotTurnoutPercent = eligible > 0 ? Math.Min(100f, (total / (float)eligible) * 100f) : 0f;
            foreach (var c in election.Candidates)
                c.SnapshotVotePercent = total > 0 ? (c.Votes / (float)total) * 100f : 0f;
        }

        // ── Newspaper: payload builders ───────────────────────────────────
        private CivicVoice.Models.NewspaperPayload DoBuildElectionPayload(MayorElection election, MayorCandidate winner, MayorCandidate? previousMayor)
        {
            string style = DoPickStyle();
            string cityName = World.GetOrCreateSystemManaged<Game.City.CityConfigurationSystem>().cityName;

            var ordered = election.Candidates.OrderByDescending(c => c.Votes).ToList();
            int eligibleVoters = Math.Max(10, TeenCount + AdultCount + SeniorCount);

            Mod.log.Info($"[CivicVoice] DoBuildElectionPayload — totalVotes={election.SnapshotTotalVotes}, eligible={eligibleVoters}, candidates: {string.Join(", ", ordered.Select(c => $"{c.Name}={c.SnapshotVotePercent:F1}%"))}");

            var challengers = ordered.Where(c => c.Name != winner.Name).ToList();

            var winnerPct = winner.SnapshotVotePercent;
            var runnerUp = challengers.FirstOrDefault();
            var runnerUpPct = runnerUp?.SnapshotVotePercent ?? 0f;

            bool incumbentLost = previousMayor != null && previousMayor.Name != winner.Name;
            var tier = DoGetElectionTier(winnerPct, runnerUpPct, incumbentLost);

            var pool = DoGetElectionHeadlinePool(style, tier);
            string winnerSurname = Surname(winner.Name).ToUpper();
            string incumbentSurname = previousMayor != null ? Surname(previousMayor.Name).ToUpper() : "";
            var vars = new Dictionary<string, string>
            {
                ["winner"] = winner.Name,
                ["incumbent"] = previousMayor?.Name ?? "",
                ["WINNERSURNAME"] = winnerSurname,
                ["INCUMBENTSURNAME"] = incumbentSurname,
            };
            string headline = ResolveTemplate(pool[_rng.Next(pool.Length)], vars);

            string[] splashPool = style == "herald"
                ? (tier == ElectionTier.Landslide ? s_HeraldSplashLandslide
                 : tier == ElectionTier.Close     ? s_HeraldSplashClose
                 :                                  s_HeraldSplashIncumbentDefeat)
                : style == "uproar"
                ? (tier == ElectionTier.Landslide ? s_UproarSplashLandslide
                 : tier == ElectionTier.Close     ? s_UproarSplashClose
                 :                                  s_UproarSplashIncumbentDefeat)
                : (tier == ElectionTier.Landslide ? s_PulseSplashLandslide
                 : tier == ElectionTier.Close     ? s_PulseSplashClose
                 :                                  s_PulseSplashIncumbentDefeat);
            string rawSplash = ResolveTemplate(splashPool[_rng.Next(splashPool.Length)], vars);
            var splashParts = rawSplash.Split('|');
            string splashLine1 = splashParts.Length > 0 ? splashParts[0] : "";
            string splashLine2 = splashParts.Length > 1 ? splashParts[1] : "";

            var content = new CivicVoice.Models.NewspaperElectionContent
            {
                CityName = cityName,
                Winner = new CivicVoice.Models.NewspaperCandidate
                {
                    Name = winner.Name,
                    Age = winner.Age,
                    Party = winner.PartyName,
                    VotePercent = winnerPct,
                    IsWinner = true,
                },
                Challengers = challengers
                    .Select(c => new CivicVoice.Models.NewspaperCandidate
                    {
                        Name = c.Name,
                        Age = c.Age,
                        Party = c.PartyName,
                        VotePercent = c.SnapshotVotePercent,
                        IsWinner = false,
                    })
                    .ToList(),
                TurnoutPercent = election.SnapshotTurnoutPercent,
                EligibleVoters = eligibleVoters,
            };

            return new CivicVoice.Models.NewspaperPayload
            {
                Style = style,
                EventType = "election",
                Headline = headline,
                SplashLine1 = splashLine1,
                SplashLine2 = splashLine2,
                Quote = ResolveTemplate(NewspaperContent.WinnerQuotePool[_rng.Next(NewspaperContent.WinnerQuotePool.Length)], vars),
                FillerText = DoPickFiller(),
                FillerText2 = DoPickBeefyFiller(),
                Teasers = DoPickTeasers(),
                ElectionContent = content,
            };
        }

        // DoBuildReviewPayload removed — review disabled

        public void DoIssueNewspaper(CivicVoice.Models.NewspaperEventType eventType, MayorCandidate? previousMayor = null)
        {
            CivicVoice.Models.NewspaperPayload? payload = null;

            if (eventType == CivicVoice.Models.NewspaperEventType.Election && Data.CurrentElection?.Winner != null)
                payload = DoBuildElectionPayload(Data.CurrentElection, Data.CurrentElection.Winner, previousMayor);
            // else if (eventType == CivicVoice.Models.NewspaperEventType.Review && Data.CurrentMayor != null)
            //     payload = DoBuildReviewPayload(Data.CurrentMayor); // review disabled

            if (payload == null)
            {
                Mod.log.Warn("[CivicVoice] DoIssueNewspaper called but required data was missing — skipped.");
                return;
            }

            Data.PendingNewspaper = payload;
            Mod.log.Info($"[CivicVoice] Newspaper issued: {payload.Style} / {payload.EventType} — \"{payload.Headline}\"");
        }

        public void CloseNewspaper()
        {
            Mod.log.Info("[CivicVoice] CloseNewspaper trigger received.");
            Data.PendingNewspaper = null;
        }

        // DoCheckForTermReview removed — review disabled

        // ── Serialization ─────────────────────────────────────────────────
        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            try
            {
                writer.Write(JsonConvert.SerializeObject(Data, Formatting.None));
                Mod.log.Info("[CivicVoice] Data serialized.");
            }
            catch (Exception ex)
            {
                writer.Write("");
                Mod.log.Warn($"[CivicVoice] Serialize failed: {ex.Message}");
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
                    Data.PendingNewspaper = null;
                    foreach (var p in Data.ProposedProjects.Where(p => p.Tier == ProjectTier.AdHoc && p.Title != "Develop a new shopping strip" && p.Title != "Build a new suburb"))
                        p.ManualCompletion = true;
                    foreach (var p in Data.ActiveProjects.Where(p => p.Tier == ProjectTier.AdHoc && p.Title != "Develop a new shopping strip" && p.Title != "Build a new suburb"))
                        p.ManualCompletion = true;
                    _lastTickDay = Data.LastTickDay;
                    Mod.log.Info($"[CivicVoice] Deserialized. Projects: {Data.ProposedProjects.Count} proposed, {Data.ActiveProjects.Count} active. Mayor: {Data.CurrentMayor?.Name ?? "None"}");
                }
                _loaded = true;
            }
            catch (Exception ex)
            {
                Mod.log.Warn($"[CivicVoice] Deserialize failed: {ex.Message}");
                Data = new CivicVoiceSaveData();
                _loaded = true;
            }
        }

        public void SetDefaults(Context context)
        {
            Data = new CivicVoiceSaveData();
            _lastTickDay = -1;
            _loaded = true;
            Mod.log.Info("[CivicVoice] SetDefaults called — new city.");
        }

        // ── Helpers ───────────────────────────────────────────────────────
        private void DoNotify(string msg)
        {
            Notifications.Add(msg);
            Mod.log.Info($"[CivicVoice] {msg}");
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
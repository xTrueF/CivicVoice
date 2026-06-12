// ============================================================
// CivicVoice — Democracy & Governance Mod for Cities: Skylines II
// Created by xTrueF | github.com/xTrueF/CivicVoice
// Licensed under MIT License
// ============================================================
using System;
using System.Collections.Generic;

namespace CivicVoice.Models
{
    public enum ProjectType { Physical, Metric }

    public enum ProjectTier { AdHoc, MetricTriggered, Major }

    public enum ProjectCategory
    {
        Healthcare, Education, Transport, Environment,
        Economy, PublicSafety, Leisure, Housing, Infrastructure
    }

    public enum ProjectStatus
    {
        Proposed, Active, Completed, Failed, Rejected
    }

    public enum MetricGoalType
    {
        UnemploymentBelow, HappinessAbove, PopulationAbove,
        CrimeRateBelow, BudgetSurplus, HomelessBelow,
        HealthAbove, WellbeingAbove,
        CommercialDemandBelow,
        LowDensityDemandBelow, MedDensityDemandBelow, HighDensityDemandBelow
    }

    [Serializable]
    public class CivicProject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ProjectType Type { get; set; }
        public ProjectTier Tier { get; set; } = ProjectTier.AdHoc;
        public ProjectCategory Category { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Proposed;
        public int VotesFor { get; set; }
        public int VotesAgainst { get; set; }
        public float VoteSharePercent => VotesFor + VotesAgainst == 0
            ? 0f : (float)VotesFor / (VotesFor + VotesAgainst) * 100f;
        public MetricGoalType? GoalType { get; set; }
        public float GoalTarget { get; set; }
        public float CurrentValue { get; set; }
        public int StartGameDay { get; set; }
        public int DeadlineGameDays { get; set; } = 60;
        public DateTime StartDate { get; set; } = DateTime.MinValue;
        public bool ManualCompletion { get; set; } = false;
        public bool MarkedComplete { get; set; } = false;

        public bool IsComplete()
        {
            if (ManualCompletion) return MarkedComplete;
            if (GoalType == null) return false;
            return GoalType switch
            {
                MetricGoalType.UnemploymentBelow => CurrentValue <= GoalTarget,
                MetricGoalType.CrimeRateBelow => CurrentValue <= GoalTarget,
                MetricGoalType.HomelessBelow => CurrentValue <= GoalTarget,
                MetricGoalType.CommercialDemandBelow => CurrentValue <= GoalTarget,
                MetricGoalType.LowDensityDemandBelow => CurrentValue <= GoalTarget,
                MetricGoalType.MedDensityDemandBelow => CurrentValue <= GoalTarget,
                MetricGoalType.HighDensityDemandBelow => CurrentValue <= GoalTarget,
                MetricGoalType.HappinessAbove => CurrentValue >= GoalTarget,
                MetricGoalType.PopulationAbove => CurrentValue >= GoalTarget,
                MetricGoalType.BudgetSurplus => CurrentValue >= GoalTarget,
                MetricGoalType.HealthAbove => CurrentValue >= GoalTarget,
                MetricGoalType.WellbeingAbove => CurrentValue >= GoalTarget,
                _ => false
            };
        }

        public string GetProgressText()
        {
            if (Tier == ProjectTier.AdHoc)
                return "In progress";
            if (ManualCompletion)
                return MarkedComplete ? "Marked as complete" : "In progress — mark complete when done";
            return GoalType switch
            {
                MetricGoalType.UnemploymentBelow => $"Unemployment: {CurrentValue:F1}% (target: <{GoalTarget:F1}%)",
                MetricGoalType.HappinessAbove => $"Happiness: {CurrentValue:F1}% (target: >{GoalTarget:F1}%)",
                MetricGoalType.PopulationAbove => $"Population: {CurrentValue:N0} (target: >{GoalTarget:N0})",
                MetricGoalType.CrimeRateBelow => $"Crime rate: {CurrentValue:F1} (target: <{GoalTarget:F1})",
                MetricGoalType.HomelessBelow => $"Homeless: {(int)CurrentValue} (target: <{(int)GoalTarget})",
                MetricGoalType.HealthAbove => $"Health: {CurrentValue:F1} (target: >{GoalTarget:F1})",
                MetricGoalType.WellbeingAbove => $"Wellbeing: {CurrentValue:F1} (target: >{GoalTarget:F1})",
                MetricGoalType.BudgetSurplus => $"Budget: £{CurrentValue:N0} (target: surplus)",
                MetricGoalType.CommercialDemandBelow => $"Commercial demand: {CurrentValue:F0}% (target: <{GoalTarget:F0}%)",
                MetricGoalType.LowDensityDemandBelow => $"Low density demand: {CurrentValue:F0}% (target: <{GoalTarget:F0}%)",
                MetricGoalType.MedDensityDemandBelow => $"Medium density demand: {CurrentValue:F0}% (target: <{GoalTarget:F0}%)",
                MetricGoalType.HighDensityDemandBelow => $"High density demand: {CurrentValue:F0}% (target: <{GoalTarget:F0}%)",
                _ => "In progress"
            };
        }
    }

    public enum MayorSpecialty
    {
        Economy, Environment, Infrastructure,
        Healthcare, Education, PublicSafety
    }

    [Serializable]
    public class MayorCandidate
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public MayorSpecialty Specialty { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string Slogan { get; set; } = string.Empty;
        public int Votes { get; set; }
        public float VoteSharePercent(int totalVotes) =>
            totalVotes == 0 ? 0f : (float)Votes / totalVotes * 100f;
    }

    [Serializable]
    public class MayorElection
    {
        public string ElectionId { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public List<MayorCandidate> Candidates { get; set; } = new();
        public MayorCandidate? Winner { get; set; }
        public bool IsActive { get; set; } = true;
        public bool HasVoted { get; set; } = false;
        public int ElectionGameDay { get; set; } = 0;
    }

    [Serializable]
    public class CivicVoiceSaveData
    {
        public List<CivicProject> ProposedProjects { get; set; } = new();
        public List<CivicProject> ActiveProjects { get; set; } = new();
        public List<CivicProject> CompletedProjects { get; set; } = new();
        public MayorElection? CurrentElection { get; set; }
        public MayorCandidate? CurrentMayor { get; set; }
        public DateTime ElectionStartDate { get; set; } = DateTime.MinValue;
        public DateTime LastElectionDate { get; set; } = DateTime.MinValue;
        public DateTime NextElectionDate { get; set; } = DateTime.MinValue;
        public int TotalProjectsCompleted { get; set; } = 0;
        public int TotalProjectsFailed { get; set; } = 0;
        public int LastTickDay { get; set; } = -1;
        public bool FirstElectionAnnounced { get; set; } = false;
        public DateTime LastProjectGenerationDate { get; set; } = DateTime.MinValue;
        public DateTime LastAdHocProjectDate { get; set; } = DateTime.MinValue;
        public DateTime LastMajorProjectDate { get; set; } = DateTime.MinValue;
        public DateTime LastVoteFluctuationDate { get; set; } = DateTime.MinValue;
        public Dictionary<string, DateTime> MetricProjectCooldowns { get; set; } = new();
    }
}
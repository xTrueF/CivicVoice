// ============================================================
// CivicVoice — Democracy & Governance Mod for Cities: Skylines II
// Created by xTrueF | github.com/xTrueF/CivicVoice
// Licensed under MIT License
// ============================================================
using CivicVoice.Models;
using CivicVoice.Systems;
using Colossal.UI.Binding;
using Game.UI;
using Game.Simulation;
using System;
using System.Collections.Generic;

namespace CivicVoice.UI
{
    public partial class CivicVoiceUISystem : UISystemBase
    {
        private CivicVoiceSystem _civicSystem = null!;

        private ValueBinding<bool> _useUniversalModMenuBinding = null!;
        private ValueBinding<int> _populationBinding = null!;
        private ValueBinding<float> _happinessBinding = null!;
        private ValueBinding<float> _unemploymentBinding = null!;
        private ValueBinding<int> _usedSlotsBinding = null!;
        private ValueBinding<int> _availableSlotsBinding = null!;
        private ValueBinding<bool> _hasElectionBinding = null!;
        private ValueBinding<string> _mayorNameBinding = null!;
        private ValueBinding<int> _votingPopulationBinding = null!;
        private ValueBinding<int> _nextElectionDaysBinding = null!;
        private ValueBinding<int> _completedProjectsBinding = null!;
        private ValueBinding<int> _failedProjectsBinding = null!;
        private ValueBinding<string> _mayorSpecialtyBinding = null!;
        private ValueBinding<string> _mayorSloganBinding = null!;
        private ValueBinding<bool> _electionsModActiveBinding = null!;
        private ValueBinding<int> _termCompletedBinding = null!;
        private ValueBinding<int> _termFailedBinding = null!;
        private ValueBinding<int> _termAbandonedBinding = null!;
        private ValueBinding<int> _mayorTermMonthsBinding = null!;
        private ValueBinding<int> _mayorAgeBinding = null!;
        private ValueBinding<int> _mayorTermsServedBinding = null!;
        private ValueBinding<bool> _showNotificationsBinding = null!;
        private ValueBinding<float> _healthBinding = null!;
        private ValueBinding<float> _crimeRateBinding = null!;
        private ValueBinding<int> _abandonedProjectsBinding = null!;

        private RawValueBinding _proposedBinding = null!;
        private RawValueBinding _activeBinding = null!;
        private RawValueBinding _electionBinding = null!;
        private RawValueBinding _notificationsBinding = null!;
        private List<string> _notificationHistory = new List<string>();
        private DateTime _lastNotificationTime = DateTime.MinValue;
        private RawValueBinding _newspaperBinding = null!;
        private bool _newspaperWasShowing = false;

        protected override void OnCreate()
        {
            base.OnCreate();
            _civicSystem = World.GetOrCreateSystemManaged<CivicVoiceSystem>();

            AddBinding(_useUniversalModMenuBinding = new ValueBinding<bool>("civicvoice", "useUniversalModMenu", Mod.Settings?.UseUniversalModMenu ?? false));
            AddBinding(_populationBinding = new ValueBinding<int>("civicvoice", "population", 0));
            AddBinding(_happinessBinding = new ValueBinding<float>("civicvoice", "happiness", 50f));
            AddBinding(_unemploymentBinding = new ValueBinding<float>("civicvoice", "unemployment", 5f));
            AddBinding(_usedSlotsBinding = new ValueBinding<int>("civicvoice", "usedSlots", 0));
            AddBinding(_availableSlotsBinding = new ValueBinding<int>("civicvoice", "availableSlots", 7));
            AddBinding(_hasElectionBinding = new ValueBinding<bool>("civicvoice", "hasElection", false));
            AddBinding(_mayorNameBinding = new ValueBinding<string>("civicvoice", "mayorName", "None"));
            AddBinding(_nextElectionDaysBinding = new ValueBinding<int>("civicvoice", "nextElectionDays", 0));
            AddBinding(_votingPopulationBinding = new ValueBinding<int>("civicvoice", "votingPopulation", 0));
            AddBinding(_completedProjectsBinding = new ValueBinding<int>("civicvoice", "totalCompleted", 0));
            AddBinding(_failedProjectsBinding = new ValueBinding<int>("civicvoice", "totalFailed", 0));
            AddBinding(_mayorSpecialtyBinding = new ValueBinding<string>("civicvoice", "mayorSpecialty", ""));
            AddBinding(_mayorSloganBinding = new ValueBinding<string>("civicvoice", "mayorSlogan", ""));
            AddBinding(_electionsModActiveBinding = new ValueBinding<bool>("civicvoice", "electionsModActive", false));
            AddBinding(_termCompletedBinding = new ValueBinding<int>("civicvoice", "termCompleted", 0));
            AddBinding(_termFailedBinding = new ValueBinding<int>("civicvoice", "termFailed", 0));
            AddBinding(_termAbandonedBinding = new ValueBinding<int>("civicvoice", "termAbandoned", 0));
            AddBinding(_mayorTermMonthsBinding = new ValueBinding<int>("civicvoice", "mayorTermMonths", 0));
            AddBinding(_mayorAgeBinding = new ValueBinding<int>("civicvoice", "mayorAge", 0));
            AddBinding(_mayorTermsServedBinding = new ValueBinding<int>("civicvoice", "mayorTermsServed", 0));
            AddBinding(_showNotificationsBinding = new ValueBinding<bool>("civicvoice", "showNotifications", true));
            AddBinding(_healthBinding = new ValueBinding<float>("civicvoice", "health", 0f));
            AddBinding(_crimeRateBinding = new ValueBinding<float>("civicvoice", "crimeRate", 0f));
            AddBinding(_abandonedProjectsBinding = new ValueBinding<int>("civicvoice", "totalAbandoned", 0));

            AddBinding(_proposedBinding = new RawValueBinding("civicvoice", "proposed", WriteProposed));
            AddBinding(_activeBinding = new RawValueBinding("civicvoice", "active", WriteActive));
            AddBinding(_electionBinding = new RawValueBinding("civicvoice", "election", WriteElection));
            AddBinding(_notificationsBinding = new RawValueBinding("civicvoice", "notifications", WriteNotifications));
            AddBinding(_newspaperBinding = new RawValueBinding("civicvoice", "newspaper", WriteNewspaper));

            AddBinding(new TriggerBinding<string>("civicvoice", "acceptProject", AcceptProject));
            AddBinding(new TriggerBinding<string>("civicvoice", "rejectProject", RejectProject));
            AddBinding(new TriggerBinding<string>("civicvoice", "castVote", CastVote));
            AddBinding(new TriggerBinding<string>("civicvoice", "abandonProject", AbandonProject));
            AddBinding(new TriggerBinding<string>("civicvoice", "markProjectComplete", MarkProjectComplete));
            AddBinding(new TriggerBinding("civicvoice", "forceElection", ForceElection));
            AddBinding(new TriggerBinding("civicvoice", "closeNewspaper", CloseNewspaper));

            _proposedBinding.Update();
            _activeBinding.Update();
            _electionBinding.Update();
            _newspaperBinding.Update();

            Mod.uiLog.Info("CivicVoiceUISystem created.");
        }

        protected override void OnUpdate()
        {
            if (_civicSystem == null) return;

            _useUniversalModMenuBinding.Update(Mod.Settings?.UseUniversalModMenu ?? false);
            _populationBinding.Update(_civicSystem.Population);
            _happinessBinding.Update(_civicSystem.Happiness);
            _unemploymentBinding.Update(_civicSystem.Unemployment);
            _usedSlotsBinding.Update(_civicSystem.Data.ActiveProjects.Count);
            _availableSlotsBinding.Update(_civicSystem.AvailableSlots);
            _hasElectionBinding.Update(_civicSystem.HasActiveElection);
            _mayorNameBinding.Update(_civicSystem.Data.CurrentMayor?.Name ?? "None");
            _votingPopulationBinding.Update((int)(_civicSystem.Population * 0.88f));
            _completedProjectsBinding.Update(_civicSystem.Data.TotalProjectsCompleted);
            _failedProjectsBinding.Update(_civicSystem.Data.TotalProjectsFailed);
            _mayorSpecialtyBinding.Update(_civicSystem.Data.CurrentMayor?.Specialty.ToString() ?? "");
            _mayorSloganBinding.Update(_civicSystem.Data.CurrentMayor?.Slogan ?? "");
            _electionsModActiveBinding.Update(CivicVoiceSystem.ElectionsModActive);
            _termCompletedBinding.Update(_civicSystem.Data.TermProjectsCompleted);
            _termFailedBinding.Update(_civicSystem.Data.TermProjectsFailed);
            _termAbandonedBinding.Update(_civicSystem.Data.TermProjectsAbandoned);
            _showNotificationsBinding.Update(Mod.Settings?.ShowNotifications ?? true);
            _healthBinding.Update(_civicSystem.Health);
            _crimeRateBinding.Update(_civicSystem.CrimeRate);
            _abandonedProjectsBinding.Update(_civicSystem.Data.TotalProjectsAbandoned);


            DateTime mayorElected = _civicSystem.Data.MayorElectedDate;
            DateTime nowForTerm = _civicSystem.GetCurrentGameDate();
            int termMonths = (mayorElected != DateTime.MinValue && nowForTerm != DateTime.MinValue)
                ? (int)(nowForTerm - mayorElected).TotalDays
                : 0;
            _mayorTermMonthsBinding.Update(termMonths);
            _mayorAgeBinding.Update(_civicSystem.Data.CurrentMayor?.Age ?? 0);
            _mayorTermsServedBinding.Update((_civicSystem.Data.CurrentMayor?.TermsServed ?? 0) == 0 ? 1 : _civicSystem.Data.CurrentMayor!.TermsServed);

            if (_civicSystem.Notifications.Count > 0)
            {
                while (_civicSystem.Notifications.Count > 0)
                {
                    _notificationHistory.Insert(0, _civicSystem.Notifications[0]);
                    _civicSystem.Notifications.RemoveAt(0);
                    if (_notificationHistory.Count > 5)
                        _notificationHistory.RemoveAt(5);
                }
                _lastNotificationTime = DateTime.UtcNow;
                _notificationsBinding.Update();
            }

            _proposedBinding.Update();
            _activeBinding.Update();
            _electionBinding.Update();
            _newspaperBinding.Update();

            DateTime gameNow = _civicSystem.GetCurrentGameDate();
            if (_civicSystem.Data.CurrentMayor != null &&
                _civicSystem.Data.NextElectionDate != DateTime.MinValue &&
                gameNow != DateTime.MinValue)
            {
                int daysLeft = (int)(_civicSystem.Data.NextElectionDate - gameNow).TotalDays;
                _nextElectionDaysBinding.Update(Math.Max(0, daysLeft));
            }
            else
            {
                _nextElectionDaysBinding.Update(-1);
            }
            if (Mod.ForceElectionRequested)
            {
                Mod.ForceElectionRequested = false;
                _civicSystem.TriggerForceElection();
            }
            if (Mod.ConcludeElectionRequested)
            {
                Mod.ConcludeElectionRequested = false;
                _civicSystem.ConcludeElection();
            }
        }

        // ── JSON writers ──────────────────────────────────────────────────

        private void WriteProposed(IJsonWriter writer)
        {
            writer.ArrayBegin(_civicSystem.Data.ProposedProjects.Count);
            foreach (var p in _civicSystem.Data.ProposedProjects)
            {
                writer.TypeBegin("CivicProject");
                writer.PropertyName("id"); writer.Write(p.Id);
                writer.PropertyName("title"); writer.Write(p.Title);
                writer.PropertyName("description"); writer.Write(_civicSystem.GetLiveDescription(p));
                writer.PropertyName("category"); writer.Write(p.Category.ToString());
                writer.PropertyName("type"); writer.Write(p.Type.ToString());
                writer.PropertyName("tier"); writer.Write(p.Tier.ToString());
                writer.PropertyName("votesFor"); writer.Write(p.VotesFor);
                writer.PropertyName("votesAgainst"); writer.Write(p.VotesAgainst);
                writer.PropertyName("voteShare"); writer.Write(p.VoteSharePercent);
                writer.PropertyName("deadline"); writer.Write(p.DeadlineGameDays);
                writer.PropertyName("progress"); writer.Write(p.GetProgressText());
                writer.TypeEnd();
            }
            writer.ArrayEnd();
        }

        private void WriteActive(IJsonWriter writer)
        {
            writer.ArrayBegin(_civicSystem.Data.ActiveProjects.Count);
            foreach (var p in _civicSystem.Data.ActiveProjects)
            {
                int daysLeft = p.DeadlineGameDays;
                if (p.StartDate != DateTime.MinValue && _civicSystem._timeSystem != null)
                {
                    DateTime now = _civicSystem.GetCurrentGameDate();
                    if (now != DateTime.MinValue)
                        daysLeft = Math.Max(0, p.DeadlineGameDays - (int)(now - p.StartDate).TotalDays);
                }

                float progressPct;
                if (p.ManualCompletion)
                    progressPct = p.MarkedComplete ? 100f : 50f;
                else if (p.GoalTarget <= 0f)
                    progressPct = 0f;
                else if (p.GoalType == MetricGoalType.HealthAbove || p.GoalType == MetricGoalType.WellbeingAbove || p.GoalType == MetricGoalType.HappinessAbove || p.GoalType == MetricGoalType.BudgetSurplus || p.GoalType == MetricGoalType.PopulationAbove)
                    progressPct = Math.Min(100f, Math.Max(0f, p.CurrentValue / p.GoalTarget * 100f));
                else
                    progressPct = Math.Min(100f, Math.Max(0f, (1f - p.CurrentValue / p.GoalTarget) * 100f));

                writer.TypeBegin("ActiveProject");
                writer.PropertyName("id"); writer.Write(p.Id);
                writer.PropertyName("title"); writer.Write(p.Title);
                writer.PropertyName("category"); writer.Write(p.Category.ToString());
                writer.PropertyName("tier"); writer.Write(p.Tier.ToString());
                writer.PropertyName("progress"); writer.Write(p.GetProgressText());
                writer.PropertyName("progressPct"); writer.Write(progressPct);
                writer.PropertyName("daysLeft"); writer.Write(daysLeft);
                writer.PropertyName("isComplete"); writer.Write(p.IsComplete());
                writer.PropertyName("manualCompletion"); writer.Write(p.ManualCompletion);
                writer.PropertyName("markedComplete"); writer.Write(p.MarkedComplete);
                writer.TypeEnd();
            }
            writer.ArrayEnd();
        }

        private void WriteElection(IJsonWriter writer)
        {
            void WriteEmpty()
            {
                writer.TypeBegin("Election");
                writer.PropertyName("isActive"); writer.Write(false);
                writer.PropertyName("hasVoted"); writer.Write(false);
                writer.PropertyName("winner"); writer.Write("");
                writer.PropertyName("progress"); writer.Write(0f);
                writer.PropertyName("candidates"); writer.ArrayBegin(0); writer.ArrayEnd();
                writer.TypeEnd();
            }

            try
            {
                var data = _civicSystem?.Data;
                var e = data?.CurrentElection;
                if (data == null || e == null) { WriteEmpty(); return; }

                float electionProgress = 0f;
                if (e.IsActive && data.ElectionStartDate != DateTime.MinValue)
                {
                    DateTime gameNow = _civicSystem!.GetCurrentGameDate();
                    if (gameNow != DateTime.MinValue && gameNow >= data.ElectionStartDate)
                        electionProgress = Math.Min(1f, (float)(gameNow - data.ElectionStartDate).TotalDays);
                }

                int totalVotes = 0;
                foreach (var c in e.Candidates) totalVotes += c.Votes;

                writer.TypeBegin("Election");
                writer.PropertyName("isActive"); writer.Write(e.IsActive);
                writer.PropertyName("hasVoted"); writer.Write(e.HasVoted);
                writer.PropertyName("winner"); writer.Write(e.Winner?.Name ?? "");
                writer.PropertyName("progress"); writer.Write(electionProgress);
                writer.PropertyName("candidates");
                writer.ArrayBegin(e.Candidates.Count);
                foreach (var c in e.Candidates)
                {
                    writer.TypeBegin("Candidate");
                    writer.PropertyName("name"); writer.Write(c.Name);
                    writer.PropertyName("age"); writer.Write(c.Age);
                    writer.PropertyName("party"); writer.Write(c.PartyName);
                    writer.PropertyName("specialty"); writer.Write(c.Specialty.ToString());
                    writer.PropertyName("slogan"); writer.Write(c.Slogan);
                    writer.PropertyName("votes"); writer.Write(c.Votes);
                    writer.PropertyName("voteShare"); writer.Write(totalVotes > 0 ? (float)c.Votes / totalVotes * 100f : 0f);
                    writer.TypeEnd();
                }
                writer.ArrayEnd();
                writer.TypeEnd();
            }
            catch { WriteEmpty(); }
        }

        private void WriteNotifications(IJsonWriter writer)
        {
            writer.ArrayBegin(_notificationHistory.Count);
            foreach (var n in _notificationHistory)
                writer.Write(n);
            writer.ArrayEnd();
        }

        private void WriteNewspaper(IJsonWriter writer)
        {
            // Coherent's IJsonWriter does not support TypeBegin nested inside TypeBegin —
            // only TypeBegin inside ArrayBegin is safe. All content fields are flattened here;
            // the TSX side reconstructs nested objects from the flat payload.
            void WriteFlat(bool has, string style, string eventType, string headline,
                           string splashLine1, string splashLine2,
                           string quote, string fillerText, string fillerText2,
                           string teaser1, string teaser2, string teaser3, string teaser4,
                           string contentType, string cityName,
                           string winnerName, int winnerAge, string winnerParty, float winnerVotePercent,
                           System.Collections.Generic.List<CivicVoice.Models.NewspaperCandidate>? challengers,
                           float turnoutPercent, int eligibleVoters,
                           string mayorName, int mayorAge, string party, float approvalPercent,
                           int projectsCompleted, int projectsFailed, int projectsAbandoned,
                           int termNumber, int monthsIntoTerm)
            {
                var c0 = challengers != null && challengers.Count > 0 ? challengers[0] : null;
                var c1 = challengers != null && challengers.Count > 1 ? challengers[1] : null;
                writer.TypeBegin("Newspaper");
                writer.PropertyName("hasNewspaper"); writer.Write(has);
                writer.PropertyName("style"); writer.Write(style);
                writer.PropertyName("eventType"); writer.Write(eventType);
                writer.PropertyName("headline"); writer.Write(headline);
                writer.PropertyName("splashLine1"); writer.Write(splashLine1);
                writer.PropertyName("splashLine2"); writer.Write(splashLine2);
                writer.PropertyName("quote"); writer.Write(quote);
                writer.PropertyName("fillerText"); writer.Write(fillerText);
                writer.PropertyName("fillerText2"); writer.Write(fillerText2);
                writer.PropertyName("teaser1"); writer.Write(teaser1);
                writer.PropertyName("teaser2"); writer.Write(teaser2);
                writer.PropertyName("teaser3"); writer.Write(teaser3);
                writer.PropertyName("teaser4"); writer.Write(teaser4);
                writer.PropertyName("contentType"); writer.Write(contentType);
                writer.PropertyName("cityName"); writer.Write(cityName);
                writer.PropertyName("winnerName"); writer.Write(winnerName);
                writer.PropertyName("winnerAge"); writer.Write(winnerAge);
                writer.PropertyName("winnerParty"); writer.Write(winnerParty);
                writer.PropertyName("winnerVotePercent"); writer.Write(winnerVotePercent);
                writer.PropertyName("challenger0Name"); writer.Write(c0?.Name ?? "");
                writer.PropertyName("challenger0Age"); writer.Write(c0?.Age ?? 0);
                writer.PropertyName("challenger0Party"); writer.Write(c0?.Party ?? "");
                writer.PropertyName("challenger0VotePercent"); writer.Write(c0 != null ? (int)Math.Round(c0.VotePercent) : 0);
                writer.PropertyName("challenger1Name"); writer.Write(c1?.Name ?? "");
                writer.PropertyName("challenger1Age"); writer.Write(c1?.Age ?? 0);
                writer.PropertyName("challenger1Party"); writer.Write(c1?.Party ?? "");
                writer.PropertyName("challenger1VotePercent"); writer.Write(c1 != null ? (int)Math.Round(c1.VotePercent) : 0);
                writer.PropertyName("turnoutPercent"); writer.Write(turnoutPercent);
                writer.PropertyName("eligibleVoters"); writer.Write(eligibleVoters);
                writer.PropertyName("mayorName"); writer.Write(mayorName);
                writer.PropertyName("mayorAge"); writer.Write(mayorAge);
                writer.PropertyName("party"); writer.Write(party);
                writer.PropertyName("approvalPercent"); writer.Write(approvalPercent);
                writer.PropertyName("projectsCompleted"); writer.Write(projectsCompleted);
                writer.PropertyName("projectsFailed"); writer.Write(projectsFailed);
                writer.PropertyName("projectsAbandoned"); writer.Write(projectsAbandoned);
                writer.PropertyName("termNumber"); writer.Write(termNumber);
                writer.PropertyName("monthsIntoTerm"); writer.Write(monthsIntoTerm);
                writer.TypeEnd();
            }

            void WriteEmpty() => WriteFlat(false, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", 0, "", 0f, null, 0f, 0, "", 0, "", 0f, 0, 0, 0, 0, 0);

            try
            {
                var payload = _civicSystem?.Data?.PendingNewspaper;
                if (payload == null) { _newspaperWasShowing = false; WriteEmpty(); return; }

                // Hold selectedSpeed=0 every frame while the newspaper is visible.
                World.GetOrCreateSystemManaged<SimulationSystem>().selectedSpeed = 0f;
                if (!_newspaperWasShowing)
                {
                    Mod.log.Info("[CivicVoice] Newspaper appeared — simulation paused");
                    _newspaperWasShowing = true;
                }

                if (payload.ElectionContent != null)
                {
                    var e = payload.ElectionContent;
                    WriteFlat(true, payload.Style, payload.EventType, payload.Headline,
                              payload.SplashLine1, payload.SplashLine2,
                              payload.Quote ?? "", payload.FillerText ?? "", payload.FillerText2 ?? "",
                              payload.Teasers[0], payload.Teasers[1], payload.Teasers[2], payload.Teasers[3],
                              "election", e.CityName,
                              e.Winner.Name, e.Winner.Age, e.Winner.Party, e.Winner.VotePercent,
                              e.Challengers, e.TurnoutPercent, e.EligibleVoters,
                              "", 0, "", 0f, 0, 0, 0, 0, 0);
                }
                // review disabled — else if (payload.ReviewContent != null) branch removed
                else
                {
                    WriteEmpty();
                }
            }
            catch { WriteEmpty(); }
        }


        // ── Trigger handlers ──────────────────────────────────────────────
        private void AcceptProject(string id) => _civicSystem.AcceptProject(id);
        private void RejectProject(string id) => _civicSystem.RejectProject(id);
        private void CastVote(string name) => _civicSystem.CastVote(name);
        private void AbandonProject(string id) => _civicSystem.AbandonProject(id);
        private void MarkProjectComplete(string id) => _civicSystem.MarkProjectComplete(id);
        private void ForceElection() => _civicSystem.TriggerForceElection();
        private void CloseNewspaper()
        {
            _civicSystem.CloseNewspaper();
            _newspaperBinding.Update();
            World.GetOrCreateSystemManaged<SimulationSystem>().selectedSpeed = 1f;
        }
    }
}
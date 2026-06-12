// ============================================================
// CivicVoice — Democracy & Governance Mod for Cities: Skylines II
// Created by xTrueF | github.com/xTrueF/CivicVoice
// Licensed under MIT License
// ============================================================
using CivicVoice.Models;
using CivicVoice.Systems;
using Colossal.UI.Binding;
using Game.UI;
using System;

namespace CivicVoice.UI
{
    public partial class CivicVoiceUISystem : UISystemBase
    {
        private CivicVoiceSystem _civicSystem;

        private ValueBinding<bool> _useUniversalModMenuBinding;
        private ValueBinding<int> _populationBinding;
        private ValueBinding<float> _happinessBinding;
        private ValueBinding<float> _unemploymentBinding;
        private ValueBinding<int> _usedSlotsBinding;
        private ValueBinding<int> _availableSlotsBinding;
        private ValueBinding<bool> _hasElectionBinding;
        private ValueBinding<string> _mayorNameBinding;
        private ValueBinding<string> _notificationBinding;
        private ValueBinding<int> _votingPopulationBinding;
        private ValueBinding<int> _nextElectionDaysBinding;

        private RawValueBinding _proposedBinding;
        private RawValueBinding _activeBinding;
        private RawValueBinding _electionBinding;

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
            AddBinding(_notificationBinding = new ValueBinding<string>("civicvoice", "notification", ""));
            AddBinding(_nextElectionDaysBinding = new ValueBinding<int>("civicvoice", "nextElectionDays", 0));
            AddBinding(_votingPopulationBinding = new ValueBinding<int>("civicvoice", "votingPopulation", 0));

            AddBinding(_proposedBinding = new RawValueBinding("civicvoice", "proposed", WriteProposed));
            AddBinding(_activeBinding = new RawValueBinding("civicvoice", "active", WriteActive));
            AddBinding(_electionBinding = new RawValueBinding("civicvoice", "election", WriteElection));

            AddBinding(new TriggerBinding<string>("civicvoice", "acceptProject", AcceptProject));
            AddBinding(new TriggerBinding<string>("civicvoice", "rejectProject", RejectProject));
            AddBinding(new TriggerBinding<string>("civicvoice", "castVote", CastVote));
            AddBinding(new TriggerBinding<string>("civicvoice", "abandonProject", AbandonProject));
            AddBinding(new TriggerBinding<string>("civicvoice", "markProjectComplete", MarkProjectComplete));
            AddBinding(new TriggerBinding("civicvoice", "forceElection", ForceElection));

            _proposedBinding.Update();
            _activeBinding.Update();
            _electionBinding.Update();

            Mod.log.Info("CivicVoiceUISystem created.");
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

            if (_civicSystem.Notifications.Count > 0)
            {
                _notificationBinding.Update(_civicSystem.Notifications[0]);
                _civicSystem.Notifications.RemoveAt(0);
            }
            else
            {
                _notificationBinding.Update("");
            }

            _proposedBinding.Update();
            _activeBinding.Update();
            _electionBinding.Update();

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

                bool isBelowGoal = p.Tier == ProjectTier.MetricTriggered || p.Title.Contains("shopping") || p.Title.Contains("suburb");
                float progressPct = p.ManualCompletion ? (p.MarkedComplete ? 100f : 50f)
                    : (p.GoalTarget > 0
                        ? (isBelowGoal
                            ? Math.Min(100f, Math.Max(0f, (1f - (p.CurrentValue / p.GoalTarget - 1f)) * 100f))
                            : Math.Min(100f, p.CurrentValue / p.GoalTarget * 100f))
                        : 0f);

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
                var e = _civicSystem?.Data?.CurrentElection;
                if (e == null) { WriteEmpty(); return; }

                float electionProgress = 0f;
                if (e.IsActive && _civicSystem.Data.ElectionStartDate != DateTime.MinValue)
                {
                    DateTime gameNow = _civicSystem.GetCurrentGameDate();
                    if (gameNow != DateTime.MinValue && gameNow >= _civicSystem.Data.ElectionStartDate)
                        electionProgress = Math.Min(1f, (float)(gameNow - _civicSystem.Data.ElectionStartDate).TotalDays);
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

        // ── Trigger handlers ──────────────────────────────────────────────
        private void AcceptProject(string id) => _civicSystem.AcceptProject(id);
        private void RejectProject(string id) => _civicSystem.RejectProject(id);
        private void CastVote(string name) => _civicSystem.CastVote(name);
        private void AbandonProject(string id) => _civicSystem.AbandonProject(id);
        private void MarkProjectComplete(string id) => _civicSystem.MarkProjectComplete(id);
        private void ForceElection() => _civicSystem.TriggerForceElection();
    }
}
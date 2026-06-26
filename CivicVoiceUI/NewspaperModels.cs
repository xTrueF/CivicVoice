using System.Collections.Generic;

namespace CivicVoice.Models
{
    public enum NewspaperStyle { Herald, Uproar, Pulse }
    public enum NewspaperEventType { Election, Review }

    public class NewspaperCandidate
    {
        public string Name = "";
        public int Age;
        public string Party = "";
        public float VotePercent;
        public bool IsWinner;
    }

    public class NewspaperElectionContent
    {
        public string Type = "election";
        public string CityName = "";
        public NewspaperCandidate Winner = new();
        public List<NewspaperCandidate> Challengers = new();
        public float TurnoutPercent;
        public int EligibleVoters;
    }

    public class NewspaperReviewContent
    {
        public string Type = "review";
        public string CityName = "";
        public string MayorName = "";
        public int MayorAge;
        public string Party = "";
        public float ApprovalPercent;
        public int ProjectsCompleted;
        public int ProjectsFailed;
        public int ProjectsAbandoned;
        public int TermNumber;
        public int MonthsIntoTerm;
    }

    public class NewspaperPayload
    {
        public string Style = "";
        public string EventType = "";
        public string Headline = "";
        public string SplashLine1 = "";
        public string SplashLine2 = "";
        public string? Quote;
        public string? FillerText;
        public string? FillerText2;
        public string[] Teasers = new string[4];
        public NewspaperElectionContent? ElectionContent;
        public NewspaperReviewContent? ReviewContent;
    }
}
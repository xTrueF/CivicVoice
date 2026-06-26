export type NewspaperStyle = 'herald' | 'uproar' | 'pulse';

export type NewspaperEventType = 'election' | 'review';

export interface CandidateResult {
    name: string;
    age: number;
    party: string;
    votePercent: number;
    isWinner: boolean;
}

export interface ElectionContent {
    type: 'election';
    cityName: string;
    winner: CandidateResult;
    challengers: CandidateResult[];
    turnoutPercent: number;
    eligibleVoters: number;
    wardsWon?: number;
    wardsTotal?: number;
}

export interface ReviewContent {
    type: 'review';
    cityName: string;
    mayorName: string;
    mayorAge: number;
    party: string;
    approvalPercent: number;
    projectsCompleted: number;
    projectsFailed: number;
    projectsAbandoned: number;
    termNumber: number;
    monthsIntoTerm: number;
}

export type NewspaperContent = ElectionContent | ReviewContent;

// Flat wire format — C# IJsonWriter cannot nest TypeBegin inside TypeBegin.
// Layouts reconstruct ElectionContent/ReviewContent locally from these flat fields.
export interface NewspaperPayloadActive {
    hasNewspaper: true;
    style: NewspaperStyle;
    eventType: NewspaperEventType;
    headline: string;
    splashLine1?: string;
    splashLine2?: string;
    quote?: string;
    fillerText?: string;
    fillerText2?: string;
    teaser1: string;
    teaser2: string;
    teaser3: string;
    teaser4: string;
    // discriminant
    contentType: string;
    cityName: string;
    // election fields
    winnerName: string;
    winnerAge: number;
    winnerParty: string;
    winnerVotePercent: number;
    challenger0Name: string;
    challenger0Age: number;
    challenger0Party: string;
    challenger0VotePercent: number;
    challenger1Name: string;
    challenger1Age: number;
    challenger1Party: string;
    challenger1VotePercent: number;
    turnoutPercent: number;
    eligibleVoters: number;
    // review fields
    mayorName: string;
    mayorAge: number;
    party: string;
    approvalPercent: number;
    projectsCompleted: number;
    projectsFailed: number;
    projectsAbandoned: number;
    termNumber: number;
    monthsIntoTerm: number;
}

export interface NewspaperPayloadEmpty {
    hasNewspaper: false;
}

export type NewspaperPayload = NewspaperPayloadActive | NewspaperPayloadEmpty;

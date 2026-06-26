// review disabled — ReviewContent branches are dead code; contentType will always be "election"
import { useState } from "react";
import { NewspaperPayloadActive, ElectionContent, ReviewContent } from "./newspaper-data";

const H = {
    paper:  "#EDE6D3",
    ink:    "#1A1815",
    accent: "#8B2E2E",
    muted:  "#5A5240",
    rule:   "#B5A88C",
    soft:   "#D9D0BA",
};

const F = {
    masthead: "'English Towne', serif",
    headline: "'Libre Baskerville', serif",
    label:    "'Oswald', sans-serif",
    body:     "'Merriweather', serif",
};

function pct(v: number) { return Math.round(v ?? 0); }

function roundPcts(vals: number[]): number[] {
    const floored = vals.map(v => Math.floor(v ?? 0));
    let rem = 100 - floored.reduce((a, b) => a + b, 0);
    const order = vals.map((v, i) => ({ i, frac: (v ?? 0) % 1 })).sort((a, b) => b.frac - a.frac);
    for (let j = 0; j < rem && j < order.length; j++) floored[order[j].i]++;
    return floored;
}

function parseTeaser(raw: string) {
    const colonIdx = raw.indexOf(": ");
    const pipeIdx  = raw.indexOf(" | ");
    const tag   = (colonIdx >= 0 ? raw.slice(0, colonIdx) : "NEWS").toUpperCase();
    const rest  = colonIdx >= 0 ? raw.slice(colonIdx + 2) : raw;
    const title = pipeIdx >= 0 ? rest.slice(0, rest.indexOf(" | ")) : rest;
    const sub   = pipeIdx >= 0 ? raw.slice(pipeIdx + 3) : "";
    return { tag, title, sub };
}

export function HeraldLayout({ payload, onClose }: { payload: NewspaperPayloadActive; onClose: () => void }) {
    const [btnHover, setBtnHover] = useState(false);
    const [btnDown, setBtnDown] = useState(false);
    const { headline, splashLine1, splashLine2, fillerText, fillerText2, quote } = payload;

    const content: ElectionContent | ReviewContent = payload.contentType === "election" ? {
        type: "election" as const,
        cityName: payload.cityName,
        winner: { name: payload.winnerName, age: payload.winnerAge, party: payload.winnerParty, votePercent: payload.winnerVotePercent, isWinner: true },
        challengers: [
            ...(payload.challenger0Name ? [{ name: payload.challenger0Name, age: payload.challenger0Age, party: payload.challenger0Party, votePercent: payload.challenger0VotePercent, isWinner: false }] : []),
            ...(payload.challenger1Name ? [{ name: payload.challenger1Name, age: payload.challenger1Age, party: payload.challenger1Party, votePercent: payload.challenger1VotePercent, isWinner: false }] : []),
        ],
        turnoutPercent: payload.turnoutPercent,
        eligibleVoters: payload.eligibleVoters,
    } : {
        type: "review" as const,
        cityName: payload.cityName,
        mayorName: payload.mayorName,
        mayorAge: payload.mayorAge,
        party: payload.party,
        approvalPercent: payload.approvalPercent,
        projectsCompleted: payload.projectsCompleted,
        projectsFailed: payload.projectsFailed,
        projectsAbandoned: payload.projectsAbandoned,
        termNumber: payload.termNumber,
        monthsIntoTerm: payload.monthsIntoTerm,
    };

    const isElection = content.type === "election";
    const editionLabel = "ELECTION EDITION";
    const sectionLabel = "ELECTION RESULT";

    const electionPcts = isElection && content.type === "election"
        ? roundPcts([content.winner.votePercent, ...content.challengers.map(c => c.votePercent)])
        : [];
    const winnerPctR = electionPcts[0] ?? 0;
    const raceDesc = winnerPctR >= 55 ? "a decisive showing" : winnerPctR >= 47 ? "a closely fought race" : "a narrow victory";

    const body1 = isElection && content.type === "election"
        ? `${content.winner.name} will take the mayoral seat after ${raceDesc} at the polls. ${content.challengers.length > 0 ? `${content.challengers[0].name} conceded shortly after counting concluded.` : "The result was clear from the outset."} Turnout reached ${pct(content.turnoutPercent)}% of eligible voters.`
        : content.type === "review"
        ? `${content.mayorName} reaches the ${content.monthsIntoTerm}-month mark of term ${content.termNumber} with an approval rating of ${pct(content.approvalPercent)}%. Council records show ${content.projectsCompleted} project${content.projectsCompleted !== 1 ? "s" : ""} completed since taking office.`
        : "";

    const body2 = isElection && content.type === "election"
        ? `${content.winner.name}, ${content.winner.age}, of the ${content.winner.party}, secured ${winnerPctR}% of votes cast.${content.challengers.length > 1 ? ` ${content.challengers[1].name} finished third with ${electionPcts[2] ?? pct(content.challengers[1].votePercent)}%.` : ""} The new mayor will be sworn in at the next full council session.`
        : "";

    const teasers = [payload.teaser1, payload.teaser2, payload.teaser3, payload.teaser4].map(parseTeaser);

    const bigPct = isElection && content.type === "election" ? winnerPctR : content.type === "review" ? pct(content.approvalPercent) : 0;
    const bigLabel = isElection ? "FINAL TALLY" : "APPROVAL";

    return (
        <div style={{ width: "600rem", background: H.paper, fontFamily: F.body, boxShadow: "0 10px 50px rgba(0,0,0,0.7)", overflow: "hidden" }}>

            {/* ── Info band ── */}
            <div style={{ borderBottom: `1rem solid ${H.ink}`, padding: "6rem 20rem", display: "flex", justifyContent: "space-between", alignItems: "baseline" }}>
                <span style={{ fontFamily: F.label, fontSize: "9rem", letterSpacing: "2px", color: H.muted }}>VOL. IV</span>
                <span style={{ fontFamily: F.label, fontSize: "9rem", letterSpacing: "2px", color: H.muted }}>{editionLabel}</span>
                <span style={{ fontFamily: F.label, fontSize: "9rem", letterSpacing: "2px", color: H.muted }}>ONE COPPER</span>
            </div>

            {/* ── Masthead ── */}
            <div style={{ padding: "10rem 20rem 8rem", textAlign: "center", borderBottom: `3rem solid ${H.ink}` }}>
                <div style={{ fontFamily: F.masthead, fontWeight: 400, fontSize: "72rem", color: H.ink, lineHeight: 1, letterSpacing: "4px" }}>
                    The Herald
                </div>
                <div style={{ fontFamily: F.label, fontSize: "9rem", letterSpacing: "4px", color: H.muted, marginTop: "5rem" }}>
                    CIVIC VOICE OF THE PEOPLE
                </div>
            </div>

            {/* ── Section label ── */}
            <div style={{ padding: "6rem 20rem", borderBottom: `1rem solid ${H.ink}`, textAlign: "center" }}>
                <div style={{ fontFamily: F.label, fontSize: "9rem", letterSpacing: "2.5px", color: H.accent }}>{sectionLabel}</div>
            </div>

            {/* ── Three-column body ── */}
            <div style={{ display: "flex", alignItems: "stretch" }}>

                {/* Left teaser panel */}
                <div style={{ width: "130rem", flexShrink: 0, borderRight: `1rem solid ${H.rule}`, padding: "12rem 12rem", display: "flex", flexDirection: "column" }}>
                    <div style={{ fontFamily: F.label, fontSize: "7rem", letterSpacing: "2px", color: H.muted, marginBottom: "10rem", paddingBottom: "6rem", borderBottom: `1rem solid ${H.rule}`, textAlign: "center" }}>
                        TODAY'S PAPER
                    </div>
                    {teasers.map((t, i) => (
                        <div key={i} style={{ marginBottom: "10rem", paddingBottom: "10rem", borderBottom: i < teasers.length - 1 ? `1rem solid ${H.soft}` : "none" }}>
                            <div style={{ fontFamily: F.label, fontSize: "6rem", letterSpacing: "1.5px", color: H.accent, marginBottom: "4rem" }}>{t.tag}</div>
                            <div style={{ fontFamily: F.headline, fontWeight: 700, fontSize: "9rem", color: H.ink, lineHeight: 1.35 }}>{t.title}</div>
                            {t.sub && <div style={{ fontFamily: F.body, fontStyle: "italic", fontSize: "8rem", color: H.muted, lineHeight: 1.3, marginTop: "3rem" }}>{t.sub}</div>}
                        </div>
                    ))}
                    {/* Decorative footer */}
                    <div style={{ marginTop: "auto", paddingTop: "12rem", textAlign: "center" }}>
                        <div style={{ borderTop: `1rem solid ${H.rule}`, paddingTop: "8rem" }}>
                            <div style={{ fontFamily: F.body, fontStyle: "italic", fontSize: "8rem", color: H.muted, lineHeight: 1.5 }}>Est. since the<br />city's founding</div>
                            <div style={{ marginTop: "6rem", display: "flex", justifyContent: "center", alignItems: "center", gap: "4rem" }}>
                                <div style={{ width: "20rem", height: "1rem", background: H.rule }} />
                                <div style={{ width: "5rem", height: "5rem", border: `1rem solid ${H.rule}`, transform: "rotate(45deg)" }} />
                                <div style={{ width: "20rem", height: "1rem", background: H.rule }} />
                            </div>
                        </div>
                    </div>
                </div>

                {/* Centre: splash + story */}
                <div style={{ flex: 1, minWidth: 0, padding: "12rem 16rem", borderRight: `1rem solid ${H.rule}` }}>

                    {/* Splash */}
                    {splashLine1 && (
                        <div style={{ textAlign: "center", marginBottom: "10rem", paddingBottom: "10rem", borderBottom: `1rem solid ${H.rule}` }}>
                            <div style={{ lineHeight: 1.0, marginBottom: "6rem" }}>
                                <div style={{ fontFamily: F.headline, fontWeight: 700, fontSize: "28rem", color: H.ink, letterSpacing: "-0.5px" }}>{splashLine1}</div>
                                {splashLine2 && <div style={{ fontFamily: F.headline, fontWeight: 700, fontSize: "22rem", color: H.ink, letterSpacing: "-0.5px", marginTop: "2rem" }}>{splashLine2}</div>}
                            </div>
                            <div style={{ fontFamily: F.body, fontStyle: "italic", fontSize: "11rem", color: H.muted, lineHeight: 1.4 }}>{headline}</div>
                        </div>
                    )}

                    {/* Byline */}
                    <div style={{ fontFamily: F.label, fontSize: "8rem", letterSpacing: "1px", color: H.muted, marginBottom: "8rem" }}>
                        BY OUR CIVIC CORRESPONDENT
                    </div>

                    {/* Body paragraphs */}
                    <p style={{ fontFamily: F.body, fontSize: "11rem", lineHeight: 1.7, color: H.ink, margin: "0 0 10rem" }}>
                        {body1}
                    </p>
                    {body2 && (
                        <p style={{ fontFamily: F.body, fontSize: "11rem", lineHeight: 1.7, color: H.ink, margin: "0 0 12rem" }}>
                            {body2}
                        </p>
                    )}

                    {fillerText2 && (
                        <>
                            <div style={{ borderTop: `1rem solid ${H.ink}`, marginBottom: "8rem" }} />
                            <div style={{ fontFamily: F.label, fontSize: "8rem", letterSpacing: "1.5px", color: H.muted, marginBottom: "5rem" }}>IN BRIEF</div>
                            <p style={{ fontFamily: F.body, fontSize: "10rem", color: H.ink, lineHeight: 1.7, margin: "0 0 12rem" }}>{fillerText2}</p>
                        </>
                    )}
                    {fillerText && (
                        <>
                            <div style={{ borderTop: `1rem solid ${H.rule}`, marginBottom: "8rem" }} />
                            <div style={{ fontFamily: F.label, fontSize: "8rem", letterSpacing: "1.5px", color: H.muted, marginBottom: "5rem" }}>ALSO TODAY</div>
                            <p style={{ fontFamily: F.body, fontSize: "10rem", color: H.ink, lineHeight: 1.7, margin: 0 }}>{fillerText}</p>
                        </>
                    )}
                </div>

                {/* Right sidebar */}
                <div style={{ width: "150rem", flexShrink: 0, padding: "12rem 14rem" }}>

                    {/* Big percentage */}
                    <div style={{ fontFamily: F.label, fontSize: "9rem", letterSpacing: "2px", color: H.muted, marginBottom: "8rem", textAlign: "center" }}>{bigLabel}</div>
                    <div style={{ textAlign: "center", marginBottom: "12rem" }}>
                        <div style={{ fontFamily: F.headline, fontWeight: 700, lineHeight: 1, whiteSpace: "nowrap" }}>
                            <span style={{ fontSize: "44rem", color: H.accent }}>{bigPct}</span><span style={{ fontSize: "22rem", color: H.accent }}>%</span>
                        </div>
                        {isElection && content.type === "election" && (
                            <>
                                <div style={{ fontFamily: F.body, fontSize: "11rem", color: H.ink, marginTop: "4rem" }}>{content.winner.name}</div>
                                <div style={{ fontFamily: F.body, fontSize: "9rem", color: H.muted }}>{`Age ${content.winner.age}`}</div>
                                <div style={{ fontFamily: F.body, fontSize: "9rem", color: H.muted }}>{content.winner.party}</div>
                            </>
                        )}
                        {!isElection && content.type === "review" && (
                            <>
                                <div style={{ fontFamily: F.body, fontSize: "11rem", color: H.ink, marginTop: "4rem" }}>{content.mayorName}</div>
                                <div style={{ fontFamily: F.body, fontSize: "9rem", color: H.muted }}>Term {content.termNumber}</div>
                            </>
                        )}
                    </div>

                    {/* Challenger list */}
                    <div style={{ borderTop: `1rem solid ${H.rule}`, paddingTop: "10rem", marginBottom: "12rem" }}>
                        {isElection && content.type === "election" && content.challengers.map((c, i) => (
                            <div key={c.name} style={{ display: "flex", justifyContent: "space-between", fontSize: "10rem", padding: "3rem 0", borderBottom: `1rem solid ${H.soft}`, fontFamily: F.body, color: H.muted }}>
                                <span style={{ overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap", minWidth: 0, marginRight: "4rem" }}>{c.name}</span>
                                <span style={{ flexShrink: 0 }}>{electionPcts[i + 1] ?? pct(c.votePercent)}%</span>
                            </div>
                        ))}
                        {!isElection && content.type === "review" && [
                            { label: "Completed", val: content.projectsCompleted },
                            { label: "Failed",    val: content.projectsFailed },
                            { label: "Abandoned", val: content.projectsAbandoned },
                        ].map(r => (
                            <div key={r.label} style={{ display: "flex", justifyContent: "space-between", fontSize: "10rem", padding: "3rem 0", borderBottom: `1rem solid ${H.soft}`, fontFamily: F.body, color: H.muted }}>
                                <span>{r.label}</span><span>{r.val}</span>
                            </div>
                        ))}
                        {isElection && content.type === "election" && (
                            <div style={{ marginTop: "8rem", fontFamily: F.body, fontSize: "9rem", color: H.muted, fontStyle: "italic" }}>
                                {`Turnout: ${pct(content.turnoutPercent)}%`}
                            </div>
                        )}
                    </div>

                    {/* Pull quote */}
                    {quote && (
                        <div style={{ borderTop: `1rem solid ${H.rule}`, paddingTop: "10rem", marginBottom: "12rem" }}>
                            <div style={{ borderLeft: `3rem solid ${H.accent}`, paddingLeft: "8rem" }}>
                                <p style={{ fontFamily: F.body, fontStyle: "italic", fontSize: "9rem", color: H.ink, lineHeight: 1.6, margin: 0 }}>{quote}</p>
                            </div>
                        </div>
                    )}

                    {/* What happens next */}
                    {isElection && (
                        <div style={{ borderTop: `1rem solid ${H.rule}`, paddingTop: "10rem" }}>
                            <div style={{ fontFamily: F.label, fontSize: "7rem", letterSpacing: "1.5px", color: H.muted, marginBottom: "8rem" }}>WHAT HAPPENS NEXT</div>
                            {[
                                "New mayor sworn in at full council session",
                                "First mayoral address to be scheduled",
                                "Incoming administration to review active projects",
                            ].map((item, i) => (
                                <div key={i} style={{ display: "flex", gap: "6rem", marginBottom: "6rem", alignItems: "flex-start" }}>
                                    <div style={{ width: "4rem", height: "4rem", borderRadius: "50%", background: H.accent, flexShrink: 0, marginTop: "4rem" }} />
                                    <div style={{ fontFamily: F.body, fontSize: "9rem", color: H.ink, lineHeight: 1.5 }}>{item}</div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
            </div>

            {/* ── Close ── */}
            <div style={{ textAlign: "center", padding: "16rem 20rem 18rem", borderTop: `1rem solid ${H.rule}` }}>
                <div
                    onClick={(e) => { e.stopPropagation(); onClose(); }}
                    onMouseEnter={() => setBtnHover(true)}
                    onMouseLeave={() => { setBtnHover(false); setBtnDown(false); }}
                    onMouseDown={() => setBtnDown(true)}
                    onMouseUp={() => setBtnDown(false)}
                    style={{
                        display: "inline-block", fontFamily: F.label, fontSize: "10rem", letterSpacing: "2px",
                        color: H.paper, padding: "10rem 28rem",
                        cursor: "pointer", userSelect: "none", pointerEvents: "all",
                        background: btnDown ? "#5A1E1E" : btnHover ? "#A83838" : H.accent,
                        transform: btnDown ? "translateY(2px)" : "none",
                        boxShadow: btnDown ? "none" : btnHover ? "0 4px 0 #5A1E1E" : "0 3px 0 #5A1E1E",
                        transition: "background 0.08s, box-shadow 0.08s, transform 0.08s",
                    }}
                >
                    CLOSE PAPER &amp; RETURN TO CITY HALL
                </div>
            </div>

        </div>
    );
}

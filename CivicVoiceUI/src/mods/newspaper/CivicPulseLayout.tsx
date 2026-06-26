// review disabled — ReviewContent branches are dead code; contentType will always be "election"
import { useState } from "react";
import { NewspaperPayloadActive, ElectionContent, ReviewContent } from "./newspaper-data";

const F = {
    masthead: "'Playfair Display', serif",
    headline: "'Merriweather', serif",
    body:     "'Source Sans 3', sans-serif",
};

const P = {
    paper:  "#F3EFE5",
    ink:    "#1C1C1A",
    green:  "#1A5940",
    gold:   "#B8860B",
    muted:  "#5A5448",
    rule:   "#C4BAA8",
    soft:   "#E8E2D5",
    white:  "#FFFFFF",
};

const TEASER_PAGES = ["P.3", "P.5", "P.8", "P.11"];

function parseTeaser(raw: string, index: number) {
    const colonIdx = raw.indexOf(": ");
    const pipeIdx  = raw.indexOf(" | ");
    const tag   = (colonIdx >= 0 ? raw.slice(0, colonIdx) : "NEWS").toUpperCase();
    const rest  = colonIdx >= 0 ? raw.slice(colonIdx + 2) : raw;
    const title = pipeIdx >= 0 ? rest.slice(0, rest.indexOf(" | ")) : rest;
    const sub   = pipeIdx >= 0 ? raw.slice(pipeIdx + 3) : "";
    return { tag, title, sub, page: TEASER_PAGES[index] ?? "P.4" };
}

function pct(val: number) { return Math.round(val ?? 0); }

function termOrdinal(n: number): string {
    if (n === 1) return "first";
    if (n === 2) return "second";
    if (n === 3) return "third";
    return `${n}th`;
}

function roundPcts(vals: number[]): number[] {
    const floored = vals.map(v => Math.floor(v ?? 0));
    let rem = 100 - floored.reduce((a, b) => a + b, 0);
    const order = vals.map((v, i) => ({ i, frac: (v ?? 0) % 1 })).sort((a, b) => b.frac - a.frac);
    for (let j = 0; j < rem && j < order.length; j++) floored[order[j].i]++;
    return floored;
}

function ResultsTable({ content }: { content: ElectionContent | ReviewContent }) {
    if (content.type === "election") {
        const allPcts = roundPcts([content.winner.votePercent, ...content.challengers.map(c => c.votePercent)]);
        const rows = [
            { name: content.winner.name, val: `${allPcts[0]}%`, lead: true },
            ...content.challengers.map((c, i) => ({ name: c.name, val: `${allPcts[i + 1]}%`, lead: false })),
        ];
        return (
            <>
                <div style={{ fontFamily: F.headline, fontWeight: 700, fontSize: "9rem", letterSpacing: "2px", color: P.green, marginBottom: "8rem" }}>FULL RESULTS</div>
                {rows.map((r, i) => (
                    <div key={i} style={{ borderTop: `1rem solid ${P.rule}`, paddingTop: "6rem", marginBottom: "8rem" }}>
                        <div style={{ display: "flex", justifyContent: "space-between", marginBottom: "3rem" }}>
                            <span style={{ fontFamily: F.body, fontSize: "11rem", color: r.lead ? P.ink : P.muted, fontWeight: r.lead ? 700 : 400 }}>{r.name}</span>
                            <span style={{ fontFamily: F.headline, fontWeight: 700, fontSize: "13rem", color: r.lead ? P.green : P.muted }}>{r.val}</span>
                        </div>
                        <div style={{ height: "5rem", background: P.rule }}>
                            <div style={{ height: "100%", width: `${Math.min(Math.max(r.lead ? content.winner.votePercent : (content.challengers.find(c => c.name === r.name)?.votePercent ?? 0), 0), 100)}%`, background: r.lead ? P.green : P.muted }} />
                        </div>
                    </div>
                ))}
                <div style={{ borderTop: `1rem solid ${P.rule}`, paddingTop: "8rem", marginTop: "4rem" }}>
                    <div style={{ display: "flex", justifyContent: "space-between", fontSize: "10rem", color: P.muted, marginBottom: "4rem" }}>
                        <span style={{ fontFamily: F.body, fontStyle: "italic" }}>Turnout</span>
                        <span style={{ fontFamily: F.headline, fontWeight: 700 }}>{pct(content.turnoutPercent)}%</span>
                    </div>
                </div>
            </>
        );
    }
    return (
        <>
            <div style={{ fontFamily: F.headline, fontWeight: 700, fontSize: "9rem", letterSpacing: "2px", color: P.green, marginBottom: "8rem" }}>TERM SUMMARY</div>
            {[
                { label: "Approval rating",    val: `${pct(content.approvalPercent)}%` },
                { label: "Projects completed", val: `${content.projectsCompleted}`     },
                { label: "Projects failed",    val: `${content.projectsFailed}`        },
                { label: "Abandoned",          val: `${content.projectsAbandoned}`     },
                { label: "Months in office",   val: `${content.monthsIntoTerm}`        },
            ].map((r, i) => (
                <div key={i} style={{ display: "flex", justifyContent: "space-between", borderTop: `1rem solid ${P.rule}`, padding: "6rem 0" }}>
                    <span style={{ fontFamily: F.body, fontStyle: "italic", fontSize: "11rem", color: P.muted }}>{r.label}</span>
                    <span style={{ fontFamily: F.headline, fontWeight: 700, fontSize: "13rem", color: P.green }}>{r.val}</span>
                </div>
            ))}
        </>
    );
}

export function CivicPulseLayout({ payload, onClose }: { payload: NewspaperPayloadActive; onClose: () => void }) {
    const [btnHover, setBtnHover] = useState(false);
    const [btnDown, setBtnDown] = useState(false);
    const { headline, splashLine1, splashLine2, fillerText, fillerText2 } = payload;

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

    const isElection    = content.type === "election";
    const editionLabel  = "ELECTION EDITION";
    const sectionLabel  = "ELECTION RESULT";
    const winnerOrMayor = isElection && content.type === "election" ? content.winner.name : content.type === "review" ? content.mayorName : "";

    const body = content.type === "election"
        ? `${content.winner.name} has taken the mayoral seat, defeating ${content.challengers.map(c => c.name).join(" and ")} in a ${(content.winner.votePercent ?? 0) >= 55 ? "decisive" : "tight"} contest. The result was called after ${pct(content.turnoutPercent)}% of eligible residents cast their vote.`
        : `${content.mayorName} reaches the ${content.monthsIntoTerm}-month mark of their ${termOrdinal(content.termNumber)} term. Approval stands at ${pct(content.approvalPercent)}%, with ${content.projectsCompleted} civic project${content.projectsCompleted !== 1 ? "s" : ""} completed since taking office.`;

    const teasers = [payload.teaser1, payload.teaser2, payload.teaser3, payload.teaser4].map(parseTeaser);

    return (
        <div style={{ width: "600rem", background: P.paper, fontFamily: F.body, boxShadow: "0 10px 50px rgba(0,0,0,0.7)" }}>

            {/* ── Top rule ── */}
            <div style={{ background: P.green, padding: "5rem 16rem", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                <span style={{ fontFamily: F.body, fontSize: "8rem", letterSpacing: "2px", color: "rgba(255,255,255,0.7)" }}>{editionLabel}</span>
                <span style={{ fontFamily: F.body, fontSize: "8rem", letterSpacing: "2px", color: "rgba(255,255,255,0.7)" }}>YOUR LOCAL RECORD</span>
            </div>

            {/* ── Masthead ── */}
            <div style={{ padding: "14rem 18rem 10rem", borderBottom: `3rem solid ${P.ink}`, textAlign: "center" }}>
                <div style={{ fontFamily: F.masthead, fontWeight: 700, fontStyle: "italic", fontSize: "58rem", color: P.ink, lineHeight: 0.9, letterSpacing: "-1px" }}>
                    CIVIC PULSE
                </div>
                <div style={{ fontFamily: F.body, fontStyle: "italic", fontSize: "11rem", color: P.muted, marginTop: "6rem", letterSpacing: "0.5px" }}>
                    {`${content.cityName}'s community newspaper of record`}
                </div>
                <div style={{ borderTop: `1rem solid ${P.rule}`, marginTop: "8rem", paddingTop: "5rem", display: "flex", justifyContent: "space-between", width: "100%", fontSize: "9rem", fontFamily: F.body, letterSpacing: "1px", color: P.muted }}>
                    <span>ESTABLISHED IN GOOD FAITH</span>
                    <span>PRICE: YOUR TIME</span>
                </div>
            </div>

            {/* ── Section banner ── */}
            <div style={{ borderBottom: `1rem solid ${P.ink}`, padding: "6rem 18rem", display: "flex", alignItems: "center", gap: "12rem" }}>
                <div style={{ fontFamily: F.headline, fontWeight: 700, fontSize: "9rem", letterSpacing: "2px", background: P.green, color: P.white, padding: "2rem 8rem", flexShrink: 0 }}>
                    {sectionLabel}
                </div>
                <div style={{ height: "1rem", flex: 1, background: P.rule }} />
            </div>

            {/* ── Body ── */}
            <div style={{ display: "flex" }}>

                {/* Left teasers */}
                <div style={{ width: "108rem", borderRight: `1rem solid ${P.ink}`, flexShrink: 0, display: "flex", flexDirection: "column" }}>
                    {teasers.map((t, i) => (
                        <div key={i} style={{ padding: "10rem 9rem 8rem", borderBottom: `1rem solid ${P.rule}` }}>
                            <div style={{ fontFamily: F.headline, fontWeight: 700, fontSize: "7rem", letterSpacing: "1.5px", color: P.green, marginBottom: "3rem" }}>{t.tag}</div>
                            <div style={{ fontFamily: F.headline, fontWeight: 700, fontSize: "11rem", color: P.ink, lineHeight: 1.2, marginBottom: "4rem" }}>{t.title}</div>
                            <div style={{ fontFamily: F.body, fontStyle: "italic", fontSize: "9rem", color: P.ink, lineHeight: 1.3, marginBottom: "5rem" }}>{t.sub}</div>
                            <div style={{ fontFamily: F.body, fontSize: "8rem", color: P.white, background: P.ink, display: "inline-block", padding: "1rem 4rem", letterSpacing: "0.5px" }}>{t.page}</div>
                        </div>
                    ))}
                    {fillerText && (
                        <div style={{ padding: "10rem 9rem 8rem", flex: 1 }}>
                            <div style={{ fontFamily: F.headline, fontWeight: 700, fontSize: "7rem", letterSpacing: "1.5px", color: P.green, marginBottom: "3rem" }}>ALSO</div>
                            <div style={{ fontFamily: F.body, fontStyle: "italic", fontSize: "9rem", color: P.muted, lineHeight: 1.4 }}>{fillerText}</div>
                        </div>
                    )}
                </div>

                {/* Centre story */}
                <div style={{ flex: 1, minWidth: 0, padding: "14rem 16rem", borderRight: `1rem solid ${P.ink}` }}>
                    {splashLine1 && (
                        <div style={{ lineHeight: 1.1, marginBottom: "8rem" }}>
                            <div style={{ fontFamily: F.headline, fontWeight: 700, fontSize: "30rem", color: P.ink, letterSpacing: "-0.5px" }}>{splashLine1}</div>
                            {splashLine2 && <div style={{ fontFamily: F.headline, fontWeight: 700, fontSize: "24rem", color: P.ink, letterSpacing: "-0.5px" }}>{splashLine2}</div>}
                        </div>
                    )}
                    <h2 style={{ fontFamily: F.headline, fontWeight: 400, fontStyle: "italic", fontSize: "14rem", color: P.muted, lineHeight: 1.3, margin: "0 0 10rem" }}>
                        {headline}
                    </h2>
                    <div style={{ borderTop: `1rem solid ${P.rule}`, borderBottom: `1rem solid ${P.rule}`, padding: "5rem 0", margin: "0 0 12rem", fontSize: "10rem", color: P.muted, fontFamily: F.body }}>
                        <span style={{ fontStyle: "italic" }}>{"By our civic correspondent"}</span>
                        <span style={{ padding: "0 6rem" }}>{"·"}</span>
                        <span style={{ letterSpacing: "1px" }}>{content.cityName.toUpperCase()}</span>
                    </div>
                    <p style={{ fontFamily: F.body, fontSize: "13rem", lineHeight: 1.65, color: P.ink, margin: "0 0 10rem" }}>
                        {body}
                    </p>
                    {isElection && content.type === "election" && (
                        <p style={{ fontFamily: F.body, fontSize: "13rem", lineHeight: 1.65, color: P.ink, margin: "0 0 12rem" }}>
                            {`${content.winner.name} will be formally sworn in at a ceremony expected within the coming days. City officials have confirmed a smooth transition of power is underway.`}
                        </p>
                    )}
                    <div style={{ borderLeft: `3rem solid ${P.green}`, padding: "8rem 12rem", margin: "0 0 12rem", background: P.soft }}>
                        <div style={{ fontFamily: F.headline, fontStyle: "italic", fontSize: "13rem", color: P.ink, lineHeight: 1.45 }}>
                            {`"${winnerOrMayor} has the city's attention."`}
                        </div>
                    </div>

                    {fillerText2 && (
                        <div style={{ borderTop: `1rem solid ${P.rule}`, paddingTop: "10rem", marginBottom: "12rem" }}>
                            <div style={{ fontFamily: F.headline, fontWeight: 700, fontSize: "9rem", letterSpacing: "1.5px", color: P.muted, marginBottom: "6rem" }}>AROUND TOWN</div>
                            <p style={{ fontFamily: F.body, fontStyle: "italic", fontSize: "11rem", color: P.muted, lineHeight: 1.6, margin: 0 }}>{fillerText2}</p>
                        </div>
                    )}

                    <div style={{ borderTop: `1rem dashed ${P.rule}`, paddingTop: "10rem", marginTop: "auto" }}>
                        <div style={{ fontFamily: F.headline, fontWeight: 700, fontStyle: "italic", fontSize: "13rem", color: P.green, lineHeight: 1.3 }}>
                            — Community reaction, page 4
                        </div>
                    </div>
                </div>

                {/* Right results */}
                <div style={{ width: "140rem", flexShrink: 0, padding: "14rem 12rem", display: "flex", flexDirection: "column" }}>
                    <ResultsTable content={content} />
                    {isElection && (
                        <>
                            <div style={{ borderTop: `1rem solid ${P.rule}`, paddingTop: "10rem", marginTop: "12rem", marginBottom: "10rem" }}>
                                <div style={{ fontFamily: F.headline, fontWeight: 700, fontSize: "7rem", letterSpacing: "1.5px", color: P.green, marginBottom: "6rem" }}>WHAT HAPPENS NEXT</div>
                                <div style={{ background: P.soft, padding: "8rem 10rem" }}>
                                    <div style={{ fontFamily: F.body, fontSize: "9rem", color: P.ink, lineHeight: 1.55, marginBottom: "5rem" }}>&#9632; Swearing-in ceremony within the week</div>
                                    <div style={{ fontFamily: F.body, fontSize: "9rem", color: P.ink, lineHeight: 1.55, marginBottom: "5rem" }}>&#9632; First council session called within 30 days</div>
                                    <div style={{ fontFamily: F.body, fontSize: "9rem", color: P.ink, lineHeight: 1.55 }}>&#9632; Outgoing staff assist transition through month end</div>
                                </div>
                            </div>
                            <div style={{ borderTop: `1rem solid ${P.rule}`, paddingTop: "10rem" }}>
                                <div style={{ fontFamily: F.headline, fontWeight: 700, fontSize: "7rem", letterSpacing: "1.5px", color: P.green, marginBottom: "8rem" }}>VOICES FROM THE CITY</div>
                                <div style={{ marginBottom: "8rem" }}>
                                    <div style={{ fontFamily: F.body, fontStyle: "italic", fontSize: "10rem", color: P.ink, lineHeight: 1.45, marginBottom: "3rem" }}>"I voted for them twice. This time it counted."</div>
                                    <div style={{ fontFamily: F.body, fontSize: "8rem", color: P.muted, letterSpacing: "0.5px" }}>— LOCAL RESIDENT</div>
                                </div>
                                <div style={{ borderTop: `1rem solid ${P.soft}`, paddingTop: "8rem" }}>
                                    <div style={{ fontFamily: F.body, fontStyle: "italic", fontSize: "10rem", color: P.ink, lineHeight: 1.45, marginBottom: "3rem" }}>"Whoever wins, the bins still need emptying."</div>
                                    <div style={{ fontFamily: F.body, fontSize: "8rem", color: P.muted, letterSpacing: "0.5px" }}>— PRAGMATIC VOTER</div>
                                </div>
                            </div>
                        </>
                    )}
                </div>
            </div>

            {/* ── Filler ticker ── */}
            {fillerText && (
                <div style={{ borderTop: `2rem solid ${P.ink}`, background: P.soft, padding: "7rem 16rem", display: "flex", gap: "10rem", alignItems: "baseline", overflow: "hidden" }}>
                    <span style={{ fontFamily: F.headline, fontWeight: 700, fontSize: "9rem", letterSpacing: "2px", color: P.green, flexShrink: 0 }}>ALSO TODAY:</span>
                    <span style={{ fontFamily: F.body, fontStyle: "italic", fontSize: "11rem", color: P.ink, overflow: "hidden", whiteSpace: "nowrap", textOverflow: "ellipsis", minWidth: 0, flex: 1 }}>{fillerText}</span>
                </div>
            )}

            {/* ── Close ── */}
            <div style={{ background: P.green, textAlign: "center", padding: "12rem 16rem 14rem" }}>
                <div
                    onClick={(e) => { e.stopPropagation(); onClose(); }}
                    onMouseEnter={() => setBtnHover(true)}
                    onMouseLeave={() => { setBtnHover(false); setBtnDown(false); }}
                    onMouseDown={() => setBtnDown(true)}
                    onMouseUp={() => setBtnDown(false)}
                    style={{
                        display: "inline-block", fontFamily: F.body, fontWeight: 700, fontSize: "11rem",
                        letterSpacing: "2px",
                        background: btnDown ? P.soft : P.white,
                        color: P.green,
                        border: btnHover ? `2rem solid ${P.green}` : `2rem solid transparent`,
                        padding: "8rem 26rem",
                        cursor: "pointer", userSelect: "none", pointerEvents: "all",
                        transform: btnDown ? "translateY(2px)" : "none",
                        boxShadow: btnDown ? "none" : `0 3px 0 #0a2a18`,
                    }}
                >
                    CLOSE PAPER &amp; RETURN TO CITY HALL
                </div>
            </div>

        </div>
    );
}

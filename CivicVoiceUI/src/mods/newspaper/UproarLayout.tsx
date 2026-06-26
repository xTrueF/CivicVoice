import { useState } from "react";
import { NewspaperPayloadActive, ElectionContent, ReviewContent } from "./newspaper-data";

const U = {
    paper:   "#F0EDE4",
    ink:     "#111111",
    red:     "#C0202A",
    yellow:  "#F5C400",
    white:   "#FFFFFF",
    muted:   "#555555",
    track:   "#CCCCCC",
    soft:    "#E8E4DA",
};

const F = {
    masthead: "'Anton', sans-serif",
    headline: "'Archivo Black', sans-serif",
    label:    "'Oswald', sans-serif",
    copy:     "'Roboto Condensed', sans-serif",
};

const TEASER_PAGES = ["PAGE 3", "PAGE 5", "PAGE 8", "PAGE 11"];

function splashSize(line1: string, line2: string, base: number): string {
    const longest = Math.max(...[line1, line2].join(" ").split(/\s+/).map(w => w.length));
    if (longest <= 6) return `${base}rem`;
    // Cap so the longest word fits within ~290rem (usable centre width).
    // 0.85 accounts for Archivo Black's wide glyph advance widths.
    const cap = Math.floor(290 / (longest * 0.85));
    return `${Math.max(24, Math.min(base, cap))}rem`;
}

function parseTeaser(raw: string, index: number) {
    const colonIdx = raw.indexOf(": ");
    const pipeIdx  = raw.indexOf(" | ");
    const tag   = (colonIdx >= 0 ? raw.slice(0, colonIdx) : "NEWS").toUpperCase();
    const rest  = colonIdx >= 0 ? raw.slice(colonIdx + 2) : raw;
    const title = (pipeIdx >= 0 ? rest.slice(0, rest.indexOf(" | ")) : rest).toUpperCase();
    const sub   = pipeIdx >= 0 ? raw.slice(pipeIdx + 3) : "";
    const page  = TEASER_PAGES[index] ?? "PAGE 4";
    return { tag, title, sub, page };
}

function surname(full: string) {
    return (full.split(" ").pop() ?? full).toUpperCase();
}

function pct(val: number | undefined) {
    return Math.round(val ?? 0);
}

function roundPcts(vals: number[]): number[] {
    const floored = vals.map(v => Math.floor(v ?? 0));
    let rem = 100 - floored.reduce((a, b) => a + b, 0);
    const order = vals.map((v, i) => ({ i, frac: (v ?? 0) % 1 })).sort((a, b) => b.frac - a.frac);
    for (let j = 0; j < rem && j < order.length; j++) floored[order[j].i]++;
    return floored;
}

function VoteBars({ content }: { content: ElectionContent }) {
    const rounded = roundPcts([content.winner.votePercent, ...content.challengers.map(c => c.votePercent)]);
    const rows = [
        { name: content.winner.name.toUpperCase(), pct: content.winner.votePercent ?? 0, display: rounded[0], lead: true },
        ...content.challengers.map((c, i) => ({ name: c.name.toUpperCase(), pct: c.votePercent ?? 0, display: rounded[i + 1], lead: false })),
    ];
    return (
        <>
            {rows.map(row => (
                <div key={row.name} style={{ marginBottom: "8rem" }}>
                    <div style={{ display: "flex", justifyContent: "space-between", alignItems: "baseline", marginBottom: "3rem" }}>
                        <span style={{ fontFamily: F.label, fontWeight: row.lead ? 700 : 400, fontSize: row.lead ? "13rem" : "11rem", color: row.lead ? U.ink : U.muted }}>
                            {row.name}
                        </span>
                        <span style={{ fontFamily: F.label, fontWeight: 700, fontSize: row.lead ? "13rem" : "11rem", color: row.lead ? U.red : U.muted, flexShrink: 0, marginLeft: "8rem" }}>
                            {row.display}%
                        </span>
                    </div>
                    <div style={{ height: row.lead ? "10rem" : "7rem", background: U.track }}>
                        <div style={{ height: "100%", width: `${Math.min(Math.max(row.pct, 0), 100)}%`, background: row.lead ? U.red : "#999" }} />
                    </div>
                </div>
            ))}
        </>
    );
}

function ReviewBars({ content }: { content: ReviewContent }) {
    const total = Math.max(content.projectsCompleted + content.projectsFailed + content.projectsAbandoned, 1);
    const rows = [
        { name: "APPROVAL RATING",    value: `${pct(content.approvalPercent)}%`, pct: content.approvalPercent ?? 0, lead: true },
        { name: "PROJECTS COMPLETED", value: `${content.projectsCompleted}`,      pct: (content.projectsCompleted / total) * 100, lead: false },
        { name: "PROJECTS FAILED",    value: `${content.projectsFailed}`,         pct: (content.projectsFailed / total) * 100,    lead: false },
    ];
    return (
        <>
            {rows.map(row => (
                <div key={row.name} style={{ marginBottom: "8rem" }}>
                    <div style={{ display: "flex", justifyContent: "space-between", alignItems: "baseline", marginBottom: "3rem" }}>
                        <span style={{ fontFamily: F.label, fontWeight: row.lead ? 700 : 400, fontSize: row.lead ? "13rem" : "11rem", color: row.lead ? U.ink : U.muted }}>
                            {row.name}
                        </span>
                        <span style={{ fontFamily: F.label, fontWeight: 700, fontSize: row.lead ? "13rem" : "11rem", color: row.lead ? U.red : U.muted, flexShrink: 0, marginLeft: "8rem" }}>
                            {row.value}
                        </span>
                    </div>
                    <div style={{ height: row.lead ? "10rem" : "7rem", background: U.track }}>
                        <div style={{ height: "100%", width: `${Math.min(Math.max(row.pct, 0), 100)}%`, background: row.lead ? U.red : "#999" }} />
                    </div>
                </div>
            ))}
        </>
    );
}

export function UproarLayout({ payload, onClose }: { payload: NewspaperPayloadActive; onClose: () => void }) {
    const [btnHover, setBtnHover] = useState(false);
    const [btnDown, setBtnDown] = useState(false);
    const { headline, quote, fillerText } = payload;

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

    let splashLine1: string;
    let splashLine2: string;
    let bannerLabel: string;

    if (isElection && content.type === "election") {
        splashLine1 = payload.splashLine1 || `${surname(content.winner.name)} WINS!`;
        splashLine2 = payload.splashLine2 || `${surname(content.winner.name)} STUNNER`;
        bannerLabel = "* ELECTION EARTHQUAKE *";
    } else if (content.type === "review") {
        splashLine1 = surname(content.mayorName);
        splashLine2 = `RATES ${pct(content.approvalPercent)}%`;
        bannerLabel = "* ELECTION RESULT *";
    } else {
        splashLine1 = "RESULT";
        splashLine2 = "IS IN";
        bannerLabel = "* BREAKING *";
    }

    const rightBoxes = isElection && content.type === "election" ? [
        { tag: "LIVE REACTION", title: `INSIDE ${surname(content.winner.name)}'S WINNING NIGHT`, sub: "Every moment, every word", page: "PAGES 12-13", yellow: false },
        { tag: "SHOCK CLAIM", title: `${content.challengers[0] ? surname(content.challengers[0].name) : "RIVAL"} REFUSES TO QUIT`, sub: `'I will fight on,' says ${content.challengers[0] ? surname(content.challengers[0].name) : "rival"}`, page: "SEE PAGE 7", yellow: true },
        { tag: "READERS SPEAK", title: "'WE WANTED CHANGE'", sub: "Your reactions", page: "PAGES 18-19", yellow: false },
    ] : [
        { tag: "ANALYSIS", title: "WHAT THE NUMBERS REALLY MEAN", sub: "Our experts weigh in", page: "PAGES 4-5", yellow: false },
        { tag: "REACTION", title: "CITY DIVIDED ON VERDICT", sub: "Residents respond", page: "SEE PAGE 6", yellow: true },
        { tag: "INSIDE", title: "WHAT HAPPENS NEXT", sub: "The road ahead", page: "PAGE 9", yellow: false },
    ];

    const byNumbers = isElection && content.type === "election" ? [
        { value: `${pct(content.turnoutPercent)}%`,    label: "TURNOUT"    },
        { value: `${1 + content.challengers.length}`, label: "CANDIDATES" },
    ] : content.type === "review" ? [
        { value: `${pct(content.approvalPercent)}%`,  label: "APPROVAL"  },
        { value: `${content.projectsCompleted}`,      label: "COMPLETED" },
        { value: `${content.projectsFailed}`,         label: "FAILED"    },
    ] : [];

    const leftTeasers = [payload.teaser1, payload.teaser2, payload.teaser3, payload.teaser4]
        .map((raw, i) => parseTeaser(raw, i));

    return (
        <div style={{ width: "600rem", background: U.paper, fontFamily: F.copy, boxShadow: "0 12px 60px rgba(0,0,0,0.8)" }}>

            {/* â"€â"€ Top bar â"€â"€ */}
            <div style={{ display: "flex", justifyContent: "space-between", padding: "5rem 16rem", fontSize: "9rem", fontFamily: F.label, letterSpacing: "1.5px", color: U.muted, borderBottom: `1rem solid ${U.track}` }}>
                <span>NOT A REAL NEWSPAPER, PROBABLY</span>
                <span>2{'Â¢'} - WORTH EVERY PENNY</span>
            </div>

            {/* â"€â"€ Masthead â"€â"€ */}
            <div style={{ background: U.ink, padding: "14rem 16rem 10rem", display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
                <div>
                    <div style={{ fontFamily: F.masthead, textTransform: "uppercase", lineHeight: 0.88, letterSpacing: "-1px" }}>
                        <div style={{ fontSize: "42rem", color: U.white }}>THE DAILY</div>
                        <div style={{ fontSize: "72rem", color: U.red }}>UPROAR</div>
                    </div>
                    <div style={{ fontFamily: F.label, fontSize: "9rem", letterSpacing: "4px", color: "#888", marginTop: "8rem" }}>
                        SHOUTING SO YOU DON'T HAVE TO
                    </div>
                </div>
                {/* Special edition box */}
                <div style={{ border: `3rem solid ${U.white}`, padding: "8rem 12rem", textAlign: "center", minWidth: "140rem" }}>
                    <div style={{ fontFamily: F.label, fontWeight: 700, fontSize: "10rem", letterSpacing: "2px", color: U.white, marginBottom: "4rem" }}>
                        ELECTION NIGHT
                    </div>
                    <div style={{ fontFamily: F.masthead, fontSize: "28rem", color: U.yellow, lineHeight: 1, letterSpacing: "-0.5px" }}>
                        SPECIAL
                    </div>
                    <div style={{ fontFamily: F.label, fontSize: "9rem", letterSpacing: "1px", color: "#aaa", marginTop: "4rem" }}>
                        SOMEWHAT VERIFIED
                    </div>
                </div>
            </div>

            {/* â"€â"€ Body: three columns â"€â"€ */}
            <div style={{ display: "flex", alignItems: "stretch" }}>

                {/* Left sidebar */}
                <div style={{ width: "120rem", borderRight: `2rem solid ${U.ink}`, flexShrink: 0 }}>
                    {leftTeasers.map((t, i) => (
                        <div key={i} style={{ padding: "10rem 10rem 8rem", borderBottom: `1rem solid ${U.track}` }}>
                            <div style={{ fontFamily: F.label, fontWeight: 700, fontSize: "8rem", letterSpacing: "1.5px", color: U.red, marginBottom: "3rem" }}>
                                {t.tag}
                            </div>
                            <div style={{ fontFamily: F.headline, fontSize: "13rem", color: U.ink, lineHeight: 1.1, marginBottom: "4rem", whiteSpace: "pre-line" }}>
                                {t.title}
                            </div>
                            <div style={{ fontFamily: F.copy, fontStyle: "italic", fontSize: "10rem", color: U.ink, lineHeight: 1.3, marginBottom: "5rem", whiteSpace: "pre-line" }}>
                                {t.sub}
                            </div>
                            <div style={{ fontFamily: F.label, fontWeight: 700, fontSize: "9rem", letterSpacing: "1px", color: U.white, background: U.ink, display: "inline-block", padding: "1rem 5rem" }}>
                                {t.page}
                            </div>
                        </div>
                    ))}
                </div>

                {/* Centre content */}
                <div style={{ flex: 1, minWidth: 0 }}>

                    {/* Red banner */}
                    <div style={{ background: U.red, padding: "5rem 14rem", display: "flex", justifyContent: "center" }}>
                        <span style={{ fontFamily: F.label, fontWeight: 700, fontSize: "12rem", letterSpacing: "2px", color: U.white }}>
                            {bannerLabel}
                        </span>
                    </div>

                    {/* Big splash headline */}
                    <div style={{ padding: "10rem 14rem 4rem", borderBottom: `2rem solid ${U.ink}`, overflow: "hidden" }}>
                        <div style={{ fontFamily: F.headline, textTransform: "uppercase", color: U.ink, lineHeight: 0.9, letterSpacing: "-2px" }}>
                            <div style={{ fontSize: splashSize(splashLine1, splashLine2, 60) }}>{splashLine1}</div>
                            <div style={{ fontSize: splashSize(splashLine1, splashLine2, 54) }}>{splashLine2}</div>
                        </div>
                        <div style={{ display: "flex", alignItems: "flex-start", gap: "5rem", marginTop: "8rem" }}>
                            <span style={{ fontFamily: F.label, fontWeight: 700, fontSize: "14rem", color: U.red, flexShrink: 0 }}>&gt;&gt;</span>
                            <span style={{ fontFamily: F.label, fontWeight: 400, fontSize: "13rem", color: U.ink, lineHeight: 1.3 }}>
                                {headline}
                            </span>
                        </div>
                    </div>

                    {/* Quote block */}
                    {quote && (
                        <div style={{ background: U.red, padding: "10rem 14rem", display: "flex", gap: "10rem", alignItems: "flex-start" }}>
                            <span style={{ fontFamily: F.copy, fontSize: "36rem", color: U.white, lineHeight: 0.7, flexShrink: 0, marginTop: "2rem" }}>"</span>
                            <div style={{ fontFamily: F.copy, fontStyle: "italic", fontSize: "13rem", color: U.white, lineHeight: 1.4, flex: 1 }}>
                                {quote}
                            </div>
                            <span style={{ fontFamily: F.copy, fontSize: "36rem", color: U.white, lineHeight: 0.7, flexShrink: 0, alignSelf: "flex-end" }}>"</span>
                        </div>
                    )}

                    {/* Live results */}
                    <div style={{ padding: "10rem 14rem" }}>
                        <div style={{ display: "flex", justifyContent: "space-between", marginBottom: "8rem", borderBottom: `1rem solid ${U.ink}`, paddingBottom: "4rem" }}>
                            <span style={{ fontFamily: F.label, fontWeight: 700, fontSize: "10rem", letterSpacing: "1.5px", background: U.red, color: U.white, padding: "2rem 6rem" }}>
                                LIVE RESULTS
                            </span>
                            <span style={{ fontFamily: F.label, fontWeight: 700, fontSize: "10rem", letterSpacing: "1.5px", color: U.muted }}>
                                VOTES %
                            </span>
                        </div>
                        {content.type === "election"
                            ? <VoteBars content={content} />
                            : <ReviewBars content={content} />}
                    </div>
                </div>

                {/* Right sidebar */}
                <div style={{ width: "148rem", borderLeft: `2rem solid ${U.ink}`, flexShrink: 0 }}>
                    {rightBoxes.map((box, i) => (
                        <div key={i} style={{ padding: "10rem 10rem 8rem", borderBottom: `1rem solid ${U.track}`, background: box.yellow ? U.yellow : "transparent" }}>
                            <div style={{ fontFamily: F.label, fontWeight: 700, fontSize: "8rem", letterSpacing: "1.5px", color: box.yellow ? U.ink : U.red, marginBottom: "3rem" }}>
                                {box.tag}
                            </div>
                            <div style={{ fontFamily: F.headline, fontSize: "12rem", color: U.ink, lineHeight: 1.1, marginBottom: "4rem" }}>
                                {box.title}
                            </div>
                            <div style={{ fontFamily: F.copy, fontStyle: "italic", fontSize: "10rem", color: U.ink, lineHeight: 1.3, marginBottom: "4rem" }}>
                                {box.sub}
                            </div>
                            <div style={{ fontFamily: F.label, fontWeight: 700, fontSize: "8rem", color: box.yellow ? U.ink : U.red }}>
                                {box.page}
                            </div>
                        </div>
                    ))}

                    {/* By the numbers */}
                    <div style={{ padding: "10rem 10rem 8rem" }}>
                        <div style={{ fontFamily: F.label, fontWeight: 700, fontSize: "9rem", letterSpacing: "1.5px", color: U.white, background: U.ink, padding: "3rem 6rem", marginBottom: "8rem" }}>
                            {isElection ? "ELECTION" : "TERM"} BY NUMBERS
                        </div>
                        {byNumbers.map((item, i) => (
                            <div key={i} style={{ textAlign: "center", marginBottom: "8rem", paddingBottom: "8rem", borderBottom: i < byNumbers.length - 1 ? `1rem solid ${U.track}` : "none" }}>
                                <div style={{ fontFamily: F.headline, fontSize: "22rem", color: U.red, lineHeight: 1 }}>
                                    {item.value}
                                </div>
                                <div style={{ fontFamily: F.label, fontWeight: 400, fontSize: "8rem", letterSpacing: "1px", color: U.muted, marginTop: "2rem" }}>
                                    {item.label}
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            </div>

            {/* â"€â"€ Bottom ticker â"€â"€ */}
            {fillerText && (
                <div style={{ background: U.ink, padding: "8rem 16rem", display: "flex", alignItems: "baseline", gap: "10rem", borderTop: `2rem solid ${U.red}` }}>
                    <span style={{ fontFamily: F.label, fontWeight: 700, fontSize: "11rem", color: U.red, flexShrink: 0 }}>UPROAR:</span>
                    <span style={{ fontFamily: F.label, fontWeight: 400, fontSize: "10rem", color: "#ccc", letterSpacing: "0.3px", overflow: "hidden", whiteSpace: "nowrap", textOverflow: "ellipsis", minWidth: 0, flex: 1 }}>{fillerText}</span>
                </div>
            )}

            {/* â"€â"€ Bottom promo strip â"€â"€ */}
            <div style={{ display: "flex", borderTop: `3rem solid ${U.ink}` }}>
                {[
                    { bg: U.red,    color: U.white,  big: "FREE",      small: "INSIDE TODAY"                  },
                    { bg: U.ink,    color: U.yellow,  big: "8-PAGE",    small: "ELECTION GUIDE"                },
                    { bg: U.yellow, color: U.ink,    big: "WIN 5,000", small: "TOKEN ON PAGE 24"              },
                    { bg: U.soft,   color: U.ink,    big: "PLUS",      small: "YOUR FULL WEEKEND TV & PUZZLES" },
                ].map((item, i) => (
                    <div key={i} style={{ flex: 1, background: item.bg, padding: "8rem 10rem", textAlign: "center", borderLeft: i > 0 ? `2rem solid ${U.ink}` : "none" }}>
                        <div style={{ fontFamily: F.headline, fontSize: "16rem", color: item.color, lineHeight: 1, letterSpacing: "-0.5px" }}>
                            {item.big}
                        </div>
                        <div style={{ fontFamily: F.label, fontWeight: 400, fontSize: "8rem", letterSpacing: "1px", color: item.color, opacity: 0.8, marginTop: "3rem", lineHeight: 1.2 }}>
                            {item.small}
                        </div>
                    </div>
                ))}
            </div>

            {/* â"€â"€ Close â"€â"€ */}
            <div style={{ textAlign: "center", padding: "12rem 16rem 14rem", background: U.ink }}>
                <div
                    onClick={(e) => { e.stopPropagation(); onClose(); }}
                    onMouseEnter={() => setBtnHover(true)}
                    onMouseLeave={() => { setBtnHover(false); setBtnDown(false); }}
                    onMouseDown={() => setBtnDown(true)}
                    onMouseUp={() => setBtnDown(false)}
                    style={{
                        display: "inline-block", fontFamily: F.label, fontWeight: 700, fontSize: "11rem",
                        letterSpacing: "2px", color: U.white, padding: "10rem 28rem",
                        cursor: "pointer", userSelect: "none", pointerEvents: "all",
                        background: btnDown ? "#8A1520" : btnHover ? "#E02535" : U.red,
                        transform: btnDown ? "translateY(2px)" : "none",
                        boxShadow: btnDown ? "none" : btnHover ? "0 4px 0 #8A1520" : "0 3px 0 #8A1520",
                        transition: "background 0.08s, box-shadow 0.08s, transform 0.08s",
                    }}
                >
                    CLOSE PAPER &amp; RETURN TO CITY HALL
                </div>
            </div>

        </div>
    );
}


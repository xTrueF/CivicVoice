import { bindValue, useValue, trigger } from "cs2/api";
import { Scrollable } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { useState, useEffect, useRef } from "react";

const population$ = bindValue<number>("civicvoice", "population", 0);
const happiness$ = bindValue<number>("civicvoice", "happiness", 50);
const unemployment$ = bindValue<number>("civicvoice", "unemployment", 5);
const usedSlots$ = bindValue<number>("civicvoice", "usedSlots", 0);
const notifications$ = bindValue<string[]>("civicvoice", "notifications", [] as string[]);
const hasElection$ = bindValue<boolean>("civicvoice", "hasElection", false);
const mayorName$ = bindValue<string>("civicvoice", "mayorName", "None");
const proposed$ = bindValue<any[]>("civicvoice", "proposed", []);
const active$ = bindValue<any[]>("civicvoice", "active", []);
const election$ = bindValue<any>("civicvoice", "election", null);
const votingPopulation$ = bindValue<number>("civicvoice", "votingPopulation", 0);
const nextElectionDays$ = bindValue<number>("civicvoice", "nextElectionDays", 0);
const totalCompleted$ = bindValue<number>("civicvoice", "totalCompleted", 0);
const totalFailed$ = bindValue<number>("civicvoice", "totalFailed", 0);
const mayorSpecialty$ = bindValue<string>("civicvoice", "mayorSpecialty", "");
const mayorSlogan$ = bindValue<string>("civicvoice", "mayorSlogan", "");
const electionsModActive$ = bindValue<boolean>("civicvoice", "electionsModActive", false);
const electionsPanel$ = bindValue<any>("Elections", "panel", null);
const termCompleted$ = bindValue<number>("civicvoice", "termCompleted", 0);
const termFailed$ = bindValue<number>("civicvoice", "termFailed", 0);
const mayorTermMonths$ = bindValue<number>("civicvoice", "mayorTermMonths", 0);
const mayorAge$ = bindValue<number>("civicvoice", "mayorAge", 0);
const mayorTermsServed$ = bindValue<number>("civicvoice", "mayorTermsServed", 0);
const showNotifications$ = bindValue<boolean>("civicvoice", "showNotifications", true);
const health$ = bindValue<number>("civicvoice", "health", 0);
const crimeRate$ = bindValue<number>("civicvoice", "crimeRate", 0);
const totalAbandoned$ = bindValue<number>("civicvoice", "totalAbandoned", 0);

const C = {
    titleBar: "rgba(8,11,18,0.95)",
    metaBar: "rgba(14,18,28,0.85)",
    body: "rgba(255,255,255,0.03)",
    card: "rgba(30,37,56,0.70)",
    footer: "rgba(8,11,18,0.90)",
    border: "rgba(255,255,255,0.08)",
    border2: "rgba(255,255,255,0.12)",
    text: "#e8eaf0",
    muted: "#7a8299",
    muted2: "#8e95a0",
    cyan: "#4bc3d4",
    green: "#6dc889",
    red: "#e06060",
    amber: "#e0a84a",
    purple: "#a78bfa",
};

function useT() {
    const { translate } = useLocalization();
    return (key: string, fallback: string): string => {
        const result = translate(key);
        return (result === null || result === key) ? fallback : result;
    };
}

function tierColor(tier: string): string {
    if (tier === "MetricTriggered") return C.red;
    if (tier === "Major") return C.purple;
    return C.cyan;
}

function isMayorPriority(category: string, mayorSpecialty: string): boolean {
    const map: Record<string, string[]> = {
        Healthcare: ["Healthcare"],
        Economy: ["Economy"],
        Environment: ["Environment", "Leisure"],
        Infrastructure: ["Infrastructure", "Transport"],
        Education: ["Education"],
        PublicSafety: ["PublicSafety"],
    };
    return (map[mayorSpecialty] ?? []).includes(category);
}

function tierLabel(tier: string, t: (k: string, f: string) => string): string {
    if (tier === "MetricTriggered") return t("CivicVoice.Tier.Urgent", "URGENT");
    if (tier === "Major") return t("CivicVoice.Tier.Major", "MAJOR");
    return t("CivicVoice.Tier.Proposal", "PROPOSAL");
}

function categoryColor(cat: string): string {
    const map: Record<string, string> = {
        Healthcare: "#e47ca6",
        Education: "#7db5e0",
        Transport: "#5cc8d4",
        Environment: "#6dc889",
        Economy: "#e0a84a",
        PublicSafety: "#a78bfa",
        Leisure: "#1abc9c",
        Housing: "#8e95a0",
        Infrastructure: "#f97316",
    };
    return map[cat] || C.cyan;
}

function splitCamel(s: string): string {
    return s.replace(/([A-Z])/g, " $1").trim();
}

function Pill({ label, color }: { label: string; color: string }) {
    return (
        <div style={{
            display: "inline-block", padding: "2rem 7rem", borderRadius: "13rem",
            fontSize: "13rem", fontWeight: 600, marginRight: "4rem",
            textTransform: "uppercase", background: color + "28", color
        }}>{label}</div>
    );
}

function ProgressBar({ pct, color }: { pct: number; color: string }) {
    return (
        <div style={{ height: "4rem", borderRadius: "2rem", background: "rgba(255,255,255,0.07)", marginTop: "4rem", marginBottom: "4rem" }}>
            <div style={{ height: "100%", width: `${Math.min(pct || 0, 100)}%`, background: color, borderRadius: "2rem" }} />
        </div>
    );
}

function Tabs({ active, onSelect, hasElection, proposed, activeProjects, electionsModActive }: { active: string; onSelect: (t: string) => void; hasElection: boolean; proposed: any[]; activeProjects: any[]; electionsModActive: boolean }) {
    const [hoveredTab, setHoveredTab] = useState<string | null>(null);
    const t = useT();
    const tabs = electionsModActive ? ["proposals", "active", "stats"] : ["proposals", "active", "election", "stats"];
    return (
        <div style={{ display: "flex", flexDirection: "row", background: C.metaBar, borderBottom: `1rem solid ${C.border}` }}>
            {tabs.map(tab => (
                <div key={tab}
                    onClick={() => onSelect(tab)}
                    onMouseEnter={() => setHoveredTab(tab)}
                    onMouseLeave={() => setHoveredTab(null)}
                    style={{
                        flex: 1, padding: "9rem 4rem", textAlign: "center", cursor: "pointer",
                        fontSize: "12rem",
                        color: active === tab ? C.cyan : hoveredTab === tab ? "#e8eaf0" : C.muted,
                        borderBottom: active === tab ? `2rem solid ${C.cyan}` : "2rem solid transparent",
                        fontWeight: active === tab ? 600 : 400,
                        letterSpacing: "0.3px",
                        background: hoveredTab === tab && active !== tab ? "rgba(255,255,255,0.05)" : "transparent",
                        display: "flex", justifyContent: "center", alignItems: "center", gap: "3rem",
                    }}>
                    {tab === "proposals" && <>
                        {t("CivicVoice.Tab.Proposals", "PROPOSALS")}{proposed?.length > 0 ? <span style={{ color: C.red, fontWeight: 700, marginLeft: "4rem" }}>({proposed.length})</span> : null}
                    </>}
                    {tab === "active" && <>
                        {t("CivicVoice.Tab.Active", "ACTIVE")}{activeProjects?.length > 0 ? <span style={{ color: C.cyan, fontWeight: 700, marginLeft: "4rem" }}>({activeProjects.length})</span> : null}
                    </>}
                    {tab === "election" && <>
                        {t("CivicVoice.Tab.Election", "ELECTION")}{hasElection ? <span style={{ color: C.red, marginLeft: "3rem" }}>●</span> : null}
                    </>}
                    {tab === "stats" && t("CivicVoice.Tab.CityStats", "CITY STATS")}
                </div>
            ))}
        </div>
    );
}

function CvButton({ color, onClick, children }: { color: "cyan" | "red"; onClick: () => void; children: any }) {
    const [hovered, setHovered] = useState(false);
    const c = color === "cyan" ? "75,195,212" : "224,96,96";
    const tc = color === "cyan" ? C.cyan : C.red;
    return (
        <div
            onClick={onClick}
            onMouseEnter={() => setHovered(true)}
            onMouseLeave={() => setHovered(false)}
            style={{
                padding: "4rem 16rem",
                borderRadius: "4rem",
                border: `1rem solid rgba(${c},${hovered ? "0.8" : "0.5"})`,
                background: hovered ? `rgba(${c},0.75)` : `rgba(${c},0.28)`,
                color: hovered ? "#fff" : tc,
                fontSize: "14rem",
                fontWeight: 600,
                cursor: "pointer",
                marginLeft: "8rem",
            }}>
            {children}
        </div>
    );
}

type CardAnimState = "idle" | "banner" | "sliding" | "gone";
type CardAnimType = "accept" | "reject" | "abandon" | "complete" | null;

function useCardAnimation() {
    const [state, setState] = useState<CardAnimState>("idle");
    const [bannerType, setBannerType] = useState<CardAnimType>(null);
    const [visible, setVisible] = useState(false);

    const trigger_anim = (type: "accept" | "reject" | "abandon" | "complete", callback: () => void) => {
        setBannerType(type);
        setState("banner");
        setVisible(false);
        setTimeout(() => setVisible(true), 10);
        setTimeout(() => {
            setVisible(false);
            setState("sliding");
        }, 1000);
        setTimeout(() => {
            setState("gone");
            callback();
        }, 1450);
    };

    return { state, bannerType, visible, trigger_anim };
}

function ProposalCard({ p, onAccept, onReject, mayorSpecialty }: { p: any; onAccept: () => void; onReject: () => void; mayorSpecialty: string }) {
    const { state, bannerType, visible, trigger_anim } = useCardAnimation();
    const t = useT();
    const tc = tierColor(p.tier);
    const cc = categoryColor(p.category);
    const forText = `${p.votesFor?.toLocaleString()} ${t("CivicVoice.Proposals.Vote.For", "for")} (${p.voteShare?.toFixed(0)}%)`;
    const againstPct = (100 - (p.voteShare || 0)).toFixed(0);
    const againstText = `${p.votesAgainst?.toLocaleString()} ${t("CivicVoice.Proposals.Vote.Against", "against")} (${againstPct}%)`;

    if (state === "gone") return null;

    const isAccept = bannerType === "accept";
    const bannerColor = isAccept ? C.green : C.red;
    const bannerBg = isAccept ? "rgba(109,200,137,0.12)" : "rgba(224,96,96,0.08)";
    const bannerText = isAccept ? t("CivicVoice.Banner.Accepted", "Accepted — added to active projects") : t("CivicVoice.Banner.Rejected", "Rejected");
    const bannerIcon = isAccept ? "+" : "x";
    const accentColor = state === "idle" ? tc : isAccept ? C.green : C.muted;

    return (
        <div style={{
            margin: "0 10rem 6rem",
            overflow: "hidden",
            maxHeight: state === "sliding" ? "0" : "500rem",
            transition: state === "sliding" ? "max-height 0.4s ease 0.05s" : "none",
        }}>
            <div style={{
                background: C.card,
                borderRadius: "6rem",
                borderLeft: `3rem solid ${accentColor}`,
                overflow: "hidden",
                transform: state === "sliding" ? "translateX(110%)" : "translateX(0)",
                opacity: state === "sliding" ? 0 : 1,
                transition: "transform 0.4s ease, opacity 0.35s ease",
            }}>
                {state === "banner" && (
                    <div style={{
                        padding: "10rem 12rem",
                        fontSize: "13rem",
                        fontWeight: 600,
                        color: bannerColor,
                        background: bannerBg,
                        transform: visible ? "translateX(0)" : "translateX(-20rem)",
                        opacity: visible ? 1 : 0,
                        transition: "transform 0.25s ease, opacity 0.25s ease",
                    }}>
                        <span>{bannerIcon} {bannerText}</span>
                    </div>
                )}
                {state === "idle" && (
                    <>
                        <div style={{ padding: "10rem 12rem 10rem 14rem" }}>
                            <div style={{ display: "flex", flexDirection: "row", justifyContent: "space-between", alignItems: "center", marginBottom: "6rem" }}>
                                <div style={{ display: "flex", flexDirection: "row", alignItems: "center", justifyContent: "space-between", width: "100%" }}>
                                    <div style={{ display: "flex", flexDirection: "row" }}>
                                        <Pill label={tierLabel(p.tier, t)} color={tc} />
                                        <Pill label={splitCamel(p.category)} color={cc} />
                                    </div>
                                    {isMayorPriority(p.category, mayorSpecialty) && (
                                        <div style={{ display: "flex", alignItems: "center", gap: "4rem", background: "rgba(224,168,74,0.15)", border: "1rem solid rgba(224,168,74,0.3)", borderRadius: "10rem", padding: "2rem 8rem" }}>
                                            <span style={{ fontSize: "11rem", color: C.amber }}>★</span>
                                            <span style={{ fontSize: "10rem", color: C.amber, fontWeight: 600, textTransform: "uppercase", letterSpacing: "0.5px" }}>Mayor priority</span>
                                        </div>
                                    )}
                                </div>
                            </div>
                            <div style={{ fontSize: "18rem", fontWeight: 600, color: C.text, marginBottom: "4rem", lineHeight: 1.3 }}>{p.title}</div>
                            <div style={{ fontSize: "14rem", color: C.muted2, lineHeight: 1.5, marginBottom: "4rem" }}>{p.description}</div>
                            <div style={{ display: "flex", flexDirection: "row", justifyContent: "space-between", fontSize: "13rem", marginBottom: "3rem" }}>
                                <div style={{ color: C.green }}>{forText}</div>
                                <div style={{ color: C.red }}>{againstText}</div>
                            </div>
                            <div style={{ height: "4rem", borderRadius: "2rem", background: C.red, marginBottom: "4rem" }}>
                                <div style={{ height: "4rem", width: `${Math.min(Math.max(p.voteShare || 0, 0), 100)}%`, borderRadius: "2rem", background: C.green }} />
                            </div>
                        </div>
                        <div style={{ display: "flex", flexDirection: "row", justifyContent: "flex-end", gap: "24rem", padding: "7rem 12rem", background: "rgba(0,0,0,0.12)", borderTop: "1rem solid rgba(255,255,255,0.04)" }}>
                            <CvButton color="red" onClick={() => trigger_anim("reject", onReject)}>{t("CivicVoice.Button.Reject", "Reject")}</CvButton>
                            <CvButton color="cyan" onClick={() => trigger_anim("accept", onAccept)}>{t("CivicVoice.Button.Accept", "Accept")}</CvButton>
                        </div>
                    </>
                )}
            </div>
        </div>
    );
}

function ActiveCard({ p, mayorSpecialty }: { p: any; mayorSpecialty: string }) {
    const { state, bannerType, visible, trigger_anim } = useCardAnimation();
    const t = useT();
    const urgent = p.daysLeft < 2 && !p.manualCompletion;
    const tc = tierColor(p.tier);
    const cc = categoryColor(p.category);
    const accent = p.isComplete ? C.green : urgent ? C.red : tc;
    const timeText = p.isComplete ? t("CivicVoice.Active.TimeComplete", "Complete!") : `${p.daysLeft}mo left`;

    if (state === "gone") return null;

    const isComplete = bannerType === "complete";
    const bannerColor = isComplete ? C.green : C.red;
    const bannerBg = isComplete ? "rgba(109,200,137,0.12)" : "rgba(224,96,96,0.08)";
    const bannerText = isComplete ? t("CivicVoice.Banner.Complete", "Complete!") : t("CivicVoice.Banner.Abandoned", "Abandoned");
    const bannerIcon = isComplete ? "+" : "x";
    const accentColor = state === "idle" ? accent : isComplete ? C.green : C.red;

    return (
        <div style={{
            margin: "0 10rem 6rem",
            overflow: "hidden",
            maxHeight: state === "sliding" ? "0" : "500rem",
            transition: state === "sliding" ? "max-height 0.4s ease 0.05s" : "none",
        }}>
            <div style={{
                background: C.card,
                borderRadius: "6rem",
                borderLeft: `3rem solid ${accentColor}`,
                overflow: "hidden",
                transform: state === "sliding" ? "translateX(110%)" : "translateX(0)",
                opacity: state === "sliding" ? 0 : 1,
                transition: "transform 0.4s ease, opacity 0.35s ease",
            }}>
                {state === "banner" && (
                    <div style={{
                        padding: "10rem 12rem",
                        fontSize: "13rem",
                        fontWeight: 600,
                        color: bannerColor,
                        background: bannerBg,
                        transform: visible ? "translateX(0)" : "translateX(-20rem)",
                        opacity: visible ? 1 : 0,
                        transition: "transform 0.25s ease, opacity 0.25s ease",
                    }}>
                        <span>{bannerIcon} {bannerText}</span>
                    </div>
                )}
                {state === "idle" && (
                    <>
                        <div style={{ padding: "10rem 12rem 10rem 14rem" }}>
                            <div style={{ display: "flex", flexDirection: "row", justifyContent: "space-between", alignItems: "center", marginBottom: "6rem" }}>
                                <div style={{ display: "flex", flexDirection: "row" }}>
                                    <Pill label={tierLabel(p.tier, t)} color={tc} />
                                    <Pill label={splitCamel(p.category)} color={cc} />
                                </div>
                                {isMayorPriority(p.category, mayorSpecialty) && (
                                    <div style={{ display: "flex", alignItems: "center", gap: "4rem", background: "rgba(224,168,74,0.15)", border: "1rem solid rgba(224,168,74,0.3)", borderRadius: "10rem", padding: "2rem 8rem" }}>
                                        <span style={{ fontSize: "11rem", color: C.amber }}>★</span>
                                        <span style={{ fontSize: "10rem", color: C.amber, fontWeight: 600, textTransform: "uppercase", letterSpacing: "0.5px" }}>Mayor priority</span>
                                    </div>
                                )}
                            </div>
                            <div style={{ fontSize: "18rem", fontWeight: 600, color: C.text, marginBottom: "4rem", lineHeight: 1.3 }}>{p.isComplete ? "+ " : ""}{p.title}</div>
                            <div style={{ fontSize: "14rem", color: C.muted2, lineHeight: 1.5, marginBottom: "8rem" }}>{p.progress}</div>
                            <ProgressBar pct={p.progressPct} color={accent} />
                        </div>
                        <div style={{ display: "flex", flexDirection: "row", justifyContent: "space-between", alignItems: "center", padding: "7rem 12rem", background: "rgba(0,0,0,0.12)", borderTop: "1rem solid rgba(255,255,255,0.04)" }}>
                            <div style={{ fontSize: "13rem", fontWeight: 600, color: urgent ? C.red : C.muted }}>{timeText}</div>
                            <div style={{ display: "flex", flexDirection: "row", gap: "24rem" }}>
                                {p.manualCompletion && !p.markedComplete && (
                                    <CvButton color="cyan" onClick={() => trigger_anim("complete", () => trigger("civicvoice", "markProjectComplete", p.id))}>{t("CivicVoice.Button.MarkComplete", "Mark complete")}</CvButton>
                                )}
                                {!p.isComplete && (
                                    <CvButton color="red" onClick={() => trigger_anim("abandon", () => trigger("civicvoice", "abandonProject", p.id))}>{t("CivicVoice.Button.Abandon", "Abandon")}</CvButton>
                                )}
                            </div>
                        </div>
                    </>
                )}
            </div>
        </div>
    );
}

function CollapsibleSection({ label, color, children, count, open, onToggle }: { label: string; color: string; children: any; count: number; open: boolean; onToggle: () => void }) {
    const [hovered, setHovered] = useState(false);
    return (
        <div>
            <div
                onClick={onToggle}
                onMouseEnter={() => setHovered(true)}
                onMouseLeave={() => setHovered(false)}
                style={{
                    display: "flex", flexDirection: "row", justifyContent: "space-between", alignItems: "center",
                    padding: "10rem 14rem 5rem", cursor: "pointer",
                    background: hovered ? "rgba(255,255,255,0.04)" : "transparent"
                }}>
                <div style={{ fontSize: "13rem", fontWeight: 700, letterSpacing: "1px", textTransform: "uppercase", color, display: "flex", flexDirection: "row", alignItems: "center", gap: "6rem" }}>{label} <span style={{ fontSize: "13rem", fontWeight: 700, color: color }}>({count})</span></div>
                <span style={{ fontSize: "11rem", color }}>{open ? "▲" : "▼"}</span>
            </div>
            {open && children}
        </div>
    );
}

function ProposalsTab({ proposed, metricOpen, setMetricOpen, adhocOpen, setAdhocOpen, majorOpen, setMajorOpen, mayorSpecialty }: { proposed: any[]; metricOpen: boolean; setMetricOpen: (v: boolean) => void; adhocOpen: boolean; setAdhocOpen: (v: boolean) => void; majorOpen: boolean; setMajorOpen: (v: boolean) => void; mayorSpecialty: string }) {
    const t = useT();
    if (!proposed?.length)
        return <div style={{ color: C.muted, textAlign: "center", padding: "16rem", fontSize: "16rem" }}>{t("CivicVoice.Proposals.Empty", "No proposals yet. Check back soon.")}</div>;

    const sortByPriority = (arr: any[]) => [...arr].sort((a, b) =>
        isMayorPriority(b.category, mayorSpecialty) ? 1 : isMayorPriority(a.category, mayorSpecialty) ? -1 : 0
    );
    const metric = sortByPriority(proposed.filter((p: any) => p.tier === "MetricTriggered"));
    const adhoc = sortByPriority(proposed.filter((p: any) => p.tier === "AdHoc"));
    const major = sortByPriority(proposed.filter((p: any) => p.tier === "Major"));

    return (
        <div>
            {metric.length > 0 && <CollapsibleSection label={t("CivicVoice.Proposals.Section.Urgent", "Urgent issues")} color={C.red} count={metric.length} open={metricOpen} onToggle={() => setMetricOpen(!metricOpen)}>
                {metric.map((p: any) => <ProposalCard key={p.id} p={p} onAccept={() => trigger("civicvoice", "acceptProject", p.id)} onReject={() => trigger("civicvoice", "rejectProject", p.id)} mayorSpecialty={mayorSpecialty} />)}
            </CollapsibleSection>}
            {adhoc.length > 0 && <CollapsibleSection label={t("CivicVoice.Proposals.Section.Citizen", "Citizen proposals")} color={C.cyan} count={adhoc.length} open={adhocOpen} onToggle={() => setAdhocOpen(!adhocOpen)}>
                {adhoc.map((p: any) => <ProposalCard key={p.id} p={p} onAccept={() => trigger("civicvoice", "acceptProject", p.id)} onReject={() => trigger("civicvoice", "rejectProject", p.id)} mayorSpecialty={mayorSpecialty} />)}
            </CollapsibleSection>}
            {major.length > 0 && <CollapsibleSection label={t("CivicVoice.Proposals.Section.Major", "Major projects")} color={C.purple} count={major.length} open={majorOpen} onToggle={() => setMajorOpen(!majorOpen)}>
                {major.map((p: any) => <ProposalCard key={p.id} p={p} onAccept={() => trigger("civicvoice", "acceptProject", p.id)} onReject={() => trigger("civicvoice", "rejectProject", p.id)} mayorSpecialty={mayorSpecialty} />)}
            </CollapsibleSection>}
            <div style={{ height: "8rem" }} />
        </div>
    );
}

function ActiveTab({ active, metricOpen, setMetricOpen, adhocOpen, setAdhocOpen, majorOpen, setMajorOpen, mayorSpecialty }: { active: any[]; metricOpen: boolean; setMetricOpen: (v: boolean) => void; adhocOpen: boolean; setAdhocOpen: (v: boolean) => void; majorOpen: boolean; setMajorOpen: (v: boolean) => void; mayorSpecialty: string }) {
    const t = useT();
    if (!active?.length)
        return <div style={{ color: C.muted, textAlign: "center", padding: "16rem", fontSize: "16rem" }}>{t("CivicVoice.Active.Empty", "No active projects. Accept proposals from the Proposals tab.")}</div>;

    const metric = active.filter((p: any) => p.tier === "MetricTriggered");
    const adhoc = active.filter((p: any) => p.tier === "AdHoc");
    const major = active.filter((p: any) => p.tier === "Major");

    return (
        <div>
            {metric.length > 0 && <CollapsibleSection label={t("CivicVoice.Active.Section.Urgent", "Urgent")} color={C.red} count={metric.length} open={metricOpen} onToggle={() => setMetricOpen(!metricOpen)}>
                {metric.map((p: any) => <ActiveCard key={p.id} p={p} mayorSpecialty={mayorSpecialty} />)}
            </CollapsibleSection>}
            {adhoc.length > 0 && <CollapsibleSection label={t("CivicVoice.Active.Section.InProgress", "In progress")} color={C.cyan} count={adhoc.length} open={adhocOpen} onToggle={() => setAdhocOpen(!adhocOpen)}>
                {adhoc.map((p: any) => <ActiveCard key={p.id} p={p} mayorSpecialty={mayorSpecialty} />)}
            </CollapsibleSection>}
            {major.length > 0 && <CollapsibleSection label={t("CivicVoice.Active.Section.Major", "Major projects")} color={C.purple} count={major.length} open={majorOpen} onToggle={() => setMajorOpen(!majorOpen)}>
                {major.map((p: any) => <ActiveCard key={p.id} p={p} mayorSpecialty={mayorSpecialty} />)}
            </CollapsibleSection>}
            <div style={{ height: "8rem" }} />
        </div>
    );
}

function ElectionTab({ election, mayorName, mayorSpecialty, mayorSlogan, termCompleted, termFailed, mayorTermMonths, mayorAge, mayorTermsServed }: { election: any; mayorName: string; mayorSpecialty: string; mayorSlogan: string; termCompleted: number; termFailed: number; mayorTermMonths: number; mayorAge: number; mayorTermsServed: number }) {
    const t = useT();
    const approvalScore = Math.round(Math.min(100, Math.max(0,
        (termCompleted * 8) - (termFailed * 12) + 50
    )));
    const approvalLabel = approvalScore >= 70 ? t("CivicVoice.Election.ApprovalGood", "Good") : approvalScore >= 50 ? t("CivicVoice.Election.ApprovalFair", "Fair") : t("CivicVoice.Election.ApprovalPoor", "Poor");
    const approvalColor = approvalScore >= 70 ? C.green : approvalScore >= 50 ? C.amber : C.red;

    const statRow = (label: string, value: any, color: string) => (
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", padding: "5rem 0", borderBottom: `1rem solid rgba(255,255,255,0.04)`, fontSize: "13rem", gap: "8rem" }}>
            <div style={{ color: C.muted2, flexShrink: 0 }}>{label}</div>
            <div style={{ color, fontWeight: 600, textAlign: "right", whiteSpace: "nowrap" }}>{value}</div>
        </div>
    );

    if (!election?.isActive)
        return (
            <div style={{ padding: "0 10rem" }}>
                {mayorName && mayorName !== "None" && (
                    <>
                        <div style={{ background: C.card, border: `1rem solid ${C.border}`, borderRadius: "6rem", padding: "10rem 12rem", margin: "10rem 0 6rem" }}>
                            <div style={{ fontSize: "13rem", color: C.muted, textTransform: "uppercase", letterSpacing: "0.5px", marginBottom: "4rem" }}>{t("CivicVoice.Election.CurrentMayor", "Current mayor")}</div>
                            <div style={{ fontWeight: 600, fontSize: "18rem", color: C.text }}>{mayorName}</div>
                            <div style={{ fontSize: "13rem", color: C.muted, marginBottom: "4rem" }}>{`Age ${mayorAge} · Term ${mayorTermsServed} of 2`}</div>
                            {mayorSpecialty && <Pill label={splitCamel(mayorSpecialty)} color={categoryColor(mayorSpecialty)} />}
                            {mayorSlogan && <div style={{ fontSize: "13rem", fontStyle: "italic", color: C.muted2, marginTop: "6rem" }}>{`"${mayorSlogan}"`}</div>}
                        </div>
                        <div style={{ background: C.card, border: `1rem solid ${C.border}`, borderRadius: "6rem", padding: "10rem 12rem", margin: "0 0 6rem" }}>
                            <div style={{ fontSize: "11rem", fontWeight: 700, color: C.cyan, letterSpacing: "0.8px", textTransform: "uppercase", marginBottom: "8rem", paddingBottom: "5rem", borderBottom: `1rem solid ${C.border}` }}>{t("CivicVoice.Election.MayorPerformance", "Mayor performance")}</div>
                            {statRow(t("CivicVoice.Election.ApprovalRating", "Approval rating"), `${approvalScore}% — ${approvalLabel}`, approvalColor)}
                            <div style={{ height: "4rem", borderRadius: "2rem", background: "rgba(255,255,255,0.07)", margin: "4rem 0 8rem" }}>
                                <div style={{ height: "4rem", width: `${approvalScore}%`, borderRadius: "2rem", background: approvalColor }} />
                            </div>
                            {statRow(t("CivicVoice.Election.TermLength", "Term length"), `${mayorTermMonths}mo`, C.text)}
                            {statRow(t("CivicVoice.Election.ProjectsCompleted", "Projects completed"), `${termCompleted} ${t("CivicVoice.Election.ThisTerm", "this term")}`, C.green)}
                            {statRow(t("CivicVoice.Election.ProjectsFailed", "Projects failed"), `${termFailed} ${t("CivicVoice.Election.ThisTerm", "this term")}`, termFailed > 0 ? C.red : C.text)}
                        </div>
                    </>
                )}
                <div style={{ color: C.muted, textAlign: "center", padding: "16rem", fontSize: "16rem" }}>{t("CivicVoice.Election.NoElection", "No election currently. Elections are held every year.")}</div>
            </div>
        );

    return (
        <div style={{ padding: "0 10rem" }}>
            <div style={{ fontSize: "14rem", color: C.muted, textAlign: "center", margin: "10rem 0 6rem", paddingBottom: "8rem", borderBottom: `1rem solid ${C.border}` }}>
                {election.hasVoted ? t("CivicVoice.Election.HasVoted", "You have cast your endorsement.") : t("CivicVoice.Election.HasNotVoted", "Citizens have voted. Endorse a candidate to add your influence.")}
            </div>
            <div style={{ margin: "0 0 10rem", padding: "8rem 12rem", background: C.card, borderRadius: "6rem" }}>
                <div style={{ display: "flex", flexDirection: "row", justifyContent: "space-between", fontSize: "14rem", marginBottom: "5rem" }}>
                    <div style={{ color: C.muted }}>{t("CivicVoice.Election.VotingInProgress", "Voting in progress")}</div>
                    <div style={{ color: C.cyan }}>{`${Math.round((election.progress || 0) * 100)}${t("CivicVoice.Election.PercentComplete", "% complete")}`}</div>
                </div>
                <div style={{ height: "4rem", borderRadius: "2rem", background: "rgba(255,255,255,0.07)" }}>
                    <div style={{ height: "4rem", width: `${Math.min((election.progress || 0) * 100, 100)}%`, borderRadius: "2rem", background: C.cyan }} />
                </div>
            </div>
            {election.candidates?.map((c: any) => {
                const leading = c.votes === Math.max(...election.candidates.map((x: any) => x.votes));
                const votesText = `${c.votes?.toLocaleString()} votes (${c.voteShare?.toFixed(1)}%)`;
                const endorseText = `${t("CivicVoice.Election.Endorse", "Endorse")} ${c.name.split(" ")[0]}`;
                return (
                    <div key={c.name} style={{ background: C.card, borderRadius: "6rem", borderLeft: `3rem solid ${leading ? C.cyan : C.border}`, overflow: "hidden", marginBottom: "6rem" }}>
                        <div style={{ padding: "10rem 12rem" }}>
                            <div style={{ fontSize: "18rem", fontWeight: 700, color: C.text, marginBottom: "2rem" }}>{c.name}</div>
                            <div style={{ fontSize: "14rem", color: C.muted, marginBottom: "6rem" }}>{`${c.party} ${t("CivicVoice.Election.Party", "·")} ${t("CivicVoice.Election.Age", "Age")} ${c.age}`}</div>
                            <div style={{ marginBottom: "6rem" }}>
                                <Pill label={splitCamel(c.specialty)} color={categoryColor(c.specialty)} />
                            </div>
                            <div style={{ fontSize: "13rem", fontStyle: "italic", color: C.muted2, marginBottom: "8rem" }}>{`"${c.slogan}"`}</div>
                            <div style={{ display: "flex", flexDirection: "row", justifyContent: "space-between", alignItems: "center" }}>
                                <div style={{ fontSize: "14rem", color: C.muted2 }}>{votesText}</div>
                                {!election.hasVoted && (
                                    <CvButton color="cyan" onClick={() => trigger("civicvoice", "castVote", c.name)}>{endorseText}</CvButton>
                                )}
                            </div>
                        </div>
                    </div>
                );
            })}
            <div style={{ height: "8rem" }} />
        </div>
    );
}

function StatsTab({ population, happiness, unemployment, votingPopulation, totalCompleted, totalFailed, totalAbandoned, health, crimeRate }: { population: number; happiness: number; unemployment: number; votingPopulation: number; totalCompleted: number; totalFailed: number; totalAbandoned: number; health: number; crimeRate: number }) {
    const t = useT();
    const [overviewOpen, setOverviewOpen] = useState(true);
    const [projectsOpen, setProjectsOpen] = useState(true);

    function StatRow({ label, value, color }: { label: string; value: string; color?: string }) {
        return (
            <div style={{ display: "flex", flexDirection: "row", justifyContent: "space-between", padding: "6rem 0", borderBottom: "1rem solid rgba(255,255,255,0.04)" }}>
                <div style={{ fontSize: "14rem", color: C.muted2 }}>{label}</div>
                <div style={{ fontSize: "14rem", fontWeight: 600, color: color || C.text }}>{value}</div>
            </div>
        );
    }

    function SectionHeader({ label, color, open, onToggle }: { label: string; color: string; open: boolean; onToggle: () => void }) {
        const [hovered, setHovered] = useState(false);
        return (
            <div onClick={onToggle} onMouseEnter={() => setHovered(true)} onMouseLeave={() => setHovered(false)}
                style={{ display: "flex", justifyContent: "space-between", alignItems: "center", padding: "8rem 0", cursor: "pointer", background: hovered ? "rgba(255,255,255,0.03)" : "transparent", marginBottom: "4rem" }}>
                <div style={{ fontSize: "11rem", fontWeight: 700, color, letterSpacing: "0.8px", textTransform: "uppercase" }}>{label}</div>
                <span style={{ fontSize: "11rem", color }}>{open ? "▲" : "▼"}</span>
            </div>
        );
    }

    return (
        <div style={{ margin: "10rem 10rem" }}>
            <div style={{ background: C.card, border: `1rem solid ${C.border}`, borderRadius: "6rem", padding: "10rem 12rem", marginBottom: "6rem" }}>
                <SectionHeader label={t("CivicVoice.Stats.Header", "City overview")} color={C.cyan} open={overviewOpen} onToggle={() => setOverviewOpen(v => !v)} />
                {overviewOpen && <>
                    <StatRow label={t("CivicVoice.Stats.Population", "Population")} value={population?.toLocaleString()} />
                    <StatRow label={t("CivicVoice.Stats.Happiness", "Happiness")} value={`${happiness?.toFixed(1)}%`} color={happiness > 70 ? C.green : happiness > 50 ? C.amber : C.red} />
                    <StatRow label={t("CivicVoice.Stats.Unemployment", "Unemployment")} value={`${unemployment?.toFixed(1)}%`} color={unemployment < 5 ? C.green : unemployment < 10 ? C.amber : C.red} />
                    <StatRow label={t("CivicVoice.Stats.EligibleVoters", "Eligible voters")} value={votingPopulation?.toLocaleString()} />
                    <StatRow label={t("CivicVoice.Stats.Health", "Health")} value={`${health?.toFixed(0)}`} color={health > 70 ? C.green : health > 45 ? C.amber : C.red} />
                    <StatRow label={t("CivicVoice.Stats.CrimeRate", "Crime probability")} value={`${crimeRate?.toFixed(1)}%`} color={crimeRate < 30 ? C.green : crimeRate < 60 ? C.amber : C.red} />
                </>}
            </div>
            <div style={{ background: C.card, border: `1rem solid ${C.border}`, borderRadius: "6rem", padding: "10rem 12rem" }}>
                <SectionHeader label={t("CivicVoice.Stats.Projects", "Project overview")} color={C.cyan} open={projectsOpen} onToggle={() => setProjectsOpen(v => !v)} />
                {projectsOpen && <>
                    <StatRow label={t("CivicVoice.Stats.ProjectsCompleted", "Completed")} value={totalCompleted?.toString()} color={C.green} />
                    <StatRow label={t("CivicVoice.Stats.ProjectsFailed", "Failed")} value={totalFailed?.toString()} color={totalFailed > 0 ? C.red : C.text} />
                    <StatRow label={t("CivicVoice.Stats.ProjectsAbandoned", "Abandoned")} value={totalAbandoned?.toString()} color={totalAbandoned > 0 ? C.amber : C.text} />
                </>}
            </div>
            <div style={{ height: "8rem" }} />
        </div>
    );
}

function NotificationHistory({ notifications }: { notifications: string[] }) {
    const [hovered, setHovered] = useState(false);
    const [expanded, setExpanded] = useState(true);
    const t = useT();
    return (
        <div style={{ borderLeft: `3rem solid ${C.cyan}`, background: "rgba(75,195,212,0.06)" }}>
            <div
                onClick={() => setExpanded(v => !v)}
                onMouseEnter={() => setHovered(true)}
                onMouseLeave={() => setHovered(false)}
                style={{
                    padding: "6rem 14rem", display: "flex", justifyContent: "space-between", alignItems: "center", cursor: "pointer",
                    background: hovered ? "rgba(75,195,212,0.1)" : "transparent"
                }}>
                <span style={{ fontSize: "14rem", color: C.cyan, textTransform: "uppercase", letterSpacing: "1px", fontWeight: 600 }}>{t("CivicVoice.Notifications.RecentActivity", "Recent activity")}</span>
                <span style={{ fontSize: "14rem", color: C.cyan }}>{expanded ? "▲" : "▼"}</span>
            </div>
            {expanded && notifications.map((n, i) => (
                <div key={i} style={{ opacity: i === 0 ? 1 : i === 1 ? 0.5 : 0.2, padding: "5rem 14rem", borderTop: `1rem solid rgba(255,255,255,0.06)`, fontSize: "13rem", color: C.text }}>
                    {n}
                </div>
            ))}
        </div>
    );
}

type ToastData = {
    eyebrow: string;
    title: string;
    body: string;
    color: string;
    icon: string;
};

function parseToast(msg: string, t: (k: string, f: string) => string): ToastData | null {
    if (/project accepted:|proposal rejected:|project abandoned:|you have endorsed/i.test(msg)) return null;
    if (/has won the election/i.test(msg))
        return { eyebrow: t("CivicVoice.Toast.ElectionResult", "Election result"), title: msg.replace(/\s*\(.*?\)/, "").replace(" has won the election!", ` ${t("CivicVoice.Toast.ElectionTitle", "elected!")}`), body: msg.match(/\(([^)]+)\)/)?.[1] ?? "", color: C.amber, icon: "🗳️" };
    if (/special election|first mayoral election|election time/i.test(msg))
        return { eyebrow: t("CivicVoice.Toast.Election", "Election"), title: t("CivicVoice.Toast.ElectionUnderway", "Election underway!"), body: msg, color: C.amber, icon: "🗳️" };
    if (/you have endorsed/i.test(msg))
        return { eyebrow: t("CivicVoice.Toast.Election", "Election"), title: t("CivicVoice.Toast.VoteCast", "Vote cast"), body: msg, color: C.amber, icon: "🗳️" };
    if (/project complete:/i.test(msg)) {
        const name = msg.match(/"([^"]+)"/)?.[1] ?? "";
        return { eyebrow: t("CivicVoice.Toast.ProjectComplete", "Project complete"), title: name, body: t("CivicVoice.Toast.ProjectComplete.Body", "Open panel to view active projects"), color: C.green, icon: "+" };
    }
    if (/project failed:/i.test(msg)) {
        const name = msg.match(/"([^"]+)"/)?.[1] ?? "";
        return { eyebrow: t("CivicVoice.Toast.ProjectFailed", "Project failed"), title: name, body: t("CivicVoice.Toast.ProjectFailed.Body", "Open panel for details"), color: C.red, icon: "x" };
    }
    if (/project accepted:/i.test(msg)) {
        const name = msg.match(/"([^"]+)"/)?.[1] ?? "";
        return { eyebrow: t("CivicVoice.Toast.ProjectAccepted", "Project accepted"), title: name, body: t("CivicVoice.Toast.ProjectAccepted.Body", "Open panel to track progress"), color: C.cyan, icon: ">" };
    }
    if (/project abandoned:/i.test(msg)) {
        const name = msg.match(/"([^"]+)"/)?.[1] ?? "";
        return { eyebrow: t("CivicVoice.Toast.ProjectAbandoned", "Project abandoned"), title: name, body: "", color: C.muted, icon: "x" };
    }
    if (/proposal rejected:/i.test(msg)) {
        const name = msg.match(/"([^"]+)"/)?.[1] ?? "";
        return { eyebrow: t("CivicVoice.Toast.ProposalRejected", "Proposal rejected"), title: name, body: "", color: C.muted, icon: "x" };
    }
    if (/citizens are demanding/i.test(msg)) {
        const name = msg.match(/"([^"]+)"/)?.[1] ?? "";
        return { eyebrow: t("CivicVoice.Toast.UrgentProposal", "Urgent proposal"), title: name, body: t("CivicVoice.Toast.UrgentProposal.Body", "Citizens need action — open panel"), color: C.red, icon: "!" };
    }
    if (/citizens have new ideas|citizens are proposing a major/i.test(msg))
        return { eyebrow: t("CivicVoice.Toast.NewProposal", "New proposal"), title: t("CivicVoice.Toast.NewProposal.Title", "Citizens have ideas"), body: t("CivicVoice.Toast.NewProposal.Body", "Open panel to review proposals"), color: C.cyan, icon: "+" };
    if (/maximum active/i.test(msg))
        return { eyebrow: t("CivicVoice.Toast.SlotsFull", "Slots full"), title: t("CivicVoice.Toast.SlotsFull.Title", "Can't accept project"), body: msg, color: C.amber, icon: "!" };
    if (/reached.*citizens/i.test(msg))
        return { eyebrow: t("CivicVoice.Toast.Milestone", "Milestone"), title: t("CivicVoice.Toast.Milestone.Title", "First election!"), body: msg, color: C.amber, icon: "🗳️" };
    return { eyebrow: t("CivicVoice.Toast.Default", "Civic Voice"), title: msg, body: "", color: C.cyan, icon: ">" };
}

function Toast({ message, onDone }: { message: string; onDone: () => void }) {
    const [progress, setProgress] = useState(100);
    const [visible, setVisible] = useState(false);
    const t = useT();
    const toast = parseToast(message, t);
    if (!toast) return null;
    const DURATION = 5000;

    useEffect(() => {
        const inTimer = setTimeout(() => setVisible(true), 10);
        const start = Date.now();
        const interval = setInterval(() => {
            const elapsed = Date.now() - start;
            const pct = Math.max(0, 100 - (elapsed / DURATION) * 100);
            setProgress(pct);
            if (pct <= 0) {
                clearInterval(interval);
                setVisible(false);
                setTimeout(onDone, 300);
            }
        }, 50);
        return () => { clearTimeout(inTimer); clearInterval(interval); };
    }, []);

    return (
        <div style={{
            marginBottom: "6rem",
            width: "300rem",
            background: "rgba(8,11,18,0.97)",
            border: `1rem solid ${toast.color}33`,
            borderLeft: `3rem solid ${toast.color}`,
            borderRadius: "8rem",
            overflow: "hidden",
            boxShadow: "0 4rem 24rem rgba(0,0,0,0.6)",
            transform: visible ? "translateX(0)" : "translateX(320rem)",
            opacity: visible ? 1 : 0,
            transition: "transform 0.3s ease, opacity 0.3s ease",
            pointerEvents: "all",
        }}>
            <div style={{ display: "flex", gap: "10rem", padding: "10rem 12rem 8rem", alignItems: "flex-start" }}>
                <div style={{ fontSize: "18rem", flexShrink: 0, marginTop: "1rem" }}>{toast.icon}</div>
                <div style={{ flex: 1, minWidth: 0 }}>
                    <div style={{ fontSize: "10rem", fontWeight: 700, letterSpacing: "1.5px", textTransform: "uppercase", color: toast.color, marginBottom: "2rem" }}>{toast.eyebrow}</div>
                    <div style={{ fontSize: "14rem", fontWeight: 600, color: C.text, marginBottom: toast.body ? "2rem" : 0, lineHeight: 1.3 }}>{toast.title}</div>
                    {toast.body ? <div style={{ fontSize: "12rem", color: C.muted, lineHeight: 1.4 }}>{toast.body}</div> : null}
                </div>
            </div>
            <div style={{ height: "3rem", background: "rgba(255,255,255,0.06)" }}>
                <div style={{ height: "100%", width: `${progress}%`, background: toast.color, transition: "width 0.05s linear" }} />
            </div>
        </div>
    );
}

export function CivicVoiceToasts({ panelOpen }: { panelOpen: boolean }) {
    const notifications = useValue(notifications$) as string[];
    const [toasts, setToasts] = useState<{ id: number; message: string }[]>([]);
    const [pending, setPending] = useState<string[]>([]);
    const [lastSeen, setLastSeen] = useState<string>("");
    const [nextId, setNextId] = useState(0);
    const showNotifications = useValue(showNotifications$);

    const latest = notifications?.[0] ?? "";

    useEffect(() => {
        if (latest && latest !== lastSeen) {
            setLastSeen(latest);
            if (!panelOpen) {
                const t = (k: string, f: string) => f;
                const toast = parseToast(latest, t);
                if (toast) setPending(prev => [...prev, latest]);
            }
        }
    }, [latest, panelOpen]);

    useEffect(() => {
        if (pending.length > 0 && toasts.length < 3) {
            const next = pending[0];
            setPending(prev => prev.slice(1));
            setNextId(id => {
                setToasts(prev => [...prev, { id, message: next }]);
                return id + 1;
            });
        }
    }, [pending, toasts.length]);

    if (panelOpen || toasts.length === 0 || !showNotifications) return null;

    return (
        <div style={{
            position: "fixed",
            bottom: "320rem",
            right: "20rem",
            zIndex: 2000,
            display: "flex",
            flexDirection: "column-reverse",
            alignItems: "flex-end",
            pointerEvents: "none",
        }}>
            {toasts.map(t => (
                <Toast
                    key={t.id}
                    message={t.message}
                    onDone={() => setToasts(prev => prev.filter(x => x.id !== t.id))}
                />
            ))}
        </div>
    );
}

export const CivicVoicePanel = () => {
    const [tab, setTab] = useState("proposals");
    const [proposalMetricOpen, setProposalMetricOpen] = useState(true);
    const [proposalAdhocOpen, setProposalAdhocOpen] = useState(true);
    const [proposalMajorOpen, setProposalMajorOpen] = useState(true);
    const [activeMetricOpen, setActiveMetricOpen] = useState(true);
    const [activeAdhocOpen, setActiveAdhocOpen] = useState(true);
    const [activeMajorOpen, setActiveMajorOpen] = useState(true);

    const [pos, setPos] = useState<{ x: number; y: number } | null>(null);
    const dragStart = useRef<{ mx: number; my: number; px: number; py: number } | null>(null);
    const panelRef = useRef<HTMLDivElement | null>(null);
    const t = useT();


    const onMouseDown = (e: React.MouseEvent) => {
        const rect = panelRef.current?.getBoundingClientRect();
        const px = pos?.x ?? (rect?.left ?? 0);
        const py = pos?.y ?? (rect?.top ?? 0);
        dragStart.current = { mx: e.clientX, my: e.clientY, px, py };
        const onMove = (e: MouseEvent) => {
            if (!dragStart.current) return;
            const dx = e.clientX - dragStart.current.mx;
            const dy = e.clientY - dragStart.current.my;
            setPos({ x: dragStart.current.px + dx, y: dragStart.current.py + dy });
        };
        const onUp = () => {
            dragStart.current = null;
            window.removeEventListener("mousemove", onMove);
            window.removeEventListener("mouseup", onUp);
        };
        window.addEventListener("mousemove", onMove);
        window.addEventListener("mouseup", onUp);
    };

    const population = useValue(population$);
    const happiness = useValue(happiness$);
    const unemployment = useValue(unemployment$);
    const usedSlots = useValue(usedSlots$);
    const hasElection = useValue(hasElection$);
    const mayorName = useValue(mayorName$);
    const notifications = useValue(notifications$);
    const proposed = useValue(proposed$);
    const active = useValue(active$);
    const election = useValue(election$);
    const votingPopulation = useValue(votingPopulation$);
    const nextElectionDays = useValue(nextElectionDays$);
    const totalCompleted = useValue(totalCompleted$);
    const totalFailed = useValue(totalFailed$);
    const mayorSpecialty = useValue(mayorSpecialty$);
    const mayorSlogan = useValue(mayorSlogan$);
    const electionsModActive = useValue(electionsModActive$);
    const electionsPanel = useValue(electionsPanel$);
    const termCompleted = useValue(termCompleted$);
    const termFailed = useValue(termFailed$);
    const mayorTermMonths = useValue(mayorTermMonths$);
    const mayorAge = useValue(mayorAge$);
    const mayorTermsServed = useValue(mayorTermsServed$);
    const mayorLabel = t("CivicVoice.Meta.Mayor", "Mayor:");
    const electionLabel = t("CivicVoice.Meta.NextElection", "Next election:");
    const activeLabel = t("CivicVoice.Meta.Active", "Active:");
    const popLabel = t("CivicVoice.Footer.Pop", "Pop ");
    const happinessLabel = t("CivicVoice.Footer.Happiness", "Happiness ");
    const unemployedLabel = t("CivicVoice.Footer.Unemployed", "Unemployed ");
    const popValue = population?.toLocaleString();
    const happinessValue = `${happiness?.toFixed(1)}%`;
    const unemployedValue = `${unemployment?.toFixed(1)}%`;
    const health = useValue(health$);
    const crimeRate = useValue(crimeRate$);
    const totalAbandoned = useValue(totalAbandoned$);


    return (
        <div ref={panelRef} style={{
            width: "var(--rightPanelWidth)",
            position: "fixed",
            left: pos ? `${pos.x}px` : "auto",
            right: pos ? "auto" : "80rem",
            top: pos ? `${pos.y}px` : "80rem",
            background: "rgba(20,26,40,0.75)",
            border: `1rem solid ${C.border2}`,
            borderRadius: "16rem",
            fontFamily: "var(--fontFamily)",
            fontSize: "20rem",
            color: C.text,
            overflow: "hidden",
            boxShadow: "0 8rem 40rem rgba(0,0,0,0.6)",
            zIndex: 1000,
            pointerEvents: "all",
        }}>
            <div
                onMouseDown={onMouseDown}
                style={{
                    background: C.titleBar,
                    padding: "10rem 16rem",
                    borderBottom: `1rem solid ${C.border2}`,
                    textAlign: "center",
                    cursor: "grab",
                    userSelect: "none",
                }}>
                <div style={{ display: "flex", flexDirection: "column", alignItems: "center", gap: "6rem" }}>
                    <div style={{ display: "flex", alignItems: "center", gap: "14rem" }}>
                        <div style={{ fontSize: "26rem", fontWeight: 700, textTransform: "uppercase", color: "#4bc3d4", letterSpacing: "3px" }}>
                            {t("CivicVoice.Title", "CIVIC VOICE")}
                        </div>
                        <div style={{ fontSize: "8rem", fontWeight: 600, letterSpacing: "0.5px", padding: "3rem 7rem", borderRadius: "20rem", background: "rgba(75,195,212,0.1)", color: "rgba(75,195,212,0.6)", border: "1rem solid rgba(75,195,212,0.2)", textTransform: "uppercase", marginLeft: "2rem" }}>{t("CivicVoice.Beta", "BETA")}</div>
                    </div>
                </div>
            </div>
            <div style={{ background: C.metaBar, padding: "8rem 14rem", borderBottom: `1rem solid ${C.border}`, display: "flex", flexDirection: "row", justifyContent: "space-between", fontSize: "14rem", color: C.muted }}>
                <div><div>{mayorLabel}</div><div style={{ color: C.amber }}>{electionsModActive ? (electionsPanel?.mayorName || "None") : mayorName}</div></div>
                <div><div>{electionLabel}</div><div style={{ color: C.cyan, fontWeight: 600 }}>{electionsModActive ? (electionsPanel?.electionDate || "TBD") : (nextElectionDays >= 0 ? `${nextElectionDays}mo` : "TBD")}</div></div>
                <div><div>{activeLabel}</div><div style={{ color: C.cyan, fontWeight: 600 }}>{`${usedSlots} / 7`}</div></div>
            </div>
            {notifications?.length > 0 && <NotificationHistory notifications={notifications} />}
            {hasElection && tab !== "election" && !electionsModActive && (
                <div onClick={() => setTab("election")} style={{ background: "rgba(255,203,0,0.06)", borderLeft: `3rem solid ${C.amber}`, padding: "7rem 14rem", fontSize: "14rem", color: C.amber, cursor: "pointer", display: "flex", flexDirection: "row", alignItems: "center" }}>
                    <div style={{ width: "6rem", height: "6rem", borderRadius: "50%", background: C.amber, marginRight: "6rem" }} />
                    {t("CivicVoice.Election.ElectionUnderway", "Election underway — tap to view candidates")}
                </div>
            )}
            <Tabs active={tab} onSelect={setTab} hasElection={hasElection} proposed={proposed} activeProjects={active} electionsModActive={electionsModActive} />
            <div style={{ background: C.body }}>
                <Scrollable style={{ maxHeight: "460rem" }}>
                    {tab === "proposals" && <ProposalsTab proposed={proposed} metricOpen={proposalMetricOpen} setMetricOpen={setProposalMetricOpen} adhocOpen={proposalAdhocOpen} setAdhocOpen={setProposalAdhocOpen} majorOpen={proposalMajorOpen} setMajorOpen={setProposalMajorOpen} mayorSpecialty={mayorSpecialty} />}
                    {tab === "active" && <ActiveTab active={active} metricOpen={activeMetricOpen} setMetricOpen={setActiveMetricOpen} adhocOpen={activeAdhocOpen} setAdhocOpen={setActiveAdhocOpen} majorOpen={activeMajorOpen} setMajorOpen={setActiveMajorOpen} mayorSpecialty={mayorSpecialty} />}
                    {tab === "election" && !electionsModActive && <ElectionTab election={election} mayorName={mayorName} mayorSpecialty={mayorSpecialty} mayorSlogan={mayorSlogan} termCompleted={termCompleted} termFailed={termFailed} mayorTermMonths={mayorTermMonths} mayorAge={mayorAge} mayorTermsServed={mayorTermsServed} />}
                    {tab === "election" && electionsModActive && (
                        <div style={{ padding: "20rem 14rem", color: C.muted2, fontSize: "14rem", lineHeight: 1.6, textAlign: "center" }}>
                            <div style={{ fontSize: "24rem", marginBottom: "8rem" }}>🗳️</div>
                            <div style={{ fontWeight: 600, color: C.amber, marginBottom: "6rem" }}>{t("CivicVoice.Election.Banner", "Elections mod detected")}</div>
                            <div>{t("CivicVoice.Election.ManagedBy", "CivicVoice elections are disabled. Mayor data is managed by the Elections mod.")}</div>
                        </div>
                    )}
                    {tab === "stats" && <StatsTab population={population} happiness={happiness} unemployment={unemployment} votingPopulation={votingPopulation} totalCompleted={totalCompleted} totalFailed={totalFailed} totalAbandoned={totalAbandoned} health={health} crimeRate={crimeRate} />}
                </Scrollable>
            </div>
            <div style={{ background: C.footer, padding: "8rem 14rem", borderTop: `1rem solid ${C.border}`, display: "flex", flexDirection: "row", justifyContent: "space-between", fontSize: "13rem", color: C.muted }}>
                <div><div>{popLabel}</div><div style={{ color: C.text, fontWeight: 500 }}>{popValue}</div></div>
                <div><div>{happinessLabel}</div><div style={{ color: happiness > 70 ? C.green : happiness > 50 ? C.amber : C.red, fontWeight: 500 }}>{happinessValue}</div></div>
                <div><div>{unemployedLabel}</div><div style={{ color: unemployment < 5 ? C.green : unemployment < 10 ? C.amber : C.red, fontWeight: 500 }}>{unemployedValue}</div></div>
            </div>
        </div>
    );
};

// ============================================================
// CivicVoice — Democracy & Governance Mod for Cities: Skylines II
// Created by xTrueF | github.com/xTrueF/CivicVoice
// Licensed under MIT License
// ============================================================
import { bindValue, useValue, trigger } from "cs2/api";
import { Scrollable } from "cs2/ui";
import { useState } from "react";

const population$ = bindValue<number>("civicvoice", "population", 0);
const happiness$ = bindValue<number>("civicvoice", "happiness", 50);
const unemployment$ = bindValue<number>("civicvoice", "unemployment", 5);
const usedSlots$ = bindValue<number>("civicvoice", "usedSlots", 0);
const hasElection$ = bindValue<boolean>("civicvoice", "hasElection", false);
const mayorName$ = bindValue<string>("civicvoice", "mayorName", "None");
const notification$ = bindValue<string>("civicvoice", "notification", "");
const proposed$ = bindValue<any[]>("civicvoice", "proposed", []);
const active$ = bindValue<any[]>("civicvoice", "active", []);
const election$ = bindValue<any>("civicvoice", "election", null);
const votingPopulation$ = bindValue<number>("civicvoice", "votingPopulation", 0);
const nextElectionDays$ = bindValue<number>("civicvoice", "nextElectionDays", 0);

const C = {
    titleBar: 'rgba(8,11,18,0.95)',
    metaBar: 'rgba(14,18,28,0.85)',
    body: 'rgba(255,255,255,0.03)',
    card: 'rgba(30,37,56,0.70)',
    footer: 'rgba(8,11,18,0.90)',
    border: 'rgba(255,255,255,0.08)',
    border2: 'rgba(255,255,255,0.12)',
    text: '#e8eaf0',
    muted: '#7a8299',
    muted2: '#8e95a0',
    cyan: '#4bc3d4',
    green: '#6dc889',
    red: '#e06060',
    amber: '#e0a84a',
    purple: '#a78bfa',
};

function tierColor(tier: string): string {
    if (tier === 'MetricTriggered') return C.red;
    if (tier === 'Major') return C.purple;
    return C.cyan;
}

function tierLabel(tier: string): string {
    if (tier === 'MetricTriggered') return 'URGENT';
    if (tier === 'Major') return 'MAJOR';
    return 'PROPOSAL';
}

function categoryColor(cat: string): string {
    const map: Record<string, string> = {
        Healthcare: '#e47ca6',
        Education: '#7db5e0',
        Transport: '#5cc8d4',
        Environment: '#6dc889',
        Economy: '#e0a84a',
        PublicSafety: '#a78bfa',
        Leisure: '#1abc9c',
        Housing: '#8e95a0',
        Infrastructure: '#f97316',
    };
    return map[cat] || C.cyan;
}

function splitCamel(s: string): string {
    return s.replace(/([A-Z])/g, ' $1').trim();
}

function Pill({ label, color }: { label: string; color: string }) {
    return (
        <div style={{
            display: 'inline-block', padding: '2rem 7rem', borderRadius: '13rem',
            fontSize: '13rem', fontWeight: 600, marginRight: '4rem',
            textTransform: 'uppercase', background: color + '28', color
        }}>{label}</div>
    );
}

function ProgressBar({ pct, color }: { pct: number; color: string }) {
    return (
        <div style={{ height: '4rem', borderRadius: '2rem', background: 'rgba(255,255,255,0.07)', marginTop: '4rem', marginBottom: '4rem' }}>
            <div style={{ height: '100%', width: `${Math.min(pct || 0, 100)}%`, background: color, borderRadius: '2rem' }} />
        </div>
    );
}

function Tabs({ active, onSelect, hasElection, proposed }: { active: string; onSelect: (t: string) => void; hasElection: boolean; proposed: any[] }) {
    const tabs = ['proposals', 'active', 'election', 'stats'];
    return (
        <div style={{ display: 'flex', flexDirection: 'row', background: C.metaBar, borderBottom: `1rem solid ${C.border}` }}>
            {tabs.map(t => (
                <div key={t} onClick={() => onSelect(t)} style={{
                    flex: 1, padding: '9rem 4rem', textAlign: 'center', cursor: 'pointer',
                    fontSize: '12rem',
                    color: active === t ? C.cyan : C.muted,
                    borderBottom: active === t ? `2rem solid ${C.cyan}` : `2rem solid transparent`,
                    fontWeight: active === t ? 600 : 400,
                    letterSpacing: '0.3px',
                }}>
                    {t === 'proposals' && <div style={{ display: 'flex', flexDirection: 'row', justifyContent: 'center', alignItems: 'center' }}>
                        PROPOSALS{proposed?.length > 0 ? <div style={{ color: C.red, marginLeft: '3rem' }}>●</div> : null}
                    </div>}
                    {t === 'active' && 'ACTIVE PROJECTS'}
                    {t === 'election' && <div style={{ display: 'flex', flexDirection: 'row', justifyContent: 'center', alignItems: 'center' }}>
                        ELECTION{hasElection ? <div style={{ color: C.red, marginLeft: '3rem' }}>●</div> : null}
                    </div>}
                    {t === 'stats' && 'CITY STATS'}
                </div>
            ))}
        </div>
    );
}

function SectionLabel({ label, color }: { label: string; color: string }) {
    return (
        <div style={{ fontSize: '13rem', fontWeight: 700, letterSpacing: '1px', textTransform: 'uppercase', padding: '10rem 14rem 5rem', color }}>
            {label}
        </div>
    );
}

function ProposalCard({ p, onAccept, onReject }: { p: any; onAccept: () => void; onReject: () => void }) {
    const tc = tierColor(p.tier);
    const cc = categoryColor(p.category);
    const forText = `${p.votesFor?.toLocaleString()} for (${p.voteShare?.toFixed(0)}%)`;
    const againstPct = (100 - (p.voteShare || 0)).toFixed(0);
    const againstText = `${p.votesAgainst?.toLocaleString()} against (${againstPct}%)`;
    return (
        <div style={{ margin: '0 10rem 6rem', background: C.card, borderRadius: '6rem', borderLeft: `3rem solid ${tc}`, overflow: 'hidden' }}>
            <div style={{ padding: '10rem 12rem 10rem 14rem' }}>
                <div style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: '6rem' }}>
                    <div style={{ display: 'flex', flexDirection: 'row' }}>
                        <Pill label={tierLabel(p.tier)} color={tc} />
                        <Pill label={splitCamel(p.category)} color={cc} />
                    </div>
                    <div style={{ fontSize: '14rem', color: C.muted, marginLeft: '8rem' }}>{`${p.deadline}mo`}</div>
                </div>
                <div style={{ fontSize: '18rem', fontWeight: 600, color: C.text, marginBottom: '4rem', lineHeight: 1.3 }}>{p.title}</div>
                <div style={{ fontSize: '14rem', color: C.muted2, lineHeight: 1.5, marginBottom: '8rem' }}>{p.description}</div>
                <div style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-between', fontSize: '13rem', marginBottom: '3rem' }}>
                    <div style={{ color: C.green }}>{forText}</div>
                    <div style={{ color: C.red }}>{againstText}</div>
                </div>
                <div style={{ height: '4rem', borderRadius: '2rem', background: C.red, marginBottom: '4rem' }}>
                    <div style={{ height: '4rem', width: `${Math.min(Math.max(p.voteShare || 0, 0), 100)}%`, borderRadius: '2rem', background: C.green }} />
                </div>
            </div>
            <div style={{ display: 'flex', flexDirection: 'row', justifyContent: 'flex-end', gap: '6rem', padding: '7rem 12rem', background: 'rgba(0,0,0,0.12)', borderTop: `1rem solid rgba(255,255,255,0.04)` }}>
                <div onClick={onReject} style={{ padding: '4rem 12rem', borderRadius: '4rem', border: `1rem solid rgba(224,96,96,0.4)`, color: C.red, fontSize: '14rem', fontWeight: 600, cursor: 'pointer' }}>Reject</div>
                <div onClick={onAccept} style={{ padding: '4rem 12rem', borderRadius: '4rem', border: `1rem solid rgba(75,195,212,0.4)`, color: C.cyan, fontSize: '14rem', fontWeight: 600, cursor: 'pointer' }}>Accept</div>
            </div>
        </div>
    );
}

function ProposalsTab({ proposed }: { proposed: any[] }) {
    if (!proposed?.length)
        return <div style={{ color: C.muted, textAlign: 'center', padding: '16rem', fontSize: '16rem' }}>No proposals yet. Check back soon.</div>;

    const metric = proposed.filter((p: any) => p.tier === 'MetricTriggered');
    const adhoc = proposed.filter((p: any) => p.tier === 'AdHoc');
    const major = proposed.filter((p: any) => p.tier === 'Major');

    return (
        <div>
            {metric.length > 0 && <>
                <SectionLabel label="⚠ Urgent issues" color={C.red} />
                {metric.map((p: any) => <ProposalCard key={p.id} p={p} onAccept={() => trigger('civicvoice', 'acceptProject', p.id)} onReject={() => trigger('civicvoice', 'rejectProject', p.id)} />)}
            </>}
            {adhoc.length > 0 && <>
                <SectionLabel label="Citizen proposals" color={C.cyan} />
                {adhoc.map((p: any) => <ProposalCard key={p.id} p={p} onAccept={() => trigger('civicvoice', 'acceptProject', p.id)} onReject={() => trigger('civicvoice', 'rejectProject', p.id)} />)}
            </>}
            {major.length > 0 && <>
                <SectionLabel label="★ Major projects" color={C.purple} />
                {major.map((p: any) => <ProposalCard key={p.id} p={p} onAccept={() => trigger('civicvoice', 'acceptProject', p.id)} onReject={() => trigger('civicvoice', 'rejectProject', p.id)} />)}
            </>}
            <div style={{ height: '8rem' }} />
        </div>
    );
}

function ActiveTab({ active }: { active: any[] }) {
    if (!active?.length)
        return <div style={{ color: C.muted, textAlign: 'center', padding: '16rem', fontSize: '16rem' }}>No active projects. Accept proposals from the Proposals tab.</div>;

    const metric = active.filter((p: any) => p.tier === 'MetricTriggered');
    const adhoc = active.filter((p: any) => p.tier === 'AdHoc');
    const major = active.filter((p: any) => p.tier === 'Major');

    const renderActive = (p: any) => {
        const urgent = p.daysLeft < 2 && !p.manualCompletion;
        const tc = tierColor(p.tier);
        const accent = p.isComplete ? C.green : urgent ? C.red : tc;
        const timeText = p.isComplete ? 'Complete!' : `${p.daysLeft}mo left`;
        return (
            <div key={p.id} style={{ margin: '0 10rem 6rem', background: C.card, borderRadius: '6rem', borderLeft: `3rem solid ${accent}`, overflow: 'hidden' }}>
                <div style={{ padding: '10rem 12rem' }}>
                    <div style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-between', marginBottom: '4rem' }}>
                        <div style={{ fontSize: '18rem', fontWeight: 600, color: C.text }}>{p.isComplete ? '✓ ' : ''}{p.title}</div>
                        <div style={{ fontSize: '14rem', color: urgent ? C.red : C.muted }}>{timeText}</div>
                    </div>
                    <div style={{ fontSize: '14rem', color: C.muted2, marginBottom: '5rem' }}>{p.progress}</div>
                    <ProgressBar pct={p.progressPct} color={accent} />
                </div>
                <div style={{ display: 'flex', flexDirection: 'row', justifyContent: 'flex-end', gap: '6rem', padding: '7rem 12rem', background: 'rgba(0,0,0,0.12)', borderTop: `1rem solid rgba(255,255,255,0.04)` }}>
                    {p.manualCompletion && !p.markedComplete && (
                        <div onClick={() => trigger('civicvoice', 'markProjectComplete', p.id)}
                            style={{ padding: '4rem 12rem', borderRadius: '4rem', border: `1rem solid rgba(75,195,212,0.4)`, color: C.cyan, fontSize: '14rem', fontWeight: 600, cursor: 'pointer' }}>
                            Mark complete
                        </div>
                    )}
                    {!p.isComplete && (
                        <div onClick={() => trigger('civicvoice', 'abandonProject', p.id)}
                            style={{ padding: '4rem 12rem', borderRadius: '4rem', border: `1rem solid rgba(224,96,96,0.4)`, color: C.red, fontSize: '14rem', fontWeight: 600, cursor: 'pointer' }}>
                            Abandon
                        </div>
                    )}
                </div>
            </div>
        );
    };

    return (
        <div>
            {metric.length > 0 && <><SectionLabel label="⚠ Urgent" color={C.red} />{metric.map(renderActive)}</>}
            {adhoc.length > 0 && <><SectionLabel label="In progress" color={C.cyan} />{adhoc.map(renderActive)}</>}
            {major.length > 0 && <><SectionLabel label="★ Major projects" color={C.purple} />{major.map(renderActive)}</>}
            <div style={{ height: '8rem' }} />
        </div>
    );
}

function ElectionTab({ election, mayorName }: { election: any; mayorName: string }) {
    if (!election?.isActive)
        return (
            <div style={{ padding: '0 10rem' }}>
                {mayorName && mayorName !== 'None' && (
                    <div style={{ background: C.card, border: `1rem solid ${C.border}`, borderRadius: '6rem', padding: '10rem 12rem', margin: '10rem 0 6rem' }}>
                        <div style={{ fontSize: '13rem', color: C.muted, textTransform: 'uppercase', letterSpacing: '0.5px', marginBottom: '4rem' }}>Current mayor</div>
                        <div style={{ fontWeight: 600, fontSize: '18rem', color: C.text }}>{mayorName}</div>
                    </div>
                )}
                <div style={{ color: C.muted, textAlign: 'center', padding: '16rem', fontSize: '16rem' }}>No election currently. Elections are held every year.</div>
            </div>
        );

    return (
        <div style={{ padding: '0 10rem' }}>
            <div style={{ fontSize: '14rem', color: C.muted, textAlign: 'center', margin: '10rem 0 6rem', paddingBottom: '8rem', borderBottom: `1rem solid ${C.border}` }}>
                {election.hasVoted ? 'You have cast your endorsement.' : 'Citizens have voted. Endorse a candidate to add your influence.'}
            </div>
            <div style={{ margin: '0 0 10rem', padding: '8rem 12rem', background: C.card, borderRadius: '6rem' }}>
                <div style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-between', fontSize: '14rem', marginBottom: '5rem' }}>
                    <div style={{ color: C.muted }}>Voting in progress</div>
                    <div style={{ color: C.cyan }}>{`${Math.round((election.progress || 0) * 100)}% complete`}</div>
                </div>
                <div style={{ height: '4rem', borderRadius: '2rem', background: 'rgba(255,255,255,0.07)' }}>
                    <div style={{ height: '4rem', width: `${Math.min((election.progress || 0) * 100, 100)}%`, borderRadius: '2rem', background: C.cyan }} />
                </div>
            </div>
            {election.candidates?.map((c: any) => {
                const leading = c.votes === Math.max(...election.candidates.map((x: any) => x.votes));
                const votesText = `${c.votes?.toLocaleString()} votes (${c.voteShare?.toFixed(1)}%)`;
                const endorseText = `Endorse ${c.name.split(' ')[0]}`;
                return (
                    <div key={c.name} style={{ background: C.card, borderRadius: '6rem', borderLeft: `3rem solid ${leading ? C.cyan : C.border}`, overflow: 'hidden', marginBottom: '6rem' }}>
                        <div style={{ padding: '10rem 12rem' }}>
                            <div style={{ fontSize: '18rem', fontWeight: 700, color: C.text, marginBottom: '2rem' }}>{c.name}</div>
                            <div style={{ fontSize: '14rem', color: C.muted, marginBottom: '6rem' }}>{`${c.party} · Age ${c.age}`}</div>
                            <div style={{ marginBottom: '6rem' }}>
                                <Pill label={splitCamel(c.specialty)} color={categoryColor(c.specialty)} />
                            </div>
                            <div style={{ fontSize: '13rem', fontStyle: 'italic', color: C.muted2, marginBottom: '8rem' }}>{`"${c.slogan}"`}</div>
                            <div style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' }}>
                                <div style={{ fontSize: '14rem', color: C.muted2 }}>{votesText}</div>
                                {!election.hasVoted && (
                                    <div onClick={() => trigger('civicvoice', 'castVote', c.name)}
                                        style={{ padding: '5rem 14rem', borderRadius: '4rem', background: C.cyan, color: '#fff', fontSize: '14rem', fontWeight: 600, cursor: 'pointer', border: `1rem solid rgba(255,255,255,0.2)` }}>
                                        {endorseText}
                                    </div>
                                )}
                            </div>
                        </div>
                    </div>
                );
            })}
            <div style={{ height: '8rem' }} />
        </div>
    );
}

function StatsTab({ population, happiness, unemployment, votingPopulation }: { population: number; happiness: number; unemployment: number; votingPopulation: number }) {
    function StatRow({ label, value, color }: { label: string; value: string; color?: string }) {
        return (
            <div style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-between', padding: '6rem 0', borderBottom: `1rem solid rgba(255,255,255,0.04)` }}>
                <div style={{ fontSize: '16rem', color: C.muted2 }}>{label}</div>
                <div style={{ fontSize: '16rem', fontWeight: 600, color: color || C.text }}>{value}</div>
            </div>
        );
    }
    return (
        <div style={{ margin: '10rem 10rem', background: C.card, border: `1rem solid ${C.border}`, borderRadius: '6rem', padding: '10rem 12rem' }}>
            <div style={{ fontWeight: 700, letterSpacing: '0.8px', textTransform: 'uppercase', fontSize: '13rem', color: C.cyan, marginBottom: '8rem', paddingBottom: '5rem', borderBottom: `1rem solid ${C.border}` }}>City overview</div>
            <StatRow label="Population" value={population?.toLocaleString()} />
            <StatRow label="Happiness" value={`${happiness?.toFixed(1)}%`} color={happiness > 70 ? C.green : happiness > 50 ? C.amber : C.red} />
            <StatRow label="Unemployment" value={`${unemployment?.toFixed(1)}%`} color={unemployment < 5 ? C.green : unemployment < 10 ? C.amber : C.red} />
            <StatRow label="Eligible voters" value={votingPopulation?.toLocaleString()} />
        </div>
    );
}

export const CivicVoicePanel = () => {
    const [tab, setTab] = useState('proposals');

    const population = useValue(population$);
    const happiness = useValue(happiness$);
    const unemployment = useValue(unemployment$);
    const usedSlots = useValue(usedSlots$);
    const hasElection = useValue(hasElection$);
    const mayorName = useValue(mayorName$);
    const notification = useValue(notification$);
    const proposed = useValue(proposed$);
    const active = useValue(active$);
    const election = useValue(election$);
    const votingPopulation = useValue(votingPopulation$);
    const nextElectionDays = useValue(nextElectionDays$);

    return (
        <div style={{
            width: 'var(--rightPanelWidth)',
            position: 'absolute',
            right: '50rem',
            top: '50rem',
            background: 'rgba(20,26,40,0.75)',
            border: `1rem solid ${C.border2}`,
            borderRadius: '16rem',
            fontFamily: 'var(--fontFamily)',
            fontSize: '20rem',
            color: C.text,
            overflow: 'hidden',
            boxShadow: '0 8rem 40rem rgba(0,0,0,0.6)',
            zIndex: 1000,
        }}>

            {/* Title bar */}
            <div style={{ background: C.titleBar, padding: '13rem 16rem', borderBottom: `1rem solid ${C.border2}`, textAlign: 'center' }}>
                <div style={{ fontSize: '22rem', fontWeight: 700, color: C.text, letterSpacing: '8rem', textTransform: 'uppercase' }}>- Civic Voice -</div>
            </div>

            {/* Meta bar */}
            <div style={{ background: C.metaBar, padding: '8rem 14rem', borderBottom: `1rem solid ${C.border}`, display: 'flex', flexDirection: 'row', justifyContent: 'space-between', fontSize: '14rem', color: C.muted }}>
                <div>Mayor: <span style={{ color: C.amber }}>{mayorName}</span></div>
                <div>Next election: <span style={{ color: C.cyan, fontWeight: 600 }}>{nextElectionDays >= 0 ? `${nextElectionDays}mo` : 'TBD'}</span></div>
                <div>Active: <span style={{ color: C.cyan, fontWeight: 600 }}>{usedSlots} / 7</span></div>
            </div>

            {/* Notification */}
            {notification && (
                <div style={{ background: 'rgba(75,195,212,0.06)', borderLeft: `3rem solid ${C.cyan}`, padding: '7rem 14rem', fontSize: '14rem', color: C.text }}>
                    {notification}
                </div>
            )}

            {/* Election alert */}
            {hasElection && tab !== 'election' && (
                <div onClick={() => setTab('election')} style={{ background: 'rgba(255,203,0,0.06)', borderLeft: `3rem solid ${C.amber}`, padding: '7rem 14rem', fontSize: '14rem', color: C.amber, cursor: 'pointer', display: 'flex', flexDirection: 'row', alignItems: 'center' }}>
                    <div style={{ width: '6rem', height: '6rem', borderRadius: '50%', background: C.amber, marginRight: '6rem' }} />
                    Election underway — tap to view candidates
                </div>
            )}

            <Tabs active={tab} onSelect={setTab} hasElection={hasElection} proposed={proposed} />

            {/* Body */}
            <div style={{ background: C.body }}>
                <Scrollable style={{ maxHeight: '460rem' }}>
                    {tab === 'proposals' && <ProposalsTab proposed={proposed} />}
                    {tab === 'active' && <ActiveTab active={active} />}
                    {tab === 'election' && <ElectionTab election={election} mayorName={mayorName} />}
                    {tab === 'stats' && <StatsTab population={population} happiness={happiness} unemployment={unemployment} votingPopulation={votingPopulation} />}
                </Scrollable>
            </div>

            {/* Footer stats */}
            <div style={{ background: C.footer, padding: '8rem 14rem', borderTop: `1rem solid ${C.border}`, display: 'flex', flexDirection: 'row', justifyContent: 'space-between', fontSize: '13rem', color: C.muted }}>
                <div>Pop <span style={{ color: C.text, fontWeight: 500 }}>{population?.toLocaleString()}</span></div>
                <div>Happiness <span style={{ color: happiness > 70 ? C.green : happiness > 50 ? C.amber : C.red, fontWeight: 500 }}>{happiness?.toFixed(1)}%</span></div>
                <div>Unemployed <span style={{ color: unemployment < 5 ? C.green : unemployment < 10 ? C.amber : C.red, fontWeight: 500 }}>{unemployment?.toFixed(1)}%</span></div>
            </div>

        </div>
    );
};
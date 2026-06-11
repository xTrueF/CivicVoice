// ============================================================
// CivicVoice — Democracy & Governance Mod for Cities: Skylines II
// Created by xTrueF | github.com/xTrueF/CivicVoice
// Licensed under MIT License
// ============================================================
import React, { useState, useEffect } from 'react';

const C = {
    bg: '#16181c',
    surface: '#20232a',
    border: 'rgba(255,255,255,0.07)',
    border2: 'rgba(255,255,255,0.12)',
    text: '#d8dce4',
    muted: '#6b7280',
    muted2: '#8e95a0',
    cyan: '#5cc8d4',
    cyanDim: 'rgba(92,200,212,0.15)',
    green: '#6dc889',
    red: '#e06060',
    amber: '#e0a84a',
    amberDim: 'rgba(224,168,74,0.12)',
    white: '#eef0f4',
};

function useBinding(group, name, defaultVal) {
    const [value, setValue] = useState(defaultVal);
    useEffect(() => {
        if (typeof engine === 'undefined') return;
        const unsub = engine.on(`${group}.${name}`, v => setValue(v));
        engine.trigger(`${group}.get${name.charAt(0).toUpperCase() + name.slice(1)}`);
        return () => unsub && unsub();
    }, []);
    return value;
}

function trigger(group, name, payload) {
    if (typeof engine !== 'undefined')
        engine.trigger(`${group}.${name}`, payload);
}

// ── Sub-components ────────────────────────────────────────────────────────

function Header({ population, happiness, unemployment, usedSlots, mayorName, hasElection, onElectionClick }) {
    return (
        <div style={{ background: C.surface, padding: '12px 14px', borderBottom: `1px solid ${C.border}`, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                <div style={{ width: 8, height: 8, borderRadius: '50%', background: C.cyan, boxShadow: `0 0 6px ${C.cyan}` }} />
                <div>
                    <div style={{ fontSize: 13, fontWeight: 600, color: C.white }}>Civic Voice</div>
                    <div style={{ fontSize: 10, color: C.muted, textTransform: 'uppercase', letterSpacing: '0.6px' }}>Citizen Democracy</div>
                </div>
            </div>
            <div style={{ textAlign: 'right', fontSize: 10, color: C.muted2, lineHeight: 1.7 }}>
                <div>Slots: <span style={{ color: C.cyan, fontWeight: 600 }}>{usedSlots} / 3</span></div>
                <div>Mayor: <span style={{ color: C.amber, fontWeight: 500 }}>{mayorName}</span></div>
            </div>
        </div>
    );
}

function Tabs({ active, onSelect, hasElection }) {
    const tabs = ['proposals', 'active', 'election', 'stats'];
    return (
        <div style={{ display: 'flex', background: C.surface, borderBottom: `1px solid ${C.border}` }}>
            {tabs.map(t => (
                <div key={t} onClick={() => onSelect(t)} style={{
                    flex: 1, padding: '8px 4px', textAlign: 'center', cursor: 'pointer',
                    fontSize: 11, letterSpacing: '0.2px',
                    color: active === t ? C.cyan : C.muted,
                    borderBottom: active === t ? `2px solid ${C.cyan}` : '2px solid transparent',
                    fontWeight: active === t ? 600 : 400,
                }}>
                    {t === 'proposals' && 'Proposals'}
                    {t === 'active' && 'Active'}
                    {t === 'election' && <span>Election{hasElection ? <span style={{ color: C.red, marginLeft: 3 }}>●</span> : null}</span>}
                    {t === 'stats' && 'City Stats'}
                </div>
            ))}
        </div>
    );
}

function VoteBar({ pct, color }) {
    return (
        <div style={{ height: 4, borderRadius: 2, background: 'rgba(255,255,255,0.07)', overflow: 'hidden', margin: '4px 0' }}>
            <div style={{ height: '100%', width: `${Math.min(pct, 100)}%`, background: color, borderRadius: 2 }} />
        </div>
    );
}

function Pill({ label, color }) {
    return (
        <span style={{ display: 'inline-block', padding: '2px 7px', borderRadius: 3, fontSize: 10, fontWeight: 600, marginRight: 4, textTransform: 'uppercase', background: color + '22', color }}>
            {label}
        </span>
    );
}

function categoryColor(cat) {
    const map = { Healthcare: '#e47ca6', Education: '#7db5e0', Transport: '#5cc8d4', Environment: '#6dc889', Economy: '#e0a84a', PublicSafety: '#a78bfa', Leisure: '#1abc9c', Housing: '#8e95a0' };
    return map[cat] || C.cyan;
}

function ProposalsTab({ proposed, usedSlots }) {
    if (!proposed || proposed.length === 0)
        return <div style={{ color: C.muted, textAlign: 'center', padding: 20, fontSize: 12 }}>No proposals yet. Check back after a few in-game days.</div>;

    return proposed.map(p => {
        const cc = categoryColor(p.category);
        const full = usedSlots >= 3;
        return (
            <div key={p.id} style={{ background: C.surface, border: `1px solid ${C.border}`, borderLeft: `2px solid ${C.cyan}`, borderRadius: 4, padding: '10px 12px', marginBottom: 6 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 5 }}>
                    <div><Pill label={p.category} color={cc} /><Pill label={p.type} color={C.muted} /></div>
                    <div style={{ fontSize: 10, color: C.muted }}>{p.deadline} days</div>
                </div>
                <div style={{ fontWeight: 600, fontSize: 12, color: C.white, marginBottom: 3 }}>{p.title}</div>
                <div style={{ fontSize: 11, color: C.muted2, marginBottom: 7 }}>{p.description}</div>
                <div style={{ fontSize: 10, display: 'flex', justifyContent: 'space-between', marginBottom: 3 }}>
                    <span style={{ color: C.green }}>{p.votesFor?.toLocaleString()} for ({p.voteShare?.toFixed(0)}%)</span>
                    <span style={{ color: C.red }}>{p.votesAgainst?.toLocaleString()} against</span>
                </div>
                <VoteBar pct={p.voteShare} color={p.voteShare > 50 ? C.green : C.amber} />
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: 7 }}>
                    <div style={{ fontSize: 10, color: C.muted2 }}>+{p.reward} happiness · -{Math.abs(p.penalty)} if failed</div>
                    <div style={{ display: 'flex', gap: 5 }}>
                        <button onClick={() => trigger('civicvoice', 'rejectProject', p.id)} style={{ padding: '4px 10px', borderRadius: 3, border: `1px solid rgba(224,96,96,0.4)`, background: 'transparent', color: C.red, fontSize: 11, fontWeight: 600, cursor: 'pointer' }}>Reject</button>
                        <button onClick={() => trigger('civicvoice', 'acceptProject', p.id)} disabled={full} style={{ padding: '4px 10px', borderRadius: 3, border: `1px solid rgba(92,200,212,0.4)`, background: 'transparent', color: full ? C.muted : C.cyan, fontSize: 11, fontWeight: 600, cursor: full ? 'default' : 'pointer', opacity: full ? 0.4 : 1 }}>
                            {full ? 'Full' : 'Accept'}
                        </button>
                    </div>
                </div>
            </div>
        );
    });
}

function ActiveTab({ active }) {
    if (!active || active.length === 0)
        return <div style={{ color: C.muted, textAlign: 'center', padding: 20, fontSize: 12 }}>No active projects. Accept proposals from the Proposals tab.</div>;

    return active.map(p => {
        const urgent = p.daysLeft < 20;
        const accent = p.isComplete ? C.green : urgent ? C.red : C.cyan;
        return (
            <div key={p.id} style={{ background: C.surface, border: `1px solid ${C.border}`, borderLeft: `2px solid ${accent}`, borderRadius: 4, padding: '10px 12px', marginBottom: 6 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                    <div style={{ fontWeight: 600, fontSize: 12, color: C.white }}>{p.isComplete ? '✓ ' : ''}{p.title}</div>
                    <div style={{ fontSize: 10, color: urgent ? C.red : C.muted }}>{p.isComplete ? 'Complete!' : `${p.daysLeft}d left`}</div>
                </div>
                <div style={{ fontSize: 11, color: C.muted2, marginBottom: 5 }}>{p.progress}</div>
                <div style={{ height: 3, borderRadius: 2, background: 'rgba(255,255,255,0.07)', overflow: 'hidden' }}>
                    <div style={{ height: '100%', width: `${Math.min(p.progressPct || 0, 100)}%`, background: accent, borderRadius: 2 }} />
                </div>
                <div style={{ fontSize: 10, color: C.muted, marginTop: 4 }}>+{p.reward} happiness on completion</div>
            </div>
        );
    });
}

function ElectionTab({ election, mayorName }) {
    if (!election || !election.isActive)
        return (
            <div>
                {mayorName && mayorName !== 'None' && (
                    <div style={{ background: C.surface, border: `1px solid ${C.border}`, borderRadius: 4, padding: '10px 12px', marginBottom: 8 }}>
                        <div style={{ fontSize: 10, color: C.muted, textTransform: 'uppercase', letterSpacing: '0.5px', marginBottom: 4 }}>Current Mayor</div>
                        <div style={{ fontWeight: 600, color: C.white }}>{mayorName}</div>
                    </div>
                )}
                <div style={{ color: C.muted, textAlign: 'center', padding: 20, fontSize: 12 }}>No election currently. Elections are held every year.</div>
            </div>
        );

    const totalVotes = election.candidates?.reduce((s, c) => s + c.votes, 0) || 1;

    return (
        <div>
            <div style={{ fontSize: 11, color: C.muted, textAlign: 'center', marginBottom: 10, paddingBottom: 8, borderBottom: `1px solid ${C.border}` }}>
                {election.hasVoted ? 'You have cast your endorsement.' : 'Citizens have voted. Endorse a candidate to add your influence.'}
            </div>
            {election.candidates?.map((c, i) => {
                const leading = c.votes === Math.max(...election.candidates.map(x => x.votes));
                return (
                    <div key={c.name} style={{ background: C.surface, border: `1px solid ${C.border}`, borderLeft: `2px solid ${leading ? C.cyan : C.border}`, borderRadius: 4, padding: '10px 12px', marginBottom: 6 }}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 4 }}>
                            <div>
                                <div style={{ fontWeight: 700, fontSize: 13, color: C.white }}>{c.name}</div>
                                <div style={{ fontSize: 10, color: C.muted }}>{c.party} · Age {c.age}</div>
                            </div>
                            <Pill label={c.specialty} color={categoryColor(c.specialty)} />
                        </div>
                        <div style={{ fontStyle: 'italic', fontSize: 11, color: C.muted2, margin: '5px 0 7px' }}>"{c.slogan}"</div>
                        <div style={{ display: 'flex', gap: 10, fontSize: 10, marginBottom: 7 }}>
                            <span style={{ color: C.green }}>+{c.happiness} happiness</span>
                            <span style={{ color: c.tax > 0 ? C.red : C.green }}>{c.tax > 0 ? '+' : ''}{c.tax?.toFixed(0)}% tax</span>
                            <span style={{ color: C.cyan }}>×{c.service?.toFixed(2)} services</span>
                        </div>
                        <VoteBar pct={c.voteShare} color={leading ? C.cyan : C.muted} />
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginTop: 4 }}>
                            <div style={{ fontSize: 10, color: C.muted2 }}>{c.votes?.toLocaleString()} votes ({c.voteShare?.toFixed(1)}%)</div>
                            {!election.hasVoted && (
                                <button onClick={() => trigger('civicvoice', 'castVote', c.name)} style={{ padding: '4px 10px', borderRadius: 3, border: `1px solid rgba(92,200,212,0.4)`, background: 'transparent', color: C.cyan, fontSize: 11, fontWeight: 600, cursor: 'pointer' }}>
                                    Endorse {c.name.split(' ')[0]}
                                </button>
                            )}
                        </div>
                    </div>
                );
            })}
        </div>
    );
}

function StatsTab({ population, happiness, unemployment, completed, failed }) {
    function StatRow({ label, value, color }) {
        return (
            <div style={{ display: 'flex', justifyContent: 'space-between', padding: '4px 0', borderBottom: `1px solid rgba(255,255,255,0.04)`, fontSize: 11 }}>
                <span style={{ color: C.muted2 }}>{label}</span>
                <span style={{ fontWeight: 600, color: color || C.text }}>{value}</span>
            </div>
        );
    }
    return (
        <div>
            <div style={{ background: C.surface, border: `1px solid ${C.border}`, borderRadius: 4, padding: '10px 12px', marginBottom: 6 }}>
                <div style={{ fontSize: 10, fontWeight: 700, letterSpacing: '0.8px', textTransform: 'uppercase', color: C.muted, marginBottom: 8, paddingBottom: 5, borderBottom: `1px solid ${C.border}` }}>City overview</div>
                <StatRow label="Population" value={population?.toLocaleString()} color={C.text} />
                <StatRow label="Happiness" value={`${happiness?.toFixed(1)}%`} color={happiness > 70 ? C.green : happiness > 50 ? C.amber : C.red} />
                <StatRow label="Unemployment" value={`${unemployment?.toFixed(1)}%`} color={unemployment < 5 ? C.green : unemployment < 10 ? C.amber : C.red} />
            </div>
            <div style={{ background: C.surface, border: `1px solid ${C.border}`, borderRadius: 4, padding: '10px 12px' }}>
                <div style={{ fontSize: 10, fontWeight: 700, letterSpacing: '0.8px', textTransform: 'uppercase', color: C.muted, marginBottom: 8, paddingBottom: 5, borderBottom: `1px solid ${C.border}` }}>Project record</div>
                <StatRow label="Completed" value={completed} color={C.green} />
                <StatRow label="Failed" value={failed} color={C.red} />
                <StatRow label="Approval rate" value={completed + failed > 0 ? `${Math.round(completed / (completed + failed) * 100)}%` : 'N/A'} color={C.cyan} />
            </div>
        </div>
    );
}

// ── Main Panel ────────────────────────────────────────────────────────────

function CivicVoicePanel() {
    const [tab, setTab] = useState('proposals');

    const population = useBinding('civicvoice', 'population', 0);
    const happiness = useBinding('civicvoice', 'happiness', 50);
    const unemployment = useBinding('civicvoice', 'unemployment', 5);
    const usedSlots = useBinding('civicvoice', 'usedSlots', 0);
    const hasElection = useBinding('civicvoice', 'hasElection', false);
    const mayorName = useBinding('civicvoice', 'mayorName', 'None');
    const notification = useBinding('civicvoice', 'notification', '');
    const proposed = useBinding('civicvoice', 'proposed', []);
    const active = useBinding('civicvoice', 'active', []);
    const election = useBinding('civicvoice', 'election', null);
    const completed = useBinding('civicvoice', 'totalCompleted', 0);
    const failed = useBinding('civicvoice', 'totalFailed', 0);

    return (
        <div style={{ width: 400, background: C.bg, border: `1px solid ${C.border2}`, borderRadius: 6, fontFamily: 'Inter, Segoe UI, sans-serif', fontSize: 12, color: C.text, overflow: 'hidden', boxShadow: '0 8px 40px rgba(0,0,0,0.6)' }}>

            <Header population={population} happiness={happiness} unemployment={unemployment}
                usedSlots={usedSlots} mayorName={mayorName} hasElection={hasElection} />

            {notification && (
                <div style={{ background: 'rgba(92,200,212,0.08)', borderLeft: `3px solid ${C.cyan}`, padding: '7px 12px', fontSize: 11, color: C.text }}>
                    {notification}
                </div>
            )}

            {hasElection && tab !== 'election' && (
                <div onClick={() => setTab('election')} style={{ background: C.amberDim, borderBottom: `1px solid rgba(224,168,74,0.2)`, padding: '7px 14px', fontSize: 11, color: C.amber, cursor: 'pointer', display: 'flex', alignItems: 'center', gap: 6 }}>
                    <div style={{ width: 6, height: 6, borderRadius: '50%', background: C.amber }} />
                    Election underway — tap to view candidates
                </div>
            )}

            <Tabs active={tab} onSelect={setTab} hasElection={hasElection} />

            <div style={{ padding: '10px 12px', maxHeight: 460, overflowY: 'auto' }}>
                {tab === 'proposals' && <ProposalsTab proposed={proposed} usedSlots={usedSlots} />}
                {tab === 'active' && <ActiveTab active={active} />}
                {tab === 'election' && <ElectionTab election={election} mayorName={mayorName} />}
                {tab === 'stats' && <StatsTab population={population} happiness={happiness} unemployment={unemployment} completed={completed} failed={failed} />}
            </div>

        </div>
    );
}

// Mount to root
const root = document.getElementById('root');
if (root && typeof ReactDOM !== 'undefined') {
    ReactDOM.render(<CivicVoicePanel />, root);
}
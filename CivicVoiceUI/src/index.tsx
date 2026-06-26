import "./fonts/fonts.css";
import { ModRegistrar } from "cs2/modding";
import { FloatingButton, Tooltip } from "cs2/ui";
import { useState, useRef, useEffect } from "react";
import { bindValue, useValue, trigger } from "cs2/api";
import { CivicVoicePanel, CivicVoiceToasts } from "./mods/civic-voice-panel";
import { NewspaperContent, newspaper$ } from "./mods/newspaper/NewspaperHost";
import { NewspaperPayloadActive } from "./mods/newspaper/newspaper-data";
import ballotIcon from "./CIVIC VOICE short logo transparent.png";

const pulseStyle = `
@keyframes civicPulse {
    0% { opacity: 1; }
    50% { opacity: 0.4; }
    100% { opacity: 1; }
}
.civic-pulse { animation: civicPulse 1.2s ease-in-out infinite; }
`;

const useUniversalModMenu$ = bindValue<boolean>("civicvoice", "useUniversalModMenu", false);
const hasElection$ = bindValue<boolean>("civicvoice", "hasElection", false);
const proposed$ = bindValue<any[]>("civicvoice", "proposed", []);

// Newspaper — visibility is pure local state, no trigger needed for close
let globalSetNewspaperVisible: ((v: boolean) => void) | null = null;

const NewspaperPanelHost = () => {
    const payload = useValue(newspaper$);
    const [visible, setVisible] = useState(false);
    globalSetNewspaperVisible = setVisible;

    useEffect(() => {
        if (payload.hasNewspaper) setVisible(true);
    }, [payload.hasNewspaper]);

    const onClose = () => {
        setVisible(false);
        trigger("civicvoice", "closeNewspaper");
    };

    if (!visible || !payload.hasNewspaper) return null;
    return <NewspaperContent payload={payload as NewspaperPayloadActive} onClose={onClose} />;
};

let globalSetPanelVisible: ((v: boolean) => void) | null = null;
let globalSetToastPanelOpen: ((v: boolean) => void) | null = null;
let globalVisible = false;

const CivicVoicePanelHost = () => {
    const [visible, setVisible] = useState(false);
    globalSetPanelVisible = (v: boolean) => {
        setVisible(v);
        globalVisible = v;
        if (globalSetToastPanelOpen) globalSetToastPanelOpen(v);
    };
    return visible ? <CivicVoicePanel /> : null;
};

const CivicVoiceToastHost = () => {
    const [panelOpen, setPanelOpen] = useState(false);
    globalSetToastPanelOpen = setPanelOpen;
    return <CivicVoiceToasts panelOpen={panelOpen} />;
};

const CivicVoiceButton = () => {
    const hasElection = useValue(hasElection$);
    const proposed = useValue(proposed$);
    const hasUrgent = proposed?.some((p: any) => p.tier === "MetricTriggered");
    const shouldPulse = hasElection || hasUrgent;
    return (
        <>
            <style>{pulseStyle}</style>
            <Tooltip tooltip="Civic Voice">
                <div className={shouldPulse ? "civic-pulse" : ""}>
                    <FloatingButton src={ballotIcon} onClick={() => { if (globalSetPanelVisible) globalSetPanelVisible(!globalVisible); }} />
                </div>
            </Tooltip>
        </>
    );
};

const CivicVoiceTopRight = () => {
    const useUMM = useValue(useUniversalModMenu$);
    return !useUMM ? <CivicVoiceButton /> : null;
};

const CivicVoiceUniversalMenu = () => {
    const useUMM = useValue(useUniversalModMenu$);
    return useUMM ? <CivicVoiceButton /> : null;
};

export const register: ModRegistrar = (moduleRegistry) => {
    moduleRegistry.append("GameTopRight", CivicVoiceTopRight);
    moduleRegistry.append("UniversalModMenu", CivicVoiceUniversalMenu);
    moduleRegistry.append("GameTopLeft", CivicVoicePanelHost);
    moduleRegistry.append("GameTopLeft", CivicVoiceToastHost);
    moduleRegistry.append("GameTopLeft", NewspaperPanelHost);
};

export default register;
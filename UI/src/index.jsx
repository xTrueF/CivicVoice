import { ModRegistrar } from "cs2/modding";
import { FloatingButton, Tooltip } from "cs2/ui";
import { useState } from "react";
import { bindValue, useValue } from "cs2/api";
import { CivicVoicePanel } from "./mods/civic-voice-panel";
import ballotIcon from "./ballot.svg";

const useUniversalModMenu$ = bindValue < boolean > ("civicvoice", "useUniversalModMenu", false);

let globalSetVisible: ((v: boolean) => void) | null = null;

const CivicVoicePanelHost = () => {
    const [visible, setVisible] = useState(false);
    globalSetVisible = setVisible;
    return visible ? (
        <div style={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%', pointerEvents: 'none' }}>
            <div style={{ pointerEvents: 'all' }}>
                <CivicVoicePanel />
            </div>
        </div>
    ) : null;
};

const CivicVoiceButton = () => (
    <Tooltip tooltip="Civic Voice">
        <FloatingButton src={ballotIcon} onClick={() => globalSetVisible && globalSetVisible(v => !v)} />
    </Tooltip>
);

const CivicVoiceTopRight = () => {
    const useUMM = useValue(useUniversalModMenu$);
    return !useUMM ? <CivicVoiceButton /> : null;
};

const CivicVoiceUniversalMenu = () => {
    const useUMM = useValue(useUniversalModMenu$);
    return useUMM ? <CivicVoiceButton /> : null;
};

export const register: ModRegistrar = (moduleRegistry) => {
    moduleRegistry.append('GameTopRight', CivicVoiceTopRight);
    moduleRegistry.append('UniversalModMenu', CivicVoiceUniversalMenu);
    moduleRegistry.append('Game', CivicVoicePanelHost);
};

export default register;
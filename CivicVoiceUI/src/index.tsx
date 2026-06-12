import { ModRegistrar } from "cs2/modding";
import { FloatingButton, Tooltip } from "cs2/ui";
import { useState } from "react";
import { bindValue, useValue } from "cs2/api";
import { CivicVoicePanel } from "./mods/civic-voice-panel";
import ballotIcon from "./ballot.svg";

const useUniversalModMenu$ = bindValue<boolean>("civicvoice", "useUniversalModMenu", false);

let globalSetVisible: ((v: boolean) => void) | null = null;
let globalVisible = false;

const CivicVoicePanelHost = () => {
    const [visible, setVisible] = useState(false);
    globalSetVisible = setVisible;
    globalVisible = visible;
    return visible ? <CivicVoicePanel /> : null;
};

const CivicVoiceButton = () => (
    <Tooltip tooltip="Civic Voice">
        <FloatingButton src={ballotIcon} onClick={() => { if (globalSetVisible) globalSetVisible(!globalVisible); }} />
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
    moduleRegistry.append("GameTopRight", CivicVoiceTopRight);
    moduleRegistry.append("UniversalModMenu", CivicVoiceUniversalMenu);
    moduleRegistry.append("GameTopLeft", CivicVoicePanelHost);
};

export default register;

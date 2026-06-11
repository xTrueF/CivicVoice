import { ModRegistrar } from "cs2/modding";
import { FloatingButton, Tooltip } from "cs2/ui";
import { useState } from "react";
import { CivicVoicePanel } from "./mods/civic-voice-panel";
import ballotIcon from "./ballot.svg";

const CivicVoiceApp = () => {
    const [visible, setVisible] = useState(false);

    return (
        <div>
            <Tooltip tooltip="Civic Voice">
                <FloatingButton src={ballotIcon} onClick={() => setVisible(v => !v)} />
            </Tooltip>
            {visible && <CivicVoicePanel />}
        </div>
    );
};

const register: ModRegistrar = (moduleRegistry) => {
    moduleRegistry.append('GameTopRight', CivicVoiceApp);
};

export default register;
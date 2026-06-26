import { bindValue, useValue, trigger } from "cs2/api";
import { Scrollable } from "cs2/ui";
import { NewspaperPayload, NewspaperPayloadActive } from "./newspaper-data";
import { HeraldLayout } from "./HeraldLayout";
import { UproarLayout } from "./UproarLayout";
import { CivicPulseLayout } from "./CivicPulseLayout";

export const newspaper$ = bindValue<NewspaperPayload>("civicvoice", "newspaper", { hasNewspaper: false } as NewspaperPayload);

interface Props {
    payload: NewspaperPayloadActive;
    onClose: () => void;
}

export function NewspaperContent({ payload, onClose }: Props) {
    return (
        <div style={{ position: "relative", width: 0, height: 0, zIndex: 2500 }}>
            {/* Backdrop — bleeds far beyond slot bounds to cover full screen */}
            <div
                style={{
                    position: "absolute",
                    top: "-4000rem", left: "-4000rem",
                    width: "12000rem", height: "12000rem",
                    background: "rgba(10,10,10,0.82)",
                    pointerEvents: "all",
                    zIndex: 0,
                }}
            />
            {/* Paper — centred for Herald (short), top-aligned for Pulse/Uproar (tall) */}
            <div
                onClick={(e) => e.stopPropagation()}
                style={{
                    position: "absolute",
                    top: payload.style === "herald" ? "50vh" : "6vh",
                    left: "50vw",
                    transform: payload.style === "herald" ? "translate(-50%, -50%)" : "translate(-50%, 0)",
                    width: "600rem",
                    zIndex: 1,
                    pointerEvents: "all",
                    maxHeight: "88vh",
                }}
            >
                <Scrollable style={{ maxHeight: "88vh" }}>
                    {payload.style === "herald" && <HeraldLayout payload={payload} onClose={onClose} />}
                    {payload.style === "uproar" && <UproarLayout payload={payload} onClose={onClose} />}
                    {payload.style === "pulse" && <CivicPulseLayout payload={payload} onClose={onClose} />}
                </Scrollable>
            </div>
        </div>
    );
}

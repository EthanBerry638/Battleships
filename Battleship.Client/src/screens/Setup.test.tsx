import { describe, expect, it, vi } from 'vitest';
import { render, screen, fireEvent } from "@testing-library/react";
import Setup from "./Setup";

describe("Setup", () => {
    it("sets the correct dataTransfer payload when drag starts", () => {
        render(<Setup playerId="1" playerName="Player" connectionStatus="connected" />);

        const draggableItem = screen.getByText(/Carrier/i);
        expect(draggableItem).toHaveAttribute("draggable", "true");

        const dataTransfer = {
            setData: vi.fn(),
            effectAllowed: "",
        };

        fireEvent.dragStart(draggableItem, { dataTransfer });

        expect(dataTransfer.setData).toHaveBeenCalledWith("text/plain", "carrier");
        expect(dataTransfer.effectAllowed).toBe("move");
    });
});
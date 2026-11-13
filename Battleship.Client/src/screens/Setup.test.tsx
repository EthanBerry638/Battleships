import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { cleanup, render, screen, fireEvent, waitFor } from "@testing-library/react";
import Setup from "./Setup";
import { clearBoard, placeShip, ShipType, validateFleet } from "../signalR.ts";

vi.mock("../signalR.ts", async (importOriginal) => ({
    ...(await importOriginal<typeof import("../signalR.ts")>()),
    placeShip: vi.fn(),
    clearBoard: vi.fn(),
    validateFleet: vi.fn(),
}));

const shipIds = ["carrier", "battleship", "destroyer", "submarine", "patrol boat"];
const rows = "ABCDE";

async function placeAllShips() {
    for (const [index, shipId] of shipIds.entries()) {
        fireEvent.drop(screen.getByTitle(`${rows[index]}1`), {
            dataTransfer: { getData: () => shipId },
        });

        await waitFor(() =>
            expect(placeShip).toHaveBeenCalledTimes(index + 1),
        );
        await waitFor(() =>
            expect(screen.getByTitle(`${rows[index]}1`)).toHaveClass("cell-occupied"),
        );
    }
}

describe("Setup", () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.mocked(placeShip).mockResolvedValue({ isSuccessful: true, invalidCoordinates: null });
        vi.mocked(clearBoard).mockResolvedValue();
    });

    afterEach(cleanup);

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

    it("only shows the Ready up button once every ship is placed", async () => {
        render(<Setup playerId="1" playerName="Player" connectionStatus="connected" />);

        expect(screen.queryByRole("button", { name: "Ready up" })).not.toBeInTheDocument();

        await placeAllShips();

        expect(screen.getByRole("button", { name: "Ready up" })).toBeInTheDocument();
    });

    it("locks the fleet and confirms when validation succeeds", async () => {
        vi.mocked(validateFleet).mockResolvedValue({
            isValid: true,
            missingShips: [],
            extraShips: [],
        });

        render(<Setup playerId="1" playerName="Player" connectionStatus="connected" />);
        await placeAllShips();

        fireEvent.click(screen.getByRole("button", { name: "Ready up" }));

        expect(await screen.findByText(/you're ready/i)).toBeInTheDocument();
        expect(validateFleet).toHaveBeenCalledWith("1");
        expect(screen.queryByRole("button", { name: "Ready up" })).not.toBeInTheDocument();
        expect(screen.getByRole("button", { name: "Clear Placement" })).toBeDisabled();
        expect(screen.getByRole("button", { name: "Toggle ship orientation" })).toBeDisabled();

        for (const ship of document.querySelectorAll(".draggable-ship")) {
            expect(ship).toHaveAttribute("draggable", "false");
        }
    });

    it("explains which ships are missing or extra when validation fails", async () => {
        vi.mocked(validateFleet).mockResolvedValue({
            isValid: false,
            missingShips: [ShipType.Carrier, ShipType.PatrolBoat],
            extraShips: [ShipType.Destroyer],
        });

        render(<Setup playerId="1" playerName="Player" connectionStatus="connected" />);
        await placeAllShips();

        fireEvent.click(screen.getByRole("button", { name: "Ready up" }));

        const alert = await screen.findByRole("alert");
        expect(alert).toHaveTextContent("Your fleet is not valid.");
        expect(alert).toHaveTextContent("Missing ships: Carrier, Patrol Boat.");
        expect(alert).toHaveTextContent("Extra ships: Destroyer.");
        expect(screen.getByRole("button", { name: "Ready up" })).toBeInTheDocument();
        expect(screen.getByRole("button", { name: "Clear Placement" })).toBeEnabled();
    });
});

import { cleanup, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, describe, expect, it, vi } from "vitest";
import Home from "./Home";

afterEach(() => {
    cleanup();
});

describe("Home", () => {
    it("calls only onCreateGame when Create Game is clicked", async () => {
        const user = userEvent.setup();
        const onCreateGame = vi.fn();
        const onJoinGame = vi.fn();

        render(
            <Home
                playerName="Alice"
                setPlayerName={vi.fn()}
                onCreateGame={onCreateGame}
                onJoinGame={onJoinGame}
            />,
        );

        await user.click(
            screen.getByRole("button", { name: "Create Game" }),
        );

        expect(onCreateGame).toHaveBeenCalledOnce();
        expect(onJoinGame).not.toHaveBeenCalled();
    });

    it("calls only onJoinGame when Join Game is clicked", async () => {
        const user = userEvent.setup();
        const onCreateGame = vi.fn();
        const onJoinGame = vi.fn();

        render(
            <Home
                playerName="Alice"
                setPlayerName={vi.fn()}
                onCreateGame={onCreateGame}
                onJoinGame={onJoinGame}
            />,
        );

        await user.click(
            screen.getByRole("button", { name: "Join Game" }),
        );

        expect(onJoinGame).toHaveBeenCalledOnce();
        expect(onCreateGame).not.toHaveBeenCalled();
    });

    it.each([
        { description: "empty", playerName: "" },
        { description: "whitespace-only", playerName: "   " },
    ])(
        "disables both game actions when the player name is empty",
        async ({ playerName }) => {
            const user = userEvent.setup();
            const onCreateGame = vi.fn();
            const onJoinGame = vi.fn();

            render(
                <Home
                    playerName={playerName}
                    setPlayerName={vi.fn()}
                    onCreateGame={onCreateGame}
                    onJoinGame={onJoinGame}
                />,
            );

            const createButton = screen.getByRole("button", {
                name: "Create Game",
            });
            const joinButton = screen.getByRole("button", {
                name: "Join Game",
            });

            expect(createButton).toBeDisabled();
            expect(joinButton).toBeDisabled();

            await user.click(createButton);
            await user.click(joinButton);

            expect(onCreateGame).not.toHaveBeenCalled();
            expect(onJoinGame).not.toHaveBeenCalled();
        },
    );
});
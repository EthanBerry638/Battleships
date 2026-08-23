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
});
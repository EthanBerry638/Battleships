import {afterEach, beforeEach, describe, expect, it, vi} from 'vitest';
import {cleanup, render, screen} from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import JoinGame from './JoinGame';

afterEach( () => { 
    cleanup()
})

const { invokeMock } = vi.hoisted(() => ({
    invokeMock: vi.fn(),
}));

vi.mock('../signalR', () => ({
    connection: {
        invoke: invokeMock,
    },
}));

describe('JoinGame', () => {
    beforeEach(() => {
        invokeMock.mockReset();
    });

    it('calls onBack when the Back button is clicked', async () => {
        const user = userEvent.setup();
        const onBack = vi.fn();

        render(
            <JoinGame
                playerId="player-1"
                onBack={onBack}
            />
        );

        await user.click(
            screen.getByRole('button', { name: 'Back' })
        );

        expect(onBack).toHaveBeenCalledOnce();
    });

    it('joins the game through SignalR and renders a confirmation message', async () => {
        const user = userEvent.setup();

        invokeMock.mockResolvedValueOnce(true);

        render(
            <JoinGame
                playerId="player-1"
                onBack={vi.fn()}
            />
        );

        await user.type(
            screen.getByPlaceholderText('Game code'),
            'GAME123'
        );

        await user.click(
            screen.getByRole('button', { name: 'Join' })
        );

        expect(invokeMock).toHaveBeenCalledOnce();

        expect(invokeMock).toHaveBeenCalledWith('JoinLobby', {
            gameCode: 'GAME123',
            playerId: 'player-1',
            playerName: 'Player 2',
        });

        expect(
            await screen.findByText(/joined game/i)
        ).toBeInTheDocument();
    });

    it('renders a game-not-found message when joining returns false', async () => {
        const user = userEvent.setup();

        invokeMock.mockResolvedValueOnce(false);

        render(
            <JoinGame
                playerId="player-1"
                onBack={vi.fn()}
            />
        );

        await user.type(
            screen.getByPlaceholderText('Game code'),
            'MISSING'
        );

        await user.click(
            screen.getByRole('button', { name: 'Join' })
        );

        expect(invokeMock).toHaveBeenCalledOnce();

        expect(
            await screen.findByText(/game not found/i)
        ).toBeInTheDocument();
    });

    it('renders an already in session message when joining throws', async () => {
        const user = userEvent.setup();

        invokeMock.mockRejectedValueOnce(
            new Error(
                'You are already in an active lobby or game.'
            )
        );

        render(
            <JoinGame
                playerId="player-1"
                onBack={vi.fn()}
            />
        );

        await user.type(
            screen.getByPlaceholderText('Game code'),
            'GAME123'
        );

        await user.click(
            screen.getByRole('button', { name: 'Join' })
        );

        expect(invokeMock).toHaveBeenCalledOnce();

        expect(
            await screen.findByText(
                /already in an active lobby or game/i
            )
        ).toBeInTheDocument();
    });

    it.each([
        { scenario: 'empty', gameCode: '' },
        { scenario: 'whitespace-only', gameCode: '   ' },
    ])(
        'disables joining and does not call SignalR for a $scenario game code',
        async ({ gameCode }) => {
            const user = userEvent.setup();

            render(
                <JoinGame
                    playerId="player-1"
                    onBack={vi.fn()}
                />
            );

            const gameCodeInput =
                screen.getByPlaceholderText('Game code');

            const joinButton =
                screen.getByRole('button', { name: 'Join' });

            if (gameCode) {
                await user.type(gameCodeInput, gameCode);
            }

            expect(joinButton).toBeDisabled();

            await user.click(joinButton);

            expect(invokeMock).not.toHaveBeenCalled();
        }
    );
});
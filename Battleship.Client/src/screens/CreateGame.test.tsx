import {afterEach, beforeEach, describe, expect, it, vi} from 'vitest';
import {cleanup, render, screen} from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import CreateGame from './CreateGame';
import { connection } from '../signalR';

afterEach(() => {
    cleanup();
});

vi.mock('../signalR', () => ({
    connection: {
        invoke: vi.fn(),
    },
}));

const invokeMock = vi.mocked(connection.invoke);

describe('CreateGame', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('calls onBack when the Back button is clicked', async () => {
        const user = userEvent.setup();
        const onBack = vi.fn();

        render(
            <CreateGame
                playerId="player-123"
                onBack={onBack}
            />
        );

        await user.click(
            screen.getByRole('button', { name: 'Back' })
        );

        expect(onBack).toHaveBeenCalledOnce();
    });

    it('generates and displays the returned game code', async () => {
        const user = userEvent.setup();
        invokeMock.mockResolvedValue('GAME123');

        render(
            <CreateGame
                playerId="player-123"
                onBack={vi.fn()}
            />
        );

        await user.click(
            screen.getByRole('button', { name: 'Generate Game Code' })
        );

        expect(invokeMock).toHaveBeenCalledWith('CreateLobby', {
            playerId: 'player-123',
            playerName: 'Player 1',
        });

        expect(
            await screen.findByText('GAME123')
        ).toBeInTheDocument();

        expect(
            screen.queryByText(
                'Unable to generate a game code. Please try again.'
            )
        ).not.toBeInTheDocument();
    });

    it('displays a generic error and no game code when generation fails', async () => {
        const user = userEvent.setup();

        invokeMock.mockRejectedValue(
            new Error('No game code was returned')
        );

        render(
            <CreateGame
                playerId="player-123"
                onBack={vi.fn()}
            />
        );

        await user.click(
            screen.getByRole('button', { name: 'Generate Game Code' })
        );

        expect(invokeMock).toHaveBeenCalledWith('CreateLobby', {
            playerId: 'player-123',
            playerName: 'Player 1',
        });

        expect(
            await screen.findByText(
                'Unable to generate a game code. Please try again.'
            )
        ).toBeInTheDocument();

        expect(
            screen.queryByText(/Game Code:/i)
        ).not.toBeInTheDocument();
    });

    it('displays already in game error and no game code when already in game/lobby', async () => {
        const user = userEvent.setup();

        invokeMock.mockRejectedValue(
            new Error('You are already in an active lobby or game.')
        );

        render(
            <CreateGame
                playerId="player-123"
                onBack={vi.fn()}
            />
        );

        await user.click(
            screen.getByRole('button', { name: 'Generate Game Code' })
        );

        expect(invokeMock).toHaveBeenCalledWith('CreateLobby', {
            playerId: 'player-123',
            playerName: 'Player 1',
        });

        expect(
            await screen.findByText(
                'You are already in an active lobby or game.'
            )
        ).toBeInTheDocument();

        expect(
            screen.queryByText(/Game Code:/i)
        ).not.toBeInTheDocument();
    });

    it('removes an existing game code when a later request fails', async () => {
        const user = userEvent.setup();

        invokeMock
            .mockResolvedValueOnce('GAME123')
            .mockRejectedValueOnce(new Error('Generation failed'));

        render(
            <CreateGame
                playerId="player-123"
                onBack={vi.fn()}
            />
        );

        const generateButton = screen.getByRole('button', {
            name: 'Generate Game Code',
        });

        await user.click(generateButton);

        expect(
            await screen.findByText('GAME123')
        ).toBeInTheDocument();

        await user.click(generateButton);

        expect(
            await screen.findByText(
                'Unable to generate a game code. Please try again.'
            )
        ).toBeInTheDocument();

        expect(
            screen.queryByText('GAME-123')
        ).not.toBeInTheDocument();
    });
});
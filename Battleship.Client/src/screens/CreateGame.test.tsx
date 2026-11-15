import {afterEach, beforeEach, describe, expect, it, vi} from 'vitest';
import {cleanup, render, screen, waitFor} from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import CreateGame from './CreateGame';
import { connection, leaveLobby } from '../signalR';

afterEach(() => {
    cleanup();
});

vi.mock('../signalR', () => ({
    connection: {
        invoke: vi.fn(),
    },
    leaveLobby: vi.fn(),
}));

const invokeMock = vi.mocked(connection.invoke);
const leaveLobbyMock = vi.mocked(leaveLobby);

function createDeferred<T>() {
    let resolve!: (value: T) => void;
    let reject!: (reason: unknown) => void;
    const promise = new Promise<T>((res, rej) => {
        resolve = res;
        reject = rej;
    });

    return { promise, resolve, reject };
}

function renderCreateGame(onBack = vi.fn()) {
    render(
        <CreateGame
            playerId="player-123"
            playerName="Alice"
            onBack={onBack}
            connectionStatus="connected"
        />
    );

    return {
        generateButton: () => screen.getByRole('button', { name: 'Generate Game Code' }),
        backButton: () => screen.getByRole('button', { name: 'Back' }),
    };
}

describe('CreateGame', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('enables Generate Game Code when connected', () => {
        render(
            <CreateGame
                playerId="player-123"
                playerName="Alice"
                onBack={vi.fn()}
                connectionStatus="connected"
            />
        );

        expect(
            screen.getByRole('button', { name: 'Generate Game Code' })
        ).toBeEnabled();
    });

    it.each([
        'connecting',
        'reconnecting',
        'disconnected',
    ] as const)(
        'disables Generate Game Code when the connection is not connected',
        (connectionStatus) => {
            render(
                <CreateGame
                    playerId="player-123"
                    playerName="Alice"
                    onBack={vi.fn()}
                    connectionStatus={connectionStatus}
                />
            );

            expect(
                screen.getByRole('button', {
                    name: 'Generate Game Code',
                })
            ).toBeDisabled();
        }
    );

    it('calls onBack when the Back button is clicked', async () => {
        const user = userEvent.setup();
        const onBack = vi.fn();

        render(
            <CreateGame
                playerId="player-123"
                playerName="Alice"
                onBack={onBack}
                connectionStatus="connected"
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
                playerName="Alice"
                onBack={vi.fn()}
                connectionStatus="connected"
            />
        );

        await user.click(
            screen.getByRole('button', { name: 'Generate Game Code' })
        );

        expect(invokeMock).toHaveBeenCalledWith('CreateLobby', {
            playerId: 'player-123',
            playerName: 'Alice',
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
                playerName="Alice"
                onBack={vi.fn()}
                connectionStatus="connected"
            />
        );

        await user.click(
            screen.getByRole('button', { name: 'Generate Game Code' })
        );

        expect(invokeMock).toHaveBeenCalledWith('CreateLobby', {
            playerId: 'player-123',
            playerName: 'Alice',
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
                playerName="Alice"
                onBack={vi.fn()}
                connectionStatus="connected"
            />
        );

        await user.click(
            screen.getByRole('button', { name: 'Generate Game Code' })
        );

        expect(invokeMock).toHaveBeenCalledWith('CreateLobby', {
            playerId: 'player-123',
            playerName: 'Alice',
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

    it('disables Generate Game Code once a game code is shown', async () => {
        const user = userEvent.setup();
        invokeMock.mockResolvedValue('GAME123');
        const { generateButton } = renderCreateGame();

        await user.click(generateButton());

        expect(await screen.findByText('GAME123')).toBeInTheDocument();
        expect(generateButton()).toBeDisabled();
    });

    it('re-enables Generate Game Code after a failed request so the user can retry', async () => {
        const user = userEvent.setup();
        invokeMock.mockRejectedValue(new Error('Generation failed'));
        const { generateButton } = renderCreateGame();

        await user.click(generateButton());

        expect(
            await screen.findByText('Unable to generate a game code. Please try again.')
        ).toBeInTheDocument();
        expect(generateButton()).toBeEnabled();
    });

    it('disables Generate Game Code and Back while the request is in progress', async () => {
        const user = userEvent.setup();
        const deferred = createDeferred<string>();
        invokeMock.mockReturnValue(deferred.promise);
        const { generateButton, backButton } = renderCreateGame();

        await user.click(generateButton());

        expect(generateButton()).toBeDisabled();
        expect(backButton()).toBeDisabled();

        deferred.reject(new Error('Generation failed'));

        await waitFor(() => expect(generateButton()).toBeEnabled());
        expect(backButton()).toBeEnabled();
    });

    it('does not leave a lobby when Back is clicked before a game code exists', async () => {
        const user = userEvent.setup();
        const onBack = vi.fn();
        const { backButton } = renderCreateGame(onBack);

        await user.click(backButton());

        expect(leaveLobbyMock).not.toHaveBeenCalled();
        expect(onBack).toHaveBeenCalledOnce();
    });

    it('leaves the lobby before going back when a game code exists', async () => {
        const user = userEvent.setup();
        const onBack = vi.fn();
        invokeMock.mockResolvedValue('GAME123');
        leaveLobbyMock.mockResolvedValue(true);
        const { generateButton, backButton } = renderCreateGame(onBack);

        await user.click(generateButton());
        await screen.findByText('GAME123');
        await user.click(backButton());

        expect(leaveLobbyMock).toHaveBeenCalledOnce();
        expect(onBack).toHaveBeenCalledOnce();
        expect(leaveLobbyMock.mock.invocationCallOrder[0])
            .toBeLessThan(onBack.mock.invocationCallOrder[0]);
    });

    it('stays on the screen when the lobby was not left because an opponent joined first', async () => {
        const user = userEvent.setup();
        const onBack = vi.fn();
        invokeMock.mockResolvedValue('GAME123');
        leaveLobbyMock.mockResolvedValue(false);
        const { generateButton, backButton } = renderCreateGame(onBack);

        await user.click(generateButton());
        await screen.findByText('GAME123');
        await user.click(backButton());

        expect(leaveLobbyMock).toHaveBeenCalledOnce();
        await waitFor(() => expect(backButton()).toBeEnabled());
        expect(onBack).not.toHaveBeenCalled();
    });

    it('still goes back when leaving the lobby fails', async () => {
        const user = userEvent.setup();
        const onBack = vi.fn();
        invokeMock.mockResolvedValue('GAME123');
        leaveLobbyMock.mockRejectedValue(new Error('Connection lost'));
        const { generateButton, backButton } = renderCreateGame(onBack);

        await user.click(generateButton());
        await screen.findByText('GAME123');
        await user.click(backButton());

        expect(leaveLobbyMock).toHaveBeenCalledOnce();
        await waitFor(() => expect(onBack).toHaveBeenCalledOnce());
    });

    it('disables Back while leaving the lobby is in progress', async () => {
        const user = userEvent.setup();
        const onBack = vi.fn();
        const deferred = createDeferred<boolean>();
        invokeMock.mockResolvedValue('GAME123');
        leaveLobbyMock.mockReturnValue(deferred.promise);
        const { generateButton, backButton } = renderCreateGame(onBack);

        await user.click(generateButton());
        await screen.findByText('GAME123');
        await user.click(backButton());

        expect(backButton()).toBeDisabled();
        expect(onBack).not.toHaveBeenCalled();

        deferred.resolve(true);

        await waitFor(() => expect(onBack).toHaveBeenCalledOnce());
    });
});
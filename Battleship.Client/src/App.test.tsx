import { act, cleanup, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import App from './App';
import { type ConnectionStatus } from './signalR';

afterEach(() => {
    cleanup();
    vi.unstubAllGlobals();
});

const {
    startConnectionMock,
    onGameCreatedMock,
    unsubscribeMock,
    onGameStartedMock,
    unsubscribeGameStartedMock,
    onGameNotStartedMock,
    unsubscribeGameNotStartedMock,
    onOpponentDisconnectedMock,
    unsubscribeOpponentDisconnectedMock,
} = vi.hoisted(() => ({
    startConnectionMock: vi.fn(),
    onGameCreatedMock: vi.fn<(handler: unknown) => () => void>(),
    unsubscribeMock: vi.fn<() => void>(),
    onGameStartedMock: vi.fn<(handler: unknown) => () => void>(),
    unsubscribeGameStartedMock: vi.fn<() => void>(),
    onGameNotStartedMock: vi.fn<(handler: unknown) => () => void>(),
    unsubscribeGameNotStartedMock: vi.fn<() => void>(),
    onOpponentDisconnectedMock: vi.fn<(handler: unknown) => () => void>(),
    unsubscribeOpponentDisconnectedMock: vi.fn<() => void>(),
}));

vi.mock('./signalR', () => ({
    startConnection: startConnectionMock,
    onGameCreated: onGameCreatedMock,
    onGameStarted: onGameStartedMock,
    onGameNotStarted: onGameNotStartedMock,
    onOpponentDisconnected: onOpponentDisconnectedMock,
}));

const gameCreated = {
    startingPlayer: { id: 'test-player-id', name: 'Player' },
    players: [
        { id: 'test-player-id', name: 'Player' },
        { id: 'opponent-id', name: 'Alice' },
    ],
};

function emit(mock: typeof onGameCreatedMock, message: unknown = {}) {
    const handler = mock.mock.calls[0]?.[0] as ((message: unknown) => void) | undefined;

    expect(handler).toEqual(expect.any(Function));

    act(() => {
        handler!(message);
    });
}

function setConnectionStatus(status: ConnectionStatus) {
    const onStatusChange = startConnectionMock.mock.calls[0]?.[0] as
        | ((status: ConnectionStatus) => void)
        | undefined;

    expect(onStatusChange).toEqual(expect.any(Function));

    act(() => {
        onStatusChange!(status);
    });
}

vi.mock('./screens/Game', () => ({
    default: ({
                  playerId,
                  opponentName,
                  startingPlayerId,
                  fleetCells,
                  onBackToHome,
              }: {
        playerId: string;
        opponentName: string;
        startingPlayerId: string | null;
        fleetCells: unknown[];
        onBackToHome: () => void;
    }) => (
        <div>
            <h1>Game screen</h1>
            <p>Player ID: {playerId}</p>
            <p>Opponent: {opponentName}</p>
            <p>Starting player: {startingPlayerId}</p>
            <p>Fleet cells: {fleetCells.length}</p>
            <button onClick={onBackToHome}>Back to home</button>
        </div>
    ),
}));

vi.mock('./screens/Home', () => ({
    default: ({
                  onCreateGame,
                  onJoinGame,
              }: {
        onCreateGame: () => void;
        onJoinGame: () => void;
    }) => (
        <div>
            <h1>Home screen</h1>
            <button onClick={onCreateGame}>Create game</button>
            <button onClick={onJoinGame}>Join game</button>
        </div>
    ),
}));

vi.mock('./screens/CreateGame', () => ({
    default: ({
                  playerId,
                  onBack,
              }: {
        playerId: string;
        onBack: () => void;
    }) => (
        <div>
            <h1>Create screen</h1>
            <p>Player ID: {playerId}</p>
            <button onClick={onBack}>Back</button>
        </div>
    ),
}));

vi.mock('./screens/JoinGame', () => ({
    default: ({
                  playerId,
                  onBack,
              }: {
        playerId: string;
        onBack: () => void;
    }) => (
        <div>
            <h1>Join screen</h1>
            <p>Player ID: {playerId}</p>
            <button onClick={onBack}>Back</button>
        </div>
    ),
}));

vi.mock('./screens/Setup', () => ({
    default: ({
                  waitingForOpponent,
                  onFleetLocked,
              }: {
        waitingForOpponent: boolean;
        onFleetLocked: (cells: { x: number; y: number }[]) => void;
    }) => (
        <div>
            <h1>Setup screen</h1>
            <p>Waiting: {String(waitingForOpponent)}</p>
            <button onClick={() => onFleetLocked([{ x: 0, y: 0 }, { x: 1, y: 0 }])}>
                Lock fleet
            </button>
        </div>
    ),
}));

describe('App', () => {
    beforeEach(() => {
        vi.stubGlobal('crypto', {
            randomUUID: vi.fn(() => 'test-player-id'),
        });

        startConnectionMock.mockReset();
        onGameCreatedMock.mockReset();
        unsubscribeMock.mockReset();

        onGameCreatedMock.mockReturnValue(unsubscribeMock);

        onGameStartedMock.mockReset();
        unsubscribeGameStartedMock.mockReset();
        onGameStartedMock.mockReturnValue(unsubscribeGameStartedMock);

        onGameNotStartedMock.mockReset();
        unsubscribeGameNotStartedMock.mockReset();
        onGameNotStartedMock.mockReturnValue(unsubscribeGameNotStartedMock);

        onOpponentDisconnectedMock.mockReset();
        unsubscribeOpponentDisconnectedMock.mockReset();
        onOpponentDisconnectedMock.mockReturnValue(unsubscribeOpponentDisconnectedMock);

        startConnectionMock.mockImplementation(
            async (onStatusChange: (status: ConnectionStatus) => void) => {
                onStatusChange('connected');
            },
        );
    });

    it('starts the SignalR connection when the app mounts', () => {
        render(<App />);

        expect(startConnectionMock).toHaveBeenCalledOnce();
    });

    it('subscribes to GameCreated when the app mounts', () => {
        render(<App />);

        expect(onGameCreatedMock).toHaveBeenCalledOnce();
        expect(onGameCreatedMock).toHaveBeenCalledWith(
            expect.any(Function),
        );
    });

    it('switches to setup when the GameCreated event is received', () => {
        render(<App />);

        const gameCreatedHandler = onGameCreatedMock.mock.calls[0]?.[0] as
            | ((message: unknown) => void)
            | undefined;

        expect(gameCreatedHandler).toEqual(expect.any(Function));

        act(() => {
            gameCreatedHandler!(gameCreated);
        });

        expect(
            screen.getByRole('heading', { name: 'Setup screen' }),
        ).toBeInTheDocument();
    });

    it('unsubscribes from GameCreated when the app unmounts', () => {
        const { unmount } = render(<App />);

        expect(unsubscribeMock).not.toHaveBeenCalled();

        unmount();

        expect(unsubscribeMock).toHaveBeenCalledOnce();
    });

    it('subscribes to GameStarted and GameNotStarted when the app mounts', () => {
        render(<App />);

        expect(onGameStartedMock).toHaveBeenCalledOnce();
        expect(onGameNotStartedMock).toHaveBeenCalledOnce();
    });

    it('switches to the game screen when the GameStarted event is received', () => {
        render(<App />);

        emit(onGameCreatedMock, gameCreated);
        emit(onGameStartedMock);

        expect(
            screen.getByRole('heading', { name: 'Game screen' }),
        ).toBeInTheDocument();
    });

    it('passes the starting player and the locked fleet to the game screen', async () => {
        const user = userEvent.setup();

        render(<App />);

        emit(onGameCreatedMock, gameCreated);
        await user.click(screen.getByRole('button', { name: 'Lock fleet' }));
        emit(onGameStartedMock, {
            isStarted: true,
            startingPlayerId: 'opponent-id',
            playerIds: ['test-player-id', 'opponent-id'],
        });

        expect(screen.getByText('Player ID: test-player-id')).toBeInTheDocument();
        expect(screen.getByText('Starting player: opponent-id')).toBeInTheDocument();
        expect(screen.getByText('Fleet cells: 2')).toBeInTheDocument();
    });

    it('passes the name of the other player to the game screen', () => {
        render(<App />);

        emit(onGameCreatedMock, gameCreated);
        emit(onGameStartedMock);

        expect(screen.getByText('Opponent: Alice')).toBeInTheDocument();
    });

    it('forgets the opponent name once the player goes back to home', async () => {
        const user = userEvent.setup();

        render(<App />);

        emit(onGameCreatedMock, gameCreated);
        emit(onGameStartedMock);
        await user.click(screen.getByRole('button', { name: 'Back to home' }));

        emit(onGameCreatedMock, { ...gameCreated, players: [] });
        emit(onGameStartedMock);

        expect(screen.getByText('Opponent: Opponent')).toBeInTheDocument();
    });

    it('returns to home with the same player ID and a cleared game when Back to home is clicked', async () => {
        const user = userEvent.setup();

        render(<App />);

        emit(onGameCreatedMock, gameCreated);
        await user.click(screen.getByRole('button', { name: 'Lock fleet' }));
        emit(onGameNotStartedMock);
        emit(onGameStartedMock, { isStarted: true, startingPlayerId: 'opponent-id', playerIds: null });
        
        vi.mocked(crypto.randomUUID).mockReturnValue('fresh-player-id-0-0');
        await user.click(screen.getByRole('button', { name: 'Back to home' }));

        expect(screen.getByRole('heading', { name: 'Home screen' })).toBeInTheDocument();

        emit(onGameCreatedMock, gameCreated);

        expect(screen.getByText('Waiting: false')).toBeInTheDocument();

        emit(onGameStartedMock, { isStarted: true, startingPlayerId: null, playerIds: null });

        expect(screen.getByText('Player ID: test-player-id')).toBeInTheDocument();
        expect(screen.getByText('Starting player:')).toBeInTheDocument();
        expect(screen.getByText('Fleet cells: 0')).toBeInTheDocument();
    });

    it('stays on setup and marks the player as waiting when GameNotStarted is received', () => {
        render(<App />);

        emit(onGameCreatedMock, gameCreated);

        expect(screen.getByText('Waiting: false')).toBeInTheDocument();

        emit(onGameNotStartedMock);

        expect(
            screen.getByRole('heading', { name: 'Setup screen' }),
        ).toBeInTheDocument();
        expect(screen.getByText('Waiting: true')).toBeInTheDocument();
    });

    it('unsubscribes from GameStarted and GameNotStarted when the app unmounts', () => {
        const { unmount } = render(<App />);

        unmount();

        expect(unsubscribeGameStartedMock).toHaveBeenCalledOnce();
        expect(unsubscribeGameNotStartedMock).toHaveBeenCalledOnce();
    });

    describe('opponent disconnect', () => {
        it('subscribes to OpponentDisconnected when the app mounts', () => {
            render(<App />);

            expect(onOpponentDisconnectedMock).toHaveBeenCalledOnce();
            expect(onOpponentDisconnectedMock).toHaveBeenCalledWith(expect.any(Function));
        });

        it('unsubscribes from OpponentDisconnected when the app unmounts', () => {
            const { unmount } = render(<App />);

            unmount();

            expect(unsubscribeOpponentDisconnectedMock).toHaveBeenCalledOnce();
        });

        it('replaces setup with the disconnect notice when the opponent disconnects', () => {
            render(<App />);

            emit(onGameCreatedMock, gameCreated);
            emit(onOpponentDisconnectedMock);

            expect(
                screen.getByRole('heading', { name: 'Alice disconnected' }),
            ).toBeInTheDocument();
            expect(
                screen.queryByRole('heading', { name: 'Setup screen' }),
            ).not.toBeInTheDocument();
        });

        it('replaces the game screen with the disconnect notice when the opponent disconnects', () => {
            render(<App />);

            emit(onGameCreatedMock, gameCreated);
            emit(onGameStartedMock, { isStarted: true, startingPlayerId: 'test-player-id', playerIds: null });
            emit(onOpponentDisconnectedMock);

            expect(
                screen.getByRole('heading', { name: 'Alice disconnected' }),
            ).toBeInTheDocument();
            expect(
                screen.queryByRole('heading', { name: 'Game screen' }),
            ).not.toBeInTheDocument();
        });

        it('returns home and starts the next game cleanly when Back to home is clicked', async () => {
            const user = userEvent.setup();

            render(<App />);

            emit(onGameCreatedMock, gameCreated);
            await user.click(screen.getByRole('button', { name: 'Lock fleet' }));
            emit(onGameNotStartedMock);
            emit(onOpponentDisconnectedMock);

            await user.click(screen.getByRole('button', { name: 'Back to home' }));

            expect(screen.getByRole('heading', { name: 'Home screen' })).toBeInTheDocument();

            emit(onGameCreatedMock, gameCreated);

            expect(screen.getByRole('heading', { name: 'Setup screen' })).toBeInTheDocument();
            expect(screen.getByText('Waiting: false')).toBeInTheDocument();

            emit(onGameStartedMock, { isStarted: true, startingPlayerId: null, playerIds: null });

            expect(screen.getByText('Fleet cells: 0')).toBeInTheDocument();
        });

        it('ignores the event outside a game and does not carry it into the next game', () => {
            render(<App />);

            emit(onOpponentDisconnectedMock);

            expect(screen.getByRole('heading', { name: 'Home screen' })).toBeInTheDocument();
            expect(
                screen.queryByRole('heading', { name: 'Alice disconnected' }),
            ).not.toBeInTheDocument();

            emit(onGameCreatedMock, gameCreated);

            expect(screen.getByRole('heading', { name: 'Setup screen' })).toBeInTheDocument();
        });
    });

    it('renders Home when connected', () => {
        render(<App />);

        expect(
            screen.getByRole('heading', { name: 'Home screen' }),
        ).toBeInTheDocument();
    });

    it('displays a loading indicator while connecting', () => {
        startConnectionMock.mockImplementation(async () => {
        });

        render(<App />);

        expect(screen.getByRole('status')).toHaveTextContent(
            'Connecting to API',
        );
        
        expect(
            screen.getByRole('heading', { name: 'Home screen' }),
        ).toBeInTheDocument();
    });

    it('displays a loading indicator while reconnecting', () => {
        startConnectionMock.mockImplementation(
            async (onStatusChange: (status: ConnectionStatus) => void) => {
                onStatusChange('reconnecting');
            },
        );

        render(<App />);

        expect(screen.getByRole('status')).toHaveTextContent(
            'Reconnecting to API',
        );
        
        expect(
            screen.getByRole('heading', { name: 'Home screen' }),
        ).toBeInTheDocument();
    });

    it('asks the player to try again later when the connection could not be made', () => {
        startConnectionMock.mockImplementation(
            async (onStatusChange: (status: ConnectionStatus) => void) => {
                onStatusChange('disconnected');
            },
        );

        render(<App />);

        expect(screen.getByRole('alert')).toHaveTextContent(
            'Could not connect. Please try again later.',
        );
        expect(screen.queryByRole('status')).not.toBeInTheDocument();
        expect(
            screen.getByRole('heading', { name: 'Home screen' }),
        ).toBeInTheDocument();
    });

    describe('connection lost', () => {
        it.each<ConnectionStatus>(['reconnecting', 'disconnected'])(
            'replaces the game with the connection lost notice when the status becomes %s',
            (status) => {
                render(<App />);

                emit(onGameCreatedMock, gameCreated);
                emit(onGameStartedMock);
                setConnectionStatus(status);

                expect(
                    screen.getByRole('heading', { name: 'Connection lost' }),
                ).toBeInTheDocument();
                expect(
                    screen.queryByRole('heading', { name: 'Game screen' }),
                ).not.toBeInTheDocument();
            },
        );

        it('keeps showing the notice after the connection is restored', () => {
            render(<App />);

            emit(onGameCreatedMock, gameCreated);
            setConnectionStatus('reconnecting');
            setConnectionStatus('connected');

            expect(
                screen.getByRole('heading', { name: 'Connection lost' }),
            ).toBeInTheDocument();
        });

        it('replaces the create screen with the notice', async () => {
            const user = userEvent.setup();

            render(<App />);

            await user.click(screen.getByRole('button', { name: 'Create game' }));
            setConnectionStatus('reconnecting');

            expect(
                screen.getByRole('heading', { name: 'Connection lost' }),
            ).toBeInTheDocument();
        });

        it('stays on home when the connection drops there', () => {
            render(<App />);

            setConnectionStatus('reconnecting');
            setConnectionStatus('connected');

            expect(screen.getByRole('heading', { name: 'Home screen' })).toBeInTheDocument();
            expect(
                screen.queryByRole('heading', { name: 'Connection lost' }),
            ).not.toBeInTheDocument();
        });

        it('returns home and starts the next game cleanly when Back to home is clicked', async () => {
            const user = userEvent.setup();

            render(<App />);

            emit(onGameCreatedMock, gameCreated);
            await user.click(screen.getByRole('button', { name: 'Lock fleet' }));
            setConnectionStatus('reconnecting');
            setConnectionStatus('connected');

            await user.click(screen.getByRole('button', { name: 'Back to home' }));

            expect(screen.getByRole('heading', { name: 'Home screen' })).toBeInTheDocument();

            emit(onGameCreatedMock, gameCreated);
            emit(onGameStartedMock, { isStarted: true, startingPlayerId: null, playerIds: null });

            expect(screen.getByText('Fleet cells: 0')).toBeInTheDocument();
        });
    });

    it('switches from home to create when Create game is clicked', async () => {
        const user = userEvent.setup();

        render(<App />);

        await user.click(
            screen.getByRole('button', { name: 'Create game' }),
        );

        expect(
            screen.getByRole('heading', { name: 'Create screen' }),
        ).toBeInTheDocument();
    });

    it('passes the generated player ID to CreateGame', async () => {
        const user = userEvent.setup();

        render(<App />);

        await user.click(
            screen.getByRole('button', { name: 'Create game' }),
        );

        expect(
            screen.getByText('Player ID: test-player-id'),
        ).toBeInTheDocument();
    });

    it('switches from home to join when Join game is clicked', async () => {
        const user = userEvent.setup();

        render(<App />);

        await user.click(
            screen.getByRole('button', { name: 'Join game' }),
        );

        expect(
            screen.getByRole('heading', { name: 'Join screen' }),
        ).toBeInTheDocument();
    });

    it('passes the generated player ID to JoinGame', async () => {
        const user = userEvent.setup();

        render(<App />);

        await user.click(
            screen.getByRole('button', { name: 'Join game' }),
        );

        expect(
            screen.getByText('Player ID: test-player-id'),
        ).toBeInTheDocument();
    });

    it('switches from join back to home when Back is clicked', async () => {
        const user = userEvent.setup();

        render(<App />);

        await user.click(
            screen.getByRole('button', { name: 'Join game' }),
        );

        await user.click(
            screen.getByRole('button', { name: 'Back' }),
        );

        expect(
            screen.getByRole('heading', { name: 'Home screen' }),
        ).toBeInTheDocument();
    });

    it('switches from create back to home when Back is clicked', async () => {
        const user = userEvent.setup();

        render(<App />);

        await user.click(
            screen.getByRole('button', { name: 'Create game' }),
        );

        await user.click(
            screen.getByRole('button', { name: 'Back' }),
        );

        expect(
            screen.getByRole('heading', { name: 'Home screen' }),
        ).toBeInTheDocument();
    });
});
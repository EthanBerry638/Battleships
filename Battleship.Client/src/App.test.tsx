import { act, cleanup, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import App from './App';
import { type ConnectionStatus } from './signalR';

afterEach(() => {
    cleanup();
    vi.unstubAllGlobals();
});

const {startConnectionMock, onGameCreatedMock, unsubscribeMock,
} = vi.hoisted(() => ({
    startConnectionMock: vi.fn(),
    onGameCreatedMock: vi.fn<(handler: unknown) => () => void>(),
    unsubscribeMock: vi.fn<() => void>(),
}));

vi.mock('./signalR', () => ({
    startConnection: startConnectionMock,
    onGameCreated: onGameCreatedMock,
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
    default: () => (
        <div>
            <h1>Setup screen</h1>
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
            | (() => void)
            | undefined;

        expect(gameCreatedHandler).toEqual(expect.any(Function));

        act(() => {
            gameCreatedHandler!();
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

    it('displays an error when disconnected', () => {
        startConnectionMock.mockImplementation(
            async (onStatusChange: (status: ConnectionStatus) => void) => {
                onStatusChange('disconnected');
            },
        );

        render(<App />);

        expect(screen.getByRole('alert')).toHaveTextContent(
            'Unable to connect to the API',
        );
        
        expect(
            screen.queryByRole('heading', { name: 'Home screen' }),
        ).toBeInTheDocument();
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
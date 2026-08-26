import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { ConnectionStatus } from './signalR';
import { startConnection } from './signalR';

const signalRMock = vi.hoisted(() => {
    const handlers = {
        reconnecting: undefined as (() => void) | undefined,
        reconnected: undefined as (() => void) | undefined,
        close: undefined as (() => void) | undefined,
    };

    const connection = {
        state: 'Disconnected',
        start: vi.fn<() => Promise<void>>().mockResolvedValue(undefined),
        onreconnecting: vi.fn((handler: () => void) => {
            handlers.reconnecting = handler;
        }),
        onreconnected: vi.fn((handler: () => void) => {
            handlers.reconnected = handler;
        }),
        onclose: vi.fn((handler: () => void) => {
            handlers.close = handler;
        }),
    };

    return { connection, handlers };
});

vi.mock('@microsoft/signalr', () => ({
    HubConnectionState: {
        Disconnected: 'Disconnected',
        Connected: 'Connected',
    },
    HubConnectionBuilder: class {
        withUrl() {
            return this;
        }

        withAutomaticReconnect() {
            return this;
        }

        build() {
            return signalRMock.connection;
        }
    },
}));

describe('startConnection', () => {
    beforeEach(() => {
        signalRMock.connection.state = 'Disconnected';
        signalRMock.connection.start.mockReset();
        signalRMock.connection.start.mockResolvedValue(undefined);
        signalRMock.connection.onreconnecting.mockClear();
        signalRMock.connection.onreconnected.mockClear();
        signalRMock.connection.onclose.mockClear();

        signalRMock.handlers.reconnecting = undefined;
        signalRMock.handlers.reconnected = undefined;
        signalRMock.handlers.close = undefined;
    });

    it('does not start the connection when it is already started', async () => {
        signalRMock.connection.state = 'Connected';
        const onStatusChange = vi.fn<(status: ConnectionStatus) => void>();

        await startConnection(onStatusChange);

        expect(signalRMock.connection.start).not.toHaveBeenCalled();
        expect(onStatusChange).not.toHaveBeenCalled();
    });

    it('reports connecting and connected when the connection starts', async () => {
        const onStatusChange = vi.fn<(status: ConnectionStatus) => void>();

        await startConnection(onStatusChange);

        expect(signalRMock.connection.start).toHaveBeenCalledOnce();
        expect(onStatusChange.mock.calls).toEqual([
            ['connecting'],
            ['connected'],
        ]);
    });

    it('reports disconnected when starting the connection fails', async () => {
        signalRMock.connection.start.mockRejectedValueOnce(
            new Error('Connection failed'),
        );
        const onStatusChange = vi.fn<(status: ConnectionStatus) => void>();

        await startConnection(onStatusChange);

        expect(onStatusChange.mock.calls).toEqual([
            ['connecting'],
            ['disconnected'],
        ]);
    });

    it('reports reconnecting when SignalR starts reconnecting', async () => {
        const onStatusChange = vi.fn<(status: ConnectionStatus) => void>();

        await startConnection(onStatusChange);
        onStatusChange.mockClear();

        signalRMock.handlers.reconnecting?.();

        expect(onStatusChange).toHaveBeenCalledOnce();
        expect(onStatusChange).toHaveBeenCalledWith('reconnecting');
    });

    it('reports connected when SignalR reconnects', async () => {
        const onStatusChange = vi.fn<(status: ConnectionStatus) => void>();

        await startConnection(onStatusChange);
        onStatusChange.mockClear();

        signalRMock.handlers.reconnected?.();

        expect(onStatusChange).toHaveBeenCalledOnce();
        expect(onStatusChange).toHaveBeenCalledWith('connected');
    });

    it('reports disconnected when SignalR closes', async () => {
        const onStatusChange = vi.fn<(status: ConnectionStatus) => void>();

        await startConnection(onStatusChange);
        onStatusChange.mockClear();

        signalRMock.handlers.close?.();

        expect(onStatusChange).toHaveBeenCalledOnce();
        expect(onStatusChange).toHaveBeenCalledWith('disconnected');
    });

    it('registers all connection lifecycle handlers', async () => {
        const onStatusChange = vi.fn<(status: ConnectionStatus) => void>();

        await startConnection(onStatusChange);

        expect(signalRMock.connection.onreconnecting).toHaveBeenCalledOnce();
        expect(signalRMock.connection.onreconnected).toHaveBeenCalledOnce();
        expect(signalRMock.connection.onclose).toHaveBeenCalledOnce();
    });
});
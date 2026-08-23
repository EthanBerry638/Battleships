import { beforeEach, describe, expect, it, vi } from 'vitest';

const signalRMock = vi.hoisted(() => {
    const connection = {
        state: 'Disconnected',
        start: vi.fn<() => Promise<void>>().mockResolvedValue(undefined),
    };

    return { connection };
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

import { startConnection } from './signalR';

describe('startConnection', () => {
    beforeEach(() => {
        signalRMock.connection.state = 'Disconnected';
        signalRMock.connection.start.mockClear();
    });

    it('does not start the connection when it is already started', async () => {
        signalRMock.connection.state = 'Connected';

        await startConnection();

        expect(signalRMock.connection.start).not.toHaveBeenCalled();
    });

    it('starts the connection when it is disconnected', async () => {
        signalRMock.connection.state = 'Disconnected';

        await startConnection();

        expect(signalRMock.connection.start).toHaveBeenCalledOnce();
    });
});
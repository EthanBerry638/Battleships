import * as signalR from '@microsoft/signalr';

export type ConnectionStatus = 'connecting' | 'connected' | 'reconnecting' | 'disconnected';

export const connection = new signalR.HubConnectionBuilder()
    .withUrl('https://localhost:7146/gameHub')
    .withAutomaticReconnect()
    .build();

export type Player = {
    id: string;
    name: string;
}

export type GameCreatedMessage = {
    player: Player;
    player1Id: string;
    player2Id: string;
}

export async function startConnection(
    onStatusChange: (status: ConnectionStatus) => void) {
    connection.onreconnecting(() => {
        onStatusChange('reconnecting');
    });

    connection.onreconnected(() => {
        onStatusChange('connected');
    });

    connection.onclose(() => {
        onStatusChange('disconnected');
    });

    if (connection.state === signalR.HubConnectionState.Disconnected) {
        try {
            onStatusChange('connecting');

            await connection.start();

            onStatusChange('connected');
        } catch {
            onStatusChange('disconnected');
        }
    }
}
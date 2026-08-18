import * as signalR from '@microsoft/signalr';

export const connection = new signalR.HubConnectionBuilder()
    .withUrl('https://localhost:7146/gameHub')
    .withAutomaticReconnect()
    .build();

export async function startConnection() {
  if (connection.state === signalR.HubConnectionState.Disconnected) {
    await connection.start();
    }
}
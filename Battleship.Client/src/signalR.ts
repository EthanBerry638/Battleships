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

export const ShipType = {
    Carrier: 0,
    Battleship: 1,
    Destroyer: 2,
    Submarine: 3,
    PatrolBoat: 4,
} as const;

export type ShipType = (typeof ShipType)[keyof typeof ShipType];

export type Coordinate = {
    x: number;
    y: number;
}

export type PlaceShipRequest = {
    playerId: string;
    type: ShipType;
    coordinates: Coordinate[];
}

export type PlacementResult = {
    isSuccessful: boolean;
    invalidCoordinates: Coordinate[] | null;
}

export function isSameCoordinate(a: Coordinate, b: Coordinate): boolean {
    return a.x === b.x && a.y === b.y;
}

export const rowLabels = 'ABCDEFGHIJ';

export function formatCoordinate({ x, y }: Coordinate): string {
    return `${rowLabels[y]}${x + 1}`;
}

export function placeShip(request: PlaceShipRequest): Promise<PlacementResult> {
    return connection.invoke<PlacementResult>('PlaceShip', request);
}

export function onGameCreated(handler: (response: GameCreatedMessage) => void){
    connection.on('GameCreated', handler);

    return () => {
        connection.off('GameCreated', handler);
    };
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

export function clearBoard(playerId: string): Promise<void> {
    return connection.invoke('ClearBoard', { playerId });
}
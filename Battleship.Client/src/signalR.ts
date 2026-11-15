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
    startingPlayer: Player;
    players: Player[];
}

export type StartGameMessage = {
    isStarted: boolean;
    startingPlayerId: string | null;
    playerIds: string[] | null;
}

export type FleetValidationResult = {
    isValid: boolean;
    missingShips: ShipType[];
    extraShips: ShipType[];
};

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
    type: ShipType;
    coordinates: Coordinate[];
}

export type PlacementResult = {
    isSuccessful: boolean;
    invalidCoordinates: Coordinate[] | null;
}

export const ShotResult = {
    Miss: 0,
    Hit: 1,
    Sunk: 2,
    Duplicate: 3,
} as const;

export type ShotResult = (typeof ShotResult)[keyof typeof ShotResult];

export type ShotMessage = {
    result: ShotResult;
    shooterId: string;
    coordinate: Coordinate;
    sunkCoordinates: Coordinate[] | null;
    isGameOver: boolean;
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

export function onGameStarted(handler: (message: StartGameMessage) => void) {
    connection.on('GameStarted', handler);

    return () => {
        connection.off('GameStarted', handler);
    };
}

export function onGameNotStarted(handler: (message: StartGameMessage) => void) {
    connection.on('GameNotStarted', handler);

    return () => {
        connection.off('GameNotStarted', handler);
    };
}

export function onOpponentDisconnected(handler: () => void) {
    connection.on('OpponentDisconnected', handler);

    return () => {
        connection.off('OpponentDisconnected', handler);
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
export function clearBoard(): Promise<void> {
    return connection.invoke('ClearBoard');
}

export function validateFleet(): Promise<FleetValidationResult> {
    return connection.invoke<FleetValidationResult>("ValidateFleet")
}

export function tryStartGame(): Promise<void> {
    return connection.invoke('TryStartGame');
}

export function leaveLobby(): Promise<void> {
    return connection.invoke('LeaveLobby');
}

export function shoot(coordinate: Coordinate): Promise<void> {
    return connection.invoke('Shoot', { coordinate });
}

export function onShot(handler: (message: ShotMessage) => void) {
    connection.on('Shot', handler);

    return () => {
        connection.off('Shot', handler);
    };
}
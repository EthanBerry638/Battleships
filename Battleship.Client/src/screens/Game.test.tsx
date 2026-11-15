import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { act, cleanup, fireEvent, render, screen, within } from '@testing-library/react';
import Game from './Game';
import { onShot, shoot, ShotResult, type ShotMessage } from '../signalR.ts';

vi.mock('../signalR.ts', async (importOriginal) => ({
    ...(await importOriginal<typeof import('../signalR.ts')>()),
    shoot: vi.fn(),
    onShot: vi.fn(),
}));

const me = 'me';
const opponent = 'opponent';
const unsubscribe = vi.fn();
const onBackToHome = vi.fn();

function renderGame(props: Partial<Parameters<typeof Game>[0]> = {}) {
    return render(
        <Game
            playerId={me}
            opponentName="Alice"
            connectionStatus="connected"
            startingPlayerId={me}
            fleetCells={[{ x: 0, y: 0 }, { x: 1, y: 0 }]}
            onBackToHome={onBackToHome}
            {...props}
        />,
    );
}

function emitShot(
    message: Omit<ShotMessage, 'isGameOver' | 'sunkCoordinates'> & Partial<ShotMessage>,
) {
    const handler = vi.mocked(onShot).mock.calls[0]![0];

    act(() => handler({ isGameOver: false, sunkCoordinates: null, ...message }));
}

const ownBoard = () => within(screen.getByRole('region', { name: 'Your fleet' }));
const opponentBoard = () => within(screen.getByRole('region', { name: "Alice's waters" }));
const turn = () => screen.getAllByRole('status')[0];

describe('Game', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.mocked(shoot).mockResolvedValue();
        vi.mocked(onShot).mockReturnValue(unsubscribe);
    });

    afterEach(cleanup);

    it('shows own ships on the own board only', () => {
        renderGame();

        expect(ownBoard().getByTitle('A1')).toHaveClass('cell-occupied');
        expect(ownBoard().getByTitle('A2')).toHaveClass('cell-occupied');
        expect(ownBoard().getByTitle('A3')).not.toHaveClass('cell-occupied');
        expect(document.querySelectorAll('.cell-occupied')).toHaveLength(2);
    });

    it('lets the starting player shoot and leaves the opponent board undimmed', () => {
        renderGame();

        expect(turn()).toHaveTextContent('Your turn');
        expect(document.querySelector('.board-dimmed')).toBeNull();

        fireEvent.click(opponentBoard().getByTitle('B4'));

        expect(shoot).toHaveBeenCalledExactlyOnceWith({ x: 3, y: 1 });
    });

    it('dims the opponent board and blocks shooting when it is not your turn', () => {
        renderGame({ startingPlayerId: opponent });

        expect(turn()).toHaveTextContent("Alice's turn");
        expect(opponentBoard().getByTitle('B4').closest('.board')).toHaveClass('board-dimmed');
        expect(ownBoard().getByTitle('B4').closest('.board')).not.toHaveClass('board-dimmed');

        fireEvent.click(opponentBoard().getByTitle('B4'));

        expect(shoot).not.toHaveBeenCalled();
    });

    it('does not mark the board until the server confirms the shot', async () => {
        renderGame();

        await act(async () => {
            fireEvent.click(opponentBoard().getByTitle('B4'));
        });

        expect(opponentBoard().getByTitle('B4')).not.toHaveClass('cell-hit');
        expect(opponentBoard().getByTitle('B4')).not.toHaveClass('cell-miss');
    });

    it('marks my shot on the opponent board and passes the turn', () => {
        renderGame();

        emitShot({ result: ShotResult.Hit, shooterId: me, coordinate: { x: 3, y: 1 } });

        expect(opponentBoard().getByTitle('B4')).toHaveClass('cell-hit');
        expect(ownBoard().getByTitle('B4')).not.toHaveClass('cell-hit');
        expect(turn()).toHaveTextContent("Alice's turn");
        expect(screen.getByText('You hit a ship at B4.')).toBeInTheDocument();

        fireEvent.click(opponentBoard().getByTitle('C1'));

        expect(shoot).not.toHaveBeenCalled();
    });

    it('marks the opponent shot on my board and gives me the turn', () => {
        renderGame({ startingPlayerId: opponent });

        emitShot({ result: ShotResult.Miss, shooterId: opponent, coordinate: { x: 2, y: 0 } });

        expect(ownBoard().getByTitle('A3')).toHaveClass('cell-miss');
        expect(opponentBoard().getByTitle('A3')).not.toHaveClass('cell-miss');
        expect(turn()).toHaveTextContent('Your turn');
        expect(document.querySelector('.board-dimmed')).toBeNull();
    });

    it('shows a sunk result as a hit', () => {
        renderGame({ startingPlayerId: opponent });

        emitShot({ result: ShotResult.Sunk, shooterId: opponent, coordinate: { x: 0, y: 0 } });

        expect(ownBoard().getByTitle('A1')).toHaveClass('cell-occupied', 'cell-hit');
        expect(screen.getByText('Alice sank a ship at A1.')).toBeInTheDocument();
    });

    it('marks every cell of a ship I sank on the opponent board as sunk', () => {
        renderGame();

        emitShot({ result: ShotResult.Hit, shooterId: me, coordinate: { x: 3, y: 1 } });
        emitShot({ result: ShotResult.Miss, shooterId: opponent, coordinate: { x: 5, y: 5 } });
        emitShot({
            result: ShotResult.Sunk,
            shooterId: me,
            coordinate: { x: 4, y: 1 },
            sunkCoordinates: [{ x: 3, y: 1 }, { x: 4, y: 1 }],
        });

        expect(opponentBoard().getByTitle('B4')).toHaveClass('cell-sunk');
        expect(opponentBoard().getByTitle('B4')).not.toHaveClass('cell-hit');
        expect(opponentBoard().getByTitle('B5')).toHaveClass('cell-sunk');
        expect(opponentBoard().getByTitle('B5').querySelector('.shot-marker-sunk')).not.toBeNull();
        expect(ownBoard().getByTitle('B4')).not.toHaveClass('cell-sunk');
        expect(ownBoard().getByTitle('F6')).toHaveClass('cell-miss');
    });

    it('marks my ship as sunk on my board when the opponent sinks it', () => {
        renderGame({ startingPlayerId: opponent });

        emitShot({ result: ShotResult.Hit, shooterId: opponent, coordinate: { x: 0, y: 0 } });
        emitShot({ result: ShotResult.Miss, shooterId: me, coordinate: { x: 5, y: 5 } });
        emitShot({
            result: ShotResult.Sunk,
            shooterId: opponent,
            coordinate: { x: 1, y: 0 },
            sunkCoordinates: [{ x: 0, y: 0 }, { x: 1, y: 0 }],
        });

        expect(ownBoard().getByTitle('A1')).toHaveClass('cell-occupied', 'cell-sunk');
        expect(ownBoard().getByTitle('A2')).toHaveClass('cell-occupied', 'cell-sunk');
        expect(document.querySelectorAll('.cell-hit')).toHaveLength(0);
    });

    it('does not shoot at a cell that has already been shot', () => {
        renderGame();

        emitShot({ result: ShotResult.Miss, shooterId: me, coordinate: { x: 3, y: 1 } });
        emitShot({ result: ShotResult.Miss, shooterId: opponent, coordinate: { x: 5, y: 5 } });

        fireEvent.click(opponentBoard().getByTitle('B4'));

        expect(shoot).not.toHaveBeenCalled();
    });

    it('blocks shooting while disconnected', () => {
        renderGame({ connectionStatus: 'reconnecting' });

        fireEvent.click(opponentBoard().getByTitle('B4'));

        expect(shoot).not.toHaveBeenCalled();
        expect(opponentBoard().getByTitle('B4').closest('.board')).toHaveClass('board-dimmed');
    });

    it('shows the server message when a shot is rejected', async () => {
        vi.mocked(shoot).mockRejectedValue(
            new Error(
                "An unexpected error occurred invoking 'Shoot' on the server. HubException: Cannot shoot when it is not your turn.",
            ),
        );
        renderGame();

        fireEvent.click(opponentBoard().getByTitle('B4'));

        expect(await screen.findByRole('alert')).toHaveTextContent(
            'Cannot shoot when it is not your turn.',
        );
        expect(opponentBoard().getByTitle('B4')).not.toHaveClass('cell-hit');
    });

    it('does not show the game over panel while the game is in progress', () => {
        renderGame();

        emitShot({ result: ShotResult.Sunk, shooterId: me, coordinate: { x: 3, y: 1 } });

        expect(screen.queryByRole('region', { name: 'Game over' })).toBeNull();
    });

    it('announces a win when my shot ends the game', () => {
        renderGame();

        emitShot({
            result: ShotResult.Sunk,
            shooterId: me,
            coordinate: { x: 3, y: 1 },
            isGameOver: true,
        });

        const gameOver = within(screen.getByRole('region', { name: 'Game over' }));

        expect(gameOver.getByRole('heading', { name: 'You win!' })).toBeInTheDocument();
        expect(opponentBoard().getByTitle('B4')).toHaveClass('cell-hit');
        expect(screen.queryByText("Alice's turn")).toBeNull();
    });

    it('announces a loss when the opponent shot ends the game', () => {
        renderGame({ startingPlayerId: opponent });

        emitShot({
            result: ShotResult.Sunk,
            shooterId: opponent,
            coordinate: { x: 0, y: 0 },
            isGameOver: true,
        });

        const gameOver = within(screen.getByRole('region', { name: 'Game over' }));

        expect(gameOver.getByRole('heading', { name: 'Alice wins!' })).toBeInTheDocument();
        expect(ownBoard().getByTitle('A1')).toHaveClass('cell-hit');
        expect(screen.queryByText('Your turn')).toBeNull();
    });

    it('blocks shooting and dims the opponent board once the game is over', () => {
        renderGame({ startingPlayerId: opponent });

        emitShot({
            result: ShotResult.Sunk,
            shooterId: opponent,
            coordinate: { x: 0, y: 0 },
            isGameOver: true,
        });

        fireEvent.click(opponentBoard().getByTitle('B4'));

        expect(shoot).not.toHaveBeenCalled();
        expect(opponentBoard().getByTitle('B4').closest('.board')).toHaveClass('board-dimmed');
    });

    it('goes back to home when Back to home is clicked', () => {
        renderGame();

        emitShot({
            result: ShotResult.Sunk,
            shooterId: me,
            coordinate: { x: 3, y: 1 },
            isGameOver: true,
        });

        fireEvent.click(screen.getByRole('button', { name: 'Back to home' }));

        expect(onBackToHome).toHaveBeenCalledOnce();
    });

    it('unsubscribes from Shot events when unmounted', () => {
        const { unmount } = renderGame();

        unmount();

        expect(unsubscribe).toHaveBeenCalledOnce();
    });
});

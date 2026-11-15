import { afterEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen } from '@testing-library/react';
import GameOver from './GameOver';

describe('GameOver', () => {
    afterEach(cleanup);

    it('announces a win', () => {
        render(<GameOver isWinner opponentName="Alice" onBackToHome={vi.fn()} />);

        expect(screen.getByRole('heading', { name: 'You win!' })).toBeInTheDocument();
    });

    it('announces a loss', () => {
        render(<GameOver isWinner={false} opponentName="Alice" onBackToHome={vi.fn()} />);

        expect(screen.getByRole('heading', { name: 'Alice wins!' })).toBeInTheDocument();
    });

    it('calls onBackToHome when Back to home is clicked', () => {
        const onBackToHome = vi.fn();

        render(<GameOver isWinner opponentName="Alice" onBackToHome={onBackToHome} />);

        fireEvent.click(screen.getByRole('button', { name: 'Back to home' }));

        expect(onBackToHome).toHaveBeenCalledOnce();
    });
});

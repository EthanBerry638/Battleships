import { afterEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen } from '@testing-library/react';
import OpponentDisconnected from './OpponentDisconnected';

describe('OpponentDisconnected', () => {
    afterEach(cleanup);

    it('names the opponent who disconnected and says the game has ended', () => {
        render(<OpponentDisconnected opponentName="Alice" onBackToHome={vi.fn()} />);

        expect(screen.getByRole('heading', { name: 'Alice disconnected' })).toBeInTheDocument();
        expect(screen.getByText('The game has ended.')).toBeInTheDocument();
    });

    it('calls onBackToHome when Back to home is clicked', () => {
        const onBackToHome = vi.fn();

        render(<OpponentDisconnected opponentName="Alice" onBackToHome={onBackToHome} />);

        fireEvent.click(screen.getByRole('button', { name: 'Back to home' }));

        expect(onBackToHome).toHaveBeenCalledOnce();
    });
});

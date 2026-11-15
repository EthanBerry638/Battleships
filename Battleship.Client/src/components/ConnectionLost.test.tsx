import { afterEach, describe, expect, it, vi } from 'vitest';
import { cleanup, fireEvent, render, screen } from '@testing-library/react';
import ConnectionLost from './ConnectionLost';

describe('ConnectionLost', () => {
    afterEach(cleanup);

    it('calls onBackToHome when Back to home is clicked', () => {
        const onBackToHome = vi.fn();

        render(<ConnectionLost onBackToHome={onBackToHome} />);

        fireEvent.click(screen.getByRole('button', { name: 'Back to home' }));

        expect(onBackToHome).toHaveBeenCalledOnce();
    });
});

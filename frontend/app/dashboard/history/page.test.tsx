import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import HistoryPage from './page';
import * as api from '@/lib/api';

// Mock the API module
vi.mock('@/lib/api', () => ({
  getResumes: vi.fn(),
}));

// Mock next/navigation
const mockPush = vi.fn();
vi.mock('next/navigation', () => ({
  useRouter: vi.fn(() => ({
    push: mockPush,
  })),
}));

// Mock framer-motion
vi.mock('framer-motion', () => ({
  motion: {
    div: ({ children, ...props }: any) => <div {...props}>{children}</div>,
  },
  AnimatePresence: ({ children }: any) => <>{children}</>,
}));

// Mock protected route and dashboard layout
vi.mock('@/components/protected-route', () => ({
  ProtectedRoute: ({ children }: any) => <div>{children}</div>,
}));

vi.mock('@/components/dashboard-layout', () => ({
  DashboardLayout: ({ children }: any) => <div>{children}</div>,
}));

describe('HistoryPage - Resume List', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockPush.mockClear();
  });

  describe('Resume List Rendering', () => {
    it('should display resume cards with name, date, and fit score', async () => {
      const mockResumes = [
        {
          resumeId: 'resume-1',
          name: 'John Doe',
          email: 'john@example.com',
          skills: ['JavaScript', 'React'],
          createdAt: '2024-01-15T10:00:00Z',
          lastAnalysis: {
            fitScore: 85,
            analyzedAt: '2024-01-15T10:05:00Z',
          },
        },
        {
          resumeId: 'resume-2',
          name: 'Jane Smith',
          email: 'jane@example.com',
          skills: ['Python', 'Django'],
          createdAt: '2024-01-10T10:00:00Z',
          lastAnalysis: {
            fitScore: 65,
            analyzedAt: '2024-01-10T10:05:00Z',
          },
        },
      ];

      vi.mocked(api.getResumes).mockResolvedValue(mockResumes);

      render(<HistoryPage />);

      await waitFor(() => {
        expect(screen.getByText('John Doe')).toBeInTheDocument();
        expect(screen.getByText('Jane Smith')).toBeInTheDocument();
      });

      // Check fit scores are displayed
      expect(screen.getByText('85% Match')).toBeInTheDocument();
      expect(screen.getByText('65% Match')).toBeInTheDocument();

      // Check dates are displayed (format may vary by locale)
      const dateElements = screen.getAllByText(/\d{1,2}\/\d{1,2}\/\d{4}/);
      expect(dateElements.length).toBeGreaterThanOrEqual(2);
    });

    it('should sort resumes by creation date descending', async () => {
      const mockResumes = [
        {
          resumeId: 'resume-1',
          name: 'Older Resume',
          email: 'old@example.com',
          skills: [],
          createdAt: '2024-01-01T10:00:00Z',
        },
        {
          resumeId: 'resume-2',
          name: 'Newer Resume',
          email: 'new@example.com',
          skills: [],
          createdAt: '2024-01-20T10:00:00Z',
        },
      ];

      vi.mocked(api.getResumes).mockResolvedValue(mockResumes);

      render(<HistoryPage />);

      await waitFor(() => {
        expect(screen.getByText('Newer Resume')).toBeInTheDocument();
      });

      const cards = screen.getAllByRole('generic').filter(el => 
        el.textContent?.includes('Resume')
      );
      
      // First card should be the newer resume
      expect(cards[0].textContent).toContain('Newer Resume');
    });
  });

  describe('Empty State Display', () => {
    it('should show empty state message when no resumes exist', async () => {
      vi.mocked(api.getResumes).mockResolvedValue([]);

      render(<HistoryPage />);

      await waitFor(() => {
        expect(screen.getByText('No resumes yet')).toBeInTheDocument();
      });

      expect(
        screen.getByText(/Upload your resume and a job description/i)
      ).toBeInTheDocument();
    });

    it('should show "Start New Analysis" button in empty state', async () => {
      vi.mocked(api.getResumes).mockResolvedValue([]);

      render(<HistoryPage />);

      await waitFor(() => {
        expect(screen.getByText('Start New Analysis')).toBeInTheDocument();
      });
    });
  });

  describe('Navigation on Card Click', () => {
    it('should navigate to analysis detail page when card is clicked', async () => {
      const user = userEvent.setup();
      const mockResumes = [
        {
          resumeId: 'resume-123',
          name: 'Test Resume',
          email: 'test@example.com',
          skills: [],
          createdAt: '2024-01-15T10:00:00Z',
          lastAnalysis: {
            fitScore: 75,
          },
        },
      ];

      vi.mocked(api.getResumes).mockResolvedValue(mockResumes);

      render(<HistoryPage />);

      await waitFor(() => {
        expect(screen.getByText('Test Resume')).toBeInTheDocument();
      });

      const card = screen.getByText('Test Resume').closest('div[class*="cursor-pointer"]');
      expect(card).toBeInTheDocument();
      
      if (card) {
        await user.click(card);
        expect(mockPush).toHaveBeenCalledWith('/analysis/resume-123');
      }
    });
  });
});

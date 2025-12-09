import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ChatbotComponent } from './ChatbotComponent';
import * as api from '@/lib/api';

// Mock the API module
vi.mock('@/lib/api', () => ({
  sendChatMessage: vi.fn(),
}));

// Mock next/navigation
const mockPush = vi.fn();
vi.mock('next/navigation', () => ({
  useRouter: vi.fn(() => ({
    push: mockPush,
  })),
}));

// Mock framer-motion to avoid animation issues in tests
vi.mock('framer-motion', () => ({
  motion: {
    div: ({ children, ...props }: any) => <div {...props}>{children}</div>,
  },
  AnimatePresence: ({ children }: any) => <>{children}</>,
}));

describe('ChatbotComponent', () => {
  const mockResumeId = 'test-resume-123';
  const mockCvText = 'Software Engineer with 5 years experience';
  const mockJobDescription = 'Looking for a Senior Software Engineer';
  const mockAnalysisResults = {
    fitScore: 85,
    matchedSkills: ['JavaScript', 'React'],
    missingSkills: ['Kubernetes'],
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Message Sending and Display', () => {
    it('should display initial empty state message', () => {
      render(
        <ChatbotComponent
          resumeId={mockResumeId}
          cvText={mockCvText}
          jobDescription={mockJobDescription}
          analysisResults={mockAnalysisResults}
        />
      );

      expect(
        screen.getByText(/Start a conversation! Ask me about your skills/i)
      ).toBeInTheDocument();
    });

    it('should display initial chat history when provided', () => {
      const initialHistory = [
        {
          role: 'user' as const,
          content: 'What are my strengths?',
          timestamp: '2024-01-01T12:00:00Z',
        },
        {
          role: 'assistant' as const,
          content: 'Your strengths include JavaScript and React.',
          timestamp: '2024-01-01T12:00:05Z',
        },
      ];

      render(
        <ChatbotComponent
          resumeId={mockResumeId}
          cvText={mockCvText}
          jobDescription={mockJobDescription}
          analysisResults={mockAnalysisResults}
          initialHistory={initialHistory}
        />
      );

      expect(screen.getByText('What are my strengths?')).toBeInTheDocument();
      expect(
        screen.getByText('Your strengths include JavaScript and React.')
      ).toBeInTheDocument();
    });

    it('should send a message when user types and clicks send button', async () => {
      const user = userEvent.setup();
      const mockResponse = {
        aiMessage: 'Based on your resume, you should focus on Kubernetes.',
        timestamp: '2024-01-01T12:00:10Z',
      };

      vi.mocked(api.sendChatMessage).mockResolvedValue(mockResponse);

      render(
        <ChatbotComponent
          resumeId={mockResumeId}
          cvText={mockCvText}
          jobDescription={mockJobDescription}
          analysisResults={mockAnalysisResults}
        />
      );

      const input = screen.getByPlaceholderText(
        /Ask about your resume, skills, or interview tips/i
      );

      await user.type(input, 'What skills should I focus on?');
      
      // Find button by its enabled state (it should be enabled after typing)
      const sendButton = screen.getByRole('button');
      await user.click(sendButton);

      // Check that user message is displayed
      expect(
        screen.getByText('What skills should I focus on?')
      ).toBeInTheDocument();

      // Wait for AI response
      await waitFor(() => {
        expect(
          screen.getByText(/Based on your resume, you should focus on Kubernetes/i)
        ).toBeInTheDocument();
      });

      // Verify API was called with correct parameters
      // Note: chatHistory now includes the user message for context continuity
      expect(api.sendChatMessage).toHaveBeenCalledWith(
        expect.objectContaining({
          resumeId: mockResumeId,
          userMessage: 'What skills should I focus on?',
          jobDescription: mockJobDescription,
          chatHistory: expect.arrayContaining([
            expect.objectContaining({
              role: 'user',
              content: 'What skills should I focus on?',
            }),
          ]),
        })
      );
    });

    it('should send a message when user presses Enter key', async () => {
      const user = userEvent.setup();
      const mockResponse = {
        aiMessage: 'Great question!',
        timestamp: '2024-01-01T12:00:10Z',
      };

      vi.mocked(api.sendChatMessage).mockResolvedValue(mockResponse);

      render(
        <ChatbotComponent
          resumeId={mockResumeId}
          cvText={mockCvText}
          jobDescription={mockJobDescription}
          analysisResults={mockAnalysisResults}
        />
      );

      const input = screen.getByPlaceholderText(
        /Ask about your resume, skills, or interview tips/i
      );

      await user.type(input, 'Tell me about my fit score{Enter}');

      // Check that user message is displayed
      expect(screen.getByText('Tell me about my fit score')).toBeInTheDocument();

      // Wait for AI response
      await waitFor(() => {
        expect(screen.getByText('Great question!')).toBeInTheDocument();
      });
    });

    it('should not send empty or whitespace-only messages', async () => {
      const user = userEvent.setup();

      render(
        <ChatbotComponent
          resumeId={mockResumeId}
          cvText={mockCvText}
          jobDescription={mockJobDescription}
          analysisResults={mockAnalysisResults}
        />
      );

      const input = screen.getByPlaceholderText(
        /Ask about your resume, skills, or interview tips/i
      );
      const sendButton = screen.getByRole('button');

      // Try to send empty message
      await user.click(sendButton);
      expect(api.sendChatMessage).not.toHaveBeenCalled();

      // Try to send whitespace-only message
      await user.type(input, '   ');
      await user.click(sendButton);
      expect(api.sendChatMessage).not.toHaveBeenCalled();
    });

    it('should clear input field after sending message', async () => {
      const user = userEvent.setup();
      const mockResponse = {
        aiMessage: 'Response',
        timestamp: '2024-01-01T12:00:10Z',
      };

      vi.mocked(api.sendChatMessage).mockResolvedValue(mockResponse);

      render(
        <ChatbotComponent
          resumeId={mockResumeId}
          cvText={mockCvText}
          jobDescription={mockJobDescription}
          analysisResults={mockAnalysisResults}
        />
      );

      const input = screen.getByPlaceholderText(
        /Ask about your resume, skills, or interview tips/i
      ) as HTMLInputElement;

      await user.type(input, 'Test message');
      expect(input.value).toBe('Test message');

      const sendButton = screen.getByRole('button');
      await user.click(sendButton);

      // Input should be cleared
      expect(input.value).toBe('');
    });
  });

  describe('Markdown Rendering', () => {
    it('should render markdown formatting in assistant messages', () => {
      const initialHistory = [
        {
          role: 'assistant' as const,
          content: '**Bold text** and *italic text* with a [link](https://example.com)',
          timestamp: '2024-01-01T12:00:00Z',
        },
      ];

      render(
        <ChatbotComponent
          resumeId={mockResumeId}
          cvText={mockCvText}
          jobDescription={mockJobDescription}
          analysisResults={mockAnalysisResults}
          initialHistory={initialHistory}
        />
      );

      // Check for bold text
      const boldElement = screen.getByText('Bold text');
      expect(boldElement.tagName).toBe('STRONG');

      // Check for link
      const linkElement = screen.getByRole('link', { name: /link/i });
      expect(linkElement).toHaveAttribute('href', 'https://example.com');
      expect(linkElement).toHaveAttribute('target', '_blank');
    });

    it('should render lists in assistant messages', () => {
      const initialHistory = [
        {
          role: 'assistant' as const,
          content: 'Here are your skills:\n- JavaScript\n- React\n- Node.js',
          timestamp: '2024-01-01T12:00:00Z',
        },
      ];

      render(
        <ChatbotComponent
          resumeId={mockResumeId}
          cvText={mockCvText}
          jobDescription={mockJobDescription}
          analysisResults={mockAnalysisResults}
          initialHistory={initialHistory}
        />
      );

      expect(screen.getByText('JavaScript')).toBeInTheDocument();
      expect(screen.getByText('React')).toBeInTheDocument();
      expect(screen.getByText('Node.js')).toBeInTheDocument();
    });

    it('should render code blocks in assistant messages', () => {
      const initialHistory = [
        {
          role: 'assistant' as const,
          content: 'Use this code: `const x = 5;`',
          timestamp: '2024-01-01T12:00:00Z',
        },
      ];

      render(
        <ChatbotComponent
          resumeId={mockResumeId}
          cvText={mockCvText}
          jobDescription={mockJobDescription}
          analysisResults={mockAnalysisResults}
          initialHistory={initialHistory}
        />
      );

      const codeElement = screen.getByText('const x = 5;');
      expect(codeElement.tagName).toBe('CODE');
    });

    it('should not render markdown in user messages', () => {
      const initialHistory = [
        {
          role: 'user' as const,
          content: '**This should not be bold**',
          timestamp: '2024-01-01T12:00:00Z',
        },
      ];

      render(
        <ChatbotComponent
          resumeId={mockResumeId}
          cvText={mockCvText}
          jobDescription={mockJobDescription}
          analysisResults={mockAnalysisResults}
          initialHistory={initialHistory}
        />
      );

      // User message should contain the raw markdown
      expect(screen.getByText('**This should not be bold**')).toBeInTheDocument();
    });
  });

  describe('Loading States', () => {
    it('should show loading indicator while waiting for response', async () => {
      const user = userEvent.setup();
      let resolvePromise: (value: any) => void;
      const mockPromise = new Promise((resolve) => {
        resolvePromise = resolve;
      });

      vi.mocked(api.sendChatMessage).mockReturnValue(mockPromise as any);

      render(
        <ChatbotComponent
          resumeId={mockResumeId}
          cvText={mockCvText}
          jobDescription={mockJobDescription}
          analysisResults={mockAnalysisResults}
        />
      );

      const input = screen.getByPlaceholderText(
        /Ask about your resume, skills, or interview tips/i
      );
      await user.type(input, 'Test message{Enter}');

      // Loading indicator should be visible
      expect(screen.getByText(/AI is thinking/i)).toBeInTheDocument();

      // Resolve the promise
      resolvePromise!({
        aiMessage: 'Response',
        timestamp: '2024-01-01T12:00:10Z',
      });

      // Wait for loading to disappear
      await waitFor(() => {
        expect(screen.queryByText(/AI is thinking/i)).not.toBeInTheDocument();
      });
    });

    it('should disable send button while loading', async () => {
      const user = userEvent.setup();
      let resolvePromise: (value: any) => void;
      const mockPromise = new Promise((resolve) => {
        resolvePromise = resolve;
      });

      vi.mocked(api.sendChatMessage).mockReturnValue(mockPromise as any);

      render(
        <ChatbotComponent
          resumeId={mockResumeId}
          cvText={mockCvText}
          jobDescription={mockJobDescription}
          analysisResults={mockAnalysisResults}
        />
      );

      const input = screen.getByPlaceholderText(
        /Ask about your resume, skills, or interview tips/i
      );

      await user.type(input, 'Test message');
      
      const sendButton = screen.getByRole('button');
      await user.click(sendButton);

      // Send button should be disabled while loading
      expect(sendButton).toBeDisabled();

      // Resolve the promise
      resolvePromise!({
        aiMessage: 'Response',
        timestamp: '2024-01-01T12:00:10Z',
      });

      // Wait for loading to complete
      await waitFor(() => {
        expect(screen.queryByText(/AI is thinking/i)).not.toBeInTheDocument();
      });

      // Button should still be disabled because input is empty
      // (This is correct behavior - button is only enabled when there's text)
      expect(sendButton).toBeDisabled();
      
      // Type new text to enable button again
      await user.type(input, 'Another message');
      expect(sendButton).not.toBeDisabled();
    });

    it('should disable input field while loading', async () => {
      const user = userEvent.setup();
      let resolvePromise: (value: any) => void;
      const mockPromise = new Promise((resolve) => {
        resolvePromise = resolve;
      });

      vi.mocked(api.sendChatMessage).mockReturnValue(mockPromise as any);

      render(
        <ChatbotComponent
          resumeId={mockResumeId}
          cvText={mockCvText}
          jobDescription={mockJobDescription}
          analysisResults={mockAnalysisResults}
        />
      );

      const input = screen.getByPlaceholderText(
        /Ask about your resume, skills, or interview tips/i
      );

      await user.type(input, 'Test message{Enter}');

      // Input should be disabled
      expect(input).toBeDisabled();

      // Resolve the promise
      resolvePromise!({
        aiMessage: 'Response',
        timestamp: '2024-01-01T12:00:10Z',
      });

      // Wait for input to be enabled again
      await waitFor(() => {
        expect(input).not.toBeDisabled();
      });
    });
  });

  describe('Error Handling', () => {
    it('should display error message when API call fails', async () => {
      const user = userEvent.setup();
      const errorMessage = 'Network error occurred';

      vi.mocked(api.sendChatMessage).mockRejectedValue(
        new Error(errorMessage)
      );

      render(
        <ChatbotComponent
          resumeId={mockResumeId}
          cvText={mockCvText}
          jobDescription={mockJobDescription}
          analysisResults={mockAnalysisResults}
        />
      );

      const input = screen.getByPlaceholderText(
        /Ask about your resume, skills, or interview tips/i
      );
      await user.type(input, 'Test message{Enter}');

      // Wait for error message
      await waitFor(() => {
        expect(screen.getByText(errorMessage)).toBeInTheDocument();
      });
    });

    it('should show retry button on error', async () => {
      const user = userEvent.setup();

      vi.mocked(api.sendChatMessage).mockRejectedValue(
        new Error('Network error')
      );

      render(
        <ChatbotComponent
          resumeId={mockResumeId}
          cvText={mockCvText}
          jobDescription={mockJobDescription}
          analysisResults={mockAnalysisResults}
        />
      );

      const input = screen.getByPlaceholderText(
        /Ask about your resume, skills, or interview tips/i
      );
      await user.type(input, 'Test message{Enter}');

      // Wait for retry button
      await waitFor(() => {
        expect(screen.getByRole('button', { name: /retry/i })).toBeInTheDocument();
      });
    });

    it('should populate input with last message when retry is clicked', async () => {
      const user = userEvent.setup();
      const testMessage = 'What are my skills?';

      vi.mocked(api.sendChatMessage).mockRejectedValue(
        new Error('Network error')
      );

      render(
        <ChatbotComponent
          resumeId={mockResumeId}
          cvText={mockCvText}
          jobDescription={mockJobDescription}
          analysisResults={mockAnalysisResults}
        />
      );

      const input = screen.getByPlaceholderText(
        /Ask about your resume, skills, or interview tips/i
      ) as HTMLInputElement;

      await user.type(input, `${testMessage}{Enter}`);

      // Wait for error and retry button
      await waitFor(() => {
        expect(screen.getByRole('button', { name: /retry/i })).toBeInTheDocument();
      });

      const retryButton = screen.getByRole('button', { name: /retry/i });
      await user.click(retryButton);

      // Input should be populated with the last message
      expect(input.value).toBe(testMessage);
    });

    it('should handle authentication errors by redirecting to login', async () => {
      const user = userEvent.setup();

      const authError = {
        response: { status: 401 },
        message: 'Unauthorized',
      };

      vi.mocked(api.sendChatMessage).mockRejectedValue(authError);

      render(
        <ChatbotComponent
          resumeId={mockResumeId}
          cvText={mockCvText}
          jobDescription={mockJobDescription}
          analysisResults={mockAnalysisResults}
        />
      );

      const input = screen.getByPlaceholderText(
        /Ask about your resume, skills, or interview tips/i
      );
      await user.type(input, 'Test message{Enter}');

      // Wait for redirect message
      await waitFor(() => {
        expect(screen.getByText(/Session expired. Redirecting to login/i)).toBeInTheDocument();
      });

      // Should not show retry button for auth errors
      expect(screen.queryByRole('button', { name: /retry/i })).not.toBeInTheDocument();
    });

    it('should clear error when sending a new message', async () => {
      const user = userEvent.setup();

      // First call fails
      vi.mocked(api.sendChatMessage)
        .mockRejectedValueOnce(new Error('Network error'))
        .mockResolvedValueOnce({
          aiMessage: 'Success',
          timestamp: '2024-01-01T12:00:10Z',
        });

      render(
        <ChatbotComponent
          resumeId={mockResumeId}
          cvText={mockCvText}
          jobDescription={mockJobDescription}
          analysisResults={mockAnalysisResults}
        />
      );

      const input = screen.getByPlaceholderText(
        /Ask about your resume, skills, or interview tips/i
      );

      // Send first message that fails
      await user.type(input, 'First message{Enter}');

      // Wait for error
      await waitFor(() => {
        expect(screen.getByText(/Network error/i)).toBeInTheDocument();
      });

      // Send second message
      await user.type(input, 'Second message{Enter}');

      // Error should be cleared and success message shown
      await waitFor(() => {
        expect(screen.queryByText(/Network error/i)).not.toBeInTheDocument();
        expect(screen.getByText('Success')).toBeInTheDocument();
      });
    });
  });

  describe('Chat History Context', () => {
    it('should include previous messages in API call', async () => {
      const user = userEvent.setup();
      const initialHistory = [
        {
          role: 'user' as const,
          content: 'First question',
          timestamp: '2024-01-01T12:00:00Z',
        },
        {
          role: 'assistant' as const,
          content: 'First answer',
          timestamp: '2024-01-01T12:00:05Z',
        },
      ];

      vi.mocked(api.sendChatMessage).mockResolvedValue({
        aiMessage: 'Second answer',
        timestamp: '2024-01-01T12:00:15Z',
      });

      render(
        <ChatbotComponent
          resumeId={mockResumeId}
          cvText={mockCvText}
          jobDescription={mockJobDescription}
          analysisResults={mockAnalysisResults}
          initialHistory={initialHistory}
        />
      );

      const input = screen.getByPlaceholderText(
        /Ask about your resume, skills, or interview tips/i
      );
      await user.type(input, 'Second question{Enter}');

      await waitFor(() => {
        // chatHistory should include initialHistory + the new user message
        expect(api.sendChatMessage).toHaveBeenCalledWith(
          expect.objectContaining({
            resumeId: mockResumeId,
            userMessage: 'Second question',
            jobDescription: mockJobDescription,
            chatHistory: expect.arrayContaining([
              ...initialHistory,
              expect.objectContaining({
                role: 'user',
                content: 'Second question',
              }),
            ]),
          })
        );
      });
    });
  });
});

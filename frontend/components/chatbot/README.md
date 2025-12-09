# ChatbotComponent

A reusable AI-powered chatbot component for the Smart Resume Analyzer application.

## Features

- ✅ Context-aware AI responses using AWS Bedrock
- ✅ Markdown rendering for formatted responses
- ✅ Real-time typing indicators
- ✅ Error handling with retry functionality
- ✅ Authentication error handling (auto-redirect to login)
- ✅ Mobile-responsive design with touch-friendly controls
- ✅ Auto-scroll to latest messages
- ✅ Message history support

## Usage

### Basic Integration

```tsx
import { ChatbotComponent } from "@/components/chatbot/ChatbotComponent";

export default function AnalysisPage() {
  const resumeId = "your-resume-id";
  const analysisResults = {
    fitScore: 85,
    matchedSkills: ["Python", "AWS", "React"],
    missingSkills: ["Kubernetes"],
    recommendation: "Focus on learning Kubernetes..."
  };

  return (
    <div className="container mx-auto p-6">
      <h1>Analysis Results</h1>
      
      {/* Your analysis display components */}
      
      {/* Chatbot Component */}
      <div className="mt-8">
        <ChatbotComponent
          resumeId={resumeId}
          cvText="Full CV text here..."
          jobDescription="Full job description here..."
          analysisResults={analysisResults}
        />
      </div>
    </div>
  );
}
```

### With Initial Chat History

```tsx
import { ChatbotComponent } from "@/components/chatbot/ChatbotComponent";
import { getChatHistory } from "@/lib/api";
import { useEffect, useState } from "react";

export default function AnalysisPage({ resumeId }: { resumeId: string }) {
  const [chatHistory, setChatHistory] = useState([]);

  useEffect(() => {
    async function loadHistory() {
      const history = await getChatHistory(resumeId);
      setChatHistory(history);
    }
    loadHistory();
  }, [resumeId]);

  return (
    <ChatbotComponent
      resumeId={resumeId}
      cvText="..."
      jobDescription="..."
      analysisResults={{}}
      initialHistory={chatHistory}
    />
  );
}
```

## Props

| Prop | Type | Required | Description |
|------|------|----------|-------------|
| `resumeId` | `string` | Yes | Unique identifier for the resume/analysis session |
| `cvText` | `string` | No | Full text of the CV for context |
| `jobDescription` | `string` | No | Job description text for context |
| `analysisResults` | `AnalysisResults` | No | Analysis results including fit score and skills |
| `initialHistory` | `ChatMessage[]` | No | Previous chat messages to display on load |

### AnalysisResults Type

```typescript
interface AnalysisResults {
  fitScore?: number;
  matchedSkills?: string[];
  missingSkills?: string[];
  recommendation?: string;
}
```

### ChatMessage Type

```typescript
interface ChatMessage {
  role: "user" | "assistant";
  content: string;
  timestamp: string; // ISO 8601 format
}
```

## API Functions

The component uses the following API functions from `@/lib/api`:

### sendChatMessage

Sends a chat message and receives AI response.

```typescript
import { sendChatMessage } from "@/lib/api";

const response = await sendChatMessage({
  resumeId: "resume-123",
  userMessage: "What skills should I focus on?",
  jobDescription: "Optional JD text",
  chatHistory: [] // Previous messages
});

console.log(response.aiMessage); // AI response text
console.log(response.timestamp); // Response timestamp
```

### getChatHistory

Retrieves chat history for a specific resume.

```typescript
import { getChatHistory } from "@/lib/api";

const messages = await getChatHistory("resume-123");
// Returns: ChatMessage[]
```

## Styling

The component uses Tailwind CSS and follows the application's design system:

- Dark theme with glassmorphism effects
- Primary color accents for user messages
- Responsive breakpoints for mobile/desktop
- Touch-friendly button sizes (44x44px minimum)
- Smooth animations with Framer Motion

## Error Handling

The component handles various error scenarios:

1. **Network Errors**: Displays error message with retry button
2. **Authentication Errors**: Redirects to login page after 2 seconds
3. **API Errors**: Shows user-friendly error messages
4. **Loading States**: Displays typing indicator while waiting for response

## Mobile Optimization

- Responsive message widths (85% on mobile, 75% on desktop)
- Touch-friendly input and button sizes
- Scrollable message area with proper viewport height
- Optimized keyboard handling for mobile devices

## Accessibility

- Semantic HTML structure
- Keyboard navigation support (Enter to send)
- ARIA labels for screen readers
- Focus management for input field
- High contrast text for readability

## Requirements Validation

This component satisfies the following requirements:

- ✅ 1.1: Display chatbot interface after analysis
- ✅ 1.2: Load analysis context
- ✅ 10.1-10.5: Message formatting with markdown
- ✅ 11.1-11.5: Real-time updates and loading indicators
- ✅ 13.1-13.4: Error handling
- ✅ 14.4: Mobile responsiveness

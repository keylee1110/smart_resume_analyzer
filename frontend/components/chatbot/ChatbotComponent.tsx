"use client";

import { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { motion, AnimatePresence } from 'framer-motion';
import ReactMarkdown from 'react-markdown';
import { MessageSquare, AlertCircle, RefreshCw, Loader2, Send } from 'lucide-react';

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { ScrollArea } from "@/components/ui/scroll-area";
import { sendChatMessage } from "@/lib/api";
import { ChatMessageSkeleton } from "./chat-message-skeleton"; // Import skeleton

interface ChatMessage {
  role: "user" | "assistant";
  content: string;
  timestamp: string;
}

interface ChatbotComponentProps {
  resumeId: string;
  cvText?: string;
  jobDescription?: string;
  analysisResults?: any;
  initialHistory?: ChatMessage[];
}

export function ChatbotComponent({
  resumeId,
  cvText = "",
  jobDescription = "",
  analysisResults = {},
  initialHistory = [],
}: ChatbotComponentProps) {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [inputValue, setInputValue] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const scrollAreaRef = useRef<HTMLDivElement>(null);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const router = useRouter();

  // Initialize messages with initialHistory on mount
  useEffect(() => {
    if (initialHistory.length > 0) {
      setMessages(initialHistory);
    }
  }, [initialHistory]);

  // Auto-scroll to latest message
  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const handleSendMessage = async () => {
    if (!inputValue.trim() || isLoading) return;

    const userMessage: ChatMessage = {
      role: "user",
      content: inputValue.trim(),
      timestamp: new Date().toISOString(),
    };

    // Add user message to UI immediately
    setMessages((prev) => [...prev, userMessage]);
    setInputValue("");
    setIsLoading(true);
    setError(null);

    try {
      // Include ALL messages (including initialHistory and current session) for context continuity
      const fullChatHistory = [...messages, userMessage];
      
      const response = await sendChatMessage({
        resumeId,
        userMessage: userMessage.content,
        jobDescription,
        chatHistory: fullChatHistory,
      });

      const assistantMessage: ChatMessage = {
        role: "assistant",
        content: response.aiMessage,
        timestamp: response.timestamp,
      };

      setMessages((prev) => [...prev, assistantMessage]);
    } catch (err: any) {
      console.error("Chat error:", err);
      
      // Handle authentication errors
      if (err?.response?.status === 401 || err?.message?.includes("Unauthorized")) {
        setError("Session expired. Redirecting to login...");
        setTimeout(() => router.push("/login"), 2000);
        return;
      }

      // Handle other errors
      setError(err?.message || "Failed to send message. Please try again.");
    } finally {
      setIsLoading(false);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSendMessage();
    }
  };

  const handleRetry = () => {
    if (messages.length > 0) {
      const lastUserMessage = messages.findLast(msg => msg.role === 'user');
      if (lastUserMessage) {
        // Remove error message and last assistant message if any before resending
        setError(null);
        // Optionally remove the last assistant response if it was an error or incomplete
        // For now, new response will just append, which is fine for retry.
        setInputValue(lastUserMessage.content); 
        handleSendMessage();
      }
    }
  };

  return (
    <div className="flex flex-col h-full max-h-[600px] rounded-3xl border border-white/10 bg-white/5 backdrop-blur-xl overflow-hidden">
      {/* Header */}
      <div className="flex items-center gap-3 px-6 py-4 border-b border-white/10 bg-white/5">
        <div className="w-10 h-10 rounded-full bg-gradient-to-br from-primary to-accent flex items-center justify-center">
          <MessageSquare className="w-5 h-5 text-white" />
        </div>
        <div>
          <h3 className="text-lg font-bold text-white">AI Career Coach</h3>
          <p className="text-xs text-muted-foreground">
            Ask me anything about your resume analysis
          </p>
        </div>
      </div>

      {/* Messages Area */}
      <ScrollArea className="flex-1 p-6">
        <div className="space-y-4">
          {messages.length === 0 && !isLoading && (
            <div className="text-center py-8">
              <p className="text-muted-foreground text-sm">
                Start a conversation! Ask me about your skills, interview tips, or career advice.
              </p>
            </div>
          )}

          <AnimatePresence mode="popLayout">
            {messages.map((message, index) => (
              <motion.div
                key={`${message.timestamp}-${index}`}
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, y: -10 }}
                transition={{ duration: 0.2 }}
                className={`flex ${message.role === "user" ? "justify-end" : "justify-start"}`}
              >
                <div
                  className={`max-w-[85%] md:max-w-[75%] rounded-2xl px-4 py-3 ${
                    message.role === "user"
                      ? "bg-primary text-white"
                      : "bg-white/10 text-slate-200"
                  }`}
                >
                  {message.role === "assistant" ? (
                    <ReactMarkdown
                      components={{
                        p: ({ node, ...props }) => (
                          <p className="mb-2 last:mb-0 leading-relaxed" {...props} />
                        ),
                        ul: ({ node, ...props }) => (
                          <ul className="list-disc pl-5 mb-2 space-y-1" {...props} />
                        ),
                        ol: ({ node, ...props }) => (
                          <ol className="list-decimal pl-5 mb-2 space-y-1" {...props} />
                        ),
                        li: ({ node, ...props }) => <li className="pl-1" {...props} />,
                        strong: ({ node, ...props }) => (
                          <strong className="font-semibold text-white" {...props} />
                        ),
                        code: ({ node, ...props }) => (
                          <code
                            className="bg-black/20 px-1.5 py-0.5 rounded text-sm font-mono"
                            {...props}
                          />
                        ),
                        pre: ({ node, ...props }) => (
                          <pre
                            className="bg-black/30 p-3 rounded-lg overflow-x-auto my-2"
                            {...props}
                          />
                        ),
                        a: ({ node, ...props }) => (
                          <a
                            className="text-primary-foreground underline hover:no-underline"
                            target="_blank"
                            rel="noopener noreferrer"
                            {...props}
                          />
                        ),
                      }}
                    >
                      {message.content}
                    </ReactMarkdown>
                  ) : (
                    <p className="leading-relaxed">{message.content}</p>
                  )}
                  <p className="text-xs opacity-60 mt-2">
                    {new Date(message.timestamp).toLocaleTimeString([], {
                      hour: "2-digit",
                      minute: "2-digit",
                    })}
                  </p>
                </div>
              </motion.div>
            ))}
          </AnimatePresence>

          {/* Typing Indicator / Skeleton */}
          {isLoading && messages.every(msg => msg.role === 'user') && ( // Only show skeleton if only user messages or no messages
            <ChatMessageSkeleton isUser={false} />
          )}
          {isLoading && messages.length > 0 && messages[messages.length -1].role === 'user' && ( // If last message is user and loading, show skeleton
            <ChatMessageSkeleton isUser={false} />
          )}

          {/* Error Message */}
          {error && (
            <motion.div
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              className="flex justify-center"
            >
              <div className="bg-rose-500/10 border border-rose-500/20 rounded-2xl px-4 py-3 flex items-start gap-3 max-w-[85%]">
                <AlertCircle className="w-5 h-5 text-rose-500 flex-shrink-0 mt-0.5" />
                <div className="flex-1">
                  <p className="text-sm text-rose-200">{error}</p>
                  {!error.includes("Redirecting") && (
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={handleRetry}
                      className="mt-2 text-rose-200 hover:text-white hover:bg-rose-500/20"
                    >
                      <RefreshCw className="w-3 h-3 mr-1" />
                      Retry
                    </Button>
                  )}
                </div>
              </div>
            </motion.div>
          )}

          <div ref={messagesEndRef} />
        </div>
      </ScrollArea>

      {/* Input Area */}
      <div className="p-4 border-t border-white/10 bg-white/5">
        <div className="flex gap-2">
          <Input
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            onKeyPress={handleKeyPress}
            placeholder="Ask about your resume, skills, or interview tips..."
            disabled={isLoading}
            className="flex-1 bg-white/10 border-white/20 text-white placeholder:text-muted-foreground focus-visible:ring-primary/50 min-h-[44px] touch-manipulation"
          />
          <Button
            onClick={handleSendMessage}
            disabled={isLoading || !inputValue.trim()}
            size="icon"
            className="bg-primary hover:bg-primary/90 text-white shadow-lg shadow-primary/20 min-w-[44px] min-h-[44px] touch-manipulation"
          >
            {isLoading ? (
              <Loader2 className="w-5 h-5 animate-spin" />
            ) : (
              <Send className="w-5 h-5" />
            )}
          </Button>
        </div>
      </div>
    </div>
  );
}

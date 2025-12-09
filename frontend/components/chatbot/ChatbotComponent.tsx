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
    <div className="flex flex-col h-full rounded-3xl border border-border bg-card/50 backdrop-blur-xl overflow-hidden shadow-sm">
      {/* Header */}
      <div className="flex items-center gap-3 px-6 py-4 border-b border-border bg-muted/20">
        <div className="w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center border border-primary/20">
          <MessageSquare className="w-5 h-5 text-primary" />
        </div>
        <div>
          <h3 className="text-sm font-bold text-foreground">AI Career Coach</h3>
          <p className="text-xs text-muted-foreground">
            Ask me anything about your resume
          </p>
        </div>
      </div>

      {/* Messages Area */}
      <ScrollArea className="flex-1 p-6" ref={scrollAreaRef}>
        <div className="space-y-6 pb-4">
          {messages.length === 0 && !isLoading && (
            <div className="flex flex-col items-center justify-center h-full py-10 text-center opacity-70">
              <div className="w-16 h-16 rounded-2xl bg-primary/5 flex items-center justify-center mb-4">
                <MessageSquare className="w-8 h-8 text-primary/50" />
              </div>
              <p className="text-foreground font-medium">No messages yet</p>
              <p className="text-muted-foreground text-sm max-w-[250px] mt-1">
                Start a conversation! Ask about your skills, interview tips, or career advice.
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
                <div className={`flex flex-col ${message.role === "user" ? "items-end" : "items-start"} max-w-[85%]`}>
                  <div
                    className={`rounded-2xl px-5 py-3 shadow-sm break-words ${
                      message.role === "user"
                        ? "bg-primary text-primary-foreground rounded-tr-sm"
                        : "bg-muted text-foreground rounded-tl-sm border border-border"
                    }`}
                  >
                    {message.role === "assistant" ? (
                      <div className="prose prose-sm dark:prose-invert max-w-none">
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
                              <strong className="font-bold text-foreground" {...props} />
                            ),
                            code: ({ node, ...props }) => (
                              <code
                                className="bg-background/50 px-1.5 py-0.5 rounded text-xs font-mono"
                                {...props}
                              />
                            ),
                            a: ({ node, ...props }) => (
                              <a
                                className="text-primary underline hover:no-underline font-medium"
                                target="_blank"
                                rel="noopener noreferrer"
                                {...props}
                              />
                            ),
                          }}
                        >
                          {message.content}
                        </ReactMarkdown>
                      </div>
                    ) : (
                      <p className="leading-relaxed whitespace-pre-wrap">{message.content}</p>
                    )}
                  </div>
                  <span className="text-[10px] text-muted-foreground mt-1 px-1">
                    {new Date(message.timestamp).toLocaleTimeString([], {
                      hour: "2-digit",
                      minute: "2-digit",
                    })}
                  </span>
                </div>
              </motion.div>
            ))}
          </AnimatePresence>

          {/* Typing Indicator / Skeleton */}
          {isLoading && messages.every(msg => msg.role === 'user') && (
            <ChatMessageSkeleton isUser={false} />
          )}
          {isLoading && messages.length > 0 && messages[messages.length -1].role === 'user' && (
             <ChatMessageSkeleton isUser={false} />
          )}

          {/* Error Message */}
          {error && (
            <motion.div
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              className="flex justify-center"
            >
              <div className="bg-destructive/10 border border-destructive/20 rounded-xl px-4 py-3 flex items-start gap-3 max-w-[90%]">
                <AlertCircle className="w-5 h-5 text-destructive flex-shrink-0 mt-0.5" />
                <div className="flex-1">
                  <p className="text-sm text-destructive-foreground font-medium">{error}</p>
                  {!error.includes("Redirecting") && (
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={handleRetry}
                      className="mt-2 h-7 text-xs border-destructive/30 text-destructive hover:bg-destructive/10 hover:text-destructive"
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
      <div className="p-4 border-t border-border bg-background/50 backdrop-blur-sm">
        <div className="flex gap-2 relative">
          <Input
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            onKeyPress={handleKeyPress}
            placeholder="Type your question..."
            disabled={isLoading}
            className="flex-1 bg-background border-input text-foreground placeholder:text-muted-foreground focus-visible:ring-primary min-h-[48px] pl-4 pr-12 rounded-2xl shadow-sm"
          />
          <Button
            onClick={handleSendMessage}
            disabled={isLoading || !inputValue.trim()}
            size="icon"
            className="absolute right-1 top-1 h-[40px] w-[40px] rounded-xl bg-primary hover:bg-primary/90 text-primary-foreground shadow-sm transition-all"
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

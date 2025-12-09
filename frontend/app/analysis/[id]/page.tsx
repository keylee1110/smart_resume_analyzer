"use client";

import { DashboardShell } from "@/components/dashboard/DashboardShell";
import { FitScoreGauge } from "@/components/analysis/FitScoreGauge";
import { SkillGapList } from "@/components/analysis/SkillGapList";
import { AiRecommendations } from "@/components/analysis/AiRecommendations";
import { ChatbotComponent } from "@/components/chatbot/ChatbotComponent";
import { use, useEffect, useState } from "react";
import { motion } from "framer-motion";
import { ArrowLeft, Download, Share2, AlertCircle, MessageSquare } from "lucide-react";
import Link from "next/link";
import { getAnalysis, AnalysisResult, getFullProfile, FullProfileData, getChatHistory, ChatMessage } from "@/lib/api";
import { Button } from "@/components/ui/button";
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from "@/components/ui/sheet";
import { Skeleton } from "@/components/ui/skeleton";
import { Card, CardContent } from "@/components/ui/card";

export default function AnalysisPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const [data, setData] = useState<AnalysisResult | null>(null);
  const [profileData, setProfileData] = useState<FullProfileData | null>(null);
  const [chatHistory, setChatHistory] = useState<ChatMessage[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isMobileChatOpen, setIsMobileChatOpen] = useState(false);

  useEffect(() => {
    let isMounted = true;
    let attempts = 0;
    const maxAttempts = 15;
    const pollInterval = 2000;

    const fetchData = async () => {
      try {
        const result = await getAnalysis(decodeURIComponent(id));

        if (!isMounted) return;

        if (result) {
          setData(result);

          const profile = await getFullProfile(decodeURIComponent(id));
          if (isMounted && profile) setProfileData(profile);

          const history = await getChatHistory(decodeURIComponent(id));
          if (isMounted && history) setChatHistory(history);

          setLoading(false);
        } else {
          if (attempts < maxAttempts) {
            attempts++;
            setTimeout(fetchData, pollInterval);
          } else {
            setError("Analysis timed out. The resume might still be processing.");
            setLoading(false);
          }
        }
      } catch (err) {
        if (!isMounted) return;
        if (attempts < maxAttempts) {
          attempts++;
          setTimeout(fetchData, pollInterval);
        } else {
          setError("Failed to load analysis. Please try again.");
          setLoading(false);
        }
      }
    };

    fetchData();
    return () => { isMounted = false; };
  }, [id]);

  if (error) {
    return (
      <DashboardShell>
        <div className="flex h-[80vh] items-center justify-center">
          <div className="text-center p-8 rounded-2xl bg-card border border-border shadow-lg">
            <AlertCircle className="w-12 h-12 text-destructive mx-auto mb-4" />
            <h2 className="text-xl font-bold text-foreground mb-2">Error Loading Analysis</h2>
            <p className="text-muted-foreground">{error}</p>
            <Link href="/resumes">
              <Button variant="outline" className="mt-6">Go Back to Resumes</Button>
            </Link>
          </div>
        </div>
      </DashboardShell>
    );
  }

  // Loading Skeleton State
  if (loading || !data) {
    return (
      <DashboardShell>
        <div className="flex flex-col lg:flex-row h-full lg:h-[calc(100vh-100px)] lg:overflow-hidden gap-6 lg:gap-0 -mx-4 lg:-mx-8 lg:-my-8 px-4 lg:px-0 py-4 lg:py-0">
          {/* LEFT PANE SKELETON */}
          <div className="w-full lg:w-[60%] lg:h-full lg:overflow-y-auto lg:px-8 lg:py-8 space-y-8 scrollbar-hide">
            {/* Header Skeleton */}
            <div className="space-y-4">
              <div className="flex justify-between items-center">
                <Skeleton className="h-10 w-10 rounded-xl" />
                <div className="flex gap-2">
                  <Skeleton className="h-9 w-9 rounded-md" />
                  <Skeleton className="h-9 w-24 rounded-md" />
                </div>
              </div>
              <div className="space-y-2">
                <Skeleton className="h-8 w-3/4 rounded-lg" />
                <Skeleton className="h-5 w-1/2 rounded-lg" />
              </div>
            </div>

            {/* Cards Grid Skeleton */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              <div className="md:col-span-1">
                <Skeleton className="h-64 w-full rounded-3xl" />
              </div>
              <div className="md:col-span-2">
                <Skeleton className="h-64 w-full rounded-3xl" />
              </div>
            </div>

            {/* Recommendations Skeleton */}
            <div className="space-y-4">
              <Skeleton className="h-7 w-48 rounded-md" />
              <Skeleton className="h-32 w-full rounded-xl" />
              <Skeleton className="h-32 w-full rounded-xl" />
            </div>
          </div>

          {/* RIGHT PANE SKELETON */}
          <div className="hidden lg:flex w-[40%] border-l border-border bg-card/30 flex-col h-full p-4 space-y-4">
            <div className="flex items-center gap-3 border-b border-border pb-4">
              <Skeleton className="h-10 w-10 rounded-full" />
              <div className="space-y-2">
                <Skeleton className="h-4 w-32" />
                <Skeleton className="h-3 w-24" />
              </div>
            </div>
            <div className="flex-1 space-y-4">
              <Skeleton className="h-16 w-3/4 rounded-xl ml-auto bg-primary/10" />
              <Skeleton className="h-24 w-5/6 rounded-xl mr-auto" />
              <Skeleton className="h-12 w-1/2 rounded-xl ml-auto bg-primary/10" />
            </div>
          </div>
        </div>
      </DashboardShell>
    );
  }

  // Determine Display Name
  const sessionName = (data.jobTitle && data.company)
    ? `${data.jobTitle} at ${data.company}`
    : (data.jobTitle || "Resume Analysis");

  return (
    <DashboardShell>
      {/* 
         LAYOUT CONTAINER
         Desktop: calc(100vh - header) to allow internal scrolling of panes.
         Mobile: Auto height.
      */}
      <div className="flex flex-col lg:flex-row h-full lg:h-[calc(100vh-100px)] lg:overflow-hidden gap-6 lg:gap-0 -mx-4 lg:-mx-8 lg:-my-8 px-4 lg:px-0 py-4 lg:py-0">

        {/* LEFT PANE: REPORT (60%) */}
        <div className="w-full lg:w-[60%] lg:h-full lg:overflow-y-auto lg:px-8 lg:py-8 space-y-8 scrollbar-hide">

          {/* Header */}
          <div className="flex flex-col gap-4">
            <div className="flex items-center justify-between">
              <Link href="/resumes" className="p-2 rounded-xl text-muted-foreground hover:text-foreground hover:bg-muted/50 transition-colors">
                <ArrowLeft className="w-5 h-5" />
              </Link>
              <div className="flex gap-2">
                <Button variant="ghost" size="icon" className="text-muted-foreground hover:text-foreground">
                  <Share2 className="w-5 h-5" />
                </Button>
                <Button variant="default" size="sm" className="gap-2 shadow-lg shadow-primary/20">
                  <Download className="w-4 h-4" />
                  Export
                </Button>
              </div>
            </div>

            <div>
              <div className="flex items-center gap-2 mb-1">
                <h1 className="text-2xl lg:text-3xl font-bold font-heading text-foreground">{sessionName}</h1>
              </div>
              <p className="text-muted-foreground">
                Analysis for <span className="text-foreground font-medium">{profileData?.name || "Candidate"}</span>
              </p>
            </div>
          </div>

          {/* Fit Score Card */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <motion.div
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              transition={{ duration: 0.4 }}
              className="md:col-span-1"
            >
              <Card className="h-full overflow-hidden border-border/50 bg-gradient-to-b from-card to-background shadow-xl">
                <CardContent className="p-6 flex flex-col items-center justify-center h-full relative">
                  <div className="absolute top-0 right-0 w-32 h-32 bg-primary/5 blur-[50px] rounded-full pointer-events-none" />
                  <h3 className="text-lg font-bold text-foreground mb-4 text-center">Fit Score</h3>
                  <FitScoreGauge score={data.fitScore ?? 0} />
                </CardContent>
              </Card>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ duration: 0.4, delay: 0.1 }}
              className="md:col-span-2"
            >
              {/* Skills Gap */}
              <div className="h-full">
                <SkillGapList
                  matchedSkills={data.matchedSkills || []}
                  missingSkills={data.missingSkills || []}
                />
              </div>
            </motion.div>
          </div>

          {/* AI Recommendations */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.4, delay: 0.2 }}
            className="pb-20 lg:pb-0"
          >
            <h3 className="text-xl font-bold text-foreground mb-6 font-heading flex items-center gap-2">
              <SparklesIcon className="w-5 h-5 text-primary" />
              Improvement Plan
            </h3>
            <AiRecommendations
              recommendations={data.improvementPlan || (data.recommendation ? [{ area: "General", advice: data.recommendation }] : [])}
            />
          </motion.div>
        </div>

        {/* RIGHT PANE: CHAT (40%) - DESKTOP ONLY */}
        <div className="hidden lg:flex w-[40%] border-l border-border bg-muted/10 flex-col h-full">
          <div className="p-4 border-b border-border flex items-center gap-3 bg-background/80 backdrop-blur-md sticky top-0 z-10">
            <div className="w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center border border-primary/20">
              <MessageSquare className="w-5 h-5 text-primary" />
            </div>
            <div>
              <h3 className="font-bold text-foreground text-sm">AI Career Coach</h3>
              <p className="text-xs text-muted-foreground">Ask anything about your resume</p>
            </div>
          </div>

          <div className="flex-1 overflow-hidden relative">
            {profileData && (
              <ChatbotComponent
                resumeId={id}
                cvText={profileData.resumeText || ""}
                jobDescription={profileData.lastAnalysis?.jobDescription || ""}
                analysisResults={{
                  fitScore: data.fitScore,
                  matchedSkills: data.matchedSkills,
                  missingSkills: data.missingSkills,
                  recommendation: data.recommendation,
                }}
                initialHistory={chatHistory}
              />
            )}
          </div>
        </div>
      </div>

      {/* MOBILE CHAT FAB & SHEET */}
      <div className="lg:hidden">
        <Sheet open={isMobileChatOpen} onOpenChange={setIsMobileChatOpen}>
          <SheetTrigger asChild>
            <Button
              className="fixed bottom-6 right-6 w-14 h-14 rounded-full shadow-2xl bg-primary hover:bg-primary/90 p-0 flex items-center justify-center z-50 animate-in zoom-in duration-300"
            >
              <MessageSquare className="w-6 h-6 text-white" />
            </Button>
          </SheetTrigger>
          <SheetContent side="bottom" className="h-[85vh] p-0 border-t border-border bg-background/95 backdrop-blur-xl rounded-t-3xl">
            <SheetHeader className="px-6 py-4 border-b border-border">
              <SheetTitle className="flex items-center gap-2">
                <MessageSquare className="w-5 h-5 text-primary" />
                AI Career Coach
              </SheetTitle>
            </SheetHeader>
            <div className="h-full pb-16">
              {profileData && (
                <ChatbotComponent
                  resumeId={id}
                  cvText={profileData.resumeText || ""}
                  jobDescription={profileData.lastAnalysis?.jobDescription || ""}
                  analysisResults={{
                    fitScore: data.fitScore,
                    matchedSkills: data.matchedSkills,
                    missingSkills: data.missingSkills,
                    recommendation: data.recommendation,
                  }}
                  initialHistory={chatHistory}
                />
              )}
            </div>
          </SheetContent>
        </Sheet>
      </div>

    </DashboardShell>
  );
}

function SparklesIcon({ className }: { className?: string }) {
  return (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className={className}>
      <path d="m12 3-1.912 5.813a2 2 0 0 1-1.275 1.275L3 12l5.813 1.912a2 2 0 0 1 1.275 1.275L12 21l1.912-5.813a2 2 0 0 1 1.275-1.275L21 12l-5.813-1.912a2 2 0 0 1-1.275-1.275L12 3Z" />
    </svg>
  )
}
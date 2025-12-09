"use client"

import { useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import { DashboardLayout } from "@/components/dashboard-layout"
import { ProtectedRoute } from "@/components/protected-route"
import { Card } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { FileText, Calendar, TrendingUp, Eye, Plus, AlertCircle } from "lucide-react"
import { getResumes, type ResumeProfile } from "@/lib/api"
import { motion, AnimatePresence } from "framer-motion"

export default function HistoryPage() {
  const router = useRouter()
  const [resumes, setResumes] = useState<ResumeProfile[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    // Fetch resumes from API
    const fetchResumes = async () => {
      try {
        setIsLoading(true)
        setError(null)
        const data = await getResumes()
        // Sort by creation date descending (most recent first)
        const sorted = data.sort((a, b) => 
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        )
        setResumes(sorted)
      } catch (err) {
        console.error("Error fetching resumes:", err)
        setError("Failed to load resume history. Please try again.")
      } finally {
        setIsLoading(false)
      }
    }

    fetchResumes()
  }, [])

  const getFitScoreBadgeVariant = (score?: number): "default" | "secondary" | "destructive" => {
    if (!score) return "secondary"
    if (score >= 75) return "default" // green
    if (score >= 50) return "secondary" // yellow
    return "destructive" // red
  }

  const getFitScoreColor = (score?: number): string => {
    if (!score) return "text-muted-foreground"
    if (score >= 75) return "text-green-500"
    if (score >= 50) return "text-yellow-500"
    return "text-red-500"
  }

  return (
    <ProtectedRoute>
      <DashboardLayout>
      <div className="space-y-6">
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
          <div>
            <h1 className="text-3xl font-bold text-foreground">My Resumes</h1>
            <p className="text-muted-foreground mt-2">View your past resume analyses and track your progress</p>
          </div>
          <Button onClick={() => router.push("/dashboard")} className="cursor-pointer">
            <Plus className="w-4 h-4 mr-2" />
            New Analysis
          </Button>
        </div>

        {isLoading ? (
           <div className="flex items-center justify-center py-20">
             <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
           </div>
        ) : error ? (
          <motion.div 
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="text-center py-20 bg-destructive/10 rounded-2xl border-2 border-dashed border-destructive/30"
          >
            <div className="w-16 h-16 rounded-full bg-destructive/20 flex items-center justify-center mx-auto mb-4">
              <AlertCircle className="w-8 h-8 text-destructive" />
            </div>
            <h3 className="text-xl font-semibold mb-2 text-foreground">Error Loading Resumes</h3>
            <p className="text-muted-foreground mb-6 max-w-md mx-auto">
              {error}
            </p>
            <Button onClick={() => window.location.reload()} variant="outline" className="cursor-pointer">
              Try Again
            </Button>
          </motion.div>
        ) : resumes.length === 0 ? (
          <motion.div 
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="text-center py-20 bg-muted/20 rounded-2xl border-2 border-dashed border-border"
          >
            <div className="w-16 h-16 rounded-full bg-muted flex items-center justify-center mx-auto mb-4">
              <FileText className="w-8 h-8 text-muted-foreground" />
            </div>
            <h3 className="text-xl font-semibold mb-2">No resumes yet</h3>
            <p className="text-muted-foreground mb-6 max-w-md mx-auto">
              Upload your resume and a job description to get your first AI-powered analysis.
            </p>
            <Button onClick={() => router.push("/dashboard")} className="cursor-pointer">
              <Plus className="w-4 h-4 mr-2" />
              Start New Analysis
            </Button>
          </motion.div>
        ) : (
          <div className="grid gap-4 sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-1">
            <AnimatePresence>
              {resumes.map((resume, index) => (
                <motion.div
                  key={resume.resumeId}
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ delay: index * 0.05 }}
                  onClick={() => router.push(`/analysis/${resume.resumeId}`)}
                >
                  <Card className="p-6 hover:shadow-lg transition-all duration-200 cursor-pointer group border-border/50 hover:border-primary/30">
                    <div className="flex items-start justify-between gap-4">
                      <div className="flex gap-4 flex-1 min-w-0">
                        <div className="w-12 h-12 rounded-lg bg-primary/10 flex items-center justify-center group-hover:scale-110 transition-transform duration-200 shrink-0">
                          <FileText className="w-6 h-6 text-primary" />
                        </div>
                        
                        <div className="flex-1 min-w-0">
                          <h3 className="text-lg font-semibold text-foreground group-hover:text-primary transition-colors truncate">
                            {resume.name || "Unnamed Candidate"}
                          </h3>
                          <p className="text-sm text-muted-foreground truncate">
                            {resume.email || "No email provided"}
                          </p>
                          
                          <div className="flex flex-wrap items-center gap-3 mt-3">
                            <div className="flex items-center gap-2 text-sm text-muted-foreground">
                              <Calendar className="w-4 h-4 shrink-0" />
                              <span className="whitespace-nowrap">
                                {new Date(resume.createdAt).toLocaleDateString()}
                              </span>
                            </div>
                            
                            {resume.lastAnalysis?.fitScore !== undefined && (
                              <>
                                <div className="flex items-center gap-2">
                                  <TrendingUp className={`w-4 h-4 shrink-0 ${getFitScoreColor(resume.lastAnalysis.fitScore)}`} />
                                  <span className={`text-sm font-semibold ${getFitScoreColor(resume.lastAnalysis.fitScore)}`}>
                                    {resume.lastAnalysis.fitScore}% Match
                                  </span>
                                </div>
                                
                                <Badge variant={getFitScoreBadgeVariant(resume.lastAnalysis.fitScore)}>
                                  {resume.lastAnalysis.fitScore >= 75 ? "High Fit" : 
                                   resume.lastAnalysis.fitScore >= 50 ? "Medium Fit" : "Low Fit"}
                                </Badge>
                              </>
                            )}
                          </div>
                        </div>
                      </div>

                      <Button 
                        variant="outline" 
                        size="sm" 
                        className="cursor-pointer opacity-0 group-hover:opacity-100 transition-opacity shrink-0 hidden sm:flex"
                      >
                        <Eye className="w-4 h-4 mr-2" />
                        View
                      </Button>
                    </div>
                  </Card>
                </motion.div>
              ))}
            </AnimatePresence>
          </div>
        )}
      </div>
    </DashboardLayout>
    </ProtectedRoute>
  )
}
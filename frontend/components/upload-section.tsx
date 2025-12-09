"use client"

import { useState } from "react"
import { Card } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Textarea } from "@/components/ui/textarea"
import { Label } from "@/components/ui/label"
import { Upload, FileText, Sparkles, Loader2 } from "lucide-react"
import { cn } from "@/lib/utils"
import { motion, AnimatePresence } from "framer-motion"
import { saveAnalysis } from "@/lib/storage"

interface UploadSectionProps {
  onAnalysisComplete: (data: any) => void
}

export function UploadSection({ onAnalysisComplete }: UploadSectionProps) {
  const [resumeFile, setResumeFile] = useState<File | null>(null)
  const [jobDescription, setJobDescription] = useState("")
  const [isAnalyzing, setIsAnalyzing] = useState(false)
  const [dragActive, setDragActive] = useState(false)

  const handleDrag = (e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    if (e.type === "dragenter" || e.type === "dragover") {
      setDragActive(true)
    } else if (e.type === "dragleave") {
      setDragActive(false)
    }
  }

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    setDragActive(false)

    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      setResumeFile(e.dataTransfer.files[0])
    }
  }

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      setResumeFile(e.target.files[0])
    }
  }

  const handleAnalyze = async () => {
    if (!resumeFile || !jobDescription) return

    setIsAnalyzing(true)

    // Simulate API call
    setTimeout(() => {
      const mockData = {
        fitScore: 78,
        matchedSkills: [
          { name: "React", level: "Advanced", yearsRequired: 3 },
          { name: "TypeScript", level: "Intermediate", yearsRequired: 2 },
          { name: "Node.js", level: "Advanced", yearsRequired: 3 },
          { name: "AWS", level: "Intermediate", yearsRequired: 2 },
        ],
        missingSkills: [
          { name: "Docker", priority: "High", estimatedLearningTime: "2-3 months" },
          { name: "Kubernetes", priority: "Medium", estimatedLearningTime: "3-4 months" },
          { name: "GraphQL", priority: "Low", estimatedLearningTime: "1-2 months" },
        ],
        recommendations: [
          {
            step: 1,
            title: "Master Docker Containerization",
            description: "Learn Docker fundamentals and container orchestration",
            resources: ["Docker Official Docs", "Udemy Docker Course"],
            duration: "2-3 months"
          },
          {
            step: 2,
            title: "Learn Kubernetes Basics",
            description: "Understand K8s architecture and deployment strategies",
            resources: ["Kubernetes.io", "Cloud Native Computing Foundation"],
            duration: "3-4 months"
          },
          {
            step: 3,
            title: "Explore GraphQL",
            description: "Build APIs with GraphQL and integrate with existing stack",
            resources: ["GraphQL.org", "Apollo GraphQL Tutorial"],
            duration: "1-2 months"
          },
        ]
      }

      // Save to local storage
      // Try to extract a job title from the first line of description, or use default
      const inferredTitle = jobDescription.split('\n')[0].substring(0, 50) || "Software Engineer"
      
      saveAnalysis({
        jobTitle: inferredTitle,
        company: "Unknown Company", // In a real app, we'd extract this too
        fitScore: mockData.fitScore,
        matchedSkills: mockData.matchedSkills,
        missingSkills: mockData.missingSkills,
        recommendations: mockData.recommendations
      })

      setIsAnalyzing(false)
      onAnalysisComplete(mockData)
    }, 2000)
  }

  const canAnalyze = resumeFile && jobDescription && !isAnalyzing

  return (
    <div className="space-y-6">
      {/* Header */}
      <motion.div 
        initial={{ opacity: 0, y: -20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
        className="text-center space-y-2"
      >
        <h1 className="text-4xl font-bold text-foreground">Analyze Your Resume</h1>
        <p className="text-lg text-muted-foreground">
          Upload your resume and job description to get AI-powered insights
        </p>
      </motion.div>

      {/* Upload Cards */}
      <div className="grid md:grid-cols-2 gap-6">
        {/* Resume Upload */}
        <motion.div
          initial={{ opacity: 0, x: -20 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: 0.5, delay: 0.1 }}
        >
          <Card className="p-6 h-full flex flex-col">
            <Label className="text-lg font-semibold mb-4 block">Resume Upload</Label>
            
            <div
              className={cn(
                "flex-1 border-2 border-dashed rounded-xl p-8 text-center transition-all duration-300 cursor-pointer flex flex-col items-center justify-center",
                dragActive ? "border-primary bg-primary/5 scale-[1.02]" : "border-border hover:border-primary/50 hover:bg-muted/30",
                resumeFile && "border-success bg-success/5"
              )}
              onDragEnter={handleDrag}
              onDragLeave={handleDrag}
              onDragOver={handleDrag}
              onDrop={handleDrop}
              onClick={() => document.getElementById("resume-upload")?.click()}
            >
              <input
                id="resume-upload"
                type="file"
                accept=".pdf,.docx"
                className="hidden"
                onChange={handleFileChange}
              />
              
              <AnimatePresence mode="wait">
                {resumeFile ? (
                  <motion.div 
                    key="file-selected"
                    initial={{ opacity: 0, scale: 0.8 }}
                    animate={{ opacity: 1, scale: 1 }}
                    exit={{ opacity: 0, scale: 0.8 }}
                    className="flex flex-col items-center gap-3"
                  >
                    <div className="w-16 h-16 rounded-full bg-success/10 flex items-center justify-center">
                      <FileText className="w-8 h-8 text-success" />
                    </div>
                    <div>
                      <p className="font-medium text-foreground">{resumeFile.name}</p>
                      <p className="text-sm text-muted-foreground">
                        {(resumeFile.size / 1024).toFixed(2)} KB
                      </p>
                    </div>
                    <Button variant="outline" size="sm" onClick={(e) => {
                      e.stopPropagation()
                      setResumeFile(null)
                    }} className="mt-2 hover:bg-destructive/10 hover:text-destructive hover:border-destructive/30">
                      Remove
                    </Button>
                  </motion.div>
                ) : (
                  <motion.div 
                    key="upload-prompt"
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    exit={{ opacity: 0 }}
                    className="flex flex-col items-center gap-3"
                  >
                    <div className="w-16 h-16 rounded-full bg-primary/10 flex items-center justify-center group-hover:scale-110 transition-transform duration-300">
                      <Upload className="w-8 h-8 text-primary" />
                    </div>
                    <div>
                      <p className="font-medium text-foreground">Drop your resume here</p>
                      <p className="text-sm text-muted-foreground">or click to browse</p>
                    </div>
                    <p className="text-xs text-muted-foreground bg-muted px-2 py-1 rounded-full">Supports PDF and DOCX (Max 10MB)</p>
                  </motion.div>
                )}
              </AnimatePresence>
            </div>
          </Card>
        </motion.div>

        {/* Job Description */}
        <motion.div
          initial={{ opacity: 0, x: 20 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: 0.5, delay: 0.2 }}
        >
          <Card className="p-6 h-full flex flex-col">
            <Label htmlFor="job-description" className="text-lg font-semibold mb-4 block">
              Job Description
            </Label>
            
            <Textarea
              id="job-description"
              placeholder="Paste the job description here...\n\nInclude:\n- Required skills\n- Experience level\n- Responsibilities\n- Qualifications"
              className="flex-1 min-h-[280px] resize-none focus-visible:ring-primary/50 transition-all duration-300 p-4 leading-relaxed"
              value={jobDescription}
              onChange={(e) => setJobDescription(e.target.value)}
            />
          </Card>
        </motion.div>
      </div>

      {/* Analyze Button */}
      <motion.div 
        className="flex justify-center pt-4"
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5, delay: 0.3 }}
      >
        <Button
          size="lg"
          className={cn(
            "px-12 py-8 text-xl rounded-2xl shadow-xl transition-all duration-300 relative overflow-hidden group",
            canAnalyze 
              ? "bg-[#F97316] hover:bg-[#EA580C] text-white hover:scale-105 hover:shadow-2xl cursor-pointer" 
              : "bg-muted text-muted-foreground cursor-not-allowed opacity-50"
          )}
          disabled={!canAnalyze}
          onClick={handleAnalyze}
        >
          {isAnalyzing ? (
            <>
              <Loader2 className="w-6 h-6 mr-3 animate-spin" />
              Analyzing Resume...
            </>
          ) : (
            <>
              <Sparkles className={cn("w-6 h-6 mr-3", canAnalyze && "animate-pulse")} />
              Analyze Resume
            </>
          )}
          
          {canAnalyze && !isAnalyzing && (
            <div className="absolute inset-0 bg-white/20 translate-x-[-100%] group-hover:translate-x-[100%] transition-transform duration-700 skew-x-12" />
          )}
        </Button>
      </motion.div>
    </div>
  )
}

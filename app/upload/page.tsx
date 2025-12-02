"use client"

import { Sidebar } from "@/components/sidebar"
import { FileUpload } from "@/components/file-upload"
import { Button } from "@/components/ui/button"
import { useState } from "react"
import Link from "next/link"

export default function UploadPage() {
  const [cvFile, setCvFile] = useState<File | null>(null)
  const [jdFile, setJdFile] = useState<File | null>(null)
  const [isAnalyzing, setIsAnalyzing] = useState(false)

  const handleAnalyze = () => {
    if (!cvFile || !jdFile) return

    setIsAnalyzing(true)
    // Simulate analysis
    setTimeout(() => {
      setIsAnalyzing(false)
      window.location.href = "/analysis/1"
    }, 2000)
  }

  const isReadyToAnalyze = cvFile && jdFile

  return (
    <div className="flex h-screen bg-background">
      <Sidebar />
      <main className="flex-1 overflow-y-auto">
        <div className="p-8 max-w-4xl">
          <div className="mb-8">
            <h1 className="text-4xl font-bold text-text">Upload Documents</h1>
            <p className="text-slate-600 mt-2">Upload your CV and job description to get started</p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-8 mb-8">
            <FileUpload
              title="Upload CV"
              description="PDF or DOCX (Max 5MB)"
              file={cvFile}
              onFileChange={setCvFile}
              acceptedFormats=".pdf,.docx"
            />
            <FileUpload
              title="Upload Job Description"
              description="PDF or DOCX (Max 5MB)"
              file={jdFile}
              onFileChange={setJdFile}
              acceptedFormats=".pdf,.docx"
            />
          </div>

          <div className="flex gap-4">
            <Button
              onClick={handleAnalyze}
              disabled={!isReadyToAnalyze || isAnalyzing}
              className="bg-primary hover:bg-primary-dark text-white font-semibold px-8 py-2"
            >
              {isAnalyzing ? "Analyzing..." : "Analyze Now"}
            </Button>
            <Link href="/dashboard">
              <Button variant="outline">Cancel</Button>
            </Link>
          </div>
        </div>
      </main>
    </div>
  )
}

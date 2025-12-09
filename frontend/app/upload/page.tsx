"use client";

import { getUploadUrl, uploadFileToS3, analyzeFit } from "@/lib/api";

import { DashboardShell } from "@/components/dashboard/DashboardShell";
import { FileDropzone } from "@/components/upload/FileDropzone";
import { UploadProgress } from "@/components/upload/UploadProgress";
import { JobDescriptionInput } from "@/components/upload/JobDescriptionInput";
import { useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { ArrowLeft, Sparkles } from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

import { ProtectedRoute } from "@/components/protected-route";

export default function UploadPage() {
  const [jdText, setJdText] = useState("");
  const [file, setFile] = useState<File | null>(null);
  const [isComplete, setIsComplete] = useState(false);
  const router = useRouter();

  const handleUploadComplete = () => {
    setIsComplete(true);
  };

  const [isAnalyzing, setIsAnalyzing] = useState(false);

  const handleAnalyze = async () => {
    if (!file || !jdText) return;

    try {
      setIsAnalyzing(true);
      toast.info("Preparing upload...");

      const { uploadUrl, key } = await getUploadUrl(file.name, file.type);
      console.log("Got upload URL:", uploadUrl);

      toast.info("Uploading resume...");
      await uploadFileToS3(uploadUrl, file);
      console.log("File uploaded successfully");

      toast.info("Analyzing fit against job description...");

      // Poll for analysis result (waiting for backend to process the file)
      let attempts = 0;
      const maxAttempts = 15; // 30 seconds timeout
      let result = null;

      while (attempts < maxAttempts && !result) {
        // Trigger Fit Analysis
        // We use the S3 Object Key as the Resume ID
        // If backend is not ready (profile not created yet), analyzeFit returns null
        result = await analyzeFit(key, jdText);
        
        if (!result) {
            attempts++;
            console.log(`Analysis polling attempt ${attempts}/${maxAttempts}...`);
            await new Promise(r => setTimeout(r, 2000));
        }
      }

      if (result) {
        toast.success("Analysis complete!");

        // Use the resume ID from the response, not the S3 key
        // The backend will return the actual resume ID after processing
        const data = result as any;
        const resumeIdFromResponse = data.ResumeId || data.resumeId || key;
        const analysisId = encodeURIComponent(resumeIdFromResponse);
        router.push(`/analysis/${analysisId}`);
      } else {
        toast.warning("Analysis continuing in background...");
        // Fallback to S3 key if no result
        const analysisId = encodeURIComponent(key);
        router.push(`/analysis/${analysisId}`);
      }

    } catch (error) {
      console.error("Upload failed:", error);
      toast.error("Failed to upload or analyze. Please try again.");
      setIsAnalyzing(false);
    }
  };

  const canProceedToUpload = jdText.length > 50;

  return (
    <ProtectedRoute>
      <DashboardShell>
        <div className="max-w-4xl mx-auto pb-20">
          {/* Header */}
          <div className="mb-8 flex items-center justify-between">
            <div className="flex items-center gap-4">
              <Link
                href="/dashboard" // Changed from "/" to "/dashboard"
                className="p-2 rounded-xl text-muted-foreground hover:text-white hover:bg-white/5 transition-colors"
              >
                <ArrowLeft className="w-5 h-5" />
              </Link>
              <div>
                <h1 className="text-3xl font-bold font-heading text-primary">Check Job Fit</h1> {/* Changed from text-white to text-primary */}
                <p className="text-muted-foreground">Compare a Resume against a Job Description.</p>
              </div>
            </div>
          </div>

          {/* Step 1: Job Description */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="mb-12"
          >
            <div className="flex items-center gap-3 mb-6 opacity-80">
              <div className="flex items-center justify-center w-8 h-8 rounded-full bg-primary text-white font-bold text-sm">1</div>
              <h2 className="text-xl font-bold text-white">Target Position</h2>
            </div>
            <JobDescriptionInput value={jdText} onChange={setJdText} />
          </motion.div>

          {/* Step 2: Resume Upload */}
          <AnimatePresence>
            {canProceedToUpload && (
              <motion.div
                initial={{ opacity: 0, height: 0 }}
                animate={{ opacity: 1, height: "auto" }}
                className="overflow-hidden"
              >
                <div className="flex items-center gap-3 mb-6 opacity-80">
                  <div className="flex items-center justify-center w-8 h-8 rounded-full bg-primary text-white font-bold text-sm">2</div>
                  <h2 className="text-xl font-bold text-white">Candidate Resume</h2>
                </div>

                <div className="min-h-[300px] flex flex-col items-center justify-center p-8 rounded-3xl border border-white/5 bg-white/5 backdrop-blur-md relative overflow-hidden">
                  {/* Decorative Background */}
                  <div className="absolute top-0 right-0 w-64 h-64 bg-primary/10 blur-[100px] rounded-full pointer-events-none" />
                  <div className="absolute bottom-0 left-0 w-64 h-64 bg-secondary/10 blur-[100px] rounded-full pointer-events-none" />

                  {!file ? (
                    <FileDropzone onFileSelect={setFile} />
                  ) : (
                    <div className="w-full">
                      <UploadProgress file={file} onComplete={handleUploadComplete} />

                      {isComplete && (
                        <motion.div
                          initial={{ opacity: 0, marginTop: 0 }}
                          animate={{ opacity: 1, marginTop: 32 }}
                          className="flex justify-center"
                        >
                          <button
                            onClick={handleAnalyze}
                            disabled={isAnalyzing}
                            className="group relative flex items-center gap-2 px-8 py-3 rounded-xl bg-primary text-white font-bold text-lg shadow-[0_0_20px_rgba(139,92,246,0.3)] hover:shadow-[0_0_40px_rgba(139,92,246,0.5)] hover:-translate-y-1 transition-all disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:translate-y-0"
                          >
                            <span className="relative z-10 flex items-center gap-2">
                              {isAnalyzing ? (
                                <Sparkles className="w-5 h-5 animate-spin" />
                              ) : (
                                <Sparkles className="w-5 h-5" />
                              )}
                              {isAnalyzing ? "Analyzing..." : "Analyze Fit"}
                            </span>
                            <div className="absolute inset-0 rounded-xl bg-gradient-to-r from-purple-500 to-pink-500 opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
                          </button>
                        </motion.div>
                      )}

                      {!isComplete && (
                        <button
                          onClick={() => setFile(null)}
                          className="mt-6 mx-auto block text-sm text-muted-foreground hover:text-white transition-colors"
                        >
                          Cancel Upload
                        </button>
                      )}
                    </div>
                  )}
                </div>
              </motion.div>
            )}
          </AnimatePresence>
        </div>
      </DashboardShell>
    </ProtectedRoute>
  );
}

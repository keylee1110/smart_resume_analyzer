"use client";

import { motion } from "framer-motion";
import { Check, Loader2, FileText } from "lucide-react";
import { useEffect, useState } from "react";

interface UploadProgressProps {
    file: File;
    onComplete: () => void;
}

export function UploadProgress({ file, onComplete }: UploadProgressProps) {
    const [progress, setProgress] = useState(0);
    const [step, setStep] = useState<"uploading" | "scanning" | "complete">("uploading");

    useEffect(() => {
        // Simulate upload progress
        const interval = setInterval(() => {
            setProgress((prev) => {
                if (prev >= 100) {
                    clearInterval(interval);
                    return 100;
                }
                // Random increment
                return Math.min(prev + Math.random() * 10, 100);
            });
        }, 200);

        return () => clearInterval(interval);
    }, []);

    useEffect(() => {
        if (progress === 100 && step === "uploading") {
            setStep("scanning");
            setTimeout(() => setStep("complete"), 2000); // Simulate scanning delay
        }
    }, [progress, step]);

    useEffect(() => {
        if (step === "complete") {
            setTimeout(onComplete, 1000);
        }
    }, [step, onComplete]);

    return (
        <div className="w-full max-w-xl mx-auto mt-8">
            <motion.div
                initial={{ opacity: 0, scale: 0.9 }}
                animate={{ opacity: 1, scale: 1 }}
                className="rounded-2xl border border-border bg-card backdrop-blur-xl p-6 shadow-2xl"
            >
                <div className="flex items-center gap-4 mb-6">
                    <div className="h-12 w-12 rounded-xl bg-purple-500/20 flex items-center justify-center border border-purple-500/30">
                        <FileText className="w-6 h-6 text-purple-400" />
                    </div>
                    <div className="flex-1 min-w-0">
                        <h4 className="font-bold text-foreground truncate">{file.name}</h4>
                        <p className="text-xs text-muted-foreground">{(file.size / 1024 / 1024).toFixed(2)} MB</p>
                    </div>
                    <div className="text-right">
                        {step === "uploading" && <span className="text-sm font-mono text-cyan-400">{Math.round(progress)}%</span>}
                        {step === "scanning" && <Loader2 className="w-5 h-5 text-accent animate-spin" />}
                        {step === "complete" && <div className="h-6 w-6 rounded-full bg-emerald-500 flex items-center justify-center"><Check className="w-4 h-4 text-primary-foreground" /></div>}
                    </div>
                </div>

                {/* Progress Bar container */}
                <div className="h-2 w-full bg-muted rounded-full overflow-hidden mb-2 relative">
                    <motion.div
                        className="h-full bg-gradient-to-r from-purple-500 to-cyan-500"
                        initial={{ width: "0%" }}
                        animate={{ width: `${progress}%` }}
                        transition={{ ease: "linear" }}
                    />
                    {/* Scan effect */}
                    {step === "scanning" && (
                        <motion.div
                            className="absolute top-0 bottom-0 w-20 bg-white/50 blur-md"
                            animate={{ left: ["-20%", "120%"] }}
                            transition={{ duration: 1.5, repeat: Infinity, ease: "easeInOut" }}
                        />
                    )}
                </div>

                <div className="flex justify-between items-center text-xs">
                    <span className={step === "uploading" ? "text-foreground font-medium" : "text-muted-foreground"}>Uploading...</span>
                    <span className={step === "scanning" ? "text-accent font-bold" : "text-muted-foreground"}>AI Scanning...</span>
                    <span className={step === "complete" ? "text-emerald-400 font-bold" : "text-muted-foreground"}>Complete</span>
                </div>
            </motion.div>
        </div>
    );
}

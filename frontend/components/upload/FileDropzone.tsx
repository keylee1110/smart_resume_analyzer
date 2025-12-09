"use client";

import { useCallback, useState } from "react";
import { useDropzone } from "react-dropzone";
import { motion, AnimatePresence } from "framer-motion";
import { UploadCloud, FileText, AlertCircle } from "lucide-react";
import { cn } from "@/lib/utils";

interface FileDropzoneProps {
    onFileSelect: (file: File) => void;
}

export function FileDropzone({ onFileSelect }: FileDropzoneProps) {
    const [error, setError] = useState<string | null>(null);

    const onDrop = useCallback((acceptedFiles: File[]) => {
        setError(null);
        if (acceptedFiles.length > 0) {
            const file = acceptedFiles[0];
            if (file.size > 5 * 1024 * 1024) { // 5MB limit
                setError("File is too large. Max 5MB.");
                return;
            }
            onFileSelect(file);
        }
    }, [onFileSelect]);

    const { getRootProps, getInputProps, isDragActive } = useDropzone({
        onDrop,
        accept: {
            'application/pdf': ['.pdf'],
            'application/vnd.openxmlformats-officedocument.wordprocessingml.document': ['.docx']
        },
        maxFiles: 1,
        multiple: false
    });

    return (
        <div className="w-full max-w-2xl mx-auto">
            <div
                {...getRootProps()}
                className={cn(
                    "relative group cursor-pointer overflow-hidden rounded-3xl border-2 border-dashed transition-all duration-300",
                    isDragActive
                        ? "border-primary bg-primary/10 shadow-[0_0_30px_rgba(139,92,246,0.3)]"
                        : "border-border bg-card/50 hover:bg-card hover:border-border"
                )}
            >
                <input {...getInputProps()} />

                {/* Animated Background Mesh */}
                <div className="absolute inset-0 z-0 opacity-0 group-hover:opacity-100 transition-opacity duration-700 pointer-events-none">
                    <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[300px] h-[300px] bg-primary/20 blur-[100px] rounded-full" />
                </div>

                <div className="relative z-10 flex flex-col items-center justify-center py-16 px-6 text-center">
                    <motion.div
                        animate={{
                            y: isDragActive ? -10 : 0,
                            scale: isDragActive ? 1.1 : 1,
                        }}
                        transition={{ type: "spring", stiffness: 300, damping: 20 }}
                        className="mb-6 p-6 rounded-full bg-muted/20 shadow-xl border border-border"
                    >
                        <UploadCloud className={cn("w-10 h-10 transition-colors", isDragActive ? "text-primary" : "text-muted-foreground")} />
                    </motion.div>

                    <h3 className="text-2xl font-bold text-foreground mb-2 font-heading">
                        {isDragActive ? "Drop it like it's hot!" : "Upload Resume"}
                    </h3>

                    <p className="text-muted-foreground max-w-sm mb-6">
                        Drag & drop your resume here, or click to browse.
                        <br />
                        <span className="text-xs text-slate-500 mt-2 block">Supports PDF & DOCX up to 5MB</span>
                    </p>

                    <div className="flex items-center gap-4 text-xs font-medium text-muted-foreground">
                        <span className="flex items-center gap-1"><FileText className="w-3 h-3" /> PDF</span>
                        <div className="w-1 h-1 rounded-full bg-border" />
                        <span className="flex items-center gap-1"><FileText className="w-3 h-3" /> DOCX</span>
                    </div>
                </div>
            </div>

            <AnimatePresence>
                {error && (
                    <motion.div
                        initial={{ opacity: 0, y: 10 }}
                        animate={{ opacity: 1, y: 0 }}
                        exit={{ opacity: 0, y: -10 }}
                        className="mt-4 p-4 rounded-xl bg-red-500/10 border border-red-500/20 flex items-center gap-3 text-red-400"
                    >
                        <AlertCircle className="w-5 h-5 flex-shrink-0" />
                        <p className="text-sm font-medium">{error}</p>
                    </motion.div>
                )}
            </AnimatePresence>
        </div>
    );
}

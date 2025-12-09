"use client";

import { motion } from "framer-motion";
import { FileText, AlignLeft } from "lucide-react";
import { cn } from "@/lib/utils";

interface JobDescriptionInputProps {
    value: string;
    onChange: (value: string) => void;
}

export function JobDescriptionInput({ value, onChange }: JobDescriptionInputProps) {
    return (
        <div className="w-full max-w-2xl mx-auto mb-8">
            <div className="flex items-center gap-2 mb-4">
                <div className="p-2 rounded-lg bg-primary/10">
                    <AlignLeft className="w-5 h-5 text-primary" />
                </div>
                <h3 className="text-lg font-bold text-foreground font-heading">Job Description</h3>
            </div>

            <motion.div
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                className="relative group rounded-3xl border border-border bg-card/50 backdrop-blur-xl focus-within:border-primary/50 focus-within:ring-1 focus-within:ring-primary/50 transition-all"
            >
                <textarea
                    value={value}
                    onChange={(e) => onChange(e.target.value)}
                    placeholder="Paste the Job Description here..."
                    className="w-full h-48 bg-transparent p-6 text-foreground placeholder:text-muted-foreground outline-none resize-none font-sans"
                />

                <div className="absolute bottom-4 right-6 text-xs text-muted-foreground flex items-center gap-2">
                    <span className={cn(value.length > 50 ? "text-emerald-400" : "text-amber-400")}>
                        {value.length} chars
                    </span>
                    {value.length > 0 && <FileText className="w-3 h-3" />}
                </div>
            </motion.div>
            <p className="mt-2 text-sm text-muted-foreground flex items-center gap-2">
                <span className="w-1.5 h-1.5 rounded-full bg-primary" />
                Paste the JD from LinkedIn, Indeed, or your internal portal.
            </p>
        </div>
    );
}

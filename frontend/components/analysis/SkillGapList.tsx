"use client";

import { motion } from "framer-motion";
import { Check, X, AlertOctagon } from "lucide-react";

interface SkillGapListProps {
    matchedSkills: string[];
    missingSkills: string[];
}

export function SkillGapList({ matchedSkills, missingSkills }: SkillGapListProps) {
    return (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            {/* Matched Skills */}
            <motion.div
                initial={{ opacity: 0, x: -20 }}
                animate={{ opacity: 1, x: 0 }}
                className="rounded-3xl border border-emerald-500/20 bg-emerald-500/5 p-6 backdrop-blur-md h-full flex flex-col"
            >
                <div className="flex items-center gap-3 mb-6 border-b border-emerald-500/10 pb-4">
                    <div className="p-2 rounded-lg bg-emerald-500/20">
                        <Check className="w-5 h-5 text-emerald-400" />
                    </div>
                    <h3 className="text-lg font-bold text-emerald-100">Matched Skills</h3>
                </div>

                <div className="flex flex-wrap gap-2 content-start">
                    {matchedSkills.map((skill, index) => (
                        <span
                            key={index}
                            className="px-3 py-1 rounded-full bg-emerald-500/20 border border-emerald-500/30 text-emerald-300 text-sm font-medium break-words max-w-full"
                        >
                            {skill}
                        </span>
                    ))}
                </div>
            </motion.div>

            {/* Missing Skills */}
            <motion.div
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.2 }}
                className="rounded-3xl border border-rose-500/20 bg-rose-500/5 p-6 backdrop-blur-md h-full flex flex-col"
            >
                <div className="flex items-center gap-3 mb-6 border-b border-rose-500/10 pb-4">
                    <div className="p-2 rounded-lg bg-rose-500/20">
                        <AlertOctagon className="w-5 h-5 text-rose-400" />
                    </div>
                    <h3 className="text-lg font-bold text-rose-100">Missing / To Improv</h3>
                </div>

                <div className="flex flex-wrap gap-2 content-start">
                    {missingSkills.map((skill, index) => (
                        <span
                            key={index}
                            className="px-3 py-1 rounded-full bg-rose-500/10 border border-rose-500/20 text-rose-400 text-sm font-medium decoration-rose-500/50 break-words max-w-full"
                        >
                            {skill}
                        </span>
                    ))}
                </div>
            </motion.div>
        </div>
    );
}

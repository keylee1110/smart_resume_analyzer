"use client";

import { motion } from "framer-motion";
import { Cpu } from "lucide-react";
import { Skeleton } from "@/components/ui/skeleton"; // Import Skeleton component

const categoryColors = [
    "bg-blue-500/10 text-blue-400 border-blue-500/20",
    "bg-purple-500/10 text-purple-400 border-purple-500/20",
    "bg-emerald-500/10 text-emerald-400 border-emerald-500/20",
    "bg-orange-500/10 text-orange-400 border-orange-500/20",
    "bg-pink-500/10 text-pink-400 border-pink-500/20",
];

interface SkillsCloudProps {
    resumes: any[];
    loading: boolean;
}

export function SkillsCloud({ resumes, loading }: SkillsCloudProps) {
    // Aggregate skills
    const skillCounts: Record<string, number> = {};
    if (!loading) {
        resumes.forEach(r => {
            if (r.skills) {
                r.skills.forEach((s: string) => {
                    skillCounts[s] = (skillCounts[s] || 0) + 1;
                });
            }
        });
    }

    // Sort by frequency and take top 20
    const topSkills = Object.entries(skillCounts)
        .sort((a, b) => b[1] - a[1])
        .slice(0, 20)
        .map(([name, count], index) => ({
            name,
            count,
            color: categoryColors[index % categoryColors.length]
        }));

    return (
        <motion.div
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ delay: 0.4 }}
            className="col-span-full lg:col-span-1 rounded-3xl border border-border bg-card backdrop-blur-xl p-6 shadow-xl"
        >
            <div className="mb-6 flex items-center gap-3">
                <div className="p-2 rounded-lg bg-pink-500/10">
                    <Cpu className="w-5 h-5 text-pink-400" />
                </div>
                <h3 className="text-lg font-bold text-foreground font-heading">Top Detected Skills</h3>
            </div>

            {loading ? (
                <div className="flex flex-wrap gap-2">
                    {[...Array(6)].map((_, i) => (
                        <Skeleton key={i} className="h-8 w-[80px] rounded-full" />
                    ))}
                </div>
            ) : topSkills.length === 0 ? (
                <div className="text-sm text-muted-foreground">No skills detected yet.</div>
            ) : (
                <div className="flex flex-wrap gap-2">
                    {topSkills.map((skill, index) => (
                        <motion.div
                            key={skill.name}
                            initial={{ opacity: 0, scale: 0 }}
                            animate={{ opacity: 1, scale: 1 }}
                            transition={{ delay: 0.5 + index * 0.05 }}
                            className={`px-3 py-1.5 rounded-full border text-sm font-medium transition-all hover:scale-105 cursor-default ${skill.color}`}
                        >
                            {skill.name} <span className="opacity-50 text-xs ml-1">{skill.count}</span>
                        </motion.div>
                    ))}
                </div>
            )}

            {/* Removed the mock stats bar at bottom as I don't have category classification yet */}
        </motion.div>
    );
}

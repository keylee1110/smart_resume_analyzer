"use client";

import { motion } from "framer-motion";
import { Skeleton } from "@/components/ui/skeleton"; // Import Skeleton component
import {
    FileText,
    Users,
    Briefcase,
    TrendingUp,
    ArrowUpRight,
    ArrowDownRight
} from "lucide-react";

interface StatsGridProps {
    resumes: any[];
    loading: boolean;
}

export function StatsGrid({ resumes, loading }: StatsGridProps) {
    // 1. Calculate Real Stats
    const totalResumes = resumes.length;

    // Unique candidates by name (primitive check)
    const uniqueCandidates = new Set(resumes.map(r => r.name)).size;

    // TODO: Connect "Interviews" and "Conversion Rate" to future features

    const stats = [
        {
            label: "Total Resumes",
            value: loading ? "" : totalResumes.toString(),
            change: "+100%", // Placeholder
            trend: "up",
            icon: FileText,
            color: "from-blue-500 to-cyan-500",
            shadow: "shadow-blue-500/20"
        },
        {
            label: "Unique Resumes", // Changed from Candidates for consistency with Job Seeker focus
            value: loading ? "" : uniqueCandidates.toString(),
            change: "+100%", // Placeholder
            trend: "up",
            icon: Users,
            color: "from-purple-500 to-pink-500",
            shadow: "shadow-purple-500/20"
        },
        // Kept purely for visual balance in dashboard for now
        {
            label: "Analyzed Skills",
            value: loading ? "" : resumes.reduce((acc, r) => acc + (r.skills?.length || 0), 0).toString(),
            change: "Total Extracted",
            trend: "up",
            icon: Briefcase,
            color: "from-orange-500 to-amber-500",
            shadow: "shadow-orange-500/20"
        },
        {
            label: "Avg Fit Score",
            value: loading ? "" : (resumes.length > 0 ? (resumes.reduce((acc, r) => acc + (r.lastAnalysis?.fitScore || 0), 0) / resumes.length).toFixed(0) + "%" : "0%"),
            change: "Overall Quality",
            trend: "up",
            icon: TrendingUp,
            color: "from-emerald-500 to-teal-500",
            shadow: "shadow-emerald-500/20"
        },
    ];

    return (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
            {stats.map((stat, index) => {
                const Icon = stat.icon;
                const isPositive = stat.trend === "up";

                return (
                    <motion.div
                        key={stat.label}
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ delay: index * 0.1 }}
                        className="group relative overflow-hidden rounded-2xl border border-border bg-card backdrop-blur-xl p-6 transition-all duration-300 hover:bg-accent/50 hover:border-primary/20 hover:shadow-xl hover:-translate-y-1"
                    >
                        {/* Glow Effect */}
                        <div className={`absolute -right-6 -top-6 h-24 w-24 rounded-full bg-gradient-to-br ${stat.color} opacity-10 blur-2xl transition-all duration-500 group-hover:opacity-20`} />

                        <div className="flex justify-between items-start mb-4">
                            {loading ? (
                                <Skeleton className="w-10 h-10 rounded-xl" />
                            ) : (
                                <div className={`p-3 rounded-xl bg-gradient-to-br ${stat.color} bg-opacity-10`}>
                                    <Icon className="w-6 h-6 text-primary" />
                                </div>
                            )}
                            {/* Detailed % changes hidden for MVP as we don't track history yet */}
                            <div className={`hidden flex items-center gap-1 text-sm font-medium ${isPositive ? 'text-emerald-400' : 'text-rose-400'}`}>
                                {isPositive ? <ArrowUpRight className="w-4 h-4" /> : <ArrowDownRight className="w-4 h-4" />}
                                {stat.change}
                            </div>
                        </div>

                        <div className="relative z-10">
                            {loading ? (
                                <Skeleton className="h-8 w-24 mb-1" />
                            ) : (
                                <h3 className="text-3xl font-bold text-foreground mb-1 font-heading">{stat.value}</h3>
                            )}
                            {loading ? (
                                <Skeleton className="h-4 w-32" />
                            ) : (
                                <p className="text-sm text-muted-foreground">{stat.label}</p>
                            )}
                        </div>
                    </motion.div>
                );
            })}
        </div>
    );
}

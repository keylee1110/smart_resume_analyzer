"use client";

import { motion } from "framer-motion";
import { MoreHorizontal, CheckCircle } from "lucide-react";
import { cn } from "@/lib/utils";
import { RecentActivityItemSkeleton } from "./recent-activity-item-skeleton";

interface RecentActivityProps {
    resumes: any[];
    loading: boolean;
}

const statusStyles = {
    analyzed: "bg-emerald-500/10 text-emerald-400 border-emerald-500/20",
    pending: "bg-amber-500/10 text-amber-400 border-amber-500/20",
    rejected: "bg-rose-500/10 text-rose-400 border-rose-500/20",
};

export function RecentActivity({ resumes, loading }: RecentActivityProps) {
    const items = loading ? [] : resumes
        .sort((a: any, b: any) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
        .slice(0, 5)
        .map((r: any) => {
            const analysis = r.lastAnalysis || {};
            const title = r.jobTitle || analysis.jobTitle;
            const company = r.company || analysis.company;
            const displayName = title && company
                ? `${title} at ${company}`
                : (title || analysis.jobDescription?.substring(0, 30) || "Job Application");

            return {
                id: r.resumeId,
                candidate: r.name || "Candidate",
                name: displayName,
                status: r.lastAnalysis ? "analyzed" : "pending",
                score: r.lastAnalysis?.fitScore || 0,
                date: new Date(r.createdAt).toLocaleDateString(),
                avatar: (r.name || "Ca").substring(0, 2).toUpperCase()
            };
        });

    return (
        <motion.div
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.3 }}
            className="col-span-full lg:col-span-1 rounded-3xl border border-border bg-card backdrop-blur-xl p-6 shadow-xl"
        >
            <div className="mb-6 flex items-center justify-between">
                <h3 className="text-lg font-bold text-foreground font-heading">Recent activity</h3>
                <button className="text-sm text-primary hover:text-primary/80 transition-colors">View All</button>
            </div>

            <div className="space-y-4">
                {loading ? (
                    [...Array(3)].map((_, i) => <RecentActivityItemSkeleton key={i} />)
                ) : items.length === 0 ? (
                    <div className="text-sm text-muted-foreground text-center py-4">No recent activity.</div>
                ) : (
                    items.map((item, index) => (
                        <motion.div
                            key={item.id}
                            initial={{ opacity: 0, x: 10 }}
                            animate={{ opacity: 1, x: 0 }}
                            transition={{ delay: 0.4 + index * 0.1 }}
                            className="group flex items-center gap-4 rounded-xl border border-border bg-muted/30 p-3 hover:bg-accent/50 transition-colors cursor-pointer"
                        >
                            <div className="relative h-10 w-10 flex-shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-indigo-500 to-purple-500 flex text-xs font-bold text-primary-foreground">
                                {item.avatar}
                                {item.status === 'analyzed' && (
                                    <div className="absolute -bottom-1 -right-1 rounded-full bg-emerald-500 border-2 border-background p-0.5">
                                        <CheckCircle className="h-2.5 w-2.5 text-primary-foreground" />
                                    </div>
                                )}
                            </div>

                            <div className="flex-1 min-w-0">
                                <p className="text-sm font-medium text-foreground truncate">{item.candidate}</p>
                                <p className="text-xs text-muted-foreground truncate">{item.name}</p>
                            </div>

                            <div className="text-right">
                                {item.status === 'analyzed' ? (
                                    <div className="flex flex-col items-end">
                                        <span className={`text-sm font-bold ${item.score && item.score > 80 ? 'text-emerald-400' : 'text-amber-400'}`}>
                                            {item.score}%
                                        </span>
                                        <span className="text-[10px] text-muted-foreground">{item.date}</span>
                                    </div>
                                ) : (
                                    <div className="flex flex-col items-end">
                                        <div className={cn("px-2 py-0.5 rounded-full border text-[10px] font-medium uppercase tracking-wider", statusStyles[item.status as keyof typeof statusStyles])}>
                                            {item.status}
                                        </div>
                                        <span className="text-[10px] text-muted-foreground mt-1">{item.date}</span>
                                    </div>
                                )}
                            </div>

                            <button className="opacity-0 group-hover:opacity-100 p-1 hover:bg-accent rounded transition-all">
                                <MoreHorizontal className="w-4 h-4 text-muted-foreground" />
                            </button>
                        </motion.div>
                    ))
                )}
            </div>
        </motion.div>
    );
}
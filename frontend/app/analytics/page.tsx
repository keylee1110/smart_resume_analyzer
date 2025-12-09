"use client";

import { DashboardShell } from "@/components/dashboard/DashboardShell";
import { ProtectedRoute } from "@/components/protected-route";
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, BarChart, Bar, Legend } from "recharts";
import { motion } from "framer-motion";
import { useEffect, useState } from "react";
import { getResumes, ResumeProfile } from "@/lib/api";
import { ChartSkeleton } from "@/components/chart-skeleton"; // Import ChartSkeleton

export default function AnalyticsPage() {
    const [trendData, setTrendData] = useState<any[]>([]);
    const [performanceData, setPerformanceData] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        async function loadData() {
            try {
                const resumes = await getResumes();

                // Helper to get display name
                const getDisplayName = (r: any) => {
                    const analysis = r.lastAnalysis || {};
                    const title = r.jobTitle || analysis.jobTitle;
                    const company = r.company || analysis.company;

                    if (title && company) return `${title} at ${company}`;
                    if (title) return title;
                    if (analysis.jobDescription) return analysis.jobDescription.substring(0, 20) + "...";

                    return r.name || "Resume";
                };

                // 1. Process Trend Data (Sort by date, take scores)
                const sorted = [...resumes].sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());

                const trends = sorted.map(r => ({
                    name: new Date(r.createdAt).toLocaleDateString(undefined, { month: 'short', day: 'numeric' }), // Format date shorter
                    score: r.lastAnalysis?.fitScore || 0,
                    fullDate: new Date(r.createdAt).toLocaleDateString(),
                    jobTitle: getDisplayName(r)
                }));
                setTrendData(trends);

                // 2. Process Recent Performance (Last 5 applications)
                const recent = sorted.slice(-5);
                const perf = recent.map(r => ({
                    name: getDisplayName(r),
                    Matched: r.lastAnalysis?.matchedSkills?.length || 0,
                    Missing: r.lastAnalysis?.missingSkills?.length || 0,
                }));
                setPerformanceData(perf);

            } catch (err) {
                console.error("Failed to load analytics", err);
            } finally {
                setLoading(false);
            }
        }
        loadData();
    }, []);

    return (
        <ProtectedRoute>
            <DashboardShell>
                <div className="max-w-6xl mx-auto pb-20">
                    <div className="mb-8">
                        <h1 className="text-3xl font-bold font-heading text-foreground">Analytics</h1>
                        <p className="text-muted-foreground">Detailed insights into your profile performance and skills.</p>
                    </div>

                    {loading ? (
                        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                            <ChartSkeleton />
                            <ChartSkeleton />
                        </div>
                    ) : trendData.length === 0 ? (
                        <div className="text-center py-20 rounded-3xl border border-white/5 bg-white/5 backdrop-blur-xl">
                            <h3 className="text-2xl font-bold text-white mb-2">No Analysis Data Yet</h3>
                            <p className="text-muted-foreground mb-6">Upload your resume to see your performance insights here.</p>
                            <a href="/upload" className="px-6 py-3 rounded-full bg-primary text-white font-semibold hover:bg-primary/90 transition-colors">
                                Start First Analysis
                            </a>
                        </div>
                    ) : (
                        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                            {/* Score Trend */}
                            <motion.div
                                initial={{ opacity: 0, scale: 0.95 }}
                                animate={{ opacity: 1, scale: 1 }}
                                className="p-6 rounded-3xl border border-white/5 bg-white/5 backdrop-blur-xl"
                            >
                                <h3 className="text-xl font-bold text-white mb-6">My Progress Trend</h3>
                                <div className="h-[300px] w-full">
                                    <ResponsiveContainer width="100%" height="100%">
                                        <AreaChart data={trendData}>
                                            <defs>
                                                <linearGradient id="colorScore" x1="0" y1="0" x2="0" y2="1">
                                                    <stop offset="5%" stopColor="#8b5cf6" stopOpacity={0.3} />
                                                    <stop offset="95%" stopColor="#8b5cf6" stopOpacity={0} />
                                                </linearGradient>
                                            </defs>
                                            <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.05)" vertical={false} />
                                            <XAxis dataKey="name" stroke="#94a3b8" tickLine={false} axisLine={false} />
                                            <YAxis stroke="#94a3b8" tickLine={false} axisLine={false} />
                                            <Tooltip
                                                contentStyle={{ backgroundColor: '#000', border: '1px solid rgba(255,255,255,0.1)', borderRadius: '12px' }}
                                                itemStyle={{ color: '#fff' }}
                                            />
                                            <Area type="monotone" dataKey="score" stroke="#8b5cf6" strokeWidth={3} fillOpacity={1} fill="url(#colorScore)" />
                                        </AreaChart>
                                    </ResponsiveContainer>
                                </div>
                            </motion.div>

                            {/* Recent Performance Bar Chart */}
                            <motion.div
                                initial={{ opacity: 0, scale: 0.95 }}
                                animate={{ opacity: 1, scale: 1 }}
                                transition={{ delay: 0.1 }}
                                className="p-6 rounded-3xl border border-white/5 bg-white/5 backdrop-blur-xl"
                            >
                                <h3 className="text-xl font-bold text-white mb-6">Skills Gap Analysis</h3>
                                <div className="h-[300px] w-full">
                                    <ResponsiveContainer width="100%" height="100%">
                                        <BarChart data={performanceData}>
                                            <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.05)" vertical={false} />
                                            <XAxis dataKey="name" stroke="#94a3b8" tickLine={false} axisLine={false} />
                                            <YAxis stroke="#94a3b8" tickLine={false} axisLine={false} />
                                            <Tooltip
                                                contentStyle={{ backgroundColor: '#000', border: '1px solid rgba(255,255,255,0.1)', borderRadius: '12px' }}
                                                cursor={{ fill: 'rgba(255,255,255,0.05)' }}
                                            />
                                            <Legend wrapperStyle={{ paddingTop: '20px' }} />
                                            <Bar dataKey="Matched" name="Matched Skills" fill="#10b981" radius={[4, 4, 0, 0]} />
                                            <Bar dataKey="Missing" name="Missing Skills" fill="#f43f5e" radius={[4, 4, 0, 0]} />
                                        </BarChart>
                                    </ResponsiveContainer>
                                </div>
                            </motion.div>
                        </div>
                    )}
                </div>
            </DashboardShell>
        </ProtectedRoute>
    );
}

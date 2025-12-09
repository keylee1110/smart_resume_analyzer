"use client";

import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from "recharts";
import { motion } from "framer-motion";

const data = [
    { name: 'Mon', resumes: 24, interviews: 12 },
    { name: 'Tue', resumes: 38, interviews: 18 },
    { name: 'Wed', resumes: 52, interviews: 22 },
    { name: 'Thu', resumes: 45, interviews: 26 },
    { name: 'Fri', resumes: 68, interviews: 32 },
    { name: 'Sat', resumes: 35, interviews: 15 },
    { name: 'Sun', resumes: 28, interviews: 10 },
];

const CustomTooltip = ({ active, payload, label }: any) => {
    if (active && payload && payload.length) {
        return (
            <div className="rounded-xl border border-border bg-card backdrop-blur-md p-4 shadow-xl">
                <p className="mb-2 font-medium text-foreground">{label}</p>
                <div className="flex flex-col gap-1">
                    {payload.map((entry: any, index: number) => (
                        <div key={index} className="flex items-center gap-2 text-sm">
                            <div className="h-2 w-2 rounded-full" style={{ backgroundColor: entry.color }} />
                            <span className="text-gray-400">{entry.name}:</span>
                            <span className="font-bold text-foreground">{entry.value}</span>
                        </div>
                    ))}
                </div>
            </div>
        );
    }
    return null;
};

export function ResumeTrendChart() {
    return (
        <motion.div
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ delay: 0.2 }}
            className="col-span-full lg:col-span-2 rounded-3xl border border-border bg-card backdrop-blur-xl p-6 shadow-xl"
        >
            <div className="mb-6 flex items-center justify-between">
                <div>
                    <h3 className="text-lg font-bold text-foreground font-heading">Activity Overview</h3>
                    <p className="text-sm text-muted-foreground">Resume uploads vs Interviews scheduled</p>
                </div>
                <select className="rounded-lg bg-muted border border-border px-3 py-1.5 text-sm text-foreground outline-none focus:ring-2 focus:ring-primary/50">
                    <option>Last 7 days</option>
                    <option>Last 30 days</option>
                    <option>Last 3 months</option>
                </select>
            </div>

            <div className="h-[300px] w-full">
                <ResponsiveContainer width="100%" height="100%">
                    <AreaChart data={data}>
                        <defs>
                            <linearGradient id="colorResumes" x1="0" y1="0" x2="0" y2="1">
                                <stop offset="5%" stopColor="#8b5cf6" stopOpacity={0.3} />
                                <stop offset="95%" stopColor="#8b5cf6" stopOpacity={0} />
                            </linearGradient>
                            <linearGradient id="colorInterviews" x1="0" y1="0" x2="0" y2="1">
                                <stop offset="5%" stopColor="#06b6d4" stopOpacity={0.3} />
                                <stop offset="95%" stopColor="#06b6d4" stopOpacity={0} />
                            </linearGradient>
                        </defs>
                        <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.05)" vertical={false} />
                        <XAxis
                            dataKey="name"
                            stroke="#94a3b8"
                            fontSize={12}
                            tickLine={false}
                            axisLine={false}
                        />
                        <YAxis
                            stroke="#94a3b8"
                            fontSize={12}
                            tickLine={false}
                            axisLine={false}
                            tickFormatter={(value) => `${value}`}
                        />
                        <Tooltip content={<CustomTooltip />} cursor={{ fill: 'transparent' }} />
                        <Area
                            type="monotone"
                            dataKey="resumes"
                            stroke="#8b5cf6"
                            strokeWidth={3}
                            fillOpacity={1}
                            fill="url(#colorResumes)"
                            name="Resumes"
                        />
                        <Area
                            type="monotone"
                            dataKey="interviews"
                            stroke="#06b6d4"
                            strokeWidth={3}
                            fillOpacity={1}
                            fill="url(#colorInterviews)"
                            name="Interviews"
                        />
                    </AreaChart>
                </ResponsiveContainer>
            </div>
        </motion.div>
    );
}

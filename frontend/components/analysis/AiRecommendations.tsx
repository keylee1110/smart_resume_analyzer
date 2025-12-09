"use client";

import { motion } from "framer-motion";
import { Sparkles, Lightbulb, ArrowRight } from "lucide-react";
import ReactMarkdown from 'react-markdown';

interface ImprovementItem {
    area: string;
    advice: string;
}

interface AiRecommendationsProps {
    recommendations: ImprovementItem[];
}

export function AiRecommendations({ recommendations }: AiRecommendationsProps) {
    if (!recommendations || recommendations.length === 0) return null;

    return (
        <div className="mt-8">
            <div className="grid grid-cols-1 gap-4">
                {recommendations.map((item, index) => (
                    <motion.div
                        key={index}
                        initial={{ opacity: 0, y: 10 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ delay: 0.1 + index * 0.1 }}
                        className="group flex flex-col md:flex-row gap-6 p-6 rounded-2xl border border-white/5 bg-white/5 hover:bg-white/10 transition-colors"
                    >
                        {/* Area Badge / Icon */}
                        <div className="flex-shrink-0 md:w-32">
                            <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-primary/10 text-primary text-xs font-bold uppercase tracking-wider mb-2">
                                <Sparkles className="w-3 h-3" />
                                {item.area || "Improvement"}
                            </div>
                        </div>

                        {/* Advice Content */}
                        <div className="flex-1 space-y-2">
                            <div className="prose prose-invert max-w-none">
                                <ReactMarkdown
                                    components={{
                                        p: ({ node, ...props }) => <p className="text-slate-300 leading-relaxed text-sm md:text-base" {...props} />,
                                        strong: ({ node, ...props }) => <strong className="text-white font-semibold" {...props} />,
                                    }}
                                >
                                    {item.advice}
                                </ReactMarkdown>
                            </div>
                        </div>
                    </motion.div>
                ))}
            </div>
        </div>
    );
}

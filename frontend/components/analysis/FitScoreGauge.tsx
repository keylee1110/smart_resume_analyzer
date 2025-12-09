"use client";

import { motion } from "framer-motion";
import { useEffect, useState } from "react";

interface FitScoreGaugeProps {
    score: number;
}

/**
 * Safe number conversion: handles string, null, undefined, and NaN values
 */
function safeParseScore(value: any): number {
    if (value === null || value === undefined) return 0;
    const parsed = typeof value === 'string' ? parseFloat(value) : Number(value);
    return isNaN(parsed) ? 0 : Math.max(0, Math.min(100, parsed));
}

export function FitScoreGauge({ score }: FitScoreGaugeProps) {
    // Safe parsing with validation
    const validScore = safeParseScore(score);
    const [displayScore, setDisplayScore] = useState(0);

    // Animate the number count-up
    useEffect(() => {
        const duration = 1500; // 1.5 seconds
        const steps = 60;
        const increment = validScore / steps;
        let current = 0;

        const timer = setInterval(() => {
            current += increment;
            if (current >= validScore) {
                setDisplayScore(validScore);
                clearInterval(timer);
            } else {
                setDisplayScore(current);
            }
        }, duration / steps);

        return () => clearInterval(timer);
    }, [validScore]);

    // SVG Circle parameters
    const radius = 70;
    const strokeWidth = 12;
    const circumference = 2 * Math.PI * radius;
    const strokeDashoffset = circumference - (validScore / 100) * circumference;

    // Color coding and messaging
    const getScoreConfig = (score: number) => {
        if (score >= 80) {
            return {
                color: "#10b981", // Emerald-500
                bgColor: "bg-emerald-500/10",
                borderColor: "border-emerald-500/20",
                textColor: "text-emerald-500",
                label: "High Match",
                icon: "🎉",
                gradient: "from-emerald-500/20 to-emerald-600/10"
            };
        } else if (score >= 51) {
            return {
                color: "#f59e0b", // Amber-500
                bgColor: "bg-amber-500/10",
                borderColor: "border-amber-500/20",
                textColor: "text-amber-500",
                label: "Good Match",
                icon: "⚡",
                gradient: "from-amber-500/20 to-amber-600/10"
            };
        } else {
            return {
                color: "#ef4444", // Red-500
                bgColor: "bg-red-500/10",
                borderColor: "border-red-500/20",
                textColor: "text-red-500",
                label: "Low Match",
                icon: "📈",
                gradient: "from-red-500/20 to-red-600/10"
            };
        }
    };

    const config = getScoreConfig(validScore);

    return (
        <div className="relative flex flex-col items-center justify-center w-full h-full">
            {/* Animated Background Glow */}
            <motion.div
                initial={{ opacity: 0, scale: 0.8 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ duration: 0.8 }}
                className={`absolute inset-0 bg-gradient-to-br ${config.gradient} blur-3xl opacity-30 rounded-full`}
            />

            {/* Circular Progress */}
            <div className="relative">
                <svg className="transform -rotate-90" width="180" height="180">
                    {/* Background Circle */}
                    <circle
                        cx="90"
                        cy="90"
                        r={radius}
                        stroke="currentColor"
                        strokeWidth={strokeWidth}
                        fill="none"
                        className="text-muted/10"
                    />

                    {/* Animated Progress Circle */}
                    <motion.circle
                        cx="90"
                        cy="90"
                        r={radius}
                        stroke={config.color}
                        strokeWidth={strokeWidth}
                        fill="none"
                        strokeLinecap="round"
                        strokeDasharray={circumference}
                        initial={{ strokeDashoffset: circumference }}
                        animate={{ strokeDashoffset }}
                        transition={{
                            duration: 1.5,
                            ease: "easeOut",
                            delay: 0.2
                        }}
                        style={{
                            filter: `drop-shadow(0 0 8px ${config.color}40)`
                        }}
                    />
                </svg>

                {/* Center Content */}
                <div className="absolute inset-0 flex flex-col items-center justify-center">
                    <motion.div
                        initial={{ opacity: 0, scale: 0.5 }}
                        animate={{ opacity: 1, scale: 1 }}
                        transition={{ delay: 0.4, duration: 0.5 }}
                        className="text-center"
                    >
                        {/* Score Percentage */}
                        <div className="flex items-baseline justify-center">
                            <span className="text-5xl font-bold text-foreground tabular-nums">
                                {Math.round(displayScore)}
                            </span>
                            <span className="text-2xl font-semibold text-muted-foreground ml-1">%</span>
                        </div>
                    </motion.div>
                </div>
            </div>

            {/* Status Badge */}
            <motion.div
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.8, duration: 0.5 }}
                className="mt-6"
            >
                <div className={`
                    px-4 py-2 rounded-full
                    ${config.bgColor}
                    ${config.borderColor}
                    border-2
                    backdrop-blur-sm
                    shadow-lg
                    flex items-center gap-2
                `}>
                    <span className="text-lg">{config.icon}</span>
                    <span className={`font-bold text-sm uppercase tracking-wider ${config.textColor}`}>
                        {config.label}
                    </span>
                </div>
            </motion.div>

            {/* Subtle pulse animation on the background */}
            <motion.div
                className={`absolute inset-0 ${config.bgColor} rounded-full opacity-20`}
                animate={{
                    scale: [1, 1.05, 1],
                    opacity: [0.2, 0.3, 0.2]
                }}
                transition={{
                    duration: 3,
                    repeat: Infinity,
                    ease: "easeInOut"
                }}
            />
        </div>
    );
}

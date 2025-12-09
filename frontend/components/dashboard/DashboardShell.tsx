"use client";

import { Sidebar } from "./Sidebar";
import { motion } from "framer-motion";

interface DashboardShellProps {
    children: React.ReactNode;
}

export function DashboardShell({ children }: DashboardShellProps) {
    return (
        <div className="min-h-screen bg-background text-foreground overflow-hidden selection:bg-primary/20">
            <Sidebar />
            <motion.main
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.2 }}
                className="md:pl-64 min-h-screen relative"
            >
                <div className="container mx-auto p-6 md:p-8 max-w-7xl">
                    {children}
                </div>
            </motion.main>
        </div>
    );
}

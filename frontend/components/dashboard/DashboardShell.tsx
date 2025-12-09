"use client";

import { useState } from "react"; // Import useState
import { Sidebar } from "./Sidebar";
import { motion } from "framer-motion";
import { cn } from "@/lib/utils"; // Import cn

interface DashboardShellProps {
    children: React.ReactNode;
}

export function DashboardShell({ children }: DashboardShellProps) {
    const [isCollapsed, setIsCollapsed] = useState(false); // Add isCollapsed state

    return (
        <div className="flex min-h-screen bg-background text-foreground overflow-hidden selection:bg-primary/20">
            <Sidebar isCollapsed={isCollapsed} setIsCollapsed={setIsCollapsed} /> {/* Pass state as props */}
            <motion.main
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.2 }}
                // Dynamically adjust padding-left based on collapsed state
                className={cn(
                    "flex-1 overflow-y-auto relative transition-all duration-300 ease-in-out",
                    isCollapsed ? "pl-[80px]" : "pl-[256px]" // 80px when collapsed, 256px when expanded
                )}
            >
                <div className="container mx-auto p-6 md:p-8 max-w-7xl">
                    {children}
                </div>
            </motion.main>
        </div>
    );
}

"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { cn } from "@/lib/utils";
import {
    LayoutDashboard,
    FileText,
    Upload,
    Settings,
    LogOut,
    Sparkles,
    BarChart2,
    Users,
} from "lucide-react";
import { motion } from "framer-motion";
import { signOut, getCurrentUser } from "aws-amplify/auth";
import { useEffect, useState } from "react";
import { toast } from "sonner";

const navItems = [
    { icon: LayoutDashboard, label: "Dashboard", href: "/dashboard" },
    { icon: Upload, label: "Check Job Fit", href: "/upload" },
    { icon: FileText, label: "My Resumes", href: "/resumes" },
    { icon: BarChart2, label: "Analytics", href: "/analytics" },
    { icon: Settings, label: "Settings", href: "/settings" },
];

export function Sidebar() {
    const pathname = usePathname();
    const router = useRouter();
    const [user, setUser] = useState<any>(null);

    useEffect(() => {
        checkUser();
    }, []);

    const checkUser = async () => {
        try {
            const currentUser = await getCurrentUser();
            setUser(currentUser);
        } catch (err) {
            console.log("User not signed in");
        }
    };

    const handleLogout = async () => {
        try {
            await signOut();
            router.push("/login");
        } catch (error) {
            console.error("Error signing out: ", error);
            toast.error("Failed to sign out");
        }
    };

    return (
        <motion.aside
            initial={{ x: -20, opacity: 0 }}
            animate={{ x: 0, opacity: 1 }}
            className="hidden md:flex flex-col w-64 h-screen fixed left-0 top-0 border-r border-sidebar-border bg-sidebar backdrop-blur-xl z-50"
        >
            {/* Logo Section */}
            <div className="p-6 flex items-center gap-2">
                <div className="relative flex items-center justify-center w-10 h-10 rounded-xl bg-gradient-to-tr from-primary to-accent shadow-lg shadow-primary/20">
                    <Sparkles className="w-6 h-6 text-primary-foreground" />
                </div>
                <span className="text-xl font-bold font-heading tracking-tight text-foreground">
                    SmartResume
                </span>
            </div>

            {/* Navigation */}
            <nav className="flex-1 px-4 py-6 space-y-2">
                {navItems.map((item) => {
                    const isActive = pathname === item.href;
                    const Icon = item.icon;

                    return (
                        <Link
                            key={item.href}
                            href={item.href}
                            className={cn(
                                "group relative flex items-center gap-3 px-4 py-3 rounded-xl transition-all duration-300",
                                isActive
                                    ? "text-primary font-semibold bg-primary/10 shadow-[0_0_20px_rgba(139,92,246,0.15)]"
                                    : "text-muted-foreground hover:text-foreground hover:bg-muted/50"
                            )}
                        >
                            {isActive && (
                                <motion.div
                                    layoutId="activeNav"
                                    className="absolute left-0 w-1 h-6 rounded-r-full bg-primary shadow-[0_0_10px_var(--primary)]"
                                />
                            )}
                            <Icon
                                className={cn(
                                    "w-5 h-5 transition-colors",
                                    isActive ? "text-primary" : "text-muted-foreground group-hover:text-foreground"
                                )}
                            />
                            <span className="font-medium">{item.label}</span>
                        </Link>
                    );
                })}
            </nav>

            {/* Footer / User Profile */}
            <div className="p-4 border-t border-sidebar-border bg-black/20">
                <button
                    onClick={handleLogout}
                    className="flex items-center gap-3 w-full p-2 rounded-xl hover:bg-muted/50 transition-colors group"
                >
                    <div className="w-8 h-8 rounded-full bg-gradient-to-tr from-cyan-500 to-blue-500 flex items-center justify-center text-xs font-bold text-primary-foreground ring-2 ring-border uppercase">
                        {user?.signInDetails?.loginId?.substring(0, 2) || user?.username?.substring(0, 2) || "US"}
                    </div>
                    <div className="flex-1 text-left overflow-hidden">
                        <p className="text-sm font-medium text-foreground truncate">
                            {user?.signInDetails?.loginId || user?.username || "User"}
                        </p>
                        <p className="text-xs text-muted-foreground">Pro Plan</p>
                    </div>
                    <LogOut className="w-4 h-4 text-muted-foreground group-hover:text-rose-400 transition-colors" />
                </button>
            </div>
        </motion.aside>
    );
}

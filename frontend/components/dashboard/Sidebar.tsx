"use client";

import {
    LayoutDashboard,
    FileText,
    Upload,
    Settings,
    LogOut,
    Sparkles,
    BarChart2,
    Users,
    ChevronLeft, // Added
    ChevronRight, // Added
} from "lucide-react";
import { motion, AnimatePresence } from "framer-motion"; // Added AnimatePresence
import { signOut, getCurrentUser } from "aws-amplify/auth";
import { useEffect, useState } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button"; // Added Button

const navItems = [
    { icon: LayoutDashboard, label: "Dashboard", href: "/dashboard" },
    { icon: Upload, label: "Check Job Fit", href: "/upload" },
    { icon: FileText, label: "My Resumes", href: "/resumes" },
    { icon: BarChart2, label: "Analytics", href: "/analytics" },
    { icon: Settings, label: "Settings", href: "/settings" },
];

interface SidebarProps {
    isCollapsed: boolean;
    setIsCollapsed: (collapsed: boolean) => void;
}

export function Sidebar({ isCollapsed, setIsCollapsed }: SidebarProps) {
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
            initial={{ width: 256 }}
            animate={{ width: isCollapsed ? 80 : 256 }}
            transition={{ duration: 0.3, ease: "easeInOut" }}
            className="flex flex-col h-screen fixed left-0 top-0 border-r border-sidebar-border bg-sidebar backdrop-blur-xl z-50 group/sidebar"
        >
            {/* Toggle Button */}
            <Button
                variant="outline"
                size="icon"
                className="absolute -right-4 top-6 h-8 w-8 rounded-full border-border bg-background shadow-md z-50 hover:bg-accent hover:text-primary md:flex items-center justify-center"
                onClick={() => setIsCollapsed(!isCollapsed)}
            >
                {isCollapsed ? <ChevronRight className="h-4 w-4" /> : <ChevronLeft className="h-4 w-4" />}
            </Button>

            {/* Logo Section */}
            <div className={cn("p-6 flex items-center h-[89px]", isCollapsed ? "justify-center px-2" : "gap-2")}>
                <div className="relative flex items-center justify-center w-10 h-10 rounded-xl bg-gradient-to-tr from-primary to-accent shadow-lg shadow-primary/20">
                    <Sparkles className="w-6 h-6 text-primary-foreground" />
                </div>
                <AnimatePresence mode="wait">
                    {!isCollapsed && (
                        <motion.span
                            initial={{ opacity: 0, width: 0 }}
                            animate={{ opacity: 1, width: "auto" }}
                            exit={{ opacity: 0, width: 0 }}
                            transition={{ duration: 0.2 }}
                            className="text-xl font-bold font-heading tracking-tight text-foreground whitespace-nowrap"
                        >
                            SmartResume
                        </motion.span>
                    )}
                </AnimatePresence>
            </div>

            {/* Navigation */}
            <nav className="flex-1 px-4 py-6 space-y-2 overflow-x-hidden">
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
                                    : "text-muted-foreground hover:text-foreground hover:bg-muted/50",
                                isCollapsed && "justify-center px-0"
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
                                    isActive ? "text-primary" : "text-muted-foreground group-hover:text-foreground",
                                    isCollapsed && "min-w-[1.25rem]"
                                )}
                            />
                            <AnimatePresence mode="wait">
                                {!isCollapsed && (
                                    <motion.span
                                        initial={{ opacity: 0, width: 0 }}
                                        animate={{ opacity: 1, width: "auto" }}
                                        exit={{ opacity: 0, width: 0 }}
                                        transition={{ duration: 0.2 }}
                                        className="font-medium whitespace-nowrap overflow-hidden"
                                    >
                                        {item.label}
                                    </motion.span>
                                )}
                            </AnimatePresence>
                        </Link>
                    );
                })}
            </nav>

            {/* Footer / User Profile */}
            <div className={cn("p-4 border-t border-sidebar-border bg-black/20", isCollapsed && "flex justify-center px-2")}>
                <button
                    onClick={handleLogout}
                    className={cn("flex items-center gap-3 w-full p-2 rounded-xl hover:bg-muted/50 transition-colors group", isCollapsed && "justify-center px-0")}
                >
                    <div className="w-8 h-8 rounded-full bg-gradient-to-tr from-cyan-500 to-blue-500 flex items-center justify-center text-xs font-bold text-primary-foreground ring-2 ring-border uppercase">
                        {user?.signInDetails?.loginId?.substring(0, 2) || user?.username?.substring(0, 2) || "US"}
                    </div>
                    <AnimatePresence mode="wait">
                        {!isCollapsed && (
                            <motion.div
                                initial={{ opacity: 0, width: 0 }}
                                animate={{ opacity: 1, width: "auto" }}
                                exit={{ opacity: 0, width: 0 }}
                                transition={{ duration: 0.2 }}
                                className="flex-1 text-left overflow-hidden whitespace-nowrap"
                            >
                                <p className="text-sm font-medium text-foreground truncate">
                                    {user?.signInDetails?.loginId || user?.username || "User"}
                                </p>
                                <p className="text-xs text-muted-foreground">Pro Plan</p>
                            </motion.div>
                        )}
                    </AnimatePresence>
                    <LogOut className={cn("w-4 h-4 text-muted-foreground group-hover:text-rose-400 transition-colors", isCollapsed && "min-w-[1rem]")} />
                </button>
            </div>
        </motion.aside>
    );
}

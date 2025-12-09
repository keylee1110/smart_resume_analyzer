"use client";

import { DashboardShell } from "@/components/dashboard/DashboardShell";
import { ProtectedRoute } from "@/components/protected-route";
import { User, Bell, Shield, LogOut, X, Check, Moon, Sun, Monitor } from "lucide-react";
import { signOut, getCurrentUser, updatePassword } from "aws-amplify/auth";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { ConfirmationDialog } from "@/components/ui/confirmation-dialog";
import { Toast } from "@/components/ui/toast";
import { motion, AnimatePresence } from "framer-motion";
import { useTheme } from "next-themes";

function SettingsContent() {
    const [user, setUser] = useState<any>(null);
    const router = useRouter();
    const { theme, setTheme } = useTheme();
    const [mounted, setMounted] = useState(false);
    
    // Dialog states
    const [showLogoutDialog, setShowLogoutDialog] = useState(false);
    const [showPasswordDialog, setShowPasswordDialog] = useState(false);
    const [showProfileDialog, setShowProfileDialog] = useState(false);
    
    // Toast state
    const [toast, setToast] = useState<{ isOpen: boolean; variant: "success" | "error" | "warning" | "info"; title: string; message?: string }>({
        isOpen: false,
        variant: "info",
        title: "",
    });

    // Form states
    const [passwordData, setPasswordData] = useState({ oldPassword: "", newPassword: "", confirmPassword: "" });
    const [profileForm, setProfileForm] = useState({ name: "", jobTitle: "" });

    useEffect(() => {
        setMounted(true);
        getCurrentUser()
            .then(u => {
                setUser(u);
                // Initialize profile form with dummy data or existing attributes if available
                setProfileForm({
                    name: u.username || "User",
                    jobTitle: "Pro Member" 
                });
            })
            .catch(() => console.log("User not signed in"));
    }, []);

    const handleSignOut = async () => {
        try {
            await signOut();
            router.push("/login");
        } catch (error) {
            console.error("Error signing out: ", error);
            setToast({
                isOpen: true,
                variant: "error",
                title: "Sign Out Failed",
                message: "Failed to sign out. Please try again.",
            });
        }
    };

    const handleChangePassword = async () => {
        if (passwordData.newPassword !== passwordData.confirmPassword) {
            setToast({
                isOpen: true,
                variant: "error",
                title: "Passwords Don't Match",
                message: "New password and confirmation password must match.",
            });
            return;
        }

        try {
            await updatePassword({
                oldPassword: passwordData.oldPassword,
                newPassword: passwordData.newPassword,
            });

            setToast({
                isOpen: true,
                variant: "success",
                title: "Password Updated",
                message: "Your password has been successfully changed.",
            });

            setShowPasswordDialog(false);
            setPasswordData({ oldPassword: "", newPassword: "", confirmPassword: "" });
        } catch (error: any) {
            setToast({
                isOpen: true,
                variant: "error",
                title: "Password Change Failed",
                message: error.message || "Failed to update password. Please try again.",
            });
        }
    };

    const handleUpdateProfile = () => {
        // Mock update - update local state to reflect changes
        // In a real app, you would call Auth.updateUserAttributes here
        setUser((prev: any) => ({
            ...prev,
            username: profileForm.name
        }));
        
        setToast({
            isOpen: true,
            variant: "success",
            title: "Profile Updated",
            message: "Your profile information has been updated successfully.",
        });
        setShowProfileDialog(false);
    };

    const handleSetting = (setting: string) => {
        switch (setting) {
            case "Profile Information":
                setShowProfileDialog(true);
                break;
            case "Email Addresses":
                setToast({
                    isOpen: true,
                    variant: "info",
                    title: "Email Management",
                    message: "Email management is handled via your identity provider.",
                });
                break;
            case "Notifications":
                setToast({
                    isOpen: true,
                    variant: "success",
                    title: "Notifications",
                    message: "You are subscribed to all important alerts.",
                });
                break;
            case "Theme":
                const newTheme = theme === "dark" ? "light" : "dark";
                setTheme(newTheme);
                setToast({
                    isOpen: true,
                    variant: "info",
                    title: "Theme Updated",
                    message: `Switched to ${newTheme === 'dark' ? 'Dark' : 'Light'} Mode.`,
                });
                break;
            case "Password":
                setShowPasswordDialog(true);
                break;
            case "2FA":
                setToast({
                    isOpen: true,
                    variant: "info",
                    title: "Two-Factor Authentication",
                    message: "2FA is currently managed by your organization's security policies.",
                });
                break;
        }
    };

    const sections = [
        {
            title: "Account",
            icon: User,
            items: [
                { label: "Profile Information", desc: "Update your name and career details" },
                { label: "Email Addresses", desc: "Manage your connected emails" },
            ]
        },
        {
            title: "Preferences",
            icon: Bell,
            items: [
                { label: "Notifications", desc: "Configure how you receive alerts" },
                { label: "Theme", desc: `Currently: ${mounted ? (theme === 'dark' ? 'Dark' : 'Light') : '...'} Mode` },
            ]
        },
        {
            title: "Security",
            icon: Shield,
            items: [
                { label: "Password", desc: "Change your password" },
                { label: "2FA", desc: "Enable two-factor authentication" },
            ]
        },
    ];

    if (!mounted) return null;

    return (
        <DashboardShell>
            <div className="max-w-4xl mx-auto pb-20">
                <div className="mb-8">
                    <h1 className="text-3xl font-bold font-heading text-foreground">Settings</h1>
                    <p className="text-muted-foreground">Manage your account and application preferences.</p>
                </div>

                <div className="space-y-8">
                    {/* User Profile Card */}
                    <div className="p-6 rounded-3xl bg-gradient-to-br from-primary/20 to-purple-900/20 border border-primary/20 flex items-center justify-between">
                        <div className="flex items-center gap-4">
                            <div className="w-20 h-20 rounded-full bg-gradient-to-tr from-cyan-500 to-blue-500 flex items-center justify-center text-3xl font-bold text-white shadow-xl ring-4 ring-black/20">
                                {user?.username?.substring(0, 2).toUpperCase() || "US"}
                            </div>
                            <div>
                                <h2 className="text-2xl font-bold text-foreground">{user?.username || "User"}</h2>
                                <p className="text-muted-foreground">{profileForm.jobTitle}</p>
                            </div>
                        </div>
                        <button
                            onClick={() => handleSetting("Profile Information")}
                            className="px-5 py-2 rounded-xl bg-primary/10 hover:bg-primary/20 text-primary font-medium transition-colors border border-primary/20"
                        >
                            Edit Profile
                        </button>
                    </div>

                    {/* Settings Grid */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        {sections.map((section) => {
                            const Icon = section.icon;
                            return (
                                <div key={section.title} className="p-6 rounded-3xl border border-border bg-card/50 backdrop-blur-sm shadow-sm">
                                    <div className="flex items-center gap-3 mb-6">
                                        <div className="p-2.5 rounded-xl bg-primary/10 text-primary">
                                            <Icon className="w-6 h-6" />
                                        </div>
                                        <h3 className="text-lg font-bold text-foreground">{section.title}</h3>
                                    </div>
                                    <div className="space-y-1">
                                        {section.items.map((item) => (
                                            <button
                                                key={item.label}
                                                onClick={() => handleSetting(item.label)}
                                                className="w-full text-left p-3 -mx-3 rounded-xl hover:bg-muted/50 transition-colors group"
                                            >
                                                <div className="font-medium text-foreground group-hover:text-primary transition-colors">{item.label}</div>
                                                <div className="text-sm text-muted-foreground">{item.desc}</div>
                                            </button>
                                        ))}
                                    </div>
                                </div>
                            );
                        })}
                    </div>

                    {/* Danger Zone */}
                    <div className="p-6 rounded-3xl border border-rose-500/20 bg-rose-500/5">
                        <h3 className="text-lg font-bold text-rose-500 mb-2">Danger Zone</h3>
                        <p className="text-muted-foreground mb-4">Sign out of your account or delete your data.</p>
                        <button
                            onClick={() => setShowLogoutDialog(true)}
                            className="flex items-center gap-2 px-6 py-3 rounded-xl bg-rose-500/10 text-rose-500 hover:bg-rose-500/20 transition-colors font-bold"
                        >
                            <LogOut className="w-4 h-4" />
                            Sign Out
                        </button>
                    </div>
                </div>

                {/* Password Change Dialog */}
                <AnimatePresence>
                    {showPasswordDialog && (
                        <>
                            <motion.div
                                initial={{ opacity: 0 }}
                                animate={{ opacity: 1 }}
                                exit={{ opacity: 0 }}
                                onClick={() => setShowPasswordDialog(false)}
                                className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50"
                            />
                            <div className="fixed inset-0 flex items-center justify-center z-50 p-4">
                                <motion.div
                                    initial={{ opacity: 0, scale: 0.95, y: 20 }}
                                    animate={{ opacity: 1, scale: 1, y: 0 }}
                                    exit={{ opacity: 0, scale: 0.95, y: 20 }}
                                    className="relative w-full max-w-md bg-card border border-border rounded-2xl p-6 shadow-2xl"
                                >
                                    <button
                                        onClick={() => setShowPasswordDialog(false)}
                                        className="absolute top-4 right-4 p-2 rounded-lg hover:bg-muted text-muted-foreground hover:text-foreground transition-colors"
                                    >
                                        <X className="w-5 h-5" />
                                    </button>

                                    <h2 className="text-2xl font-bold text-foreground mb-6">Change Password</h2>

                                    <div className="space-y-4">
                                        <div>
                                            <label className="block text-sm font-medium text-foreground mb-2">Old Password</label>
                                            <input
                                                type="password"
                                                value={passwordData.oldPassword}
                                                onChange={(e) => setPasswordData({ ...passwordData, oldPassword: e.target.value })}
                                                className="w-full px-4 py-2 rounded-lg bg-background border border-border text-foreground focus:outline-none focus:border-primary"
                                                placeholder="Enter old password"
                                            />
                                        </div>
                                        <div>
                                            <label className="block text-sm font-medium text-foreground mb-2">New Password</label>
                                            <input
                                                type="password"
                                                value={passwordData.newPassword}
                                                onChange={(e) => setPasswordData({ ...passwordData, newPassword: e.target.value })}
                                                className="w-full px-4 py-2 rounded-lg bg-background border border-border text-foreground focus:outline-none focus:border-primary"
                                                placeholder="Enter new password"
                                            />
                                        </div>
                                        <div>
                                            <label className="block text-sm font-medium text-foreground mb-2">Confirm Password</label>
                                            <input
                                                type="password"
                                                value={passwordData.confirmPassword}
                                                onChange={(e) => setPasswordData({ ...passwordData, confirmPassword: e.target.value })}
                                                className="w-full px-4 py-2 rounded-lg bg-background border border-border text-foreground focus:outline-none focus:border-primary"
                                                placeholder="Confirm new password"
                                            />
                                        </div>
                                    </div>

                                    <div className="flex items-center gap-3 mt-6">
                                        <button
                                            onClick={() => setShowPasswordDialog(false)}
                                            className="flex-1 px-4 py-2 rounded-lg border border-border hover:bg-muted text-foreground transition-colors"
                                        >
                                            Cancel
                                        </button>
                                        <button
                                            onClick={handleChangePassword}
                                            className="flex-1 px-4 py-2 rounded-lg bg-primary hover:bg-primary/90 text-primary-foreground transition-colors font-medium"
                                        >
                                            Update Password
                                        </button>
                                    </div>
                                </motion.div>
                            </div>
                        </>
                    )}
                </AnimatePresence>

                 {/* Profile Edit Dialog */}
                 <AnimatePresence>
                    {showProfileDialog && (
                        <>
                            <motion.div
                                initial={{ opacity: 0 }}
                                animate={{ opacity: 1 }}
                                exit={{ opacity: 0 }}
                                onClick={() => setShowProfileDialog(false)}
                                className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50"
                            />
                            <div className="fixed inset-0 flex items-center justify-center z-50 p-4">
                                <motion.div
                                    initial={{ opacity: 0, scale: 0.95, y: 20 }}
                                    animate={{ opacity: 1, scale: 1, y: 0 }}
                                    exit={{ opacity: 0, scale: 0.95, y: 20 }}
                                    className="relative w-full max-w-md bg-card border border-border rounded-2xl p-6 shadow-2xl"
                                >
                                    <button
                                        onClick={() => setShowProfileDialog(false)}
                                        className="absolute top-4 right-4 p-2 rounded-lg hover:bg-muted text-muted-foreground hover:text-foreground transition-colors"
                                    >
                                        <X className="w-5 h-5" />
                                    </button>

                                    <h2 className="text-2xl font-bold text-foreground mb-6">Edit Profile</h2>

                                    <div className="space-y-4">
                                        <div>
                                            <label className="block text-sm font-medium text-foreground mb-2">Display Name</label>
                                            <input
                                                type="text"
                                                value={profileForm.name}
                                                onChange={(e) => setProfileForm({ ...profileForm, name: e.target.value })}
                                                className="w-full px-4 py-2 rounded-lg bg-background border border-border text-foreground focus:outline-none focus:border-primary"
                                                placeholder="Your Name"
                                            />
                                        </div>
                                        <div>
                                            <label className="block text-sm font-medium text-foreground mb-2">Job Title / Status</label>
                                            <input
                                                type="text"
                                                value={profileForm.jobTitle}
                                                onChange={(e) => setProfileForm({ ...profileForm, jobTitle: e.target.value })}
                                                className="w-full px-4 py-2 rounded-lg bg-background border border-border text-foreground focus:outline-none focus:border-primary"
                                                placeholder="e.g. Software Engineer"
                                            />
                                        </div>
                                    </div>

                                    <div className="flex items-center gap-3 mt-6">
                                        <button
                                            onClick={() => setShowProfileDialog(false)}
                                            className="flex-1 px-4 py-2 rounded-lg border border-border hover:bg-muted text-foreground transition-colors"
                                        >
                                            Cancel
                                        </button>
                                        <button
                                            onClick={handleUpdateProfile}
                                            className="flex-1 px-4 py-2 rounded-lg bg-primary hover:bg-primary/90 text-primary-foreground transition-colors font-medium"
                                        >
                                            Save Changes
                                        </button>
                                    </div>
                                </motion.div>
                            </div>
                        </>
                    )}
                </AnimatePresence>

                {/* Logout Confirmation Dialog */}
                <ConfirmationDialog
                    isOpen={showLogoutDialog}
                    onClose={() => setShowLogoutDialog(false)}
                    onConfirm={handleSignOut}
                    title="Sign Out?"
                    message="Are you sure you want to sign out from your account?"
                    confirmText="Sign Out"
                    cancelText="Cancel"
                    variant="warning"
                />

                {/* Toast Notification */}
                <Toast
                    isOpen={toast.isOpen}
                    onClose={() => setToast({ ...toast, isOpen: false })}
                    title={toast.title}
                    message={toast.message}
                    variant={toast.variant}
                />
            </div>
        </DashboardShell>
    );
}

export default function SettingsPage() {
    return (
        <ProtectedRoute>
            <SettingsContent />
        </ProtectedRoute>
    );
}
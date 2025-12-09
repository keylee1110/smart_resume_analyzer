"use client";

import { DashboardShell } from "@/components/dashboard/DashboardShell";
import { ProtectedRoute } from "@/components/protected-route";
import { FileText, Calendar, MoreVertical, Download, Upload, Trash2, RefreshCw, Eye } from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { ConfirmationDialog } from "@/components/ui/confirmation-dialog";
import { Toast } from "@/components/ui/toast";
import { useRouter } from "next/navigation";
import { useEffect, useState, useRef } from "react";
import { getResumes, ResumeProfile, getFullProfile } from "@/lib/api";
import { ResumeCardSkeleton } from "@/components/resume-card-skeleton";

export default function ResumesPage() {
    const router = useRouter();
    const [resumes, setResumes] = useState<any[]>([]);
    const [loading, setLoading] = useState(true);
    const [openMenuId, setOpenMenuId] = useState<string | null>(null);
    const menuRef = useRef<HTMLDivElement>(null);

    // Dialog and Toast state
    const [showDeleteDialog, setShowDeleteDialog] = useState(false);
    const [resumeToDelete, setResumeToDelete] = useState<string | null>(null);
    const [toast, setToast] = useState<{ isOpen: boolean; variant: "success" | "error" | "warning" | "info"; title: string; message?: string }>({
        isOpen: false,
        variant: "info",
        title: "",
    });

    useEffect(() => {
        async function loadResumes() {
            try {
                const data = await getResumes();
                // Transform data to match UI
                const formatted = data.map(r => {
                    const analysis = r.lastAnalysis || {};
                    const title = r.jobTitle || analysis.jobTitle;
                    const company = r.company || analysis.company;

                    let displayTitle = "Job Application";
                    if (title && company) {
                        displayTitle = `${title} at ${company}`;
                    } else if (title) {
                        displayTitle = title;
                    } else if (analysis.jobDescription) {
                        displayTitle = analysis.jobDescription.substring(0, 40) + "...";
                    }

                    return {
                        id: r.resumeId,
                        name: displayTitle,
                        role: r.name || "Candidate",
                        date: new Date(r.createdAt).toLocaleDateString(),
                        status: "Analyzed",
                        score: analysis.fitScore || 0,
                        rawData: r, // Keep raw data for download
                    };
                });
                setResumes(formatted);
            } catch (err) {
                console.error(err);
            } finally {
                setLoading(false);
            }
        }
        loadResumes();
    }, []);

    // Close menu when clicking outside
    useEffect(() => {
        function handleClickOutside(event: MouseEvent) {
            if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
                setOpenMenuId(null);
            }
        }
        document.addEventListener("mousedown", handleClickOutside);
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, []);

    const handleCardClick = (id: string) => {
        router.push(`/analysis/${id}`);
    };

    const handleDownload = async (e: React.MouseEvent, resume: any) => {
        e.stopPropagation();

        try {
            // Get full profile data
            const profile = await getFullProfile(resume.id);
            if (!profile) {
                setToast({
                    isOpen: true,
                    variant: "error",
                    title: "Download Failed",
                    message: "Unable to download report. Profile not found.",
                });
                return;
            }

            // Generate a text-based report
            const report = generateReportText(resume, profile);

            // Create a Blob and download as Markdown
            const blob = new Blob([report], { type: 'text/markdown' });
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `${resume.name.replace(/[^a-z0-9]/gi, '_')}_Report.md`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);

            // Show success toast
            setToast({
                isOpen: true,
                variant: "success",
                title: "Report Downloaded",
                message: "Your resume analysis report has been downloaded successfully.",
            });
        } catch (error) {
            console.error("Error downloading report:", error);
            setToast({
                isOpen: true,
                variant: "error",
                title: "Download Failed",
                message: "Failed to download report. Please try again.",
            });
        }
    };

    const toggleMenu = (e: React.MouseEvent, resumeId: string) => {
        e.stopPropagation();
        setOpenMenuId(openMenuId === resumeId ? null : resumeId);
    };

    const handleMenuAction = (e: React.MouseEvent, action: string, resumeId: string) => {
        e.stopPropagation();
        setOpenMenuId(null);

        switch (action) {
            case "view":
                router.push(`/analysis/${resumeId}`);
                break;
            case "reanalyze":
                router.push(`/upload?resumeId=${resumeId}`);
                break;
            case "delete":
                setResumeToDelete(resumeId);
                setShowDeleteDialog(true);
                break;
        }
    };

    const handleDeleteConfirm = async () => {
        if (!resumeToDelete) return;

        try {
            // Optimistic UI update - remove from list immediately
            setResumes(prev => prev.filter(r => r.id !== resumeToDelete));

            // TODO: Implement actual backend delete API when ready
            // await deleteResume(resumeToDelete);

            setToast({
                isOpen: true,
                variant: "success",
                title: "Resume Deleted",
                message: "The resume has been successfully removed from your account.",
            });
        } catch (error) {
            console.error("Error deleting resume:", error);
            // Reload resumes on error
            const data = await getResumes();
            setResumes(data.map(r => ({
                id: r.resumeId,
                name: (r.jobTitle && r.company) ? `${r.jobTitle} at ${r.company}` : r.jobTitle || "Job Application",
                role: r.name || "Candidate",
                date: new Date(r.createdAt).toLocaleDateString(),
                status: "Analyzed",
                score: r.lastAnalysis?.fitScore || 0,
                rawData: r,
            })));

            setToast({
                isOpen: true,
                variant: "error",
                title: "Delete Failed",
                message: "Failed to delete resume. Please try again.",
            });
        } finally {
            setResumeToDelete(null);
        }
    };

    const generateReportText = (resume: any, profile: any): string => {
        const analysis = profile.lastAnalysis || {};

        return `
========================================
RESUME ANALYSIS REPORT
========================================

Position: ${resume.name}
Candidate: ${resume.role}
Date: ${resume.date}

========================================
CANDIDATE INFORMATION
========================================

Name: ${profile.name || "N/A"}
Email: ${profile.email || "N/A"}
Phone: ${profile.phone || "N/A"}

========================================
ANALYSIS RESULTS
========================================

Overall Fit Score: ${resume.score}%

Matched Skills:
${analysis.matchedSkills?.map((s: string) => `  • ${s}`).join('\n') || "  None"}

Missing Skills:
${analysis.missingSkills?.map((s: string) => `  • ${s}`).join('\n') || "  None"}

========================================
STRENGTHS
========================================

${analysis.strengths?.map((s: string) => `  • ${s}`).join('\n') || "  No strengths data available"}

========================================
AREAS FOR IMPROVEMENT
========================================

${analysis.weaknesses?.map((w: string) => `  • ${w}`).join('\n') || "  No weaknesses data available"}

========================================
RECOMMENDATIONS
========================================

${analysis.recommendation || "No recommendations available"}

${analysis.improvementPlan?.map((item: any, idx: number) => `
${idx + 1}. ${item.area}
   ${item.advice}
`).join('\n') || ""}

========================================
Generated on: ${new Date().toLocaleString()}
========================================
        `.trim();
    };

    return (
        <ProtectedRoute>
            <DashboardShell>
                <div className="max-w-5xl mx-auto pb-20">
                    <div className="flex items-center justify-between mb-8">
                        <div>
                            <h1 className="text-3xl font-bold font-heading text-white">My Resumes</h1>
                            <p className="text-muted-foreground">Manage your uploaded resumes and past analyses.</p>
                        </div>
                        <Button onClick={() => router.push("/upload")}>
                            <Upload className="mr-2 h-4 w-4" />
                            Upload New
                        </Button>
                    </div>

                    {loading ? (
                        <div className="grid gap-4">
                            {[...Array(3)].map((_, i) => (
                                <ResumeCardSkeleton key={i} />
                            ))}
                        </div>
                    ) : (
                        <div className="grid gap-4">
                            {resumes.length === 0 ? (
                                <div className="p-8 text-center text-muted-foreground border border-dashed border-white/10 rounded-xl">
                                    No resumes found. Upload one to get started!
                                </div>
                            ) : (
                                resumes.map((resume, index) => (
                                    <motion.div
                                        key={resume.id}
                                        initial={{ opacity: 0, y: 10 }}
                                        animate={{ opacity: 1, y: 0 }}
                                        transition={{ delay: index * 0.1 }}
                                        onClick={() => handleCardClick(resume.id)}
                                        className="group p-5 rounded-2xl bg-white/5 border border-white/10 hover:bg-white/10 hover:border-primary/50 transition-all cursor-pointer flex items-center justify-between"
                                    >
                                        <div className="flex items-center gap-4">
                                            <div className="w-12 h-12 rounded-xl bg-primary/10 flex items-center justify-center text-primary group-hover:scale-110 transition-transform">
                                                <FileText className="w-6 h-6" />
                                            </div>
                                            <div>
                                                <h3 className="font-semibold text-white group-hover:text-primary transition-colors text-lg">{resume.name}</h3>
                                                <div className="flex items-center gap-3 text-sm text-muted-foreground">
                                                    <span>{resume.role}</span>
                                                    <span className="w-1 h-1 rounded-full bg-white/20"></span>
                                                    <span className="flex items-center gap-1">
                                                        <Calendar className="w-3 h-3" />
                                                        {resume.date}
                                                    </span>
                                                </div>
                                            </div>
                                        </div>

                                        <div className="flex items-center gap-6">
                                            <div className="text-right hidden md:block">
                                                <div className="text-sm font-medium text-white mb-1">Match Score</div>
                                                {resume.score > 0 ? (
                                                    <div className="flex items-center justify-end gap-2">
                                                        <span className={`text-lg font-bold ${resume.score > 80 ? 'text-emerald-400' : resume.score > 60 ? 'text-yellow-400' : 'text-rose-400'}`}>
                                                            {resume.score}%
                                                        </span>
                                                    </div>
                                                ) : (
                                                    <span className="text-sm text-muted-foreground">--</span>
                                                )}
                                            </div>

                                            <Badge variant={resume.status === 'Analyzed' ? 'default' : 'secondary'} className={resume.status === 'Analyzed' ? 'bg-emerald-500/10 text-emerald-500 hover:bg-emerald-500/20' : ''}>
                                                {resume.status}
                                            </Badge>

                                            <div className="flex items-center gap-2">
                                                <button
                                                    onClick={(e) => handleDownload(e, resume)}
                                                    className="p-2 rounded-lg hover:bg-white/10 text-muted-foreground hover:text-white transition-colors"
                                                    title="Download Report"
                                                >
                                                    <Download className="w-4 h-4" />
                                                </button>
                                                <div className="relative">
                                                    <button
                                                        onClick={(e) => toggleMenu(e, resume.id)}
                                                        className="p-2 rounded-lg hover:bg-white/10 text-muted-foreground hover:text-white transition-colors"
                                                    >
                                                        <MoreVertical className="w-4 h-4" />
                                                    </button>

                                                    <AnimatePresence>
                                                        {openMenuId === resume.id && (
                                                            <motion.div
                                                                ref={menuRef}
                                                                initial={{ opacity: 0, scale: 0.95, y: -10 }}
                                                                animate={{ opacity: 1, scale: 1, y: 0 }}
                                                                exit={{ opacity: 0, scale: 0.95, y: -10 }}
                                                                transition={{ duration: 0.15 }}
                                                                className="absolute right-0 mt-2 w-48 rounded-xl bg-black/90 border border-white/10 shadow-2xl z-50 overflow-hidden backdrop-blur-xl"
                                                            >
                                                                <div className="py-1">
                                                                    <button
                                                                        onClick={(e) => handleMenuAction(e, 'view', resume.id)}
                                                                        className="w-full px-4 py-2.5 text-left text-sm text-white hover:bg-white/10 transition-colors flex items-center gap-3"
                                                                    >
                                                                        <Eye className="w-4 h-4 text-blue-400" />
                                                                        <span>View Details</span>
                                                                    </button>
                                                                    <button
                                                                        onClick={(e) => handleMenuAction(e, 'reanalyze', resume.id)}
                                                                        className="w-full px-4 py-2.5 text-left text-sm text-white hover:bg-white/10 transition-colors flex items-center gap-3"
                                                                    >
                                                                        <RefreshCw className="w-4 h-4 text-green-400" />
                                                                        <span>Re-analyze</span>
                                                                    </button>
                                                                    <div className="border-t border-white/10 my-1"></div>
                                                                    <button
                                                                        onClick={(e) => handleMenuAction(e, 'delete', resume.id)}
                                                                        className="w-full px-4 py-2.5 text-left text-sm text-red-400 hover:bg-red-500/10 transition-colors flex items-center gap-3"
                                                                    >
                                                                        <Trash2 className="w-4 h-4" />
                                                                        <span>Delete</span>
                                                                    </button>
                                                                </div>
                                                            </motion.div>
                                                        )}
                                                    </AnimatePresence>
                                                </div>
                                            </div>
                                        </div>
                                    </motion.div>
                                ))
                            )}
                        </div>
                    )}
                </div>

                {/* Confirmation Dialog */}
                <ConfirmationDialog
                    isOpen={showDeleteDialog}
                    onClose={() => {
                        setShowDeleteDialog(false);
                        setResumeToDelete(null);
                    }}
                    onConfirm={handleDeleteConfirm}
                    title="Delete Resume?"
                    message="Are you sure you want to delete this resume analysis? This action cannot be undone and all associated data will be permanently removed."
                    confirmText="Delete"
                    cancelText="Cancel"
                    variant="danger"
                />

                {/* Toast Notification */}
                <Toast
                    isOpen={toast.isOpen}
                    onClose={() => setToast({ ...toast, isOpen: false })}
                    title={toast.title}
                    message={toast.message}
                    variant={toast.variant}
                />
            </DashboardShell>
        </ProtectedRoute>
    );
}

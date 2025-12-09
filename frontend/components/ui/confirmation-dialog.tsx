"use client";

import { motion, AnimatePresence } from "framer-motion";
import { AlertTriangle, X } from "lucide-react";
import { Button } from "./button";

interface ConfirmationDialogProps {
    isOpen: boolean;
    onClose: () => void;
    onConfirm: () => void;
    title: string;
    message: string;
    confirmText?: string;
    cancelText?: string;
    variant?: "danger" | "warning" | "info";
    icon?: React.ReactNode;
}

export function ConfirmationDialog({
    isOpen,
    onClose,
    onConfirm,
    title,
    message,
    confirmText = "Confirm",
    cancelText = "Cancel",
    variant = "danger",
    icon,
}: ConfirmationDialogProps) {
    const variantStyles = {
        danger: {
            iconBg: "bg-red-500/10",
            iconColor: "text-red-500",
            buttonBg: "bg-red-500 hover:bg-red-600",
        },
        warning: {
            iconBg: "bg-yellow-500/10",
            iconColor: "text-yellow-500",
            buttonBg: "bg-yellow-500 hover:bg-yellow-600",
        },
        info: {
            iconBg: "bg-blue-500/10",
            iconColor: "text-blue-500",
            buttonBg: "bg-blue-500 hover:bg-blue-600",
        },
    };

    const styles = variantStyles[variant];

    return (
        <AnimatePresence>
            {isOpen && (
                <>
                    {/* Backdrop */}
                    <motion.div
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        onClick={onClose}
                        className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50"
                    />

                    {/* Dialog */}
                    <div className="fixed inset-0 flex items-center justify-center z-50 p-4">
                        <motion.div
                            initial={{ opacity: 0, scale: 0.95, y: 20 }}
                            animate={{ opacity: 1, scale: 1, y: 0 }}
                            exit={{ opacity: 0, scale: 0.95, y: 20 }}
                            transition={{ type: "spring", duration: 0.5 }}
                            className="relative w-full max-w-md bg-gradient-to-br from-gray-900 to-black border border-white/10 rounded-2xl shadow-2xl overflow-hidden"
                        >
                            {/* Gradient Overlay */}
                            <div className="absolute inset-0 bg-gradient-to-br from-primary/5 to-transparent pointer-events-none" />

                            {/* Close Button */}
                            <button
                                onClick={onClose}
                                className="absolute top-4 right-4 p-2 rounded-lg hover:bg-white/10 text-muted-foreground hover:text-white transition-colors z-10"
                            >
                                <X className="w-5 h-5" />
                            </button>

                            <div className="relative p-6">
                                {/* Icon */}
                                <div className={`w-14 h-14 rounded-2xl ${styles.iconBg} flex items-center justify-center mb-4`}>
                                    {icon || <AlertTriangle className={`w-7 h-7 ${styles.iconColor}`} />}
                                </div>

                                {/* Content */}
                                <h2 className="text-2xl font-bold text-white mb-2 font-heading">
                                    {title}
                                </h2>
                                <p className="text-muted-foreground mb-6 leading-relaxed">
                                    {message}
                                </p>

                                {/* Actions */}
                                <div className="flex items-center gap-3">
                                    <Button
                                        onClick={onClose}
                                        variant="outline"
                                        className="flex-1 border-white/10 hover:bg-white/5"
                                    >
                                        {cancelText}
                                    </Button>
                                    <Button
                                        onClick={() => {
                                            onConfirm();
                                            onClose();
                                        }}
                                        className={`flex-1 ${styles.buttonBg} text-white shadow-lg`}
                                    >
                                        {confirmText}
                                    </Button>
                                </div>
                            </div>
                        </motion.div>
                    </div>
                </>
            )}
        </AnimatePresence>
    );
}

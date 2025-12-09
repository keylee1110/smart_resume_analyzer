"use client";

import { motion, AnimatePresence } from "framer-motion";
import { CheckCircle2, XCircle, AlertCircle, Info, X } from "lucide-react";
import { useEffect } from "react";

interface ToastProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  message?: string;
  variant?: "success" | "error" | "warning" | "info";
  duration?: number;
}

export function Toast({
  isOpen,
  onClose,
  title,
  message,
  variant = "info",
  duration = 5000,
}: ToastProps) {
  useEffect(() => {
    if (isOpen && duration > 0) {
      const timer = setTimeout(onClose, duration);
      return () => clearTimeout(timer);
    }
  }, [isOpen, duration, onClose]);

  const variantConfig = {
    success: {
      icon: CheckCircle2,
      iconColor: "text-emerald-500",
      bgGradient: "from-emerald-500/20 to-emerald-500/5",
      borderColor: "border-emerald-500/30",
    },
    error: {
      icon: XCircle,
      iconColor: "text-red-500",
      bgGradient: "from-red-500/20 to-red-500/5",
      borderColor: "border-red-500/30",
    },
    warning: {
      icon: AlertCircle,
      iconColor: "text-yellow-500",
      bgGradient: "from-yellow-500/20 to-yellow-500/5",
      borderColor: "border-yellow-500/30",
    },
    info: {
      icon: Info,
      iconColor: "text-blue-500",
      bgGradient: "from-blue-500/20 to-blue-500/5",
      borderColor: "border-blue-500/30",
    },
  };

  const config = variantConfig[variant];
  const Icon = config.icon;

  return (
    <AnimatePresence>
      {isOpen && (
        <motion.div
          initial={{ opacity: 0, y: -50, scale: 0.9 }}
          animate={{ opacity: 1, y: 0, scale: 1 }}
          exit={{ opacity: 0, y: -20, scale: 0.95 }}
          transition={{ type: "spring", stiffness: 500, damping: 30 }}
          className="fixed top-4 right-4 z-[100] max-w-md"
        >
          <div
            className={`bg-gradient-to-br ${config.bgGradient} backdrop-blur-xl border ${config.borderColor} rounded-2xl shadow-2xl overflow-hidden`}
          >
            <div className="flex items-start gap-3 p-4">
              <div className={`flex-shrink-0 ${config.iconColor}`}>
                <Icon className="w-6 h-6" />
              </div>
              <div className="flex-1 min-w-0">
                <h3 className="font-semibold text-white mb-1">
                  {title}
                </h3>
                {message && (
                  <p className="text-sm text-muted-foreground">
                    {message}
                  </p>
                )}
              </div>
              <button
                onClick={onClose}
                className="flex-shrink-0 p-1 rounded-lg hover:bg-white/10 text-muted-foreground hover:text-white transition-colors"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            {/* Progress bar */}
            {duration > 0 && (
              <motion.div
                initial={{ width: "100%" }}
                animate={{ width: "0%" }}
                transition={{ duration: duration / 1000, ease: "linear" }}
                className={`h-1 bg-gradient-to-r ${config.iconColor.replace('text-', 'from-')} to-transparent`}
              />
            )}
          </div>
        </motion.div>
      )}
    </AnimatePresence>
  );
}

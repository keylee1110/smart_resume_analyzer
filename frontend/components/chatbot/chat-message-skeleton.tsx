import { Skeleton } from "@/components/ui/skeleton";
import { motion } from "framer-motion";

interface ChatMessageSkeletonProps {
  isUser?: boolean;
}

export function ChatMessageSkeleton({ isUser = false }: ChatMessageSkeletonProps) {
  return (
    <motion.div
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      className={`flex ${isUser ? "justify-end" : "justify-start"}`}
    >
      <div
        className={`max-w-[85%] md:max-w-[75%] rounded-2xl px-4 py-3 ${
          isUser ? "bg-primary text-white" : "bg-white/10 text-slate-200"
        }`}
      >
        <Skeleton className="h-4 w-48 mb-2 bg-gray-700/50" />
        <Skeleton className="h-3 w-32 bg-gray-700/50" />
      </div>
    </motion.div>
  );
}

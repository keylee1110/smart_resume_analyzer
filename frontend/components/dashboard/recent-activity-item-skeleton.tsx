import { Skeleton } from "@/components/ui/skeleton";
import { motion } from "framer-motion";

export function RecentActivityItemSkeleton() {
  return (
    <motion.div
      className="group flex items-center gap-4 rounded-xl border border-white/5 bg-white/5 p-3"
    >
      <Skeleton className="relative h-10 w-10 flex-shrink-0 rounded-full bg-gray-700/50" />
      <div className="flex-1 min-w-0 space-y-1">
        <Skeleton className="h-4 w-3/4 bg-gray-700/50" />
        <Skeleton className="h-3 w-1/2 bg-gray-700/50" />
      </div>
      <div className="text-right space-y-1">
        <Skeleton className="h-4 w-12 bg-gray-700/50" />
        <Skeleton className="h-3 w-16 bg-gray-700/50" />
      </div>
      <Skeleton className="w-8 h-8 rounded bg-gray-700/50" />
    </motion.div>
  );
}

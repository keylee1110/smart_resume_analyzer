import { Skeleton } from "@/components/ui/skeleton";
import { Card } from "@/components/ui/card";

export function ResumeCardSkeleton() {
  return (
    <Card className="p-5 rounded-2xl bg-white/5 border border-white/10 flex items-center justify-between">
      <div className="flex items-center gap-4">
        <Skeleton className="w-12 h-12 rounded-xl bg-gray-700/50" />
        <div>
          <Skeleton className="h-4 w-48 mb-2 bg-gray-700/50" />
          <Skeleton className="h-3 w-32 bg-gray-700/50" />
        </div>
      </div>
      <div className="flex items-center gap-6">
        <div className="text-right hidden md:block">
          <Skeleton className="h-4 w-24 mb-1 bg-gray-700/50" />
          <Skeleton className="h-5 w-16 bg-gray-700/50" />
        </div>
        <Skeleton className="h-8 w-20 rounded-full bg-gray-700/50" />
        <div className="flex items-center gap-2">
          <Skeleton className="h-8 w-8 rounded-lg bg-gray-700/50" />
          <Skeleton className="h-8 w-8 rounded-lg bg-gray-700/50" />
        </div>
      </div>
    </Card>
  );
}

import { Skeleton } from "@/components/ui/skeleton";

export function ChartSkeleton() {
  return (
    <div className="p-6 rounded-3xl border border-white/5 bg-white/5 backdrop-blur-xl h-[380px] flex flex-col justify-between">
      <Skeleton className="h-6 w-3/4 mb-6 bg-gray-700/50" />
      <div className="flex-1 flex items-end justify-around pb-4">
        {[...Array(5)].map((_, i) => (
          <Skeleton
            key={i}
            className="w-10 rounded-t-lg bg-gray-700/50"
            style={{ height: `${Math.random() * 70 + 30}%` }} // Random height for visual variation
          />
        ))}
      </div>
      <div className="flex justify-between mt-4">
        {[...Array(5)].map((_, i) => (
          <Skeleton key={i} className="h-3 w-12 bg-gray-700/50" />
        ))}
      </div>
    </div>
  );
}

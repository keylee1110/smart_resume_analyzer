"use client";

import { DashboardShell } from "@/components/dashboard/DashboardShell";
import { StatsGrid } from "@/components/dashboard/StatsGrid";
import { ProtectedRoute } from "@/components/protected-route";
import dynamic from 'next/dynamic';
import { Skeleton } from "@/components/ui/skeleton";

// Lazy load heavy chart components


const RecentActivity = dynamic(() => import('@/components/dashboard/RecentActivity').then(mod => mod.RecentActivity), {
  loading: () => <Skeleton className="w-full h-[400px] rounded-3xl" />,
});

export default function Home() {
  return (
    <ProtectedRoute>
      <DashboardShell>
        <div className="max-w-7xl mx-auto space-y-8 pb-20">
          {/* Header */}
          <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
            <div>
              <h1 className="text-3xl font-bold font-heading text-white">Dashboard</h1>
              <p className="text-muted-foreground">Welcome back, Khoa Lee</p>
            </div>
            <div className="flex items-center gap-3">
              <button className="flex items-center gap-2 px-4 py-2 rounded-xl bg-white/5 border border-white/10 hover:bg-white/10 transition-colors text-sm font-medium">
                <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />
                System Online
              </button>
            </div>
          </div>

          <StatsGrid />

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            <div className="lg:col-span-3">
              <RecentActivity />
            </div>
          </div>
        </div>
      </DashboardShell>
    </ProtectedRoute>
  );
}

"use client"

import { Sidebar } from "@/components/sidebar"
import { OverviewCards } from "@/components/overview-cards"
import { RecentAnalyses } from "@/components/recent-analyses"

export default function DashboardPage() {
  return (
    <div className="flex h-screen bg-background">
      <Sidebar />
      <main className="flex-1 overflow-y-auto">
        <div className="p-8 max-w-7xl">
          <div className="mb-8">
            <h1 className="text-4xl font-bold text-text">Dashboard</h1>
            <p className="text-slate-600 mt-2">Welcome back! Here's your analysis overview.</p>
          </div>

          <OverviewCards />

          <div className="mt-8">
            <RecentAnalyses />
          </div>
        </div>
      </main>
    </div>
  )
}

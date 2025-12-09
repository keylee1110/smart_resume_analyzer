"use client";

import { DashboardShell } from "@/components/dashboard/DashboardShell";
import { StatsGrid } from "@/components/dashboard/StatsGrid";
import { RecentActivity } from "@/components/dashboard/RecentActivity";
import { SkillsCloud } from "@/components/dashboard/SkillsCloud";
import { ProtectedRoute } from "@/components/protected-route";
import { motion } from "framer-motion";
import { useEffect, useState } from "react";
import { getResumes, ResumeProfile } from "@/lib/api";

export default function DashboardOverviewPage() {
  const [resumes, setResumes] = useState<ResumeProfile[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadData() {
      try {
        const data = await getResumes();
        setResumes(data);
      } catch (err) {
        console.error("Failed to load dashboard data", err);
      } finally {
        setLoading(false);
      }
    }
    loadData();
  }, []);

  return (
    <ProtectedRoute>
      <DashboardShell>
        <div className="space-y-8">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold font-heading text-foreground">Dashboard</h1>
              <p className="text-muted-foreground">Welcome back, here's what's happening today.</p>
            </div>
            <div className="text-sm text-right hidden sm:block">
              <p className="text-foreground font-medium">Last updated</p>
              <p className="text-muted-foreground">{new Date().toLocaleTimeString()}</p>
            </div>
          </div>

          <StatsGrid resumes={resumes} loading={loading} />

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            <div className="lg:col-span-2">
              <RecentActivity resumes={resumes} loading={loading} />
            </div>
            <div className="lg:col-span-1">
              <SkillsCloud resumes={resumes} loading={loading} />
            </div>
          </div>
        </div>
      </DashboardShell>
    </ProtectedRoute>
  );
}
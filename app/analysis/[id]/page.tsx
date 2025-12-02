"use client"

import { Sidebar } from "@/components/sidebar"
import { FitScoreGauge } from "@/components/fit-score-gauge"
import { MatchedSkillsTable } from "@/components/matched-skills-table"
import { MissingSkillsTable } from "@/components/missing-skills-table"
import { Recommendations } from "@/components/recommendations"
import { Button } from "@/components/ui/button"
import Link from "next/link"

export default function AnalysisPage({ params }: { params: { id: string } }) {
  return (
    <div className="flex h-screen bg-background">
      <Sidebar />
      <main className="flex-1 overflow-y-auto">
        <div className="p-8 max-w-7xl">
          <div className="mb-8 flex justify-between items-start">
            <div>
              <h1 className="text-4xl font-bold text-text">Analysis Results</h1>
              <p className="text-slate-600 mt-2">Analysis ID: {params.id}</p>
            </div>
            <Link href="/upload">
              <Button className="bg-primary hover:bg-primary-dark text-white">New Analysis</Button>
            </Link>
          </div>

          {/* Fit Score Gauge */}
          <div className="mb-8">
            <FitScoreGauge />
          </div>

          {/* Tables Grid */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-8">
            <MatchedSkillsTable />
            <MissingSkillsTable />
          </div>

          {/* Recommendations */}
          <div>
            <Recommendations />
          </div>
        </div>
      </main>
    </div>
  )
}

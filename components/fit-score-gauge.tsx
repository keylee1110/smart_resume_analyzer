import { Card } from "@/components/ui/card"

export function FitScoreGauge() {
  const fitScore = 82
  const circumference = 2 * Math.PI * 45
  const offset = circumference - (fitScore / 100) * circumference

  return (
    <Card className="p-8">
      <div className="flex flex-col items-center justify-center py-8">
        <h2 className="text-2xl font-bold text-text mb-8">Overall Fit Score</h2>

        <div className="relative w-48 h-48">
          <svg className="w-full h-full transform -rotate-90" viewBox="0 0 120 120">
            {/* Background circle */}
            <circle cx="60" cy="60" r="45" fill="none" stroke="#e2e8f0" strokeWidth="8" />
            {/* Progress circle */}
            <circle
              cx="60"
              cy="60"
              r="45"
              fill="none"
              stroke="#2563eb"
              strokeWidth="8"
              strokeDasharray={circumference}
              strokeDashoffset={offset}
              strokeLinecap="round"
              className="transition-all duration-1000"
            />
          </svg>

          <div className="absolute inset-0 flex flex-col items-center justify-center">
            <p className="text-5xl font-bold text-primary">{fitScore}</p>
            <p className="text-slate-600">%</p>
          </div>
        </div>

        <div className="mt-8 text-center">
          <p className="text-lg font-semibold text-text">Excellent Match</p>
          <p className="text-slate-600">Your profile is well-aligned with this position</p>
        </div>
      </div>
    </Card>
  )
}

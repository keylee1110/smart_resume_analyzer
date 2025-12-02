import { Card } from "@/components/ui/card"
import Link from "next/link"

export function RecentAnalyses() {
  const analyses = [
    {
      id: 1,
      jobTitle: "Senior Software Engineer",
      company: "Tech Corp",
      fitScore: 85,
      date: "2 days ago",
    },
    {
      id: 2,
      jobTitle: "Product Manager",
      company: "Startup Inc",
      fitScore: 72,
      date: "5 days ago",
    },
    {
      id: 3,
      jobTitle: "DevOps Engineer",
      company: "Cloud Systems",
      fitScore: 91,
      date: "1 week ago",
    },
  ]

  return (
    <Card className="p-6">
      <h2 className="text-2xl font-bold text-text mb-6">Recent Analyses</h2>
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead>
            <tr className="border-b border-border">
              <th className="text-left py-3 px-4 font-semibold text-text">Job Title</th>
              <th className="text-left py-3 px-4 font-semibold text-text">Company</th>
              <th className="text-left py-3 px-4 font-semibold text-text">Fit Score</th>
              <th className="text-left py-3 px-4 font-semibold text-text">Date</th>
              <th className="text-left py-3 px-4 font-semibold text-text">Action</th>
            </tr>
          </thead>
          <tbody>
            {analyses.map((analysis) => (
              <tr key={analysis.id} className="border-b border-border hover:bg-slate-50">
                <td className="py-3 px-4">{analysis.jobTitle}</td>
                <td className="py-3 px-4">{analysis.company}</td>
                <td className="py-3 px-4">
                  <div className="flex items-center gap-2">
                    <div className="w-24 h-2 bg-border rounded-full overflow-hidden">
                      <div className="h-full bg-success" style={{ width: `${analysis.fitScore}%` }}></div>
                    </div>
                    <span className="text-sm font-semibold text-text">{analysis.fitScore}%</span>
                  </div>
                </td>
                <td className="py-3 px-4 text-slate-600">{analysis.date}</td>
                <td className="py-3 px-4">
                  <Link href={`/analysis/${analysis.id}`} className="text-primary font-semibold hover:underline">
                    View
                  </Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </Card>
  )
}

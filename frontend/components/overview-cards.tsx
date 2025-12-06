import { Card } from "@/components/ui/card"
import { BarChart3, TrendingUp, CheckCircle2 } from "lucide-react"

export function OverviewCards() {
  const cards = [
    {
      title: "Total Analyses",
      value: "12",
      icon: BarChart3,
      color: "text-primary",
    },
    {
      title: "Avg Fit Score",
      value: "78%",
      icon: TrendingUp,
      color: "text-success",
    },
    {
      title: "Completed",
      value: "11",
      icon: CheckCircle2,
      color: "text-accent",
    },
  ]

  return (
    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
      {cards.map((card, idx) => {
        const Icon = card.icon
        return (
          <Card key={idx} className="p-6">
            <div className="flex items-start justify-between">
              <div>
                <p className="text-slate-600 text-sm font-medium">{card.title}</p>
                <h3 className="text-3xl font-bold text-text mt-2">{card.value}</h3>
              </div>
              <Icon className={`${card.color} w-8 h-8`} />
            </div>
          </Card>
        )
      })}
    </div>
  )
}

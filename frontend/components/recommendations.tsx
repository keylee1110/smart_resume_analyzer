import { Card } from "@/components/ui/card"
import { Lightbulb } from "lucide-react"

interface RecommendationsProps {
  recommendations: string[];
}

export function Recommendations({ recommendations }: RecommendationsProps) {
  return (
    <Card className="p-6">
      <div className="flex items-center gap-2 mb-6">
        <Lightbulb className="w-6 h-6 text-accent" />
        <h3 className="text-xl font-bold text-text">Recommendations</h3>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {recommendations.length > 0 ? (
          recommendations.map((rec, idx) => (
            <div key={idx} className="p-4 border border-border rounded-lg hover:shadow-md transition">
              <div className="flex justify-between items-start mb-2">
                <h4 className="font-semibold text-text">{rec}</h4>
                {/* Impact is not provided by backend yet, so just a generic tag */}
                <span className="text-xs px-2 py-1 rounded font-medium bg-blue-100 text-blue-700">Suggestion</span>
              </div>
              {/* <p className="text-sm text-slate-600">{rec.description}</p> Uncomment and use if backend provides description */}
            </div>
          ))
        ) : (
          <p className="text-center text-muted-foreground">No specific recommendations at this time.</p>
        )}
      </div>
    </Card>
  )
}

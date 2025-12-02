import { Card } from "@/components/ui/card"
import { Lightbulb } from "lucide-react"

export function Recommendations() {
  const recommendations = [
    {
      title: "Learn Kubernetes",
      description: "This is a key requirement for the role. Consider taking courses or working on projects.",
      impact: "High",
    },
    {
      title: "Enhance AWS Expertise",
      description: "Advance from intermediate to advanced level in AWS. Focus on EC2, RDS, and Lambda.",
      impact: "Medium",
    },
    {
      title: "Get Docker Certification",
      description: "Obtaining a Docker certification would significantly boost your profile.",
      impact: "High",
    },
    {
      title: "GraphQL Project Experience",
      description: "Add a project using GraphQL to your portfolio to demonstrate hands-on experience.",
      impact: "Medium",
    },
  ]

  return (
    <Card className="p-6">
      <div className="flex items-center gap-2 mb-6">
        <Lightbulb className="w-6 h-6 text-accent" />
        <h3 className="text-xl font-bold text-text">Recommendations</h3>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {recommendations.map((rec, idx) => (
          <div key={idx} className="p-4 border border-border rounded-lg hover:shadow-md transition">
            <div className="flex justify-between items-start mb-2">
              <h4 className="font-semibold text-text">{rec.title}</h4>
              <span
                className={`text-xs px-2 py-1 rounded font-medium ${
                  rec.impact === "High" ? "bg-red-100 text-red-700" : "bg-blue-100 text-blue-700"
                }`}
              >
                {rec.impact}
              </span>
            </div>
            <p className="text-sm text-slate-600">{rec.description}</p>
          </div>
        ))}
      </div>
    </Card>
  )
}

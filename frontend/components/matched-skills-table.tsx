import { Card } from "@/components/ui/card"
import { CheckCircle2 } from "lucide-react"

export function MatchedSkillsTable() {
  const matchedSkills = [
    { skill: "React", proficiency: "Advanced" },
    { skill: "TypeScript", proficiency: "Advanced" },
    { skill: "Node.js", proficiency: "Intermediate" },
    { skill: "AWS", proficiency: "Intermediate" },
    { skill: "Database Design", proficiency: "Advanced" },
    { skill: "REST APIs", proficiency: "Advanced" },
  ]

  return (
    <Card className="p-6">
      <div className="flex items-center gap-2 mb-6">
        <CheckCircle2 className="w-6 h-6 text-success" />
        <h3 className="text-xl font-bold text-text">Matched Skills</h3>
      </div>

      <div className="space-y-3">
        {matchedSkills.map((item, idx) => (
          <div
            key={idx}
            className="flex items-center justify-between p-3 bg-success/5 rounded-lg border border-success/20"
          >
            <span className="font-medium text-text">{item.skill}</span>
            <span className="text-sm text-slate-600 bg-success/10 px-3 py-1 rounded">{item.proficiency}</span>
          </div>
        ))}
      </div>
    </Card>
  )
}

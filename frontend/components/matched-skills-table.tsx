import { Card } from "@/components/ui/card"
import { CheckCircle2 } from "lucide-react"

interface MatchedSkillsTableProps {
  matchedSkills: string[];
}

export function MatchedSkillsTable({ matchedSkills }: MatchedSkillsTableProps) {
  return (
    <Card className="p-6">
      <div className="flex items-center gap-2 mb-6">
        <CheckCircle2 className="w-6 h-6 text-success" />
        <h3 className="text-xl font-bold text-text">Matched Skills</h3>
      </div>

      <div className="space-y-3">
        {matchedSkills.length > 0 ? (
          matchedSkills.map((skill, idx) => (
            <div
              key={idx}
              className="flex items-center justify-between p-3 bg-success/5 rounded-lg border border-success/20"
            >
              <span className="font-medium text-text">{skill}</span>
              {/* If proficiency is needed, it would come from the backend data for each skill */}
              <span className="text-sm text-slate-600 bg-success/10 px-3 py-1 rounded">Matched</span>
            </div>
          ))
        ) : (
          <p className="text-center text-muted-foreground">No matched skills found.</p>
        )}
      </div>
    </Card>
  )
}

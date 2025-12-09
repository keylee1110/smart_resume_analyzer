import { Card } from "@/components/ui/card"
import { AlertCircle } from "lucide-react"

interface MissingSkillsTableProps {
  missingSkills: string[];
}

export function MissingSkillsTable({ missingSkills }: MissingSkillsTableProps) {
  return (
    <Card className="p-6 border-warning/20 bg-warning/5">
      <div className="flex items-center gap-2 mb-6">
        <AlertCircle className="w-6 h-6 text-warning" />
        <h3 className="text-xl font-bold text-text">Skill Gaps</h3>
      </div>

      <div className="space-y-3">
        {missingSkills.length > 0 ? (
          missingSkills.map((skill, idx) => (
            <div
              key={idx}
              className="flex items-center justify-between p-3 bg-warning/10 rounded-lg border border-warning/20"
            >
              <span className="font-medium text-text">{skill}</span>
              <span className="text-sm px-3 py-1 rounded bg-red-100 text-red-700">Missing</span>
            </div>
          ))
        ) : (
          <p className="text-center text-muted-foreground">No missing skills identified.</p>
        )}
      </div>
    </Card>
  )
}

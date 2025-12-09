export interface AnalysisResult {
  id: string
  jobTitle: string
  company: string // We might need to extract this or let user input it, for now we can infer or use placeholder
  fitScore: number
  date: string
  status: "completed" | "pending" | "failed"
  matchedSkills: any[]
  missingSkills: any[]
  recommendations: any[]
}

const STORAGE_KEY = "resume_analysis_history"

export const getHistory = (): AnalysisResult[] => {
  if (typeof window === "undefined") return []
  const stored = localStorage.getItem(STORAGE_KEY)
  return stored ? JSON.parse(stored) : []
}

export const saveAnalysis = (analysis: Omit<AnalysisResult, "id" | "date" | "status">) => {
  const history = getHistory()
  const newEntry: AnalysisResult = {
    ...analysis,
    id: crypto.randomUUID(),
    date: new Date().toISOString(),
    status: "completed",
  }
  
  const updatedHistory = [newEntry, ...history]
  localStorage.setItem(STORAGE_KEY, JSON.stringify(updatedHistory))
  return newEntry
}

export const clearHistory = () => {
  localStorage.removeItem(STORAGE_KEY)
}

"use client"

import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { FitScoreGauge } from "@/components/fit-score-gauge"
import { ArrowLeft, CheckCircle2, XCircle, BookOpen, Clock, Target } from "lucide-react"

interface ResultsSectionProps {
  data: any
  onNewAnalysis: () => void
}

export function ResultsSection({ data, onNewAnalysis }: ResultsSectionProps) {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Analysis Results</h1>
          <p className="text-muted-foreground mt-2">Your resume has been analyzed</p>
        </div>
        <Button onClick={onNewAnalysis} variant="outline" className="cursor-pointer">
          <ArrowLeft className="w-4 h-4 mr-2" />
          New Analysis
        </Button>
      </div>
      <FitScoreGauge fitScore={data.fitScore} />
      <div className="grid md:grid-cols-2 gap-6">
        <Card className="p-6">
          <div className="flex items-center gap-3 mb-6">
            <div className="w-10 h-10 rounded-lg bg-success/10 flex items-center justify-center">
              <CheckCircle2 className="w-6 h-6 text-success" />
            </div>
            <div>
              <h2 className="text-xl font-semibold">Matched Skills</h2>
              <p className="text-sm text-muted-foreground">{data.matchedSkills.length} skills found</p>
            </div>
          </div>
          <div className="space-y-3">
            {data.matchedSkills.map((skill: any, index: number) => (
              <div key={index} className="p-4 rounded-lg bg-success/5 border border-success/20">
                <div className="flex items-center justify-between mb-2">
                  <span className="font-semibold">{skill.name}</span>
                  <Badge variant="outline" className="bg-success/10 text-success">{skill.level}</Badge>
                </div>
                <p className="text-sm text-muted-foreground">{skill.yearsRequired} years required</p>
              </div>
            ))}
          </div>
        </Card>
        <Card className="p-6">
          <div className="flex items-center gap-3 mb-6">
            <div className="w-10 h-10 rounded-lg bg-destructive/10 flex items-center justify-center">
              <XCircle className="w-6 h-6 text-destructive" />
            </div>
            <div>
              <h2 className="text-xl font-semibold">Missing Skills</h2>
              <p className="text-sm text-muted-foreground">{data.missingSkills.length} skills to improve</p>
            </div>
          </div>
          <div className="space-y-3">
            {data.missingSkills.map((skill: any, index: number) => (
              <div key={index} className="p-4 rounded-lg bg-destructive/5 border border-destructive/20">
                <div className="flex items-center justify-between mb-2">
                  <span className="font-semibold">{skill.name}</span>
                  <Badge variant="outline">{skill.priority} Priority</Badge>
                </div>
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                  <Clock className="w-4 h-4" />
                  {skill.estimatedLearningTime}
                </div>
              </div>
            ))}
          </div>
        </Card>
      </div>
      <Card className="p-6">
        <div className="flex items-center gap-3 mb-6">
          <div className="w-10 h-10 rounded-lg bg-accent/10 flex items-center justify-center">
            <Target className="w-6 h-6 text-accent" />
          </div>
          <div>
            <h2 className="text-xl font-semibold">Learning Roadmap</h2>
            <p className="text-sm text-muted-foreground">Follow this path to improve</p>
          </div>
        </div>
        <div className="space-y-6">
          {data.recommendations.map((rec: any, index: number) => (
            <div key={index} className="relative">
              {index < data.recommendations.length - 1 && (
                <div className="absolute left-6 top-12 bottom-0 w-0.5 bg-border" />
              )}
              <div className="flex gap-4">
                <div className="w-12 h-12 rounded-full bg-primary text-primary-foreground flex items-center justify-center font-bold text-lg flex-shrink-0">
                  {rec.step}
                </div>
                <div className="flex-1 pb-6">
                  <h3 className="text-lg font-semibold mb-2">{rec.title}</h3>
                  <p className="text-muted-foreground mb-4">{rec.description}</p>
                  <div className="flex items-center gap-2 text-sm text-muted-foreground mb-3">
                    <Clock className="w-4 h-4" />
                    {rec.duration}
                  </div>
                  <div className="flex flex-wrap gap-2">
                    {rec.resources.map((resource: string, idx: number) => (
                      <Badge key={idx} variant="secondary" className="gap-1">
                        <BookOpen className="w-3 h-3" />
                        {resource}
                      </Badge>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </Card>
    </div>
  )
}

"use client"

import { useState } from "react"
import { useLanguage } from "@/lib/language-context"
import { translations } from "@/lib/translations"
import { Sidebar } from "@/components/sidebar"
import { LanguageSwitcher } from "@/components/language-switcher"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"
import { Switch } from "@/components/ui/switch"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Button } from "@/components/ui/button"

export default function SettingsPage() {
  const { language } = useLanguage()
  const t = translations[language]

  // CV Parsing Settings
  const [ocrEnabled, setOcrEnabled] = useState(true)
  const [dataCleaningLevel, setDataCleaningLevel] = useState("medium")
  const [ocrLanguage, setOcrLanguage] = useState("english")
  const [autoDetectSections, setAutoDetectSections] = useState(true)

  // Job Description Matching Settings
  const [keywordWeight, setKeywordWeight] = useState(35)
  const [skillWeight, setSkillWeight] = useState(30)
  const [experienceWeight, setExperienceWeight] = useState(20)
  const [educationWeight, setEducationWeight] = useState(15)
  const [targetIndustry, setTargetIndustry] = useState("general")

  // Fit Score Settings
  const [scoringMode, setScoringMode] = useState("weighted")
  const [aiSensitivity, setAiSensitivity] = useState("balanced")
  const [resumeType, setResumeType] = useState("ats")

  // Skill Gap & Recommendation Settings
  const [recommendationLevel, setRecommendationLevel] = useState("intermediate")
  const [learningPathSource, setLearningPathSource] = useState("ai-general")
  const [enableLearningPath, setEnableLearningPath] = useState(true)

  // Document Privacy Settings
  const [storeInS3, setStoreInS3] = useState(false)
  const [allowModelImprovement, setAllowModelImprovement] = useState(false)
  const [encryptionLevel, setEncryptionLevel] = useState("default")

  // Performance & Limits
  const [processingMode, setProcessingMode] = useState("accurate")
  const [maxFileSize, setMaxFileSize] = useState(10)
  const [dailyLimit, setDailyLimit] = useState(50)

  const handleSave = () => {
    console.log("[v0] Settings saved")
  }

  return (
    <div className="flex h-screen bg-background">
      <Sidebar />
      <main className="flex-1 overflow-y-auto">
        <div className="p-8 max-w-4xl">
          {/* Header */}
          <div className="mb-8 flex items-center justify-between">
            <div>
              <h1 className="text-4xl font-bold text-foreground">{t.settingsTitle}</h1>
              <p className="text-muted-foreground mt-2">{t.settingsDescription}</p>
            </div>
            <LanguageSwitcher />
          </div>

          <div className="space-y-8">
            <Card>
              <CardHeader>
                <CardTitle>{t.language}</CardTitle>
                <CardDescription>{t.selectLanguage}</CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="space-y-2">
                  <Label htmlFor="language">{t.language}</Label>
                  <LanguageSwitcher />
                </div>
              </CardContent>
            </Card>

            {/* 1. CV Parsing Settings */}
            <Card>
              <CardHeader>
                <CardTitle>{t.cvParsing}</CardTitle>
                <CardDescription>{t.cvParsingDesc}</CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>{t.enableOcr}</Label>
                    <p className="text-sm text-muted-foreground">{t.enableOcrDesc}</p>
                  </div>
                  <Switch checked={ocrEnabled} onCheckedChange={setOcrEnabled} />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="data-cleaning">{t.dataCleaningLevel}</Label>
                  <Select value={dataCleaningLevel} onValueChange={setDataCleaningLevel}>
                    <SelectTrigger id="data-cleaning">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="low">{t.lowCleaning}</SelectItem>
                      <SelectItem value="medium">{t.mediumCleaning}</SelectItem>
                      <SelectItem value="high">{t.highCleaning}</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="ocr-language">{t.ocrLanguage}</Label>
                  <Select value={ocrLanguage} onValueChange={setOcrLanguage}>
                    <SelectTrigger id="ocr-language">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="english">English</SelectItem>
                      <SelectItem value="vietnamese">Vietnamese</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>{t.autoDetectSections}</Label>
                    <p className="text-sm text-muted-foreground">{t.autoDetectSectionsDesc}</p>
                  </div>
                  <Switch checked={autoDetectSections} onCheckedChange={setAutoDetectSections} />
                </div>
              </CardContent>
            </Card>

            {/* 2. Job Description Matching Settings */}
            <Card>
              <CardHeader>
                <CardTitle>{t.jobMatching}</CardTitle>
                <CardDescription>{t.jobMatchingDesc}</CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="space-y-2">
                  <Label htmlFor="keyword-weight">
                    {t.keywordWeight}: {keywordWeight}%
                  </Label>
                  <Input
                    id="keyword-weight"
                    type="number"
                    min="0"
                    max="100"
                    value={keywordWeight}
                    onChange={(e) => setKeywordWeight(Number.parseInt(e.target.value))}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="skill-weight">
                    {t.skillWeight}: {skillWeight}%
                  </Label>
                  <Input
                    id="skill-weight"
                    type="number"
                    min="0"
                    max="100"
                    value={skillWeight}
                    onChange={(e) => setSkillWeight(Number.parseInt(e.target.value))}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="experience-weight">
                    {t.experienceWeight}: {experienceWeight}%
                  </Label>
                  <Input
                    id="experience-weight"
                    type="number"
                    min="0"
                    max="100"
                    value={experienceWeight}
                    onChange={(e) => setExperienceWeight(Number.parseInt(e.target.value))}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="education-weight">
                    {t.educationWeight}: {educationWeight}%
                  </Label>
                  <Input
                    id="education-weight"
                    type="number"
                    min="0"
                    max="100"
                    value={educationWeight}
                    onChange={(e) => setEducationWeight(Number.parseInt(e.target.value))}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="target-industry">{t.targetIndustry}</Label>
                  <Select value={targetIndustry} onValueChange={setTargetIndustry}>
                    <SelectTrigger id="target-industry">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="it">Information Technology</SelectItem>
                      <SelectItem value="marketing">Marketing</SelectItem>
                      <SelectItem value="sales">Sales</SelectItem>
                      <SelectItem value="finance">Finance</SelectItem>
                      <SelectItem value="general">General</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </CardContent>
            </Card>

            {/* 3. Fit Score Settings */}
            <Card>
              <CardHeader>
                <CardTitle>{t.fitScore}</CardTitle>
                <CardDescription>{t.fitScoreDesc}</CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="space-y-2">
                  <Label htmlFor="scoring-mode">{t.scoringMode}</Label>
                  <Select value={scoringMode} onValueChange={setScoringMode}>
                    <SelectTrigger id="scoring-mode">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="weighted">{t.weighted}</SelectItem>
                      <SelectItem value="semantic">{t.semantic}</SelectItem>
                      <SelectItem value="hybrid">{t.hybrid}</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="ai-sensitivity">{t.analysisSensitivity}</Label>
                  <Select value={aiSensitivity} onValueChange={setAiSensitivity}>
                    <SelectTrigger id="ai-sensitivity">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="strict">{t.strict}</SelectItem>
                      <SelectItem value="balanced">{t.balanced}</SelectItem>
                      <SelectItem value="flexible">{t.flexible}</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="resume-type">{t.resumeTypeOptimization}</Label>
                  <Select value={resumeType} onValueChange={setResumeType}>
                    <SelectTrigger id="resume-type">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="ats">{t.ats}</SelectItem>
                      <SelectItem value="creative">{t.creative}</SelectItem>
                      <SelectItem value="technical">{t.technical}</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </CardContent>
            </Card>

            {/* 4. Skill Gap & Recommendation Settings */}
            <Card>
              <CardHeader>
                <CardTitle>{t.skillGap}</CardTitle>
                <CardDescription>{t.skillGapDesc}</CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="space-y-2">
                  <Label htmlFor="recommendation-detail">{t.recommendationDetail}</Label>
                  <Select value={recommendationLevel} onValueChange={setRecommendationLevel}>
                    <SelectTrigger id="recommendation-detail">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="basic">{t.basic}</SelectItem>
                      <SelectItem value="intermediate">{t.intermediate}</SelectItem>
                      <SelectItem value="advanced">{t.advanced}</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="learning-path">{t.learningPathSource}</Label>
                  <Select value={learningPathSource} onValueChange={setLearningPathSource}>
                    <SelectTrigger id="learning-path">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="ai-general">{t.generalLearning}</SelectItem>
                      <SelectItem value="aws-learning">{t.awsLearning}</SelectItem>
                      <SelectItem value="custom">{t.custom}</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>{t.enableLearningPath}</Label>
                    <p className="text-sm text-muted-foreground">{t.enableLearningPathDesc}</p>
                  </div>
                  <Switch checked={enableLearningPath} onCheckedChange={setEnableLearningPath} />
                </div>
              </CardContent>
            </Card>

            {/* 5. Document Privacy Settings */}
            <Card>
              <CardHeader>
                <CardTitle>{t.privacy}</CardTitle>
                <CardDescription>{t.privacyDesc}</CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>{t.storeInS3}</Label>
                    <p className="text-sm text-muted-foreground">{t.storeInS3Desc}</p>
                  </div>
                  <Switch checked={storeInS3} onCheckedChange={setStoreInS3} />
                </div>

                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>{t.allowImprovement}</Label>
                    <p className="text-sm text-muted-foreground">{t.allowImprovementDesc}</p>
                  </div>
                  <Switch checked={allowModelImprovement} onCheckedChange={setAllowModelImprovement} />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="encryption-level">{t.encryptionLevel}</Label>
                  <Select value={encryptionLevel} onValueChange={setEncryptionLevel}>
                    <SelectTrigger id="encryption-level">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="default">{t.defaultEncryption}</SelectItem>
                      <SelectItem value="aes256">{t.aes256}</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </CardContent>
            </Card>

            {/* 6. Performance & Limits */}
            <Card>
              <CardHeader>
                <CardTitle>{t.performance}</CardTitle>
                <CardDescription>{t.performanceDesc}</CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="space-y-2">
                  <Label htmlFor="processing-mode">{t.processingMode}</Label>
                  <Select value={processingMode} onValueChange={setProcessingMode}>
                    <SelectTrigger id="processing-mode">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="fast">{t.fast}</SelectItem>
                      <SelectItem value="accurate">{t.accurate}</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="file-size">{t.maxFileSize}</Label>
                  <Input
                    id="file-size"
                    type="number"
                    min="1"
                    max="50"
                    value={maxFileSize}
                    onChange={(e) => setMaxFileSize(Number.parseInt(e.target.value))}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="daily-limit">{t.dailyLimit}</Label>
                  <Input
                    id="daily-limit"
                    type="number"
                    min="1"
                    max="500"
                    value={dailyLimit}
                    onChange={(e) => setDailyLimit(Number.parseInt(e.target.value))}
                  />
                </div>
              </CardContent>
            </Card>

            {/* Save Button */}
            <div className="flex justify-end pt-4">
              <Button onClick={handleSave} className="bg-primary text-primary-foreground hover:bg-primary/90">
                {t.saveSettings}
              </Button>
            </div>
          </div>
        </div>
      </main>
    </div>
  )
}

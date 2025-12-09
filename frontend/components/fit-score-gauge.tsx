import { Card } from "@/components/ui/card"

interface FitScoreGaugeProps {
  fitScore: number;
}

export function FitScoreGauge({ fitScore }: FitScoreGaugeProps) {
  // 1. Validation: Ensure fitScore is a valid number between 0 and 100
  const validScore = isNaN(fitScore) ? 0 : Math.max(0, Math.min(100, fitScore));
  
  // 2. SVG Geometry
  // Radius 40px, Stroke 8px. 
  // Center (60, 60).
  // ViewBox 120x120.
  // This leaves 60 - 40 - 4 = 16px padding on each side, plenty of room.
  const radius = 40;
  const circumference = 2 * Math.PI * radius;
  const offset = circumference - (validScore / 100) * circumference;

  const getFitScoreMessage = (score: number) => {
    if (score >= 90) return "Outstanding Match";
    if (score >= 70) return "Excellent Match";
    if (score >= 50) return "Good Match";
    if (score >= 30) return "Moderate Match";
    return "Low Match";
  };

  const getFitScoreDescription = (score: number) => {
    if (score >= 90) return "Your profile is exceptionally well-aligned with this position.";
    if (score >= 70) return "Your profile is well-aligned with this position.";
    if (score >= 50) return "Your profile generally matches this position.";
    if (score >= 30) return "Your profile has some alignment with this position.";
    return "Consider improving your profile for this position.";
  };

  const getColor = (score: number) => {
    if (score >= 70) return "#10b981"; // Emerald
    if (score >= 50) return "#f59e0b"; // Amber
    return "#ef4444"; // Red
  };

  const strokeColor = getColor(validScore);

  return (
    <div className="flex flex-col items-center justify-center w-full h-full">
      <div className="relative w-40 h-40">
        <svg className="w-full h-full transform -rotate-90" viewBox="0 0 120 120">
          {/* Background circle */}
          <circle 
            cx="60" 
            cy="60" 
            r={radius} 
            fill="none" 
            stroke="currentColor" 
            strokeWidth="8" 
            className="text-muted/20"
          />
          {/* Progress circle */}
          <circle
            cx="60"
            cy="60"
            r={radius}
            fill="none"
            stroke={strokeColor}
            strokeWidth="8"
            strokeDasharray={circumference}
            strokeDashoffset={offset}
            strokeLinecap="round"
            className="transition-all duration-1000 ease-out"
          />
        </svg>

        <div className="absolute inset-0 flex flex-col items-center justify-center">
          <span className="text-4xl font-bold text-foreground">{Math.round(validScore)}</span>
          <span className="text-xs text-muted-foreground uppercase tracking-wider font-medium">%</span>
        </div>
      </div>

      <div className="mt-4 text-center">
        <h4 className="text-lg font-bold text-foreground mb-1">{getFitScoreMessage(validScore)}</h4>
        <p className="text-sm text-muted-foreground max-w-[200px] leading-relaxed">
            {getFitScoreDescription(validScore)}
        </p>
      </div>
    </div>
  )
}


"use client"

import { Card } from "@/components/ui/card"
import { BarChart3, TrendingUp, CheckCircle2 } from "lucide-react"
import { motion } from "framer-motion"

export function OverviewCards() {
  const cards = [
    {
      title: "Total Analyses",
      value: "12",
      icon: BarChart3,
      color: "text-primary",
      bgColor: "bg-primary/10",
    },
    {
      title: "Avg Fit Score",
      value: "78%",
      icon: TrendingUp,
      color: "text-success",
      bgColor: "bg-success/10",
    },
    {
      title: "Completed",
      value: "11",
      icon: CheckCircle2,
      color: "text-accent",
      bgColor: "bg-accent/10",
    },
  ]

  const container = {
    hidden: { opacity: 0 },
    show: {
      opacity: 1,
      transition: {
        staggerChildren: 0.1
      }
    }
  }

  const item = {
    hidden: { opacity: 0, y: 20 },
    show: { opacity: 1, y: 0 }
  }

  return (
    <motion.div 
      variants={container}
      initial="hidden"
      animate="show"
      className="grid grid-cols-1 md:grid-cols-3 gap-6"
    >
      {cards.map((card, idx) => {
        const Icon = card.icon
        return (
          <motion.div key={idx} variants={item}>
            <Card className="p-6 hover:shadow-lg transition-shadow duration-300 border-border/50 bg-card/50 backdrop-blur-sm">
              <div className="flex items-start justify-between">
                <div>
                  <p className="text-muted-foreground text-sm font-medium">{card.title}</p>
                  <h3 className="text-3xl font-bold text-foreground mt-2">{card.value}</h3>
                </div>
                <div className={`p-3 rounded-xl ${card.bgColor}`}>
                  <Icon className={`${card.color} w-6 h-6`} />
                </div>
              </div>
            </Card>
          </motion.div>
        )
      })}
    </motion.div>
  )
}
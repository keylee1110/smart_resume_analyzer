"use client"

import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { ArrowRight, FileText, BarChart3, Zap, Target } from "lucide-react"
import Link from "next/link"

export default function Home() {
  const features = [
    {
      icon: FileText,
      title: "Smart Analysis",
      description: "Advanced algorithms analyze every aspect of your resume in seconds",
    },
    {
      icon: Zap,
      title: "Instant Feedback",
      description: "Get real-time recommendations to boost your resume impact",
    },
    {
      icon: Target,
      title: "Job Matching",
      description: "Align your skills with specific job descriptions for better opportunities",
    },
    {
      icon: BarChart3,
      title: "Performance Score",
      description: "See your resume strength across multiple professional dimensions",
    },
  ]

  const stats = [
    { label: "Resumes Analyzed", value: "50K+" },
    { label: "Success Rate", value: "92%" },
    { label: "Average Improvement", value: "+35%" },
  ]

  const steps = [
    {
      step: "1",
      title: "Upload Resume",
      description: "Upload your resume in PDF or DOCX format - takes just one click",
    },
    {
      step: "2",
      title: "Smart Analysis",
      description: "Our advanced tool instantly analyzes your resume against industry standards",
    },
    {
      step: "3",
      title: "Get Insights",
      description: "Receive detailed recommendations and an actionable improvement plan",
    },
    {
      step: "4",
      title: "Land Your Job",
      description: "Apply with confidence knowing your resume is optimized for success",
    },
  ]

  return (
    <div className="min-h-screen bg-background text-foreground">
      {/* Navigation */}
      <nav className="sticky top-0 z-50 border-b border-border bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="flex h-16 items-center justify-between">
            <Link href="/" className="flex items-center gap-2">
              <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary">
                <FileText className="h-5 w-5 text-primary-foreground" />
              </div>
              <span className="text-xl font-bold">Smart Resume Analyzer</span>
            </Link>
            <div className="flex items-center gap-3">
              <Link href="/signin">
                <Button variant="ghost" size="sm">
                  Sign In
                </Button>
              </Link>
              <Link href="/signup">
                <Button size="sm" className="gap-2">
                  Get Started <ArrowRight className="h-4 w-4" />
                </Button>
              </Link>
            </div>
          </div>
        </div>
      </nav>

      {/* Hero Section */}
      <section className="relative overflow-hidden">
        {/* Gradient background effect */}
        <div className="absolute inset-0 bg-gradient-to-b from-primary/10 via-background to-background" />

        <div className="relative mx-auto max-w-6xl px-4 sm:px-6 lg:px-8 py-32 text-center">
          <div className="inline-flex items-center gap-2 rounded-full border border-primary/20 bg-primary/5 px-4 py-1.5 mb-6">
            <FileText className="h-4 w-4 text-primary" />
            <span className="text-sm font-medium text-primary">Powered by Advanced Analysis</span>
          </div>

          <h1 className="text-5xl sm:text-6xl font-bold tracking-tight mb-6 text-balance">
            Your Resume,{" "}
            <span className="bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent">Perfected</span>
          </h1>

          <p className="mx-auto max-w-2xl text-xl text-muted-foreground mb-10 text-balance leading-relaxed">
            Get intelligent insights, real-time feedback, and actionable recommendations to make your resume stand out
            to recruiters and hiring managers.
          </p>

          <div className="flex flex-col sm:flex-row gap-4 justify-center mb-16">
            <Link href="/signup">
              <Button size="lg" className="gap-2">
                Analyze Your Resume <ArrowRight className="h-5 w-5" />
              </Button>
            </Link>
            <Button size="lg" variant="outline">
              View Demo
            </Button>
          </div>

          {/* Stats */}
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-8 pt-8 border-t border-border">
            {stats.map((stat, index) => (
              <div key={index} className="flex flex-col items-center">
                <div className="text-3xl font-bold text-primary mb-1">{stat.value}</div>
                <div className="text-sm text-muted-foreground">{stat.label}</div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-20">
        <div className="text-center mb-16">
          <h2 className="text-4xl font-bold mb-4 text-balance">Everything you need to succeed</h2>
          <p className="text-lg text-muted-foreground max-w-2xl mx-auto">
            Our comprehensive suite of tools helps you optimize every aspect of your resume
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {features.map((feature, index) => {
            const Icon = feature.icon
            return (
              <Card
                key={index}
                className="border border-border/50 bg-card/50 backdrop-blur hover:border-primary/50 transition-colors"
              >
                <CardContent className="pt-6">
                  <div className="mb-4 inline-flex h-12 w-12 items-center justify-center rounded-lg bg-primary/10">
                    <Icon className="h-6 w-6 text-primary" />
                  </div>
                  <h3 className="font-semibold text-foreground mb-2">{feature.title}</h3>
                  <p className="text-sm text-muted-foreground">{feature.description}</p>
                </CardContent>
              </Card>
            )
          })}
        </div>
      </section>

      {/* How It Works */}
      <section className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-20 bg-card/30 rounded-2xl border border-border my-12">
        <div className="text-center mb-16">
          <h2 className="text-4xl font-bold mb-4 text-balance">Four steps to success</h2>
          <p className="text-lg text-muted-foreground">Get your resume analyzed and optimized in minutes</p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
          {steps.map((item, index) => (
            <div key={index} className="relative">
              {index < steps.length - 1 && (
                <div className="hidden lg:block absolute top-8 left-[calc(100%+1rem)] right-auto w-[calc(100%-2rem)] h-0.5 bg-gradient-to-r from-primary/50 to-transparent" />
              )}
              <div className="flex flex-col items-start">
                <div className="flex h-14 w-14 items-center justify-center rounded-full bg-gradient-to-br from-primary to-accent text-primary-foreground font-bold text-lg mb-4">
                  {item.step}
                </div>
                <h3 className="font-semibold text-lg text-foreground mb-2">{item.title}</h3>
                <p className="text-muted-foreground">{item.description}</p>
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* CTA Section */}
      <section className="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8 py-20 text-center">
        <div className="rounded-2xl border border-primary/20 bg-gradient-to-br from-primary/5 via-background to-background p-12">
          <h2 className="text-3xl sm:text-4xl font-bold mb-4 text-balance">Ready to transform your resume?</h2>
          <p className="text-lg text-muted-foreground mb-8 max-w-2xl mx-auto">
            Join thousands of job seekers who have already improved their resumes with professional analysis and
            recommendations.
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link href="/signup">
              <Button size="lg" className="gap-2">
                Start Free Analysis <ArrowRight className="h-5 w-5" />
              </Button>
            </Link>
            <Button size="lg" variant="outline">
              Learn More
            </Button>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="border-t border-border mt-24">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-12">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-8 mb-8">
            <div>
              <div className="flex items-center gap-2 mb-4">
                <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary">
                  <FileText className="h-5 w-5 text-primary-foreground" />
                </div>
                <span className="font-bold">Smart Resume Analyzer</span>
              </div>
              <p className="text-sm text-muted-foreground">
                Transform your resume with professional analysis and insights.
              </p>
            </div>
            <div>
              <h4 className="font-semibold mb-4">Product</h4>
              <ul className="space-y-2 text-sm text-muted-foreground">
                <li>
                  <Link href="#" className="hover:text-foreground transition-colors">
                    Features
                  </Link>
                </li>
                <li>
                  <Link href="#" className="hover:text-foreground transition-colors">
                    Pricing
                  </Link>
                </li>
                <li>
                  <Link href="#" className="hover:text-foreground transition-colors">
                    FAQ
                  </Link>
                </li>
              </ul>
            </div>
            <div>
              <h4 className="font-semibold mb-4">Company</h4>
              <ul className="space-y-2 text-sm text-muted-foreground">
                <li>
                  <Link href="#" className="hover:text-foreground transition-colors">
                    About
                  </Link>
                </li>
                <li>
                  <Link href="#" className="hover:text-foreground transition-colors">
                    Blog
                  </Link>
                </li>
                <li>
                  <Link href="#" className="hover:text-foreground transition-colors">
                    Contact
                  </Link>
                </li>
              </ul>
            </div>
            <div>
              <h4 className="font-semibold mb-4">Legal</h4>
              <ul className="space-y-2 text-sm text-muted-foreground">
                <li>
                  <Link href="#" className="hover:text-foreground transition-colors">
                    Privacy
                  </Link>
                </li>
                <li>
                  <Link href="#" className="hover:text-foreground transition-colors">
                    Terms
                  </Link>
                </li>
              </ul>
            </div>
          </div>
          <div className="border-t border-border pt-8 text-center text-sm text-muted-foreground">
            <p>&copy; 2025 Smart Resume Analyzer. All rights reserved.</p>
          </div>
        </div>
      </footer>
    </div>
  )
}

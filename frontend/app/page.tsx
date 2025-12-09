"use client";

import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Sparkles, TrendingUp, Target, Zap, ArrowRight, CheckCircle2, UploadCloud, FileText, BrainCircuit } from "lucide-react";
import { motion } from "framer-motion";

export default function LandingPage() {
  return (
    <div className="min-h-screen flex flex-col bg-background selection:bg-primary/20">
      {/* Navbar */}
      <header className="fixed top-0 w-full z-50 bg-background/80 backdrop-blur-lg border-b border-border/40">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
          <Link href="/" className="flex items-center gap-2 group">
            <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-primary to-blue-600 flex items-center justify-center group-hover:scale-110 transition-transform">
              <Sparkles className="w-5 h-5 text-white" />
            </div>
            <span className="font-bold text-xl tracking-tight">Smart Resume</span>
          </Link>
          
          <nav className="flex items-center gap-4">
            <Link href="/login">
              <Button variant="ghost" className="text-muted-foreground hover:text-foreground">
                Sign In
              </Button>
            </Link>
            <Link href="/signup">
              <Button className="bg-primary hover:bg-primary/90 text-white rounded-full px-6">
                Get Started
              </Button>
            </Link>
          </nav>
        </div>
      </header>

      <main className="flex-grow pt-16">
        {/* Hero Section */}
        <section className="relative overflow-hidden pt-20 pb-32 lg:pt-32 lg:pb-40">
          <div className="absolute top-0 left-1/2 -translate-x-1/2 w-full h-full z-0 pointer-events-none">
             <div className="absolute top-0 left-1/4 w-96 h-96 bg-blue-500/10 rounded-full blur-3xl" />
             <div className="absolute bottom-0 right-1/4 w-96 h-96 bg-purple-500/10 rounded-full blur-3xl" />
          </div>

          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative z-10 text-center">
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.5 }}
            >
              <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-primary/10 border border-primary/20 text-primary text-sm font-medium mb-8">
                <Sparkles className="w-4 h-4" />
                <span>Powered by Advanced AI</span>
              </div>
              
              <h1 className="text-5xl md:text-7xl font-bold font-heading tracking-tight mb-6 bg-gradient-to-r from-foreground to-foreground/70 bg-clip-text text-transparent">
                Optimize Your Resume <br />
                <span className="text-primary">In Seconds</span>
              </h1>
              
              <p className="text-xl text-muted-foreground max-w-2xl mx-auto mb-10 leading-relaxed">
                Stop guessing what recruiters want. Our AI analyzes your resume against job descriptions to give you a personalized fit score and actionable improvements.
              </p>
              
              <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
                <Link href="/dashboard">
                  <Button size="lg" className="h-12 px-8 rounded-full text-lg bg-primary hover:bg-primary/90 shadow-lg shadow-primary/25 hover:shadow-primary/40 transition-all">
                    Analyze My Resume
                    <ArrowRight className="w-5 h-5 ml-2" />
                  </Button>
                </Link>
                <Link href="#how-it-works">
                  <Button size="lg" variant="outline" className="h-12 px-8 rounded-full text-lg border-2">
                    How it Works
                  </Button>
                </Link>
              </div>
            </motion.div>
          </div>
        </section>

        {/* Stats/Social Proof (Optional) */}
        <div className="border-y border-border/40 bg-muted/30">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
            <div className="grid grid-cols-2 md:grid-cols-4 gap-8 text-center">
              {[
                { label: "Resumes Analyzed", value: "10,000+" },
                { label: "Success Rate", value: "94%" },
                { label: "Time Saved", value: "5hrs/wk" },
                { label: "Interviews Landed", value: "2,500+" },
              ].map((stat, i) => (
                <div key={i}>
                  <div className="text-3xl font-bold text-foreground mb-1">{stat.value}</div>
                  <div className="text-sm text-muted-foreground">{stat.label}</div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Features Grid */}
        <section className="py-24 bg-background">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="text-center mb-16">
              <h2 className="text-3xl md:text-4xl font-bold mb-4">Everything you need to get hired</h2>
              <p className="text-lg text-muted-foreground">Comprehensive tools to perfect your application</p>
            </div>

            <div className="grid md:grid-cols-3 gap-8">
              {[
                {
                  icon: Target,
                  title: "Targeted Insights",
                  desc: "Get specific recommendations based on the job description you're applying for.",
                  color: "text-blue-500",
                  bg: "bg-blue-500/10"
                },
                {
                  icon: BrainCircuit,
                  title: "AI Analysis",
                  desc: "Advanced NLP algorithms detect missing keywords and skills gaps instantly.",
                  color: "text-purple-500",
                  bg: "bg-purple-500/10"
                },
                {
                  icon: TrendingUp,
                  title: "Score Tracking",
                  desc: "Monitor your resume's fit score improvements over time as you edit.",
                  color: "text-emerald-500",
                  bg: "bg-emerald-500/10"
                }
              ].map((feature, i) => (
                <div key={i} className="p-8 rounded-3xl border border-border/50 bg-card hover:shadow-lg transition-all hover:-translate-y-1">
                  <div className={`w-14 h-14 rounded-2xl ${feature.bg} ${feature.color} flex items-center justify-center mb-6`}>
                    <feature.icon className="w-7 h-7" />
                  </div>
                  <h3 className="text-xl font-bold mb-3">{feature.title}</h3>
                  <p className="text-muted-foreground leading-relaxed">
                    {feature.desc}
                  </p>
                </div>
              ))}
            </div>
          </div>
        </section>

        {/* How It Works */}
        <section id="how-it-works" className="py-24 bg-muted/30">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="grid lg:grid-cols-2 gap-16 items-center">
              <div className="space-y-8">
                <h2 className="text-3xl md:text-4xl font-bold">
                  Three steps to your <br />
                  <span className="text-primary">dream job</span>
                </h2>
                
                <div className="space-y-6">
                  {[
                    {
                      step: "01",
                      title: "Upload Your Resume",
                      desc: "Upload your existing PDF or DOCX resume to our secure platform."
                    },
                    {
                      step: "02",
                      title: "Paste Job Description",
                      desc: "Add the job description you want to apply for to get targeted analysis."
                    },
                    {
                      step: "03",
                      title: "Get Actionable Feedback",
                      desc: "Receive instant feedback, missing keywords, and a match score."
                    }
                  ].map((step, i) => (
                    <div key={i} className="flex gap-4">
                      <div className="flex-shrink-0 w-12 h-12 rounded-xl bg-background border border-border flex items-center justify-center font-bold text-lg text-primary shadow-sm">
                        {step.step}
                      </div>
                      <div>
                        <h4 className="text-lg font-bold mb-1">{step.title}</h4>
                        <p className="text-muted-foreground">{step.desc}</p>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
              
              <div className="relative">
                <div className="absolute inset-0 bg-gradient-to-tr from-blue-500 to-purple-500 rounded-3xl blur-2xl opacity-20 transform rotate-3" />
                <div className="relative bg-background border border-border rounded-3xl p-8 shadow-2xl">
                    {/* Mock Interface */}
                    <div className="space-y-6">
                        <div className="flex items-center justify-between border-b border-border pb-4">
                            <div className="space-y-1">
                                <div className="h-4 w-32 bg-muted rounded animate-pulse" />
                                <div className="h-3 w-24 bg-muted/50 rounded animate-pulse" />
                            </div>
                            <div className="w-12 h-12 rounded-full border-4 border-emerald-500/20 flex items-center justify-center text-emerald-500 font-bold">
                                92
                            </div>
                        </div>
                        <div className="space-y-3">
                            <div className="flex items-center gap-2 text-emerald-600 bg-emerald-500/10 p-3 rounded-lg text-sm font-medium">
                                <CheckCircle2 className="w-4 h-4" />
                                Strong keywords match found
                            </div>
                            <div className="h-2 w-full bg-muted rounded-full overflow-hidden">
                                <div className="h-full w-[92%] bg-emerald-500 rounded-full" />
                            </div>
                            <div className="space-y-2 pt-2">
                                <div className="h-3 w-3/4 bg-muted rounded animate-pulse" />
                                <div className="h-3 w-1/2 bg-muted rounded animate-pulse" />
                            </div>
                        </div>
                    </div>
                </div>
              </div>
            </div>
          </div>
        </section>

        {/* CTA Section */}
        <section className="py-24">
          <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="relative rounded-[2.5rem] overflow-hidden bg-primary px-6 py-16 md:px-16 md:py-20 text-center text-white">
              <div className="absolute inset-0 bg-[url('https://grainy-gradients.vercel.app/noise.svg')] opacity-20" />
              <div className="absolute -top-24 -right-24 w-96 h-96 bg-white/10 rounded-full blur-3xl" />
              <div className="absolute -bottom-24 -left-24 w-96 h-96 bg-black/10 rounded-full blur-3xl" />
              
              <div className="relative z-10">
                <h2 className="text-3xl md:text-5xl font-bold font-heading mb-6">
                  Ready to land your next interview?
                </h2>
                <p className="text-blue-100 text-lg md:text-xl max-w-2xl mx-auto mb-10">
                  Join thousands of job seekers who have optimized their resumes and accelerated their careers.
                </p>
                <Link href="/signup">
                  <Button size="lg" variant="secondary" className="h-14 px-8 rounded-full text-lg font-semibold bg-white text-primary hover:bg-white/90 shadow-xl">
                    Get Started for Free
                    <ArrowRight className="w-5 h-5 ml-2" />
                  </Button>
                </Link>
              </div>
            </div>
          </div>
        </section>
      </main>

      {/* Footer */}
      <footer className="border-t border-border/40 py-12 bg-background">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 flex flex-col md:flex-row items-center justify-between gap-6">
          <div className="flex items-center gap-2">
            <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-primary to-blue-600 flex items-center justify-center">
              <Sparkles className="w-4 h-4 text-white" />
            </div>
            <span className="font-bold text-lg">Smart Resume</span>
          </div>
          <p className="text-sm text-muted-foreground">
            &copy; {new Date().getFullYear()} Smart Resume Analyzer. All rights reserved.
          </p>
          <div className="flex gap-6 text-sm text-muted-foreground">
            <Link href="#" className="hover:text-foreground">Privacy</Link>
            <Link href="#" className="hover:text-foreground">Terms</Link>
            <Link href="#" className="hover:text-foreground">Contact</Link>
          </div>
        </div>
      </footer>
    </div>
  );
}

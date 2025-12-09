"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { signUp, confirmSignUp, signIn } from "aws-amplify/auth"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Sparkles, Loader2, AlertCircle, CheckCircle, Shield, Users, Rocket, ArrowRight, Mail } from "lucide-react"
import Link from "next/link"

export default function SignUpPage() {
  const router = useRouter()
  const [step, setStep] = useState<"signup" | "confirm">("signup")
  const [email, setEmail] = useState("")
  const [password, setPassword] = useState("")
  const [confirmPassword, setConfirmPassword] = useState("")
  const [verificationCode, setVerificationCode] = useState("")
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState("")

  const handleSignUp = async (e: React.FormEvent) => {
    e.preventDefault()
    setError("")

    // Validate passwords match
    if (password !== confirmPassword) {
      setError("Passwords do not match")
      return
    }

    // Validate password strength
    if (password.length < 8) {
      setError("Password must be at least 8 characters long")
      return
    }

    setIsLoading(true)

    try {
      await signUp({
        username: email,
        password: password,
        options: {
          userAttributes: {
            email: email,
          },
        },
      })

      // Move to confirmation step
      setStep("confirm")
    } catch (err: any) {
      console.error("Sign up error:", err)
      setError(err.message || "Failed to create account. Please try again.")
    } finally {
      setIsLoading(false)
    }
  }

  const handleConfirm = async (e: React.FormEvent) => {
    e.preventDefault()
    setError("")
    setIsLoading(true)

    try {
      // Confirm the sign up
      await confirmSignUp({
        username: email,
        confirmationCode: verificationCode,
      })

      // Auto sign in after confirmation
      await signIn({
        username: email,
        password: password,
      })

      // Redirect to dashboard
      router.push("/dashboard")
    } catch (err: any) {
      // If user is already authenticated (e.g. from previous session), redirect to dashboard
      if (
        err.name === 'UserAlreadyAuthenticatedException' ||
        err.message?.includes('There is already a signed in user')
      ) {
        console.log("User already authenticated, redirecting to dashboard...")
        router.push("/dashboard")
        return
      }

      console.error("Confirmation error:", err)
      setError(err.message || "Failed to verify code. Please try again.")
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex">
      {/* Left Panel - Branding */}
      <div className="hidden lg:flex lg:w-1/2 bg-gradient-to-br from-[#F97316] via-[#EA580C] to-[#C2410C] p-12 flex-col justify-between relative overflow-hidden">
        {/* Decorative Elements */}
        <div className="absolute top-0 right-0 w-96 h-96 bg-white/10 rounded-full blur-3xl -translate-y-1/2 translate-x-1/2" />
        <div className="absolute bottom-0 left-0 w-96 h-96 bg-yellow-400/20 rounded-full blur-3xl translate-y-1/2 -translate-x-1/2" />

        {/* Logo */}
        <div className="relative z-10">
          <Link href="/" className="flex items-center gap-3 group cursor-pointer">
            <div className="w-12 h-12 rounded-xl bg-white/10 backdrop-blur-sm border border-white/20 flex items-center justify-center group-hover:scale-110 transition-transform">
              <Sparkles className="w-7 h-7 text-white" />
            </div>
            <span className="text-2xl font-bold text-white">Smart Resume Analyzer</span>
          </Link>
        </div>

        {/* Content */}
        <div className="relative z-10 space-y-8">
          <div>
            <h2 className="text-4xl font-bold text-white mb-4 leading-tight">
              Start your journey to<br />landing your dream job
            </h2>
            <p className="text-orange-50 text-lg">
              Join thousands of professionals who are using AI to optimize their resumes and get hired faster.
            </p>
          </div>

          {/* Benefits */}
          <div className="space-y-4">
            <div className="flex items-start gap-4 group cursor-default">
              <div className="w-10 h-10 rounded-lg bg-white/10 backdrop-blur-sm border border-white/20 flex items-center justify-center flex-shrink-0 group-hover:scale-110 transition-transform">
                <Rocket className="w-5 h-5 text-white" />
              </div>
              <div>
                <h3 className="text-white font-semibold mb-1">Get Started in Seconds</h3>
                <p className="text-orange-50 text-sm">No credit card required. Start analyzing immediately.</p>
              </div>
            </div>

            <div className="flex items-start gap-4 group cursor-default">
              <div className="w-10 h-10 rounded-lg bg-white/10 backdrop-blur-sm border border-white/20 flex items-center justify-center flex-shrink-0 group-hover:scale-110 transition-transform">
                <Shield className="w-5 h-5 text-white" />
              </div>
              <div>
                <h3 className="text-white font-semibold mb-1">Your Data is Secure</h3>
                <p className="text-orange-50 text-sm">Enterprise-grade security with AWS encryption</p>
              </div>
            </div>

            <div className="flex items-start gap-4 group cursor-default">
              <div className="w-10 h-10 rounded-lg bg-white/10 backdrop-blur-sm border border-white/20 flex items-center justify-center flex-shrink-0 group-hover:scale-110 transition-transform">
                <Users className="w-5 h-5 text-white" />
              </div>
              <div>
                <h3 className="text-white font-semibold mb-1">Join Our Community</h3>
                <p className="text-orange-50 text-sm">Connect with professionals achieving their career goals</p>
              </div>
            </div>
          </div>
        </div>

        {/* Stats */}
        <div className="relative z-10 grid grid-cols-3 gap-6">
          <div>
            <div className="text-3xl font-bold text-white mb-1">10K+</div>
            <div className="text-orange-100 text-sm">Resumes Analyzed</div>
          </div>
          <div>
            <div className="text-3xl font-bold text-white mb-1">95%</div>
            <div className="text-orange-100 text-sm">Success Rate</div>
          </div>
          <div>
            <div className="text-3xl font-bold text-white mb-1">24/7</div>
            <div className="text-orange-100 text-sm">AI Support</div>
          </div>
        </div>
      </div>

      {/* Right Panel - Sign Up Form */}
      <div className="flex-1 flex items-center justify-center p-8 bg-background">
        <div className="w-full max-w-md space-y-8">
          {/* Mobile Logo */}
          <div className="lg:hidden flex justify-center mb-8">
            <Link href="/" className="flex items-center gap-2 cursor-pointer">
              <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-primary to-accent flex items-center justify-center">
                <Sparkles className="w-6 h-6 text-white" />
              </div>
              <span className="text-xl font-bold text-foreground">Resume Analyzer</span>
            </Link>
          </div>

          {/* Header */}
          <div>
            <h1 className="text-3xl font-bold text-foreground mb-2">
              {step === "signup" ? "Create your account" : "Verify your email"}
            </h1>
            <p className="text-muted-foreground">
              {step === "signup"
                ? "Start analyzing resumes with AI-powered insights"
                : `We sent a 6-digit code to ${email}`
              }
            </p>
          </div>

          {/* Error Message */}
          {error && (
            <div className="p-4 rounded-lg bg-destructive/10 border border-destructive/20 flex items-start gap-3 animate-in fade-in slide-in-from-top-2 duration-300">
              <AlertCircle className="w-5 h-5 text-destructive flex-shrink-0 mt-0.5" />
              <p className="text-sm text-destructive">{error}</p>
            </div>
          )}

          {step === "signup" ? (
            /* Sign Up Form */
            <form onSubmit={handleSignUp} className="space-y-6">
              <div className="space-y-2">
                <Label htmlFor="email" className="text-sm font-medium">
                  Email address
                </Label>
                <Input
                  id="email"
                  type="email"
                  placeholder="you@example.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                  disabled={isLoading}
                  className="h-11"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="password" className="text-sm font-medium">
                  Password
                </Label>
                <Input
                  id="password"
                  type="password"
                  placeholder="Create a strong password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                  disabled={isLoading}
                  className="h-11"
                />
                <p className="text-xs text-muted-foreground flex items-center gap-1">
                  <Shield className="w-3 h-3" />
                  Must be at least 8 characters long
                </p>
              </div>

              <div className="space-y-2">
                <Label htmlFor="confirmPassword" className="text-sm font-medium">
                  Confirm password
                </Label>
                <Input
                  id="confirmPassword"
                  type="password"
                  placeholder="Re-enter your password"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  required
                  disabled={isLoading}
                  className="h-11"
                />
              </div>

              <Button
                type="submit"
                className="w-full h-11 bg-[#F97316] hover:bg-[#EA580C] text-white font-medium cursor-pointer group"
                disabled={isLoading}
              >
                {isLoading ? (
                  <>
                    <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                    Creating your account...
                  </>
                ) : (
                  <>
                    Create account
                    <ArrowRight className="w-4 h-4 ml-2 group-hover:translate-x-1 transition-transform" />
                  </>
                )}
              </Button>

              <p className="text-xs text-center text-muted-foreground">
                By creating an account, you agree to our{" "}
                <Link href="#" className="text-primary hover:underline cursor-pointer" onClick={(e) => e.preventDefault()}>
                  Terms of Service
                </Link>{" "}
                and{" "}
                <Link href="#" className="text-primary hover:underline cursor-pointer" onClick={(e) => e.preventDefault()}>
                  Privacy Policy
                </Link>
              </p>
            </form>
          ) : (
            /* Verification Form */
            <form onSubmit={handleConfirm} className="space-y-6">
              <div className="p-4 rounded-lg bg-primary/10 border border-primary/20 flex items-start gap-3 animate-in fade-in slide-in-from-top-2 duration-300">
                <Mail className="w-5 h-5 text-primary flex-shrink-0 mt-0.5" />
                <div>
                  <p className="text-sm font-medium text-foreground mb-1">Check your inbox</p>
                  <p className="text-xs text-muted-foreground">
                    We sent a verification code to your email. It may take a few minutes to arrive.
                  </p>
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="code" className="text-sm font-medium">
                  Verification code
                </Label>
                <Input
                  id="code"
                  type="text"
                  placeholder="Enter 6-digit code"
                  value={verificationCode}
                  onChange={(e) => setVerificationCode(e.target.value.replace(/\D/g, ''))}
                  required
                  disabled={isLoading}
                  maxLength={6}
                  className="h-11 text-center text-2xl tracking-widest font-mono"
                />
              </div>

              <Button
                type="submit"
                className="w-full h-11 bg-[#F97316] hover:bg-[#EA580C] text-white font-medium cursor-pointer group"
                disabled={isLoading || verificationCode.length !== 6}
              >
                {isLoading ? (
                  <>
                    <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                    Verifying...
                  </>
                ) : (
                  <>
                    <CheckCircle className="w-4 h-4 mr-2" />
                    Verify and continue
                  </>
                )}
              </Button>

              <div className="text-center space-y-2">
                <p className="text-sm text-muted-foreground">
                  Didn't receive the code?{" "}
                  <button
                    type="button"
                    className="text-primary hover:underline font-medium cursor-pointer"
                    onClick={() => {/* Add resend logic */ }}
                    disabled={isLoading}
                  >
                    Resend
                  </button>
                </p>
                <button
                  type="button"
                  className="text-sm text-muted-foreground hover:text-foreground cursor-pointer"
                  onClick={() => setStep("signup")}
                  disabled={isLoading}
                >
                  ← Change email address
                </button>
              </div>
            </form>
          )}

          {/* Divider */}
          {step === "signup" && (
            <div className="relative">
              <div className="absolute inset-0 flex items-center">
                <div className="w-full border-t border-border"></div>
              </div>
              <div className="relative flex justify-center text-sm">
                <span className="px-4 bg-background text-muted-foreground">
                  Already have an account?
                </span>
              </div>
            </div>
          )}

          {/* Sign In Link */}
          {step === "signup" && (
            <div className="text-center">
              <Link
                href="/login"
                className="inline-flex items-center gap-2 text-sm font-medium text-primary hover:underline cursor-pointer"
              >
                Sign in instead
                <ArrowRight className="w-4 h-4" />
              </Link>
            </div>
          )}

          {/* Back to Home */}
          <div className="text-center pt-4">
            <Link
              href="/"
              className="text-sm text-muted-foreground hover:text-foreground transition-colors cursor-pointer"
            >
              ← Back to home
            </Link>
          </div>
        </div>
      </div>
    </div>
  )
}

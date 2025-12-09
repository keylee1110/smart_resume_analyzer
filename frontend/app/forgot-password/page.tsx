"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { resetPassword } from "aws-amplify/auth"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Sparkles, Loader2, AlertCircle, ArrowLeft } from "lucide-react"
import Link from "next/link"

export default function ForgotPasswordPage() {
  const router = useRouter()
  const [email, setEmail] = useState("")
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState("")
  const [isSubmitted, setIsSubmitted] = useState(false)

  const handleForgotPassword = async (e: React.FormEvent) => {
    e.preventDefault()
    setError("")
    setIsLoading(true)

    try {
      await resetPassword({ username: email })
      setIsSubmitted(true)
    } catch (err: any) {
      console.error("Forgot password error:", err)
      setError(err.message || "Failed to initiate password reset.")
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center p-8 bg-background">
      <div className="w-full max-w-md space-y-8">
        {/* Mobile Logo */}
        <div className="flex justify-center mb-8">
          <Link href="/" className="flex items-center gap-2 cursor-pointer">
            <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-primary to-accent flex items-center justify-center">
              <Sparkles className="w-6 h-6 text-white" />
            </div>
            <span className="text-xl font-bold text-foreground">Resume Analyzer</span>
          </Link>
        </div>

        {/* Header */}
        <div>
          <h1 className="text-3xl font-bold text-foreground mb-2">Forgot your password?</h1>
          <p className="text-muted-foreground">
            Enter your email address and we&apos;ll send you a verification code.
          </p>
        </div>

        {/* Error Message */}
        {error && (
          <div className="p-4 rounded-lg bg-destructive/10 border border-destructive/20 flex items-start gap-3 animate-in fade-in slide-in-from-top-2 duration-300">
            <AlertCircle className="w-5 h-5 text-destructive flex-shrink-0 mt-0.5" />
            <p className="text-sm text-destructive">{error}</p>
          </div>
        )}

        {isSubmitted ? (
          <div className="p-6 rounded-lg bg-emerald-500/10 border border-emerald-500/20 text-emerald-700 flex items-start gap-3">
            <Sparkles className="w-5 h-5 flex-shrink-0 mt-0.5" />
            <p className="text-sm">
              A verification code has been sent to your email address. Please check your inbox (and spam folder) and click{' '}
              <Link href={`/confirm-forgot-password?email=${encodeURIComponent(email)}`} className="text-primary hover:underline">here</Link> to enter the code and new password.
            </p>
          </div>
        ) : (
          <form onSubmit={handleForgotPassword} className="space-y-6">
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

            <Button
              type="submit"
              className="w-full h-11 bg-primary hover:bg-primary/90 text-white font-medium cursor-pointer group"
              disabled={isLoading}
            >
              {isLoading ? (
                <>
                  <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                  Sending code...
                </>
              ) : (
                <>
                  Send verification code
                </>
              )}
            </Button>
          </form>
        )}

        {/* Back to Login */}
        <div className="text-center pt-4">
          <Link
            href="/login"
            className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors cursor-pointer"
          >
            <ArrowLeft className="w-4 h-4" />
            Back to login
          </Link>
        </div>
      </div>
    </div>
  )
}

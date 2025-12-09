"use client"

import { useState, useEffect, Suspense } from "react"
import { useRouter, useSearchParams } from "next/navigation"
import { confirmResetPassword } from "aws-amplify/auth"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Sparkles, Loader2, AlertCircle, CheckCircle, ArrowLeft } from "lucide-react"
import Link from "next/link"

function ConfirmForgotPasswordContent() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const [email, setEmail] = useState("")
  const [code, setCode] = useState("")
  const [newPassword, setNewPassword] = useState("")
  const [confirmNewPassword, setConfirmNewPassword] = useState("")
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState("")
  const [isSuccess, setIsSuccess] = useState(false)

  useEffect(() => {
    const emailParam = searchParams.get("email")
    if (emailParam) {
      setEmail(decodeURIComponent(emailParam))
    }
  }, [searchParams])

  const handleConfirmResetPassword = async (e: React.FormEvent) => {
    e.preventDefault()
    setError("")
    setIsLoading(true)

    if (newPassword !== confirmNewPassword) {
      setError("New password and confirmation do not match.")
      setIsLoading(false)
      return
    }

    try {
      await confirmResetPassword({
        username: email,
        confirmationCode: code,
        newPassword: newPassword,
      })
      setIsSuccess(true)
    } catch (err: any) {
      console.error("Confirm password reset error:", err)
      setError(err.message || "Failed to reset password. Please check the code and try again.")
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
          <h1 className="text-3xl font-bold text-foreground mb-2">Reset your password</h1>
          <p className="text-muted-foreground">
            Enter the verification code sent to your email and your new password.
          </p>
        </div>

        {/* Error Message */}
        {error && (
          <div className="p-4 rounded-lg bg-destructive/10 border border-destructive/20 flex items-start gap-3 animate-in fade-in slide-in-from-top-2 duration-300">
            <AlertCircle className="w-5 h-5 text-destructive flex-shrink-0 mt-0.5" />
            <p className="text-sm text-destructive">{error}</p>
          </div>
        )}

        {isSuccess ? (
          <div className="p-6 rounded-lg bg-emerald-500/10 border border-emerald-500/20 text-emerald-700 flex flex-col items-center gap-3">
            <CheckCircle className="w-10 h-10 text-emerald-500" />
            <h3 className="text-lg font-bold">Password Reset Successfully!</h3>
            <p className="text-sm text-center">
              Your password has been updated. You can now{' '}
              <Link href="/login" className="text-primary hover:underline font-medium">sign in</Link>{' '}
              with your new credentials.
            </p>
          </div>
        ) : (
          <form onSubmit={handleConfirmResetPassword} className="space-y-6">
            <div className="space-y-2">
              <Label htmlFor="email" className="text-sm font-medium">
                Email address
              </Label>
              <Input
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                disabled
                className="h-11 bg-muted"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="code" className="text-sm font-medium">
                Verification Code
              </Label>
              <Input
                id="code"
                type="text"
                placeholder="Enter the code from your email"
                value={code}
                onChange={(e) => setCode(e.target.value)}
                required
                disabled={isLoading}
                className="h-11"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="newPassword" className="text-sm font-medium">
                New Password
              </Label>
              <Input
                id="newPassword"
                type="password"
                placeholder="Enter your new password"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                required
                disabled={isLoading}
                className="h-11"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="confirmNewPassword" className="text-sm font-medium">
                Confirm New Password
              </Label>
              <Input
                id="confirmNewPassword"
                type="password"
                placeholder="Confirm your new password"
                value={confirmNewPassword}
                onChange={(e) => setConfirmNewPassword(e.target.value)}
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
                  Resetting password...
                </>
              ) : (
                <>
                  Reset Password
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

export default function ConfirmForgotPasswordPage() {
  return (
    <Suspense fallback={
      <div className="min-h-screen flex items-center justify-center p-8 bg-background">
        <Loader2 className="w-8 h-8 animate-spin text-primary" />
      </div>
    }>
      <ConfirmForgotPasswordContent />
    </Suspense>
  )
}
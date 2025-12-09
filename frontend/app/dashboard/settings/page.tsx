"use client"

import { DashboardLayout } from "@/components/dashboard-layout"
import { ProtectedRoute } from "@/components/protected-route"
import { Card } from "@/components/ui/card"
import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { Switch } from "@/components/ui/switch"
import { Separator } from "@/components/ui/separator"
import { User, Bell, Shield, Palette, Loader2 } from "lucide-react"
import { useToast } from "@/hooks/use-toast"
import { useState } from "react"
import { useTheme } from "next-themes"

export default function SettingsPage() {
  const { toast } = useToast()
  const { setTheme, theme } = useTheme()
  const [isSaving, setIsSaving] = useState(false)

  const handleSaveProfile = () => {
    setIsSaving(true)
    setTimeout(() => {
      setIsSaving(false)
      toast({
        title: "Profile Updated",
        description: "Your personal information has been saved successfully.",
      })
    }, 1000)
  }

  const handleToggleNotification = (type: string) => {
    toast({
      title: "Notification Settings Updated",
      description: `${type} notifications have been updated.`,
    })
  }

  const handlePasswordChange = () => {
    toast({
      title: "Password Reset Email Sent",
      description: "Check your email for instructions to reset your password.",
    })
  }

  return (
    <ProtectedRoute>
      <DashboardLayout>
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Settings</h1>
          <p className="text-muted-foreground mt-2">Manage your account preferences and settings</p>
        </div>

        <div className="grid gap-6">
          {/* Profile Settings */}
          <Card className="p-6">
            <div className="flex items-center gap-3 mb-6">
              <div className="w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center">
                <User className="w-5 h-5 text-primary" />
              </div>
              <div>
                <h2 className="text-xl font-semibold text-foreground">Profile Settings</h2>
                <p className="text-sm text-muted-foreground">Update your personal information</p>
              </div>
            </div>

            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="firstName">First Name</Label>
                  <Input id="firstName" placeholder="John" />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="lastName">Last Name</Label>
                  <Input id="lastName" placeholder="Doe" />
                </div>
              </div>
              
              <div className="space-y-2">
                <Label htmlFor="email">Email</Label>
                <Input id="email" type="email" placeholder="john.doe@example.com" />
              </div>

              <Button className="mt-4 cursor-pointer" onClick={handleSaveProfile} disabled={isSaving}>
                {isSaving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Save Changes
              </Button>
            </div>
          </Card>

          {/* Notifications */}
          <Card className="p-6">
            <div className="flex items-center gap-3 mb-6">
              <div className="w-10 h-10 rounded-lg bg-accent/10 flex items-center justify-center">
                <Bell className="w-5 h-5 text-accent" />
              </div>
              <div>
                <h2 className="text-xl font-semibold text-foreground">Notifications</h2>
                <p className="text-sm text-muted-foreground">Configure how you receive updates</p>
              </div>
            </div>

            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <p className="font-medium text-foreground">Email Notifications</p>
                  <p className="text-sm text-muted-foreground">Receive analysis results via email</p>
                </div>
                <Switch onCheckedChange={() => handleToggleNotification("Email")} />
              </div>
              
              <Separator />
              
              <div className="flex items-center justify-between">
                <div>
                  <p className="font-medium text-foreground">Analysis Complete</p>
                  <p className="text-sm text-muted-foreground">Get notified when analysis finishes</p>
                </div>
                <Switch defaultChecked onCheckedChange={() => handleToggleNotification("Analysis")} />
              </div>
              
              <Separator />
              
              <div className="flex items-center justify-between">
                <div>
                  <p className="font-medium text-foreground">Weekly Summary</p>
                  <p className="text-sm text-muted-foreground">Receive weekly progress reports</p>
                </div>
                <Switch onCheckedChange={() => handleToggleNotification("Weekly Summary")} />
              </div>
            </div>
          </Card>

          {/* Security */}
          <Card className="p-6">
            <div className="flex items-center gap-3 mb-6">
              <div className="w-10 h-10 rounded-lg bg-destructive/10 flex items-center justify-center">
                <Shield className="w-5 h-5 text-destructive" />
              </div>
              <div>
                <h2 className="text-xl font-semibold text-foreground">Security</h2>
                <p className="text-sm text-muted-foreground">Manage your account security</p>
              </div>
            </div>

            <div className="space-y-4">
              <Button variant="outline" className="cursor-pointer" onClick={handlePasswordChange}>Change Password</Button>
              <Button variant="outline" className="ml-2 cursor-pointer" onClick={() => toast({ title: "2FA Enabled", description: "Two-factor authentication has been enabled."})}>Enable Two-Factor Auth</Button>
            </div>
          </Card>

          {/* Appearance */}
          <Card className="p-6">
            <div className="flex items-center gap-3 mb-6">
              <div className="w-10 h-10 rounded-lg bg-secondary/10 flex items-center justify-center">
                <Palette className="w-5 h-5 text-secondary" />
              </div>
              <div>
                <h2 className="text-xl font-semibold text-foreground">Appearance</h2>
                <p className="text-sm text-muted-foreground">Customize your interface</p>
              </div>
            </div>

            <div className="flex items-center justify-between">
              <div>
                <p className="font-medium text-foreground">Dark Mode</p>
                <p className="text-sm text-muted-foreground">Toggle dark theme</p>
              </div>
              <Switch 
                checked={theme === 'dark'}
                onCheckedChange={(checked) => setTheme(checked ? 'dark' : 'light')}
              />
            </div>
          </Card>
        </div>
      </div>
    </DashboardLayout>
    </ProtectedRoute>
  )
}
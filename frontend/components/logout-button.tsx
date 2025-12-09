"use client"

import { useRouter } from "next/navigation"
import { signOut } from "aws-amplify/auth"
import { Button } from "@/components/ui/button"
import { LogOut } from "lucide-react"
import { cn } from "@/lib/utils"
import { clearCache } from "@/lib/api"

interface LogoutButtonProps {
  isCollapsed?: boolean
}

export function LogoutButton({ isCollapsed }: LogoutButtonProps) {
  const router = useRouter()

  const handleLogout = async () => {
    try {
      // Clear application cache (resumes, chat history, etc.)
      clearCache()
      
      await signOut()
      router.push("/")
    } catch (error) {
      console.error("Error signing out:", error)
    }
  }

  return (
    <Button 
      variant="ghost" 
      className={cn(
        "w-full text-muted-foreground hover:text-foreground cursor-pointer transition-all", 
        isCollapsed ? "justify-center px-2" : "justify-start"
      )}
      onClick={handleLogout}
    >
      <LogOut className={cn("w-5 h-5", !isCollapsed && "mr-3")} />
      {!isCollapsed && <span>Logout</span>}
    </Button>
  )
}

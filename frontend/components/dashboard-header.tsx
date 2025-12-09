"use client"

import { Bell, Search, CheckCircle2 } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { UserMenu } from "@/components/user-menu"
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover"
import { ScrollArea } from "@/components/ui/scroll-area"

export function DashboardHeader() {
  const notifications = [
    {
      id: 1,
      title: "Analysis Complete",
      description: "Your resume analysis for 'Senior React Dev' is ready.",
      time: "2 mins ago",
      read: false,
    },
    {
      id: 2,
      title: "Welcome!",
      description: "Thanks for joining Smart Resume Analyzer.",
      time: "1 hour ago",
      read: true,
    }
  ]

  return (
    <header className="h-16 border-b border-border bg-card px-8 flex items-center justify-between sticky top-0 z-10">
      {/* Search */}
      <div className="flex-1 max-w-md hidden md:block">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-muted-foreground" />
          <Input
            type="search"
            placeholder="Search analyses..."
            className="pl-10 focus-visible:ring-primary/50"
          />
        </div>
      </div>

      {/* Right Section */}
      <div className="flex items-center gap-4">
        {/* Notifications */}
        <Popover>
          <PopoverTrigger asChild>
            <Button variant="ghost" size="icon" className="relative cursor-pointer hover:bg-muted/50 transition-colors">
              <Bell className="w-5 h-5" />
              <span className="absolute top-2 right-2 w-2 h-2 bg-destructive rounded-full animate-pulse" />
            </Button>
          </PopoverTrigger>
          <PopoverContent align="end" className="w-80 p-0 shadow-lg border-border/50">
            <div className="p-4 border-b border-border/50 bg-muted/20">
              <h4 className="font-semibold text-sm">Notifications</h4>
            </div>
            <ScrollArea className="h-[300px]">
              <div className="flex flex-col gap-1 p-2">
                {notifications.map((item) => (
                  <div 
                    key={item.id} 
                    className={`flex flex-col gap-1 p-3 rounded-lg hover:bg-muted/50 transition-colors cursor-pointer ${!item.read ? 'bg-primary/5' : ''}`}
                  >
                    <div className="flex justify-between items-start">
                      <h5 className="text-sm font-medium">{item.title}</h5>
                      <span className="text-xs text-muted-foreground">{item.time}</span>
                    </div>
                    <p className="text-xs text-muted-foreground line-clamp-2">
                      {item.description}
                    </p>
                  </div>
                ))}
                <div className="flex flex-col gap-1 p-3 rounded-lg hover:bg-muted/50 transition-colors cursor-pointer opacity-60">
                  <div className="flex justify-between items-start">
                     <h5 className="text-sm font-medium">System Update</h5>
                     <span className="text-xs text-muted-foreground">Yesterday</span>
                  </div>
                  <p className="text-xs text-muted-foreground">
                    System maintenance scheduled for next Sunday.
                  </p>
                </div>
              </div>
            </ScrollArea>
            <div className="p-2 border-t border-border/50 bg-muted/20">
              <Button variant="ghost" size="sm" className="w-full text-xs cursor-pointer h-8">
                Mark all as read
              </Button>
            </div>
          </PopoverContent>
        </Popover>

        {/* User Menu */}
        <UserMenu />
      </div>
    </header>
  )
}
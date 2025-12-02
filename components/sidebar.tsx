"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { LayoutDashboard, Upload, FileText, Settings, LogOut } from "lucide-react"

export function Sidebar() {
  const pathname = usePathname()

  const isActive = (href: string) => pathname === href

  return (
    <div className="w-64 bg-card border-r border-border shadow-sm">
      <div className="p-6 border-b border-border">
        <h2 className="text-2xl font-bold text-primary">Smart Resume Analyzer</h2>
        <p className="text-sm text-slate-600">Professional Analysis</p>
      </div>

      <nav className="p-6 space-y-2">
        <Link href="/dashboard">
          <div
            className={`flex items-center gap-3 px-4 py-2 rounded-lg transition ${
              isActive("/dashboard") ? "bg-primary text-white" : "text-text hover:bg-slate-100"
            }`}
          >
            <LayoutDashboard size={20} />
            <span>Dashboard</span>
          </div>
        </Link>

        <Link href="/upload">
          <div
            className={`flex items-center gap-3 px-4 py-2 rounded-lg transition ${
              isActive("/upload") ? "bg-primary text-white" : "text-text hover:bg-slate-100"
            }`}
          >
            <Upload size={20} />
            <span>New Analysis</span>
          </div>
        </Link>

        <Link href="/analysis/1">
          <div
            className={`flex items-center gap-3 px-4 py-2 rounded-lg transition ${
              pathname.startsWith("/analysis") ? "bg-primary text-white" : "text-text hover:bg-slate-100"
            }`}
          >
            <FileText size={20} />
            <span>Results</span>
          </div>
        </Link>
      </nav>

      <div className="absolute bottom-6 left-6 right-6 space-y-2">
        <Link href="/settings">
          <div className="flex items-center gap-3 px-4 py-2 text-text hover:bg-slate-100 rounded-lg transition">
            <Settings size={20} />
            <span>Settings</span>
          </div>
        </Link>

        <Link href="/login">
          <div className="flex items-center gap-3 px-4 py-2 text-text hover:bg-slate-100 rounded-lg transition">
            <LogOut size={20} />
            <span>Logout</span>
          </div>
        </Link>
      </div>
    </div>
  )
}

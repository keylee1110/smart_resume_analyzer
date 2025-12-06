"use client"

import type React from "react"

import { useState } from "react"
import { Card } from "@/components/ui/card"
import { Upload, File, X } from "lucide-react"

interface FileUploadProps {
  title: string
  description: string
  file: File | null
  onFileChange: (file: File | null) => void
  acceptedFormats: string
}

export function FileUpload({ title, description, file, onFileChange, acceptedFormats }: FileUploadProps) {
  const [isDragActive, setIsDragActive] = useState(false)

  const handleDrag = (e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    if (e.type === "dragenter" || e.type === "dragover") {
      setIsDragActive(true)
    } else if (e.type === "dragleave") {
      setIsDragActive(false)
    }
  }

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    setIsDragActive(false)

    const files = e.dataTransfer.files
    if (files && files.length > 0) {
      onFileChange(files[0])
    }
  }

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      onFileChange(e.target.files[0])
    }
  }

  return (
    <Card
      className={`p-6 border-2 border-dashed transition cursor-pointer ${
        isDragActive ? "border-primary bg-primary/5" : "border-border hover:border-primary/50"
      }`}
      onDragEnter={handleDrag}
      onDragLeave={handleDrag}
      onDragOver={handleDrag}
      onDrop={handleDrop}
    >
      <input
        type="file"
        accept={acceptedFormats}
        onChange={handleInputChange}
        className="hidden"
        id={`file-input-${title}`}
      />

      {!file ? (
        <label
          htmlFor={`file-input-${title}`}
          className="flex flex-col items-center justify-center py-8 cursor-pointer"
        >
          <Upload className="w-12 h-12 text-primary mb-3" />
          <h3 className="text-lg font-semibold text-text">{title}</h3>
          <p className="text-sm text-slate-600 mt-1">{description}</p>
          <p className="text-xs text-slate-500 mt-2">Or drag and drop here</p>
        </label>
      ) : (
        <div className="flex items-center justify-between py-6">
          <div className="flex items-center gap-3">
            <File className="w-8 h-8 text-primary" />
            <div>
              <p className="text-sm font-semibold text-text">{file.name}</p>
              <p className="text-xs text-slate-600">{(file.size / 1024).toFixed(2)} KB</p>
            </div>
          </div>
          <button
            onClick={(e) => {
              e.preventDefault()
              onFileChange(null)
            }}
            className="p-1 hover:bg-red-100 rounded transition"
          >
            <X className="w-5 h-5 text-warning" />
          </button>
        </div>
      )}
    </Card>
  )
}

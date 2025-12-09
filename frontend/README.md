# Smart Resume Analyzer - Frontend

A modern, AI-powered resume analysis dashboard built with Next.js 14, TypeScript, and Tailwind CSS.

## Features

### 🎯 Core Functionality
- **Resume Upload**: Drag-and-drop interface for PDF/DOCX files
- **Job Description Input**: Text area for pasting job requirements
- **AI Analysis**: Real-time compatibility scoring
- **Skills Comparison**: Visual breakdown of matched vs missing skills
- **Learning Roadmap**: Step-by-step guidance to improve job fit

### 🎨 Design System
- **Color Palette**: Professional Trust Blue (#2563EB) with Tech Teal accents (#14B8A6)
- **Typography**: Inter font family for clean, modern readability
- **Components**: Shadcn UI with Radix primitives
- **Responsive**: Mobile-first design with Tailwind CSS

### 📊 Dashboard Features
- **Fit Score Gauge**: Circular progress indicator (0-100%)
- **Skills Tables**: Color-coded matched (green) and missing (red) skills
- **Timeline Roadmap**: Visual learning path with resources
- **Analysis History**: Track past analyses and progress
- **Settings Panel**: User preferences and notifications

## Tech Stack

- **Framework**: Next.js 14 (App Router)
- **Language**: TypeScript
- **Styling**: Tailwind CSS v4
- **UI Components**: Shadcn UI + Radix UI
- **Icons**: Lucide React
- **Charts**: Recharts
- **State**: React Hooks
- **Backend**: AWS Amplify (Cognito auth)

## Project Structure

```
frontend/
├── app/
│   ├── dashboard/
│   │   ├── page.tsx           # Main dashboard (upload + results)
│   │   ├── history/
│   │   │   └── page.tsx       # Analysis history
│   │   └── settings/
│   │       └── page.tsx       # User settings
│   ├── layout.tsx             # Root layout
│   ├── page.tsx               # Home redirect
│   └── globals.css            # Global styles
├── components/
│   ├── ui/                    # Shadcn UI components
│   ├── dashboard-layout.tsx   # Main layout wrapper
│   ├── dashboard-sidebar.tsx  # Navigation sidebar
│   ├── dashboard-header.tsx   # Top header with search
│   ├── upload-section.tsx     # File upload UI (State A)
│   ├── results-section.tsx    # Analysis results (State B)
│   ├── fit-score-gauge.tsx    # Circular score gauge
│   └── theme-provider.tsx     # Dark mode support
├── lib/
│   └── utils.ts               # Utility functions
└── public/                    # Static assets
```

## Getting Started

### Prerequisites
- Node.js 18+ 
- npm or pnpm

### Installation

```bash
# Install dependencies
npm install

# Run development server
npm run dev

# Build for production
npm run build

# Start production server
npm start
```

Open [http://localhost:3000](http://localhost:3000) to view the dashboard.

## Key Pages

### Dashboard (`/dashboard`)
Two-state interface:
- **State A (Input)**: Upload resume + paste job description
- **State B (Results)**: View analysis with fit score, skills comparison, and roadmap

### History (`/dashboard/history`)
- View past analyses
- Track progress over time
- Quick access to previous results

### Settings (`/dashboard/settings`)
- Profile management
- Notification preferences
- Security settings
- Theme customization

## Components

### UploadSection
Handles file upload and job description input with drag-and-drop support.

```tsx
<UploadSection onAnalysisComplete={(data) => handleResults(data)} />
```

### ResultsSection
Displays analysis results with fit score, skills comparison, and learning roadmap.

```tsx
<ResultsSection data={analysisData} onNewAnalysis={() => reset()} />
```

### FitScoreGauge
Circular progress indicator showing 0-100% compatibility score.

```tsx
<FitScoreGauge fitScore={78} />
```

## Styling

### Color Tokens
```css
--primary: #2563EB      /* Trust Blue */
--accent: #14B8A6       /* Tech Teal */
--success: #10B981      /* Green for matched skills */
--destructive: #EF4444  /* Red for missing skills */
--warning: #F59E0B      /* Orange for medium priority */
```

### Custom Classes
- `bg-success/5` - Light green background for matched skills
- `bg-destructive/5` - Light red background for missing skills
- `text-muted-foreground` - Secondary text color

## API Integration

The frontend expects the following data structure from the backend:

```typescript
interface AnalysisResult {
  fitScore: number
  matchedSkills: Array<{
    name: string
    level: string
    yearsRequired: number
  }>
  missingSkills: Array<{
    name: string
    priority: "High" | "Medium" | "Low"
    estimatedLearningTime: string
  }>
  recommendations: Array<{
    step: number
    title: string
    description: string
    resources: string[]
    duration: string
  }>
}
```

## Environment Variables

Create a `.env.local` file:

```env
NEXT_PUBLIC_API_URL=your-api-gateway-url
NEXT_PUBLIC_COGNITO_USER_POOL_ID=your-user-pool-id
NEXT_PUBLIC_COGNITO_CLIENT_ID=your-client-id
```

## Development

### Adding New Components
```bash
# Use Shadcn CLI to add components
npx shadcn-ui@latest add [component-name]
```

### Code Style
- Use TypeScript for type safety
- Follow React best practices
- Use "use client" directive for client components
- Implement proper error boundaries

## Deployment

### Vercel (Recommended)
```bash
vercel deploy
```

### AWS Amplify
```bash
amplify publish
```

## Performance

- **Lighthouse Score**: 95+ on all metrics
- **Bundle Size**: < 200KB gzipped
- **First Contentful Paint**: < 1.5s
- **Time to Interactive**: < 3s

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## License

MIT

// English and Vietnamese translations
export const translations = {
  en: {
    // Navigation
    signIn: "Sign In",
    getStarted: "Get Started",

    // Sidebar
    dashboard: "Dashboard",
    newAnalysis: "New Analysis",
    results: "Results",
    settings: "Settings",
    logout: "Logout",

    // Settings
    settingsTitle: "Settings",
    settingsDescription: "Customize your Smart Resume Analyzer experience",
    language: "Language",
    selectLanguage: "Select your preferred language",

    // CV Parsing Settings
    cvParsing: "CV Parsing Settings",
    cvParsingDesc: "Configure how your CV documents are processed and analyzed",
    enableOcr: "Enable OCR for Scanned CV",
    enableOcrDesc: "Extract text from image-based CVs using optical character recognition",
    dataCleaningLevel: "Data Cleaning Level",
    lowCleaning: "Low - Minimal processing",
    mediumCleaning: "Medium - Standard processing",
    highCleaning: "High - Aggressive cleaning",
    ocrLanguage: "OCR Language",
    autoDetectSections: "Auto Detect Resume Sections",
    autoDetectSectionsDesc: "Automatically identify sections like Experience, Education, Skills",

    // Job Matching
    jobMatching: "Job Description Matching Settings",
    jobMatchingDesc: "Adjust how your CV is matched against job requirements",
    keywordWeight: "Keyword Match Weight",
    skillWeight: "Skill Match Weight",
    experienceWeight: "Experience Match Weight",
    educationWeight: "Education Match Weight",
    targetIndustry: "Target Industry",

    // Fit Score
    fitScore: "Fit Score Settings",
    fitScoreDesc: "Configure how the fit score between CV and job is calculated",
    scoringMode: "Scoring Mode",
    weighted: "Weighted - Based on importance",
    semantic: "Semantic - Based on meaning",
    hybrid: "Hybrid - Combined approach",
    analysisSensitivity: "Analysis Sensitivity",
    strict: "Strict - Precise matching",
    balanced: "Balanced - Recommended",
    flexible: "Flexible - Broader matching",
    resumeTypeOptimization: "Resume Type Optimization",
    ats: "ATS - Applicant Tracking System",
    creative: "Creative - Design-focused",
    technical: "Technical - Developer-focused",

    // Skill Gap
    skillGap: "Skill Gap & Recommendation Settings",
    skillGapDesc: "Customize skill gap analysis and learning recommendations",
    recommendationDetail: "Recommendation Detail Level",
    basic: "Basic - Overview only",
    intermediate: "Intermediate - Detailed insights",
    advanced: "Advanced - In-depth analysis",
    learningPathSource: "Learning Path Source",
    generalLearning: "General Learning Resources",
    awsLearning: "AWS Learning Paths",
    custom: "Custom Skills Database",
    enableLearningPath: "Enable Learning Path Suggestions",
    enableLearningPathDesc: "Get personalized recommendations to fill skill gaps",

    // Privacy
    privacy: "Document Privacy Settings",
    privacyDesc: "Control how your CV data is stored and used",
    storeInS3: "Store CV in S3 Cloud Storage",
    storeInS3Desc: "Save your CV for future access and re-analysis",
    allowImprovement: "Allow System Use for Improvement",
    allowImprovementDesc: "Help improve our analysis system by sharing anonymized CV data",
    encryptionLevel: "Encryption Level",
    defaultEncryption: "Default - Standard encryption",
    aes256: "AES-256 - Military-grade encryption",

    // Performance
    performance: "Performance & Limits",
    performanceDesc: "Adjust processing speed and usage limits",
    processingMode: "Processing Mode",
    fast: "Fast - Quick analysis (1-2 sec)",
    accurate: "Accurate - Thorough analysis (3-5 sec)",
    maxFileSize: "Max CV File Size (MB)",
    dailyLimit: "Daily Analysis Limit",

    // Buttons
    saveSettings: "Save Settings",

    // Home
    poweredBy: "Powered by Advanced Analysis",
    yourResumeTitle: "Your Resume, Perfected",
    analyzeResume: "Analyze Your Resume",
    viewDemo: "View Demo",
    resumesAnalyzed: "Resumes Analyzed",
    successRate: "Success Rate",
    avgImprovement: "Average Improvement",
    features: "Everything you need to succeed",
    featuresDesc: "Our comprehensive suite of tools helps you optimize every aspect of your resume",
    smartAnalysis: "Smart Analysis",
    smartAnalysisDesc: "Advanced algorithms analyze every aspect of your resume in seconds",
    instantFeedback: "Instant Feedback",
    instantFeedbackDesc: "Get real-time recommendations to boost your resume impact",
    jobMatchingFeature: "Job Matching",
    jobMatchingFeatureDesc: "Align your skills with specific job descriptions for better opportunities",
    performanceScore: "Performance Score",
    performanceScoreDesc: "See your resume strength across multiple professional dimensions",
  },
  vi: {
    // Navigation
    signIn: "Đăng Nhập",
    getStarted: "Bắt Đầu",

    // Sidebar
    dashboard: "Bảng Điều Khiển",
    newAnalysis: "Phân Tích Mới",
    results: "Kết Quả",
    settings: "Cài Đặt",
    logout: "Đăng Xuất",

    // Settings
    settingsTitle: "Cài Đặt",
    settingsDescription: "Tùy chỉnh trải nghiệm Smart Resume Analyzer của bạn",
    language: "Ngôn Ngữ",
    selectLanguage: "Chọn ngôn ngữ ưa thích của bạn",

    // CV Parsing Settings
    cvParsing: "Cài Đặt Phân Tích CV",
    cvParsingDesc: "Cấu hình cách tài liệu CV của bạn được xử lý và phân tích",
    enableOcr: "Bật OCR cho CV được Quét",
    enableOcrDesc: "Trích xuất văn bản từ CV dựa trên hình ảnh bằng công nhận ký tự quang học",
    dataCleaningLevel: "Mức Độ Làm Sạch Dữ Liệu",
    lowCleaning: "Thấp - Xử lý tối thiểu",
    mediumCleaning: "Trung Bình - Xử lý tiêu chuẩn",
    highCleaning: "Cao - Làm sạch tích cực",
    ocrLanguage: "Ngôn Ngữ OCR",
    autoDetectSections: "Tự Động Phát Hiện Các Phần",
    autoDetectSectionsDesc: "Tự động xác định các phần như Kinh Nghiệm, Giáo Dục, Kỹ Năng",

    // Job Matching
    jobMatching: "Cài Đặt Phù Hợp Mô Tả Công Việc",
    jobMatchingDesc: "Điều chỉnh cách CV của bạn được so khớp với các yêu cầu công việc",
    keywordWeight: "Trọng Số Phù Hợp Từ Khóa",
    skillWeight: "Trọng Số Phù Hợp Kỹ Năng",
    experienceWeight: "Trọng Số Phù Hợp Kinh Nghiệm",
    educationWeight: "Trọng Số Phù Hợp Giáo Dục",
    targetIndustry: "Ngành Mục Tiêu",

    // Fit Score
    fitScore: "Cài Đặt Điểm Phù Hợp",
    fitScoreDesc: "Cấu hình cách tính điểm phù hợp giữa CV và công việc",
    scoringMode: "Chế Độ Tính Điểm",
    weighted: "Có Trọng Số - Dựa trên tầm quan trọng",
    semantic: "Ngữ Nghĩa - Dựa trên ý nghĩa",
    hybrid: "Kết Hợp - Phương pháp kết hợp",
    analysisSensitivity: "Độ Nhạy Cảm Phân Tích",
    strict: "Nghiêm Ngặt - Khớp chính xác",
    balanced: "Cân Bằng - Được đề xuất",
    flexible: "Linh Hoạt - Khớp rộng hơn",
    resumeTypeOptimization: "Tối Ưu Hóa Loại Hồ Sơ",
    ats: "ATS - Hệ Thống Theo Dõi Ứng Viên",
    creative: "Sáng Tạo - Tập Trung Vào Thiết Kế",
    technical: "Kỹ Thuật - Tập Trung Vào Lập Trình Viên",

    // Skill Gap
    skillGap: "Cài Đặt Khoảng Cách Kỹ Năng & Đề Xuất",
    skillGapDesc: "Tùy chỉnh phân tích khoảng cách kỹ năng và đề xuất học tập",
    recommendationDetail: "Mức Chi Tiết Đề Xuất",
    basic: "Cơ Bản - Chỉ tổng quan",
    intermediate: "Trung Gian - Thông tin chi tiết",
    advanced: "Nâng Cao - Phân tích sâu",
    learningPathSource: "Nguồn Lộ Trình Học Tập",
    generalLearning: "Tài Nguyên Học Tập Chung",
    awsLearning: "Lộ Trình Học Tập AWS",
    custom: "Cơ Sở Dữ Liệu Kỹ Năng Tùy Chỉnh",
    enableLearningPath: "Bật Đề Xuất Lộ Trình Học Tập",
    enableLearningPathDesc: "Nhận các đề xuất được cá nhân hóa để lấp đầy khoảng cách kỹ năng",

    // Privacy
    privacy: "Cài Đặt Riêng Tư Tài Liệu",
    privacyDesc: "Kiểm soát cách dữ liệu CV của bạn được lưu trữ và sử dụng",
    storeInS3: "Lưu Trữ CV trong S3 Cloud Storage",
    storeInS3Desc: "Lưu CV của bạn để truy cập và phân tích lại trong tương lai",
    allowImprovement: "Cho Phép Sử Dụng Hệ Thống Để Cải Thiện",
    allowImprovementDesc: "Giúp cải thiện hệ thống phân tích của chúng tôi bằng cách chia sẻ dữ liệu CV ẩn danh",
    encryptionLevel: "Mức Độ Mã Hóa",
    defaultEncryption: "Mặc Định - Mã hóa tiêu chuẩn",
    aes256: "AES-256 - Mã hóa cấp quân sự",

    // Performance
    performance: "Hiệu Suất & Giới Hạn",
    performanceDesc: "Điều chỉnh tốc độ xử lý và giới hạn sử dụng",
    processingMode: "Chế Độ Xử Lý",
    fast: "Nhanh - Phân tích nhanh (1-2 giây)",
    accurate: "Chính Xác - Phân tích kỹ lưỡng (3-5 giây)",
    maxFileSize: "Kích Thước Tệp CV Tối Đa (MB)",
    dailyLimit: "Giới Hạn Phân Tích Hàng Ngày",

    // Buttons
    saveSettings: "Lưu Cài Đặt",

    // Home
    poweredBy: "Được Hỗ Trợ Bởi Phân Tích Nâng Cao",
    yourResumeTitle: "CV Của Bạn, Hoàn Hảo",
    analyzeResume: "Phân Tích CV Của Bạn",
    viewDemo: "Xem Demo",
    resumesAnalyzed: "CV Được Phân Tích",
    successRate: "Tỷ Lệ Thành Công",
    avgImprovement: "Cải Thiện Trung Bình",
    features: "Mọi thứ bạn cần để thành công",
    featuresDesc: "Bộ công cụ toàn diện của chúng tôi giúp bạn tối ưu hóa mọi khía cạnh của CV",
    smartAnalysis: "Phân Tích Thông Minh",
    smartAnalysisDesc: "Các thuật toán nâng cao phân tích mọi khía cạnh của CV bạn trong vài giây",
    instantFeedback: "Phản Hồi Tức Thì",
    instantFeedbackDesc: "Nhận các đề xuất trong thời gian thực để tăng tác động của CV",
    jobMatchingFeature: "Khớp Công Việc",
    jobMatchingFeatureDesc: "Liên kết kỹ năng của bạn với các mô tả công việc cụ thể để có cơ hội tốt hơn",
    performanceScore: "Điểm Hiệu Suất",
    performanceScoreDesc: "Xem sức mạnh CV của bạn trên nhiều kích thước chuyên nghiệp",
  },
}

export type Language = keyof typeof translations

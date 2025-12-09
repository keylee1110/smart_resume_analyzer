import { fetchAuthSession } from 'aws-amplify/auth';
import { get, post, put } from 'aws-amplify/api';

const API_NAME = 'ResumeAnalyzerApi';

// --- Cache Implementation ---
interface CacheItem<T> {
    data: T;
    timestamp: number;
}

const cache: Record<string, CacheItem<any>> = {};

// Cache durations in milliseconds
const CACHE_DURATION = {
    RESUMES: 5 * 60 * 1000, // 5 minutes
    CHAT_HISTORY: 1 * 60 * 1000, // 1 minute
};

function getFromCache<T>(key: string, duration: number): T | null {
    const item = cache[key];
    if (!item) return null;

    const now = Date.now();
    if (now - item.timestamp > duration) {
        delete cache[key];
        return null;
    }

    return item.data;
}

function setInCache<T>(key: string, data: T) {
    cache[key] = {
        data,
        timestamp: Date.now(),
    };
}

export function clearCache(keyPattern?: string) {
    if (!keyPattern) {
        // Clear all
        for (const key in cache) delete cache[key];
    } else {
        // Clear matching keys
        for (const key in cache) {
            if (key.includes(keyPattern)) {
                delete cache[key];
            }
        }
    }
}
// --- End Cache Implementation ---

export async function getUploadUrl(fileName: string, contentType: string) {
    try {
        const session = await fetchAuthSession();
        const token = session.tokens?.idToken?.toString();

        if (!token) throw new Error("No session found");

        const restOperation = get({
            apiName: API_NAME,
            path: '/upload-url',
            options: {
                headers: { Authorization: `Bearer ${token}` },
                queryParams: {
                    fileName,
                    contentType
                }
            }
        });

        const { body } = await restOperation.response;
        const data = await body.json() as { uploadUrl: string; key: string };
        return data;
    } catch (error) {
        console.error("Error getting upload URL:", error);
        throw error;
    }
}

export async function uploadFileToS3(uploadUrl: string, file: File) {
    await fetch(uploadUrl, {
        method: "PUT",
        headers: {
            "Content-Type": file.type
        },
        body: file
    });
}

export interface AnalysisResult {
    analysisId?: string;
    fitScore?: number;
    matchedSkills?: string[];
    missingSkills?: string[];
    recommendation?: string;
    improvementPlan?: { area: string; advice: string }[];
    jobTitle?: string;
    company?: string;
    jobDescription?: string;
    resumeEntities?: any;
    status?: "PENDING" | "COMPLETED" | "FAILED";
}

export async function analyzeFit(resumeId: string, jobDescription: string): Promise<AnalysisResult | null> {
    try {
        const session = await fetchAuthSession();
        const token = session.tokens?.idToken?.toString();
        const headers = token ? { Authorization: `Bearer ${token}` } : undefined;

        const restOperation = post({
            apiName: API_NAME,
            path: '/analyze',
            options: {
                headers,
                body: {
                    ResumeId: resumeId,
                    JobDescription: jobDescription
                }
            }
        });

        const { body } = await restOperation.response;
        const data = await body.json();

        // Invalidate resumes cache since a new analysis updates the profile
        clearCache('resumes');

        // Return the full response which includes { ResumeId, Message, Analysis }
        return data as any;
    } catch (error: any) {
        if (error?.message?.includes("still being processed") ||
            error?.response?.statusCode === 404) {
            console.log("Resume processing in progress...");
            return null;
        }
        console.error("Error analyzing fit:", error);
        return null;
    }
}

export interface FullProfileData {
    resumeId: string;
    name?: string;
    email?: string;
    phone?: string;
    skills: string[];
    createdAt: string;
    resumeText?: string;
    lastAnalysis?: {
        jobDescription?: string;
        fitScore?: number;
        matchedSkills?: string[];
        missingSkills?: string[];
        recommendation?: string;
        improvementPlan?: { area: string; advice: string }[];
        strengths?: string[];
        weaknesses?: string[];
        recommendations?: string[];
        analyzedAt?: string;
        jobTitle?: string;
        company?: string;
    };
}

export async function getAnalysis(id: string): Promise<AnalysisResult | null> {
    try {
        const session = await fetchAuthSession();
        const token = session.tokens?.idToken?.toString();

        // Allow public access for MVP if token is missing
        const headers = token ? { Authorization: `Bearer ${token}` } : undefined;

        const restOperation = get({
            apiName: API_NAME,
            path: `/resumes/${id}`,
            options: { headers }
        });

        const { body } = await restOperation.response;
        const profile = await body.json() as any;

        if (!profile) return null;

        // Map Profile + LastAnalysis to AnalysisResult
        // Backend returns camelCase (lastAnalysis)
        const analysis = profile.lastAnalysis || profile.LastAnalysis || {};

        // Safe number parsing helper to prevent NaN issues
        const parseNumber = (value: any, defaultValue: number = 0): number => {
            if (value === null || value === undefined) return defaultValue;
            const parsed = typeof value === 'string' ? parseFloat(value) : Number(value);
            return isNaN(parsed) ? defaultValue : parsed;
        };

        return {
            analysisId: profile.resumeId,
            fitScore: parseNumber(analysis.fitScore, 0),
            matchedSkills: analysis.matchedSkills || [],
            missingSkills: analysis.missingSkills || [],
            recommendation: analysis.recommendation || "",
            improvementPlan: analysis.improvementPlan || [],
            jobTitle: profile.jobTitle || analysis.jobTitle,
            company: profile.company || analysis.company,
            resumeEntities: {
                name: profile.name,
                email: profile.email,
                phone: profile.phone,
                skills: profile.skills
            },
            status: "COMPLETED"
        };
    } catch (error) {
        console.error("Error fetching analysis:", error);
        return null;
    }
}

export async function getFullProfile(id: string): Promise<FullProfileData | null> {
    try {
        const session = await fetchAuthSession();
        const token = session.tokens?.idToken?.toString();

        // Allow public access for MVP if token is missing
        const headers = token ? { Authorization: `Bearer ${token}` } : undefined;

        const restOperation = get({
            apiName: API_NAME,
            path: `/resumes/${id}`,
            options: { headers }
        });

        const { body } = await restOperation.response;
        const profile = await body.json() as any;

        if (!profile) return null;

        return {
            resumeId: profile.resumeId || id,
            name: profile.name,
            email: profile.email,
            phone: profile.phone,
            skills: profile.skills || [],
            createdAt: profile.createdAt || new Date().toISOString(),
            resumeText: profile.resumeText,
            lastAnalysis: profile.lastAnalysis || profile.LastAnalysis,
        };
    } catch (error) {
        console.error("Error fetching full profile:", error);
        return null;
    }
}

export interface ResumeProfile {
    resumeId: string;
    name?: string;
    email?: string;
    phone?: string;
    skills: string[];
    createdAt: string;
    lastAnalysis?: AnalysisResult;
    jobTitle?: string;
    company?: string;
}

export async function getResumes(): Promise<ResumeProfile[]> {
    // Check Cache First
    const cached = getFromCache<ResumeProfile[]>('resumes', CACHE_DURATION.RESUMES);
    if (cached) {
        console.log("Serving resumes from cache");
        return cached;
    }

    try {
        const session = await fetchAuthSession();
        const token = session.tokens?.idToken?.toString();

        if (!token) throw new Error("No session found");

        const restOperation = get({
            apiName: API_NAME,
            path: '/resumes',
            options: {
                headers: { Authorization: `Bearer ${token}` }
            }
        });

        const { body } = await restOperation.response;
        // The backend returns a JSON array of profiles
        const data = await body.json() as unknown as ResumeProfile[];

        // Update Cache
        setInCache('resumes', data);

        return data;
    } catch (error) {
        console.error("Error fetching resumes:", error);
        return [];
    }
}

export interface ChatMessage {
    role: "user" | "assistant";
    content: string;
    timestamp: string;
}

export interface ChatRequest {
    resumeId: string;
    userMessage: string;
    jobDescription?: string;
    chatHistory: ChatMessage[];
}

export interface ChatResponse {
    aiMessage: string;
    timestamp: string;
}

export async function sendChatMessage(request: ChatRequest): Promise<ChatResponse> {
    try {
        const session = await fetchAuthSession();
        const token = session.tokens?.idToken?.toString();

        if (!token) throw new Error("No session found");

        const restOperation = post({
            apiName: API_NAME,
            path: '/chat',
            options: {
                headers: { Authorization: `Bearer ${token}` },
                body: request as any
            }
        });

        const { body } = await restOperation.response;
        const data = await body.json() as unknown as ChatResponse;

        // Invalidate chat history cache for this resume
        clearCache(`chat_history_${request.resumeId}`);

        return data;
    } catch (error) {
        console.error("Error sending chat message:", error);
        throw error;
    }
}

export async function getChatHistory(resumeId: string): Promise<ChatMessage[]> {
    const cacheKey = `chat_history_${resumeId}`;
    const cached = getFromCache<ChatMessage[]>(cacheKey, CACHE_DURATION.CHAT_HISTORY);
    if (cached) {
        return cached;
    }

    try {
        const session = await fetchAuthSession();
        const token = session.tokens?.idToken?.toString();

        if (!token) throw new Error("No session found");

        const restOperation = get({
            apiName: API_NAME,
            path: `/chat/history/${resumeId}`,
            options: {
                headers: { Authorization: `Bearer ${token}` }
            }
        });

        const { body } = await restOperation.response;
        const data = await body.json();
        const response = data as unknown as { resumeId: string; messages: ChatMessage[] };
        const messages = response.messages || [];

        setInCache(cacheKey, messages);

        return messages;
    } catch (error) {
        console.error("Error fetching chat history:", error);
        return [];
    }
}

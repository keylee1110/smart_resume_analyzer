# Proposal – Smart Resume Analyzer
_A Unified AWS Serverless solution to analyze CVs vs JDs and generate Fit Scores_

> **Note:** This proposal follows the sectioning style of your previous `_index.md` sample but is rewritten for the Smart Resume Analyzer project.

---

## 1) Executive Summary
**Smart Resume Analyzer** is a serverless web platform that evaluates the match between a candidate’s **CV** and a **Job Description (JD)**. It calculates a **Fit Score**, detects **skill gaps**, and provides **personalized learning suggestions**.  
The solution is implemented by a 5‑member team in **4 weeks** on **AWS** using managed, pay‑as‑you‑go services to keep costs near zero for a demo workload. The UI is built with **Next.js** and hosted on **AWS Amplify**; the backend uses **API Gateway + Lambda** with **DynamoDB**, **S3**, **Comprehend**, **Textract**, and **Cognito**.

**Key outcomes**
- 90% faster CV screening for demo scenarios.
- Objective Fit Score with visual reports.
- Actionable learning roadmap per candidate.

---

## 2) Problem Statement
### 2.1 What’s the problem?
- Recruiters spend significant time manually reading CVs and comparing them to JDs.  
- Candidates lack insight into which skills they are missing and how to improve.  
- Existing tools are expensive or not tailored for Vietnamese/SEA use cases.

### 2.2 The solution
- Upload CV (PDF/DOCX) and JD → automatic text extraction and NLP.  
- Detect **skills, experience, education**; compute **Fit Score** vs JD.  
- Recommend **skill pathways** mapped from a small **SkillOntology** store.  
- Secure login with **Cognito**; results shown in a clean **Next.js** dashboard.

---

## 3) Solution Architecture (overview)

![Solution Architecture Diagram](https://i.ibb.co/ZR0VcspJ/Solution-Architecture.png)

Serverless, event‑driven architecture on AWS.

**Main components**
- **Frontend**: Next.js UI (Amplify Hosting) for upload & result dashboard.  
- **API Layer**: Amazon API Gateway → AWS Lambda functions.  
- **Processing**: 
  - `parseResume` → Textract (if scanned PDF) → normalized text.  
  - `nlpAnalyze` → Comprehend → entities/skills/phrases.  
  - `recommendSkills` → compares to JD + `SkillOntology` in DynamoDB.  
- **Data**: DynamoDB (results, ontology), S3 (temporary CV/JD).  
- **Identity**: Cognito (JWT access tokens).  
- **Ops**: IaC with AWS SAM, CI/CD via CodeBuild + CodePipeline, logging in CloudWatch.

**(A Mermaid architecture diagram is provided separately.)**

---

## 4) Technical Implementation
### 4.1 Tech stack
- **Backend**: .NET 8 (C# Minimal API on Lambda)  
- **Frontend**: Next.js + TailwindCSS (Amplify Hosting)  
- **AWS**: Lambda, API Gateway, DynamoDB, S3, Cognito, Comprehend, Textract  
- **IaC**: AWS SAM  
- **CI/CD**: CodeBuild + CodePipeline

### 4.2 End‑to‑end flow
1. User authenticates via **Cognito** and obtains JWT.
2. Frontend requests **presigned URL** to **S3** → uploads CV/JD.
3. API Gateway invokes **Lambda `parseResume`**:  
   - If PDF scan → **Textract** → extract text; otherwise direct parse.  
   - Clean & normalize → store interim artifacts on S3.
4. **Lambda `nlpAnalyze`** uses **Comprehend** to detect entities/skills → writes results to **DynamoDB**.
5. **Lambda `recommendSkills`** loads **SkillOntology** from DynamoDB → compares CV vs JD → computes **Fit Score** + gaps.
6. Frontend queries results via API → renders charts/tables.

### 4.3 Data model (DynamoDB – simplified)
- **Table `Profiles`** (PK: `userId`, SK: `profileId`) – store latest CV parse.  
- **Table `Analyses`** (PK: `analysisId`) – fit score, gaps, timestamps.  
- **Table `SkillOntology`** (PK: `skillId`, attributes: `name`, `tags`, `learningPath[]`).

### 4.4 API (high level)
- `POST /upload-url` → presign for CV/JD.  
- `POST /analyze` → triggers pipeline for a given S3 key pair.  
- `GET /analyses/{id}` → returns Fit Score & recommendations.  
- `GET /skills/{id}` → (optional) fetch a skill’s learning path.

---

## 5) Timeline & Milestones (4 weeks)
| Week | Milestone                    | Deliverables                                        |
| ---- | ---------------------------- | --------------------------------------------------- |
| 1    | Foundation                   | SAM template, DynamoDB tables, Cognito, base UI     |
| 2    | Parsing & NLP                | `parseResume`, `nlpAnalyze`, JD parsing, unit tests |
| 3    | Recommender & FE integration | `recommendSkills`, dashboard, charts                |
| 4    | Demo & hardening             | E2E tests, logging, cost tuning, slide deck         |

---

## 6) Budget Estimation (demo scale)
_Indicative, assuming < 500 requests/month_
- **Lambda**: ~$0.02  
- **API Gateway**: ~$0.01  
- **S3** (few GB, low requests): ~$0.10  
- **DynamoDB** (on‑demand, low R/W): ~$0.05  
- **Amplify Hosting**: ~$0.30  
- **Comprehend + Textract (small pages)**: ~$0.40  
- **Cognito**: $0.00  
**Total ≈ $0.9 / month (~$10 / year)**

---

## 7) Security, Risks & Mitigations
**Security**
- Private S3 buckets with **SSE‑KMS**; presigned uploads only.  
- **IAM least privilege**; API protected by **Cognito JWT**.  
- **PII masking** for logs; **CloudWatch** alarms.  
- Optional: set lifecycle rules to delete raw CV/JD after analysis.

**Risks & mitigations**
- _NLP accuracy_: Provide supported formats + fallback to keyword rules.  
- _Large/unclean CVs_: Validate size/format; sanitize before NLP.  
- _Cost spikes_: AWS Budget alarms; cap page counts per request.

---

## 8) Expected Outcomes
- Automated CV‑JD matching with transparent **Fit Score**.  
- Visual breakdown of **skills matched vs gaps** and **learning roadmap**.  
- Serverless, low‑ops stack that’s easy to demo, extend, and localize.

---

## 📄 Proposal Document (Google Docs)


👉 **Review Proposal here:**  
[GOOGLE DOC LINK](https://docs.google.com/document/d/1ALFieRvZWl1Azg3C8a7L8Z-iL6-chpzS/edit?usp=sharing&ouid=100398969873071071371&rtpof=true&sd=true)




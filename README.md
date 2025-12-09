# Smart Resume Analyzer

A unified AWS Serverless solution designed to analyze Curriculum Vitae (CV) against Job Descriptions (JD) to generate Fit Scores, identify skill gaps, and provide personalized learning recommendations.

## 🚀 Key Features

*   **Automated Resume Parsing:** Extracts text and structured data from PDF/DOCX resumes using Amazon Textract and custom parsing logic.
*   **Smart NLP Analysis:** Utilizes Amazon Comprehend and Large Language Models (via Amazon Bedrock) to understand context, skills, and experience.
*   **Fit Score Calculation:** Objectives scores candidates against job requirements.
*   **Skill Gap Analysis:** Visualizes missing skills and recommends learning paths.
*   **Interactive Chatbot:** AI-powered assistant to query resume details and analysis results.
*   **Modern Dashboard:** Responsive Next.js frontend with data visualization.

## 🏗 Architecture

The project follows a serverless, event-driven architecture on AWS:

*   **Frontend:** Next.js application hosted on AWS Amplify.
*   **API Layer:** Amazon API Gateway routing requests to AWS Lambda functions.
*   **Compute:** AWS Lambda (.NET 8) for business logic, parsing, and analysis.
*   **Data Storage:** Amazon DynamoDB for profiles and analysis results; Amazon S3 for temporary file storage.
*   **AI/ML:** Amazon Bedrock, Amazon Comprehend, Amazon Textract.
*   **Authentication:** Amazon Cognito.

## 📂 Project Structure

*   `frontend/`: Next.js web application code.
*   `backend/`: AWS SAM serverless application, including Lambda functions and infrastructure definitions.
*   `docs/`: Project documentation, architecture diagrams, and planning documents.

## 🛠 Getting Started

### Prerequisites

*   AWS CLI and SAM CLI installed.
*   Node.js (v18+) and npm/pnpm.
*   .NET 8 SDK.
*   Docker (optional, for local testing).

### Deployment

Please refer to the documentation in the `docs/` folder for detailed deployment steps.
*   [Deployment Guide](docs/archive/DEPLOYMENT_GUIDE.md) (Note: Check `docs/archive` for historical deployment notes)

## 📄 Documentation

Detailed documentation can be found in the `docs/` directory:
*   [Project Proposal](docs/Proposal_Smart_Resume_Analyzer.md)
*   [Architecture](docs/PROJECT_ARCHITECTURE.md)
*   [Chatbot Plan](docs/PLAN_CHATBOT.md)

## 📜 License

[MIT](LICENSE)

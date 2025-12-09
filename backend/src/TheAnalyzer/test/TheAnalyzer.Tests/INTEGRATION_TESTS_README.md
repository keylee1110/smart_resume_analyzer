# API Endpoint Integration Tests

## Overview

The `ApiEndpointIntegrationTests.cs` file contains integration tests that validate the API endpoints against a deployed AWS infrastructure. These tests make actual HTTP requests to the API Gateway endpoints.

## Prerequisites

1. **Deployed Infrastructure**: The SAM application must be deployed to AWS
2. **API Gateway URL**: You need the API Gateway endpoint URL
3. **Authentication Token** (optional): A valid Cognito JWT token for authenticated endpoints

## Running the Tests

### Step 1: Get Your API Gateway URL

After deploying with SAM, get the API Gateway URL from the CloudFormation outputs:

```powershell
# Get the API endpoint URL
aws cloudformation describe-stacks --stack-name smart-resume-analyzer --query "Stacks[0].Outputs[?OutputKey=='ResumeAnalyzerApiEndpoint'].OutputValue" --output text
```

Or check the SAM deployment output for the `ResumeAnalyzerApiEndpoint` value.

### Step 2: Get an Authentication Token (Optional)

For endpoints that require authentication (`/resumes`, `/upload-url`), you need a valid Cognito JWT token:

```powershell
# Sign in and get a token
aws cognito-idp initiate-auth `
  --auth-flow USER_PASSWORD_AUTH `
  --client-id YOUR_CLIENT_ID `
  --auth-parameters USERNAME=your-email@example.com,PASSWORD=YourPassword123
```

Extract the `IdToken` from the response.

### Step 3: Set Environment Variables

```powershell
# Windows PowerShell
$env:API_BASE_URL = "https://your-api-id.execute-api.us-east-1.amazonaws.com/Prod"
$env:AUTH_TOKEN = "your-jwt-token-here"

# Or in CMD
set API_BASE_URL=https://your-api-id.execute-api.us-east-1.amazonaws.com/Prod
set AUTH_TOKEN=your-jwt-token-here
```

### Step 4: Run the Tests

```powershell
# Run all integration tests
cd backend/src/TheAnalyzer/test/TheAnalyzer.Tests
dotnet test --filter "FullyQualifiedName~ApiEndpointIntegrationTests"

# Run a specific test
dotnet test --filter "FullyQualifiedName~ApiEndpointIntegrationTests.AnalyzeEndpoint_ValidInput_Returns200"
```

## Test Coverage

The integration tests validate:

### 1. `/analyze` POST Endpoint
- **Test**: `AnalyzeEndpoint_ValidInput_Returns200`
- **Validates**: 
  - Returns 200 status code with valid input
  - Includes CORS headers in response
  - Returns valid JSON with ResumeId

### 2. `/resumes/{id}` GET Endpoint
- **Test**: `GetResumeEndpoint_ValidId_Returns200`
- **Validates**:
  - Returns 200 status code with valid ID
  - Includes CORS headers in response
  - Returns complete profile data

### 3. `/resumes` GET Endpoint
- **Test**: `ListResumesEndpoint_ValidAuth_Returns200`
- **Validates**:
  - Returns 200 status code with valid authentication
  - Includes CORS headers in response
  - Returns array of resumes

### 4. `/upload-url` GET Endpoint
- **Test**: `GetUploadUrlEndpoint_ValidParams_Returns200`
- **Validates**:
  - Returns 200 status code with valid parameters
  - Includes CORS headers in response
  - Returns presigned upload URL

### 5. CORS Preflight Requests
- **Tests**: 
  - `CorsPreflightRequest_AnalyzeEndpoint_Returns200`
  - `CorsPreflightRequest_GetResumeEndpoint_Returns200`
  - `CorsPreflightRequest_ListResumesEndpoint_Returns200`
  - `CorsPreflightRequest_UploadUrlEndpoint_Returns200`
- **Validates**:
  - OPTIONS requests return 200 or 204
  - All required CORS headers are present

## Test Behavior

- **Automatic Skipping**: If `API_BASE_URL` is not set, tests will automatically skip (pass without running)
- **Authentication**: Tests requiring authentication will skip if `AUTH_TOKEN` is not set
- **Real Data**: Tests create real data in your DynamoDB tables
- **Cleanup**: Tests do not automatically clean up created data

## Troubleshooting

### Tests are Skipped

If all tests show as skipped, ensure the `API_BASE_URL` environment variable is set:

```powershell
echo $env:API_BASE_URL
```

### 401 Unauthorized Errors

If you get 401 errors on authenticated endpoints:
1. Verify your `AUTH_TOKEN` is valid and not expired
2. Ensure the token is from the correct Cognito User Pool
3. Check that the Cognito Authorizer is properly configured in API Gateway

### CORS Errors

If CORS tests fail:
1. Verify the API Gateway CORS configuration in `template.yaml`
2. Check that Lambda functions include CORS headers in responses
3. Ensure the `ApiResponseHelper` is being used in all Lambda handlers

### Connection Errors

If you get connection errors:
1. Verify the API Gateway URL is correct
2. Check that the API is deployed and accessible
3. Ensure your AWS credentials have permission to invoke the API

## Example Output

```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:     9, Skipped:     0, Total:     9, Duration: 3 s
```

## Notes

- These tests make real API calls and may incur AWS costs
- Tests create real data in DynamoDB that persists after test completion
- For CI/CD pipelines, consider using a dedicated test environment
- Authentication tokens expire after 1 hour by default

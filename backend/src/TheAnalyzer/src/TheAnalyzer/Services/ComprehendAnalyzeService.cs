using Amazon.Comprehend;
using Amazon.Comprehend.Model;
using Microsoft.Extensions.Logging;
using TheAnalyzer.Interfaces;
using TheAnalyzer.Models;

namespace TheAnalyzer.Services;

/// <summary>
/// Service for analyzing CV text using AWS Bedrock (GenAI) and Comprehend
/// </summary>
public class ComprehendAnalyzeService : IAnalyzeService
{
    private readonly IAmazonComprehend _comprehendClient;
    private readonly ILogger<ComprehendAnalyzeService> _logger;
    private readonly RegexEntityExtractor _regexFallback;
    private readonly Amazon.BedrockRuntime.IAmazonBedrockRuntime _bedrockRuntimeClient;
    private readonly string _bedrockModelId;
    
    public ComprehendAnalyzeService(
        IAmazonComprehend comprehendClient,
        ILogger<ComprehendAnalyzeService> logger,
        RegexEntityExtractor regexFallback,
        Amazon.BedrockRuntime.IAmazonBedrockRuntime bedrockRuntimeClient)
    {
        _comprehendClient = comprehendClient ?? throw new ArgumentNullException(nameof(comprehendClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _regexFallback = regexFallback ?? throw new ArgumentNullException(nameof(regexFallback));
        _bedrockRuntimeClient = bedrockRuntimeClient ?? throw new ArgumentNullException(nameof(bedrockRuntimeClient));
        _bedrockModelId = Environment.GetEnvironmentVariable("BEDROCK_MODEL_ID") ?? "anthropic.claude-3-sonnet-20240229-v1:0";
    }
    
    /// <summary>
    /// Validates that CV text is suitable for processing
    /// </summary>
    public bool ValidateInput(string cvText)
    {
        if (string.IsNullOrWhiteSpace(cvText))
        {
            return false;
        }
        return true;
    }
    
    /// <summary>
    /// Analyzes CV text against an optional job description
    /// </summary>
    public async Task<AnalysisResult> AnalyzeCvAsync(string cvText, string? jobDescription, CancellationToken cancellationToken = default)
    {
        // 1. Extract entities from CV
        var resumeEntities = await ExtractEntitiesAsync(cvText, cancellationToken);
        
        var result = new AnalysisResult
        {
            ResumeEntities = resumeEntities,
            JobDescription = jobDescription
        };
        
        // 2. If Job Description is provided, perform GenAI comparison
        if (!string.IsNullOrWhiteSpace(jobDescription))
        {
            _logger.LogInformation("Job Description provided, performing GenAI analysis");
            
            try 
            {
               var aiResult = await AnalyzeWithBedrockAsync(cvText, jobDescription, cancellationToken);
               
               // Merge AI results
               result.FitScore = aiResult.FitScore;
               result.Recommendation = aiResult.Recommendation; // Match Reasoning
               result.MissingSkills = aiResult.MissingSkills;
               result.ImprovementPlan = aiResult.ImprovementPlan;
               result.JobTitle = aiResult.JobTitle;
               result.Company = aiResult.Company;
               
               // Use AI-extracted matched skills for consistency
               result.MatchedSkills = aiResult.MatchedSkills;
               
               // If AI didn't return extracted skills (or empty), fallback to comprehension/fuzzy match logic could be added here,
               // but we want to rely on Bedrock as per plan.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GenAI Analysis failed, falling back to heuristic analysis");
                
                // Fallback to heuristic analysis
                var heuristicResult = CalculateHeuristicFitScore(result.ResumeEntities.Skills ?? new List<string>(), jobDescription);
                
                result.FitScore = heuristicResult.FitScore;
                result.MatchedSkills = heuristicResult.MatchedSkills;
                result.MissingSkills = heuristicResult.MissingSkills;
                result.Recommendation = heuristicResult.Recommendation;
                result.ImprovementPlan = heuristicResult.ImprovementPlan;
                result.JobTitle = heuristicResult.JobTitle;
                result.Company = heuristicResult.Company;
            }
        }
        else
        {
            result.Recommendation = "Provide a Job Description to get a Fit Score and gap analysis.";
        }
        
        return result;
    }

    /// <summary>
    /// Fallback heuristic analysis when AI is unavailable
    /// </summary>
    private AnalysisResult CalculateHeuristicFitScore(IEnumerable<string> resumeSkills, string jobDescription)
    {
        // 1. Extract potential skills from JD using our keyword list
        var skillKeywords = GetExpandedSkillKeywords();
        var jdSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var skill in skillKeywords)
        {
            if (jobDescription.Contains(skill, StringComparison.OrdinalIgnoreCase))
            {
                jdSkills.Add(skill);
            }
        }

        // 2. Compare Resume Skills vs JD Skills
        var resumeSkillsSet = new HashSet<string>(resumeSkills, StringComparer.OrdinalIgnoreCase);
        var matchedSkills = jdSkills.Intersect(resumeSkillsSet).ToList();
        var missingSkills = jdSkills.Except(resumeSkillsSet).ToList();

        // 3. Calculate Score
        double score = 0;
        if (jdSkills.Count > 0)
        {
            score = Math.Round((double)matchedSkills.Count / jdSkills.Count * 100, 1);
        }
        else
        {
            // If no skills found in JD, but we have a JD, maybe give a neutral score or 0?
            // Let's stick to 0 or maybe 10 if resume has skills (effort points)
            score = resumeSkillsSet.Count > 0 ? 10 : 0; 
        }

        return new AnalysisResult
        {
            FitScore = score,
            MatchedSkills = matchedSkills,
            MissingSkills = missingSkills,
            Recommendation = $"Basic keyword match analysis (AI unavailable). Found {matchedSkills.Count} matching skills out of {jdSkills.Count} detected in JD.",
            ImprovementPlan = new List<ImprovementItem> 
            { 
                new ImprovementItem 
                { 
                    Area = "System", 
                    Advice = "The advanced AI analysis feature is currently unavailable. This report relies on simple keyword matching. Please ensure your resume explicitly mentions the skills listed in the job description." 
                } 
            },
            JobTitle = "Target Role", // Placeholder
            Company = "Target Company" // Placeholder
        };
    }

    private class BedrockAnalysisResponse
    {
        public double fit_score { get; set; }
        public string match_reasoning { get; set; } = string.Empty;
        public List<string> matched_skills { get; set; } = new();
        public List<string> missing_skills { get; set; } = new();
        public List<ImprovementItem> improvement_plan { get; set; } = new();
        public string job_title { get; set; } = string.Empty;
        public string company { get; set; } = string.Empty;
    }

    /// <summary>
    /// Performs analysis using Bedrock (Claude) with strict JSON output
    /// </summary>
    private async Task<AnalysisResult> AnalyzeWithBedrockAsync(string cvText, string jobDescription, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Invoking Bedrock for structured analysis");
        
        var prompt = $@"
You are a Senior Career Coach and Technical Recruiter.
Analyze the following Candidate Resume against the Target Job Description.

Your goal is to provide a structured, data-driven analysis in strict JSON format.

=== CANDIDATE RESUME ===
{TruncateText(cvText, 5000)}

=== TARGET JOB DESCRIPTION ===
{TruncateText(jobDescription, 3000)}

=== INSTRUCTIONS ===
1. **Fit Score**: Calculate a fit score from 0-100 based on skills vs requirements.
2. **Missing Skills**: Identify specific missing technical skills.
3. **improvement_plan**: Provide concrete, actionable advice.
4. **Job Details**: Extract 'job_title' and 'company' from the JD.

Output ONLY valid JSON matching this schema:
{{
  ""fit_score"": number, 
  ""match_reasoning"": ""Concise summary (max 2 sentences)."",
  ""matched_skills"": [""skill1"", ""skill2""],
  ""missing_skills"": [""skill1"", ""skill2""],
  ""improvement_plan"": [
    {{ ""area"": ""Skill/Section"", ""advice"": ""Actionable advice"" }}
  ],
  ""job_title"": ""string or null"",
  ""company"": ""string or null""
}}
";

        var requestBody = new
        {
            anthropic_version = "bedrock-2023-05-31",
            max_tokens = 4000,
            system = "You are a JSON extraction engine. Output valid JSON only.",
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.1
        };

        var request = new Amazon.BedrockRuntime.Model.InvokeModelRequest
        {
            ModelId = _bedrockModelId,
            ContentType = "application/json",
            Accept = "application/json",
            Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(requestBody)))
        };

        var response = await _bedrockRuntimeClient.InvokeModelAsync(request, cancellationToken);
        
        using var responseReader = new StreamReader(response.Body);
        var responseBody = await responseReader.ReadToEndAsync();
        
        // Extract JSON from Cloud 3 response structure
        using var jsonDoc = System.Text.Json.JsonDocument.Parse(responseBody);
        var contentText = jsonDoc.RootElement.GetProperty("content")[0].GetProperty("text").GetString();
        
        // Clean markdown
        contentText = contentText.Replace("```json", "").Replace("```", "").Trim();
        
        _logger.LogInformation("Bedrock Raw Output: {Content}", contentText);

        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var bedrockData = System.Text.Json.JsonSerializer.Deserialize<BedrockAnalysisResponse>(contentText, options);
        
        return new AnalysisResult
        {
            FitScore = bedrockData.fit_score,
            Recommendation = bedrockData.match_reasoning,
            MatchedSkills = bedrockData.matched_skills,
            MissingSkills = bedrockData.missing_skills,
            ImprovementPlan = bedrockData.improvement_plan,
            JobTitle = bedrockData.job_title,
            Company = bedrockData.company
        };
    }
    
    private string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength) return text;
        return text.Substring(0, maxLength) + "...";
    }

    /// <summary>
    /// Extracts entities from CV text using AWS Comprehend or regex fallback
    /// </summary>
    public async Task<ExtractedEntities> ExtractEntitiesAsync(string cvText, CancellationToken cancellationToken = default)
    {
        try
        {
            // Primary: Use AWS Comprehend
            _logger.LogInformation("Attempting entity extraction using AWS Comprehend");
            return await ExtractUsingComprehendAsync(cvText, cancellationToken);
        }
        catch (Exception ex)
        {
            // Fallback: Use regex patterns
            _logger.LogWarning(ex, "Comprehend failed, using regex fallback");
            return _regexFallback.Extract(cvText);
        }
    }
    
    private async Task<ExtractedEntities> ExtractUsingComprehendAsync(string cvText, CancellationToken cancellationToken)
    {
        var request = new DetectEntitiesRequest
        {
            Text = cvText.Length > 5000 ? cvText.Substring(0, 5000) : cvText,
            LanguageCode = "en"
        };
        
        var response = await _comprehendClient.DetectEntitiesAsync(request, cancellationToken);
        
        var entities = new ExtractedEntities { Method = "Comprehend" };
        
        var personEntities = response.Entities
            .Where(e => e.Type == EntityType.PERSON)
            .OrderByDescending(e => e.Score)
            .ToList();
        
        if (personEntities.Any()) entities.Name = personEntities.First().Text;
        
        entities.Email = ExtractEmailFromText(cvText);
        entities.Phone = ExtractPhoneFromText(cvText);
        entities.Skills = ExtractSkillsFromComprehend(cvText, response.Entities);
        
        entities.TotalEntitiesFound = 
            (string.IsNullOrEmpty(entities.Name) ? 0 : 1) +
            (string.IsNullOrEmpty(entities.Email) ? 0 : 1) +
            (string.IsNullOrEmpty(entities.Phone) ? 0 : 1) +
            entities.Skills.Count;
        
        return entities;
    }
    
    private string? ExtractEmailFromText(string text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(text, 
            @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success ? match.Value : null;
    }
    
    private string? ExtractPhoneFromText(string text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(text, 
            @"(\+?\d{1,3}[-.\s]?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}");
        return match.Success ? match.Value : null;
    }
    
    private List<string> ExtractSkillsFromComprehend(string text, List<Entity> entities)
    {
        var skillKeywords = GetExpandedSkillKeywords();
        var foundSkills = new List<string>();
        
        foreach (var skill in skillKeywords)
        {
            if (text.Contains(skill, StringComparison.OrdinalIgnoreCase))
                foundSkills.Add(skill);
        }
        return foundSkills;
    }
    
     private string[] GetExpandedSkillKeywords()
    {
        return new[] 
        { 
            "C#", "C++", "Java", "Python", "JavaScript", "TypeScript", "Go", "Rust", "Ruby", "PHP",
            ".NET", "ASP.NET", "Node.js", "React", "Angular", "Vue", "Django", "Flask", "Spring",
            "AWS", "Azure", "GCP", "Docker", "Kubernetes", "Jenkins", "Git", "CI/CD",
            "SQL", "MySQL", "PostgreSQL", "MongoDB", "DynamoDB", "Redis", "Elasticsearch",
            "REST", "GraphQL", "gRPC", "Microservices", "API", "Agile", "Scrum",
            "Machine Learning", "AI", "Data Science", "TensorFlow", "PyTorch",
            "HTML", "CSS", "SASS", "Bootstrap", "Tailwind",
            "Linux", "Unix", "Windows", "MacOS", "Terraform", "Ansible", "CloudFormation", "Serverless", "Lambda",
            "SAP", "Salesforce", "Oracle EBS", "ERP", "CRM", "ABAP"
        };
    }
}

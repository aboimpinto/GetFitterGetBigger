using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GetFitterGetBigger.Scripts
{
    /// <summary>
    /// Script to update memory banks when API documentation changes
    /// </summary>
    public class UpdateMemoryBanks
    {
        private static readonly string RootPath = Directory.GetCurrentDirectory();
        private static readonly string ApiDocsPath = Path.Combine(RootPath, "api-docs");
        
        private static readonly Dictionary<string, string> ProjectPaths = new Dictionary<string, string>
        {
            { "admin", Path.Combine(RootPath, "GetFitterGetBigger.Admin") },
            { "client", Path.Combine(RootPath, "GetFitterGetBigger.Clients") },
            { "api", Path.Combine(RootPath, "GetFitterGetBigger.API") },
            { "shared", Path.Combine(RootPath, "Shared") }
        };

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting memory bank update...");
            
            // Get all API documentation files
            var apiDocFiles = Directory.GetFiles(ApiDocsPath, "*.md", SearchOption.TopDirectoryOnly)
                .Where(file => !Path.GetFileName(file).Equals("README.md", StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            Console.WriteLine($"Found {apiDocFiles.Count} API documentation files.");
            
            foreach (var apiDocFile in apiDocFiles)
            {
                await ProcessApiDocFile(apiDocFile);
            }
            
            Console.WriteLine("Memory bank update completed successfully.");
        }

        private static async Task ProcessApiDocFile(string apiDocFile)
        {
            var fileName = Path.GetFileName(apiDocFile);
            Console.WriteLine($"Processing {fileName}...");
            
            // Read the API documentation file
            var content = await File.ReadAllTextAsync(apiDocFile);
            
            // Extract metadata
            var metadata = ExtractMetadata(content);
            
            if (metadata == null || !metadata.ContainsKey("used_by"))
            {
                Console.WriteLine($"No valid metadata found in {fileName}. Skipping.");
                return;
            }
            
            var usedBy = metadata["used_by"] as List<object>;
            if (usedBy == null || usedBy.Count == 0)
            {
                Console.WriteLine($"No 'used_by' information found in {fileName}. Skipping.");
                return;
            }
            
            // Get endpoint information
            var endpointInfo = ExtractEndpointInfo(content);
            
            // Update memory banks for each project that uses this endpoint
            foreach (var project in usedBy.Select(p => p.ToString()))
            {
                if (!ProjectPaths.TryGetValue(project, out var projectPath))
                {
                    Console.WriteLine($"Unknown project: {project}. Skipping.");
                    continue;
                }
                
                await UpdateProjectMemoryBank(projectPath, fileName, endpointInfo);
            }
        }

        private static Dictionary<string, object> ExtractMetadata(string content)
        {
            try
            {
                var metadataMatch = Regex.Match(content, @"^---\s*\n(.*?)\n---", RegexOptions.Singleline);
                if (!metadataMatch.Success)
                {
                    return null;
                }
                
                var metadataYaml = metadataMatch.Groups[1].Value;
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();
                
                return deserializer.Deserialize<Dictionary<string, object>>(metadataYaml);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting metadata: {ex.Message}");
                return null;
            }
        }

        private static Dictionary<string, string> ExtractEndpointInfo(string content)
        {
            var endpointInfo = new Dictionary<string, string>();
            
            // Extract endpoint URL
            var urlMatch = Regex.Match(content, @"## Endpoint URL\s*\n\s*`([^`]+)`");
            if (urlMatch.Success)
            {
                endpointInfo["url"] = urlMatch.Groups[1].Value;
            }
            
            // Extract HTTP method
            var methodMatch = Regex.Match(content, @"## HTTP Method\s*\n\s*`([^`]+)`");
            if (methodMatch.Success)
            {
                endpointInfo["method"] = methodMatch.Groups[1].Value;
            }
            
            // Extract title
            var titleMatch = Regex.Match(content, @"# (.+)");
            if (titleMatch.Success)
            {
                endpointInfo["title"] = titleMatch.Groups[1].Value;
            }
            
            return endpointInfo;
        }

        private static async Task UpdateProjectMemoryBank(string projectPath, string apiDocFileName, Dictionary<string, string> endpointInfo)
        {
            var memoryBankPath = Path.Combine(projectPath, "memory-bank");
            
            if (!Directory.Exists(memoryBankPath))
            {
                Console.WriteLine($"Memory bank directory not found for project at {projectPath}. Skipping.");
                return;
            }
            
            // Update techContext.md
            await UpdateTechContextFile(memoryBankPath, apiDocFileName, endpointInfo);
            
            // Update activeContext.md
            await UpdateActiveContextFile(memoryBankPath, apiDocFileName, endpointInfo);
            
            Console.WriteLine($"Updated memory bank for project at {projectPath}.");
        }

        private static async Task UpdateTechContextFile(string memoryBankPath, string apiDocFileName, Dictionary<string, string> endpointInfo)
        {
            var techContextPath = Path.Combine(memoryBankPath, "techContext.md");
            
            if (!File.Exists(techContextPath))
            {
                // Create the file if it doesn't exist
                await File.WriteAllTextAsync(techContextPath, "# Technical Context\n\n## API Endpoints\n\n");
            }
            
            var content = await File.ReadAllTextAsync(techContextPath);
            
            // Check if the API section exists
            if (!content.Contains("## API Endpoints"))
            {
                content += "\n## API Endpoints\n\n";
            }
            
            // Check if this endpoint is already documented
            var endpointPattern = $@"### {Regex.Escape(endpointInfo.GetValueOrDefault("title", ""))}";
            var endpointMatch = Regex.Match(content, endpointPattern);
            
            var endpointDocumentation = $"### {endpointInfo.GetValueOrDefault("title", "")}\n\n" +
                                       $"- **URL**: `{endpointInfo.GetValueOrDefault("url", "")}`\n" +
                                       $"- **Method**: `{endpointInfo.GetValueOrDefault("method", "")}`\n" +
                                       $"- **Documentation**: [View API Documentation](../../api-docs/{apiDocFileName})\n\n";
            
            if (endpointMatch.Success)
            {
                // Update existing documentation
                var endpointSection = Regex.Match(content, $@"{endpointPattern}.*?(?=###|\z)", RegexOptions.Singleline);
                content = content.Replace(endpointSection.Value, endpointDocumentation);
            }
            else
            {
                // Add new documentation
                var apiEndpointsSection = Regex.Match(content, @"## API Endpoints\s*\n");
                var insertPosition = apiEndpointsSection.Index + apiEndpointsSection.Length;
                content = content.Insert(insertPosition, endpointDocumentation);
            }
            
            await File.WriteAllTextAsync(techContextPath, content);
        }

        private static async Task UpdateActiveContextFile(string memoryBankPath, string apiDocFileName, Dictionary<string, string> endpointInfo)
        {
            var activeContextPath = Path.Combine(memoryBankPath, "activeContext.md");
            
            if (!File.Exists(activeContextPath))
            {
                // Create the file if it doesn't exist
                await File.WriteAllTextAsync(activeContextPath, "# Active Context\n\n## Recent API Changes\n\n");
            }
            
            var content = await File.ReadAllTextAsync(activeContextPath);
            
            // Check if the Recent API Changes section exists
            if (!content.Contains("## Recent API Changes"))
            {
                content += "\n## Recent API Changes\n\n";
            }
            
            // Add the API change to the Recent API Changes section
            var apiChangesSection = Regex.Match(content, @"## Recent API Changes\s*\n");
            var insertPosition = apiChangesSection.Index + apiChangesSection.Length;
            
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var changeDocumentation = $"- **{timestamp}**: Updated documentation for `{endpointInfo.GetValueOrDefault("title", "")}` " +
                                     $"(`{endpointInfo.GetValueOrDefault("method", "")} {endpointInfo.GetValueOrDefault("url", "")}`). " +
                                     $"[View API Documentation](../../api-docs/{apiDocFileName})\n";
            
            content = content.Insert(insertPosition, changeDocumentation);
            
            await File.WriteAllTextAsync(activeContextPath, content);
        }
    }

    /// <summary>
    /// Class representing the metadata in an API documentation file
    /// </summary>
    public class ApiDocMetadata
    {
        public List<string> UsedBy { get; set; }
    }
}

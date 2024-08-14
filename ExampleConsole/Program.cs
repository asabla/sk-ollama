using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// Currently endpoint override is experimental and causes a warning
#pragma warning disable SKEXP0010
var builder = Kernel
    .CreateBuilder()
    .AddOpenAIChatCompletion(
        modelId: "llama3.1:8b",
        apiKey: "sk12345",  // Ollama doesn't use API-keys, this can be anything
        endpoint: new Uri("http://localhost:11434"));

Kernel kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Add a plugin (the LightsPlugin class is defined below)
kernel.Plugins.AddFromType<LightsPlugin>("Lights");

// Enable planning
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};

// Create a history store the conversation
var history = new ChatHistory();

string? userInput;
do
{
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine() ?? string.Empty;

    if (string.IsNullOrWhiteSpace(userInput))
    {
        Console.WriteLine("Error > Please enter a valid input.");
        continue;
    }

    history.AddUserMessage(userInput);

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Print the results
    Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);
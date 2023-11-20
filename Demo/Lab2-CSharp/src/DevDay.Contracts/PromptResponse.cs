namespace DevDay.Contracts;

public record PromptResponse(
    string Prompt,
    string Response,
    bool IsComplete = false);

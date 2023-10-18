using Spectre.Console;

namespace EvoSC.Tool.Utils;

public static class PromptUtils
{
    public static SelectionPrompt<T> NewSelectionPrompt<T>(string title, params T[] choices)
    {
        var prompt = new SelectionPrompt<T>()
            .Title(title)
            .AddChoices(choices);
        prompt.HighlightStyle = new Style(foreground: Color.HotPink);

        return prompt;
    }

    public static Task<T> ShowSelectionPromptAsync<T>(this IAnsiConsole console, string title, params T[] choices) =>
        NewSelectionPrompt<T>(title, choices).ShowAsync(console, CancellationToken.None);

    public static Task<string> ShowStringSelectionPromptAsync(this IAnsiConsole console, string title, params string[] choices)
    {
        var prompt = NewSelectionPrompt<string>(title, choices);
        prompt.AddChoice("Cancel");

        return prompt.ShowAsync(console, CancellationToken.None);
    }

    public static Task<T> ShowInputPromptAsync<T>(this IAnsiConsole console, string prompt, T? defaultValue, Func<T, bool>? validator) where T : class
    {
        var promptObj = new TextPrompt<T>(prompt)
        {
            PromptStyle = new Style(Color.HotPink),
        };

        if (validator != null)
        {
            promptObj.Validate(validator);
        }

        if (defaultValue != null)
        {
            promptObj.DefaultValue(defaultValue);
        }

        return promptObj.ShowAsync(console, CancellationToken.None);
    }

    public static Task<T> ShowInputPromptAsync<T>(this IAnsiConsole console, string prompt, Func<T, bool>? validator) where T : class =>
        ShowInputPromptAsync<T>(console, prompt, null, validator);

    public static Task<bool> ConfirmAsync(this IAnsiConsole console, string prompt) =>
        new ConfirmationPrompt(prompt).ShowAsync(console, CancellationToken.None);

}
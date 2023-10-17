namespace EvoSC.Tool.Interfaces.Utils;

public interface IToolCommand<TOptions>
where TOptions : class, IToolCommandOptions
{
    public Task<int> ExecuteAsync(TOptions options);
}
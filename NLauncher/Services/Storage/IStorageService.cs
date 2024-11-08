namespace NLauncher.Services.Storage;

public interface IStorageService
{
    public ValueTask WriteAll(string text);
    public ValueTask<string> ReadAll();
}

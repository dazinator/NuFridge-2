namespace NuFridge.Shared.Commands.Options
{
    public delegate void OptionAction<TKey, TValue>(TKey key, TValue value);
}

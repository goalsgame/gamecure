using System;
using System.Threading;
using Gamecure.GUI.Models;

namespace Gamecure.GUI.ViewModels;

public class NerdQuotesViewModel : ViewModelBase
{
    private readonly Random _random = new();
    private Quote _quote;
    public Quote Quote
    {
        get => _quote;
        set => SetProperty(ref _quote, value);
    }
    private readonly Quote[] _quotes;
    private readonly Timer _timer;

    public NerdQuotesViewModel()
    {
        _quotes = Quotes.ReadQuotes();
        if (_quotes.Length > 0)
        {
            _quote = _quotes[_random.NextInt64(0, _quotes.Length)];
        }
        else
        {
            _quote = new Quote("No quotes available...", "GOALS");
        }

        _timer = new Timer(ChangeQuote, null, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
    }

    private void ChangeQuote(object? _)
    {
        if (_quotes.Length == 0)
        {
            return;
        }
        Quote = _quotes[_random.NextInt64(0, _quotes.Length)];
    }
}
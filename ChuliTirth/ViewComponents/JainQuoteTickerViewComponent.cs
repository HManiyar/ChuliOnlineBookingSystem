using ChuliTirth.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChuliTirth.ViewComponents;

public class JainQuoteTickerViewComponent : ViewComponent
{
    private readonly IJainQuoteService _quoteService;
    public JainQuoteTickerViewComponent(IJainQuoteService quoteService) => _quoteService = quoteService;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var quotes = await _quoteService.GetActiveQuotesAsync();
        return View(quotes);
    }
}

using LibraryRecommendation.Mvc.Models;
using LibraryRecommendation.Mvc.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryRecommendation.Mvc.Controllers;

public class BooksController : Controller
{
    private readonly IBookApiService _bookApi;

    public BooksController(IBookApiService bookApi)
    {
        _bookApi = bookApi;
    }

    public async Task<IActionResult> Index(string? term, CancellationToken cancellationToken)
    {
        ViewData["Term"] = term;
        var books = string.IsNullOrWhiteSpace(term)
            ? await _bookApi.GetAllAsync(cancellationToken)
            : await _bookApi.SearchAsync(term, cancellationToken: cancellationToken);
        return View(books);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var book = await _bookApi.GetByIdAsync(id, cancellationToken);
        return book is null ? NotFound() : View(book);
    }

    public IActionResult Create() => View(new BookViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookViewModel book, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(book);
        }

        await _bookApi.CreateAsync(book, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var book = await _bookApi.GetByIdAsync(id, cancellationToken);
        return book is null ? NotFound() : View(book);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BookViewModel book, CancellationToken cancellationToken)
    {
        if (id != book.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(book);
        }

        var updated = await _bookApi.UpdateAsync(book, cancellationToken);
        return updated ? RedirectToAction(nameof(Index)) : NotFound();
    }

    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var book = await _bookApi.GetByIdAsync(id, cancellationToken);
        return book is null ? NotFound() : View(book);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        await _bookApi.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}

using LibraryRecommendation.Api.Models;
using LibraryRecommendation.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LibraryRecommendation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookRepository _repository;

    public BooksController(IBookRepository repository)
    {
        _repository = repository;
    }

    // GET: api/books
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetAll(CancellationToken cancellationToken)
    {
        var books = await _repository.GetAllAsync(cancellationToken);
        return Ok(books);
    }

    // GET: api/books/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Book>> GetById(int id, CancellationToken cancellationToken)
    {
        var book = await _repository.GetByIdAsync(id, cancellationToken);
        return book is null ? NotFound() : Ok(book);
    }

    // GET: api/books/search?term=&genre=&minRating=
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Book>>> Search(
        [FromQuery] string? term,
        [FromQuery] string? genre,
        [FromQuery] double? minRating,
        CancellationToken cancellationToken)
    {
        var books = await _repository.SearchAsync(term, genre, minRating, cancellationToken);
        return Ok(books);
    }

    // POST: api/books
    [HttpPost]
    public async Task<ActionResult<Book>> Create([FromBody] Book book, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var created = await _repository.AddAsync(book, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/books/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Book book, CancellationToken cancellationToken)
    {
        if (id != book.Id)
        {
            return BadRequest("Route id does not match the body id.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var updated = await _repository.UpdateAsync(book, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    // DELETE: api/books/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

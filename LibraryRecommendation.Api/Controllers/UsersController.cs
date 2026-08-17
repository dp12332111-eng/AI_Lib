using LibraryRecommendation.Api.Models;
using LibraryRecommendation.Api.Models.Dtos;
using LibraryRecommendation.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LibraryRecommendation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IBookRepository _bookRepository;

    public UsersController(IUserRepository userRepository, IBookRepository bookRepository)
    {
        _userRepository = userRepository;
        _bookRepository = bookRepository;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    // GET: api/users/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> GetById(int id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    // GET: api/users/5/profile
    [HttpGet("{id:int}/profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile(int id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        var ratings = await _userRepository.GetRatingsAsync(id, cancellationToken);
        var history = await _userRepository.GetReadingHistoryAsync(id, cancellationToken);
        var books = (await _bookRepository.GetAllAsync(cancellationToken)).ToDictionary(b => b.Id);

        var profile = new UserProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            FavouriteGenres = user.FavouriteGenreList.ToList(),
            FavouriteAuthors = user.FavouriteAuthorList.ToList(),
            RatedBooks = ratings
                .Where(r => books.ContainsKey(r.BookId))
                .OrderByDescending(r => r.Stars)
                .ThenBy(r => books[r.BookId].Title)
                .Select(r => new RatedBookDto
                {
                    Book = books[r.BookId],
                    Stars = r.Stars,
                    RatedDate = r.RatedDate
                })
                .ToList(),
            ReadingHistory = history
                .Where(h => books.ContainsKey(h.BookId))
                .OrderBy(h => h.Status)
                .ThenBy(h => books[h.BookId].Title)
                .Select(h => new ReadingHistoryItemDto
                {
                    Book = books[h.BookId],
                    Status = h.Status
                })
                .ToList()
        };

        return Ok(profile);
    }
}

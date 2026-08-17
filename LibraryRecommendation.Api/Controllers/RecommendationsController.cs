using LibraryRecommendation.Api.Models;
using LibraryRecommendation.Api.Repositories;
using LibraryRecommendation.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryRecommendation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;
    private readonly IUserRepository _userRepository;

    public RecommendationsController(
        IRecommendationService recommendationService,
        IUserRepository userRepository)
    {
        _recommendationService = recommendationService;
        _userRepository = userRepository;
    }

    // GET: api/recommendations/user/5?count=10
    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<IEnumerable<BookRecommendation>>> GetForUser(
        int userId,
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        var recommendations = await _recommendationService.GetRecommendationsForUserAsync(
            userId, count, cancellationToken);

        return Ok(recommendations);
    }
}

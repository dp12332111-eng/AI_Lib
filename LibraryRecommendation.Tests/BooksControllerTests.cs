using LibraryRecommendation.Api.Controllers;
using LibraryRecommendation.Api.Models;
using LibraryRecommendation.Api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LibraryRecommendation.Tests;

// All tests mock IBookRepository — no DbContext, no database access.
public class BooksControllerTests
{
    private readonly Mock<IBookRepository> _repository = new(MockBehavior.Strict);

    private static Book SampleBook(int id = 1) => new()
    {
        Id = id,
        Title = "Dune",
        Author = "Frank Herbert",
        Genre = "Science Fiction",
        Description = "Desert planet politics.",
        Rating = 4.5,
        PublishedYear = 1965
    };

    [Fact]
    public async Task GetAll_ReturnsOkWithBooks()
    {
        var books = new List<Book> { SampleBook(1), SampleBook(2) };
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var controller = new BooksController(_repository.Object);
        var result = await controller.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var returned = Assert.IsAssignableFrom<IEnumerable<Book>>(ok.Value);
        Assert.Equal(2, returned.Count());
        _repository.VerifyAll();
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenBookExists()
    {
        _repository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleBook());

        var controller = new BooksController(_repository.Object);
        var result = await controller.GetById(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var book = Assert.IsType<Book>(ok.Value);
        Assert.Equal("Dune", book.Title);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenBookMissing()
    {
        _repository.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var controller = new BooksController(_repository.Object);
        var result = await controller.GetById(99, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var book = SampleBook(0);
        _repository.Setup(r => r.AddAsync(book, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => { book.Id = 7; return book; });

        var controller = new BooksController(_repository.Object);
        var result = await controller.Create(book, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(BooksController.GetById), created.ActionName);
        Assert.Equal(7, created.RouteValues!["id"]);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenIdMismatch()
    {
        var controller = new BooksController(_repository.Object);
        var result = await controller.Update(2, SampleBook(1), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenUpdated()
    {
        var book = SampleBook(1);
        _repository.Setup(r => r.UpdateAsync(book, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = new BooksController(_repository.Object);
        var result = await controller.Update(1, book, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenRepositoryReportsMissing()
    {
        var book = SampleBook(1);
        _repository.Setup(r => r.UpdateAsync(book, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var controller = new BooksController(_repository.Object);
        var result = await controller.Update(1, book, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleted()
    {
        _repository.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = new BooksController(_repository.Object);
        var result = await controller.Delete(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        _repository.Setup(r => r.DeleteAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var controller = new BooksController(_repository.Object);
        var result = await controller.Delete(99, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Search_PassesFiltersToRepository()
    {
        _repository.Setup(r => r.SearchAsync("dune", "Science Fiction", 4.0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Book> { SampleBook() });

        var controller = new BooksController(_repository.Object);
        var result = await controller.Search("dune", "Science Fiction", 4.0, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Single(Assert.IsAssignableFrom<IEnumerable<Book>>(ok.Value));
        _repository.VerifyAll();
    }
}

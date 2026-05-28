using ECommerce.Model.Entities;
using ECommerce.Model.Repositories;
using ECommerce.Presenter.ViewModels;

namespace ECommerce.Presenter.Presenters;

public class ReviewPresenter
{
    private readonly IRepository<Review> _reviewRepo;

    public ReviewPresenter(IRepository<Review> reviewRepo)
    {
        _reviewRepo = reviewRepo;
    }

    public async Task<List<ReviewViewModel>> GetReviewsAsync(Guid productId)
    {
        var reviews = await _reviewRepo.FindAsync(r => r.ProductId == productId);
        return reviews.OrderByDescending(r => r.CreatedAt).Select(r => new ReviewViewModel
        {
            Id = r.Id,
            ProductId = r.ProductId,
            UserName = r.UserName,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    public async Task<double> GetAverageRatingAsync(Guid productId)
    {
        var reviews = await _reviewRepo.FindAsync(r => r.ProductId == productId);
        return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
    }

    public async Task<int> GetReviewCountAsync(Guid productId)
    {
        var reviews = await _reviewRepo.FindAsync(r => r.ProductId == productId);
        return reviews.Count;
    }

    public async Task AddReviewAsync(ReviewRequest request, string? userId, string userName)
    {
        var review = new Review
        {
            ProductId = request.ProductId,
            UserId = userId,
            UserName = userName,
            Rating = Math.Clamp(request.Rating, 1, 5),
            Comment = request.Comment
        };
        await _reviewRepo.AddAsync(review);
    }

    public async Task<List<ReviewViewModel>> GetRecentReviewsAsync(int count = 6)
    {
        var reviews = await _reviewRepo.GetAllAsync();
        return reviews.OrderByDescending(r => r.CreatedAt).Take(count).Select(r => new ReviewViewModel
        {
            Id = r.Id,
            ProductId = r.ProductId,
            UserName = r.UserName,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        }).ToList();
    }
}

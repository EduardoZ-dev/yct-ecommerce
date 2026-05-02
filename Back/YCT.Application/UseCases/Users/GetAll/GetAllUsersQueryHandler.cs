using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;
using YCT.Domain.Entities;
using YCT.Domain.Interfaces;

namespace YCT.Application.UseCases.Users.GetAll;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, ResponseBase<List<UserDto>>>
{
    private readonly IGenericRepository<User> _repository;

    public GetAllUsersQueryHandler(IGenericRepository<User> repository)
    {
        _repository = repository;
    }

    public async Task<ResponseBase<List<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _repository.GetAllAsync();
        var dtos = users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }).ToList();

        return ResponseBase<List<UserDto>>.Ok(dtos);
    }
}

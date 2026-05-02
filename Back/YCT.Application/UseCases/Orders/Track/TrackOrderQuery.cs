using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Orders.Track;

public record TrackOrderQuery(string Search) : IRequest<ResponseBase<List<TrackedOrderDto>>>;

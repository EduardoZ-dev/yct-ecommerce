using MediatR;
using YCT.Application.Common;
using YCT.Application.DTOs;

namespace YCT.Application.UseCases.Orders.Transition;

public enum OrderAction { Validate, Confirm, Ship, Deliver, Cancel }

public record TransitionOrderCommand(
    int Id,
    OrderAction Action,
    int? DistributorId = null,
    string? TrackingNumber = null,
    int? CustomerRating = null,
    string? FeedbackComment = null) : IRequest<ResponseBase<OrderDto>>;

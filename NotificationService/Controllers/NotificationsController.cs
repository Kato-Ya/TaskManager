using Microsoft.AspNetCore.Mvc;
using NotificationService.Interfaces;
using NotificationService.Dto;
using Microsoft.AspNetCore.Components;

namespace NotificationService.Controllers;

[Microsoft.AspNetCore.Mvc.Route("api/notification")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications(int userId)
    {
        var notifications = await _notificationService.GetNotificationAsync(userId);
        return Ok(notifications);
    }
}
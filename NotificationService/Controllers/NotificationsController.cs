using Microsoft.AspNetCore.Mvc;
using NotificationService.Interfaces;
using NotificationService.Dto;
using Common.Auth;
using Microsoft.AspNetCore.Authorization;

namespace NotificationService.Controllers;

[Microsoft.AspNetCore.Mvc.Route("api/notification")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IEmailSenderService _emailSenderService;

    public NotificationsController(INotificationService notificationService, IEmailSenderService emailSenderService)
    {
        _notificationService = notificationService;
        _emailSenderService = emailSenderService;
    }

    [HttpGet("{userId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications(int userId)
    {
        if (!User.CanAccessUser(userId))
        {
            return Forbid();
        }

        var notifications = await _notificationService.GetNotificationAsync(userId);
        return Ok(notifications);
    }

    [HttpPost("send-test-email")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> SendTestEmail([FromQuery] string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest("Email doesn't exist");
        }

        await _emailSenderService.SendEmailAsync(
            email,
            "Notification for users",
            "Testing letter for checking SMTP's work");

        return Ok("SMTP IS WORK");
    }

}

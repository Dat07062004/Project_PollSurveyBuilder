using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PollSurveyBuilder.Application.DTOs;
using PollSurveyBuilder.Application.IServices;

namespace PollSurveyBuilder.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PollsController : ControllerBase
{
    private readonly IPollService _pollService;
    private readonly IVoteService _voteService;
    private readonly IQnAService _qnaService;
    private readonly IAuthService _authService;

    public PollsController(
        IPollService pollService,
        IVoteService voteService,
        IQnAService qnaService,
        IAuthService authService)
    {
        _pollService = pollService;
        _voteService = voteService;
        _qnaService = qnaService;
        _authService = authService;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequest dto)
    {
        var userId = GetCurrentUserId();
        var result = await _pollService.CreatePollAsync(dto, userId);
        return CreatedAtAction(nameof(GetPollByCode), new { code = result.Code }, result);
    }

    [HttpGet("my-polls")]
    [Authorize]
    public async Task<IActionResult> GetMyPolls()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized(new { message = "Vui lòng đăng nhập để xem danh sách cuộc thăm dò." });

        var polls = await _authService.GetUserPollsAsync(userId.Value);
        return Ok(polls);
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetPollByCode(string code)
    {
        var poll = await _pollService.GetPollByCodeAsync(code);
        if (poll == null) return NotFound(new { message = "Không tìm thấy cuộc thăm dò." });
        return Ok(poll);
    }

    [HttpPost("{code}/vote")]
    public async Task<IActionResult> CastVote(string code, [FromBody] SubmitVoteRequest dto)
    {
        var userId = GetCurrentUserId();
        try
        {
            var results = await _voteService.CastVoteAsync(code, dto, userId);
            return Ok(results);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{code}/results")]
    public async Task<IActionResult> GetPollResults(string code)
    {
        try
        {
            var results = await _voteService.GetPollResultsAsync(code);
            return Ok(results);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{code}/analytics")]
    public async Task<IActionResult> GetPollAnalytics(string code)
    {
        try
        {
            var analytics = await _voteService.GetPollAnalyticsAsync(code);
            return Ok(analytics);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{code}/qna")]
    public async Task<IActionResult> SubmitQuestion(string code, [FromBody] CreateQnARequest dto)
    {
        var userId = GetCurrentUserId();
        try
        {
            var question = await _qnaService.SubmitQuestionAsync(code, dto, userId);
            return Ok(question);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{code}/qna")]
    public async Task<IActionResult> GetQuestions(string code)
    {
        try
        {
            var questions = await _qnaService.GetQuestionsAsync(code);
            return Ok(questions);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("qna/{questionId:guid}/upvote")]
    public async Task<IActionResult> UpvoteQuestion(Guid questionId)
    {
        var success = await _qnaService.UpvoteQuestionAsync(questionId);
        if (!success) return NotFound();
        return Ok(new { success = true });
    }

    [HttpPost("qna/{questionId:guid}/pin")]
    [Authorize]
    public async Task<IActionResult> TogglePin(Guid questionId)
    {
        var success = await _qnaService.TogglePinAsync(questionId);
        if (!success) return NotFound();
        return Ok(new { success = true });
    }

    [HttpPost("{code}/close")]
    [Authorize]
    public async Task<IActionResult> ClosePoll(string code)
    {
        var userId = GetCurrentUserId();
        try
        {
            var success = await _pollService.ClosePollAsync(code, userId);
            if (!success) return NotFound();
            return Ok(new { success = true, message = "Đã đóng cuộc thăm dò thành công." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(claim, out var userId)) return userId;
        return null;
    }
}

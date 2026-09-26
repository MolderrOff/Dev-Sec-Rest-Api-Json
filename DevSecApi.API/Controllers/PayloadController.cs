using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DevSecApi.Application;
using DevSecApi.Application.Services;
using DevSecApi.Domain.Entities;

namespace DevSecApi.API.Controllers;

[ApiController]
[Route("api")]
public class PayloadController : ControllerBase
{
    private readonly PayloadService _payloadService;
    public PayloadController(PayloadService payloadService)
    {
        _payloadService = payloadService;
    }


    [HttpPost("process")]

    public async Task<IActionResult> ProcessTask([FromBody] Payload request)
    {
        if (request == null)
        {
            return BadRequest(new ProcessTaskResponse
            {
                IsError = 1,
                ErrorCode = "INVALID_REQUEST",
                ErrorMessage = "Тело запроса не может быть пустым."
            });
        }
        ProcessTaskResponse response = await _payloadService.ProcessPayloadAsync(request);

        return Ok(response);
    }
}

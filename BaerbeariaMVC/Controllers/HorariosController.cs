using BaerbeariaMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace BaerbeariaMVC.Controllers
{
    [ApiController]
    [Route("api/horarios")]
    public class HorariosController : ControllerBase
    {
        private readonly AgendamentoService _service;

        public HorariosController(AgendamentoService service)
        {
            _service = service;
        }

        
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] int barbeiroId, [FromQuery] DateTime data)
        {
            var horarios = await _service.ObterHorariosDisponiveis(barbeiroId, data);
            return Ok(horarios);
        }
    }
}
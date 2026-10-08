using API_SISTEMA.Services;
using Microsoft.AspNetCore.Mvc;

namespace API_SISTEMA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagosController : ControllerBase
    {
            private readonly PagoService _service;

            public PagosController(PagoService service)
            {
                _service = service;
            }

           


    }
}
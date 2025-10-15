using Data.SDK;
using Data.SDK.Repository;
using Microsoft.AspNetCore.Mvc;
using ProcessImage.Domain;

namespace ProcessImage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptiiController : ControllerBase
    {
        // *** NOU: Înlocuim ApplicationDbContext cu IRepository<Subscription> ***
        private readonly IRepository<Subscription> _subscriptieRepository;

        // Injectăm IRepository în constructor
        public SubscriptiiController(IRepository<Subscription> subscriptieRepository)
        {
            _subscriptieRepository = subscriptieRepository;
        }

        // Extrage toate abonamentele
        [HttpGet("toate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Subscription>>> GetSubscriptii()
        {
            try
            {
                // Folosim GetAllAsync() din Repositor-ul nostru
                var subscriptii = await _subscriptieRepository.GetAllAsync();

                return Ok(subscriptii);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Eroare la extragerea datelor: {ex.Message}");
            }
        }

        // Extrage un abonament după ID
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Subscription>> GetSubscriptie(int id)
        {
            // Folosim GetByIdAsync() din Repositor-ul nostru
            var subscriptie = await _subscriptieRepository.GetByIdAsync(id);

            if (subscriptie == null)
            {
                return NotFound($"Abonamentul cu ID-ul {id} nu a fost găsit.");
            }
            return subscriptie;
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using SchedulIX.Models.DTOs;
using SchedulIX.Models.Entities;
using SchedulIX.Repositories.Interfaces;

namespace SchedulIX.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomRepository _roomRepository;

        public RoomsController(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        /// <summary>
        /// Obține lista tuturor sălilor
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<RoomDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var rooms = await _roomRepository.GetAllAsync(ct);
            var result = rooms.Select(r => new RoomDto(
                r.Id,
                r.RoomNumber,
                r.Capacity,
                new RoomTypeDto(r.RoomType.Id, r.RoomType.Name, r.RoomType.HasComputers),
                r.IsAvailable
            )).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Obține detaliile unei săli după ID
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(RoomDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var room = await _roomRepository.GetByIdAsync(id, ct);
            if (room is null)
                return NotFound(new { Message = $"Sala cu ID-ul {id} nu a fost găsită." });

            var result = new RoomDto(
                room.Id,
                room.RoomNumber,
                room.Capacity,
                new RoomTypeDto(room.RoomType.Id, room.RoomType.Name, room.RoomType.HasComputers),
                room.IsAvailable
            );

            return Ok(result);
        }

        /// <summary>
        /// Obține calendarul de ocupare al unei săli
        /// </summary>
        [HttpGet("{id:int}/calendar")]
        [ProducesResponseType(typeof(RoomCalendarDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoomCalendar(int id, CancellationToken ct)
        {
            var room = await _roomRepository.GetByIdAsync(id, ct);
            if (room is null)
                return NotFound(new { Message = $"Sala cu ID-ul {id} nu a fost găsită." });

            var calendarItems = new List<RoomScheduleItemDto>();
            // Aici se vor adăuga elementele din programul generat

            var result = new RoomCalendarDto(room.Id, room.RoomNumber, calendarItems);
            return Ok(result);
        }

        /// <summary>
        /// Creează o nouă sală
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(RoomDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateRoomRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.RoomNumber) || request.Capacity <= 0)
                return BadRequest(new { Message = "Datele sălii sunt invalide." });

            var room = new Room
            {
                RoomNumber = request.RoomNumber,
                Capacity = request.Capacity,
                RoomTypeId = request.RoomTypeId,
                IsAvailable = true
            };

            if (request.Availabilities?.Any() == true)
            {
                room.Availabilities = request.Availabilities.Select(a => new RoomAvailability
                {
                    DayOfWeek = a.DayOfWeek,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime
                }).ToList();
            }

            var createdRoom = await _roomRepository.CreateAsync(room, ct);
            var result = new RoomDto(
                createdRoom.Id,
                createdRoom.RoomNumber,
                createdRoom.Capacity,
                new RoomTypeDto(createdRoom.RoomType.Id, createdRoom.RoomType.Name, createdRoom.RoomType.HasComputers),
                createdRoom.IsAvailable
            );

            return CreatedAtAction(nameof(GetById), new { id = createdRoom.Id }, result);
        }

        /// <summary>
        /// Actualizează starea unei săli
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(RoomDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateRoomRequest request, CancellationToken ct)
        {
            var room = await _roomRepository.GetByIdAsync(id, ct);
            if (room is null)
                return NotFound(new { Message = $"Sala cu ID-ul {id} nu a fost găsită." });

            room.RoomNumber = request.RoomNumber;
            room.Capacity = request.Capacity;
            room.RoomTypeId = request.RoomTypeId;

            var updatedRoom = await _roomRepository.UpdateAsync(room, ct);
            var result = new RoomDto(
                updatedRoom.Id,
                updatedRoom.RoomNumber,
                updatedRoom.Capacity,
                new RoomTypeDto(updatedRoom.RoomType.Id, updatedRoom.RoomType.Name, updatedRoom.RoomType.HasComputers),
                updatedRoom.IsAvailable
            );

            return Ok(result);
        }

        /// <summary>
        /// Marchează o sală ca indisponibilă (reparație)
        /// </summary>
        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateRoomStatusRequest request, CancellationToken ct)
        {
            var room = await _roomRepository.GetByIdAsync(id, ct);
            if (room is null)
                return NotFound(new { Message = $"Sala cu ID-ul {id} nu a fost găsită." });

            room.IsAvailable = request.IsAvailable;
            await _roomRepository.UpdateAsync(room, ct);

            return NoContent();
        }
    }

    public record UpdateRoomStatusRequest(bool IsAvailable);
}

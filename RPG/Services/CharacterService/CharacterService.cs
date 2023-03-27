using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RPG.Data;
using RPG.Dtos.Character;
using RPG.Models;
using System.Security.Claims;

namespace RPG.Services.CharacterService
{
    public class CharacterService : ICharacterService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CharacterService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User
            .FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            Character character = _mapper.Map<Character>(newCharacter);
            character.User = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());

            _context.Characters.Add(character);
            await _context.SaveChangesAsync();

            serviceResponse.Data = await _context.Characters
                .Where(c => c.User.Id == GetUserId())
                .Select(c => _mapper.Map<GetCharacterDto>(c))
                .ToListAsync();

            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var dbCharacters = await _context.Characters
                .Where(c => c.User.Id == GetUserId()).ToListAsync();
            serviceResponse.Data = dbCharacters.Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();

            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            var dbCharacters = await _context.Characters
                .Include(c => c.Weapon)
                .Include(c => c.Skills)
                .FirstOrDefaultAsync(c => c.Id == id && c.User.Id == GetUserId());
            serviceResponse.Data = _mapper.Map<GetCharacterDto>(dbCharacters);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updatedCharacter)
        {
            ServiceResponse<GetCharacterDto> serviceResponse = new ServiceResponse<GetCharacterDto>();
            try
            {
                Character character = await _context.Characters
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == updatedCharacter.Id);

                if (character.User.Id == GetUserId())
                {
                    _mapper.Map(updatedCharacter, character);
                    //character.Name = updatedCharacter.Name;
                    //character.Class = updatedCharacter.Class;
                    //character.Defense = updatedCharacter.Defense;
                    //character.HitPoints = updatedCharacter.HitPoints;
                    //character.Intelligence = updatedCharacter.Intelligence;
                    //character.Strength = updatedCharacter.Strength;

                    await _context.SaveChangesAsync();

                    serviceResponse.Data = _mapper.Map<GetCharacterDto>(character);

                }
                else
                {
                    serviceResponse.Succes = false;
                    serviceResponse.Message = "Character not found !";
                }


            }
            catch (Exception ex)
            {
                serviceResponse.Succes = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacter(int id)
        {
            ServiceResponse<List<GetCharacterDto>> serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            try
            {
                Character character = await _context.Characters
                    .FirstOrDefaultAsync(c => c.Id == id && c.User.Id == GetUserId());
                if (character != null)
                {
                    _context.Characters.Remove(character);
                    await _context.SaveChangesAsync();

                    serviceResponse.Data = _context.Characters
                        .Where(c => c.User.Id == GetUserId())
                        .Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();
                }
                else
                {
                    serviceResponse.Succes = false;
                    serviceResponse.Message = "Character not found !";
                }

            }
            catch (Exception ex)
            {
                serviceResponse.Succes = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> AddCharacterSkill(AddCharacterSkillDto newCharacterSkill)
        {
            var response = new ServiceResponse<GetCharacterDto>();

            try
            {
                var character = await _context.Characters
                    .Include(c => c.Weapon)
                    .Include(c => c.Skills)
                    .FirstOrDefaultAsync(c => c.Id == newCharacterSkill.CharacterId &&
                    c.User.Id == GetUserId());

                if (character == null)
                {
                    response.Succes = false;
                    response.Message = "Chatacter not found !";
                    return response;
                }

                var skill = await _context.Skills
                    .FirstOrDefaultAsync(s => s.Id == newCharacterSkill.SkillId);
                
                if (skill == null)
                {
                    response.Succes = false;
                    response.Message = "Skill not found !";
                    return response;
                }

                character.Skills.Add(skill);
                await _context.SaveChangesAsync();
                response.Data = _mapper.Map<GetCharacterDto>(character);
            }
            catch (Exception ex)
            {
                response.Succes = false;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}

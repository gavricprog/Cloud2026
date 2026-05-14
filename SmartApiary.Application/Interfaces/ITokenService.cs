using SmartApiary.Domain.Entities;

namespace SmartApiary.Application.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(User user);
}

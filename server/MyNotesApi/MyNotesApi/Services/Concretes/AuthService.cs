﻿using IdentityModel;
using MyNotesApi.Contexts;
using MyNotesApi.DTOs;
using MyNotesApi.Entities;
using MyNotesApi.Helpers.Exceptions;
using MyNotesApi.Helpers;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using MyNotesApi.Services.Abstracts;

namespace MyNotesApi.Services.Concretes
{
    public class AuthService:IAuthService
    {
        private readonly PostgreSqlDbContext _postgreSqlDbContext;
        private readonly JwtSettings _jwtSettings;
        public AuthService(PostgreSqlDbContext postgreSqlDbContext, IOptions<JwtSettings> jwtSettings)
        {
            _postgreSqlDbContext= postgreSqlDbContext;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<LoginResponseDTO> Login(LoginRequestDTO requestBody)
        {
            LoginResponseDTO ressult = new LoginResponseDTO() { };

            User? currentUser = await _postgreSqlDbContext.Users.FirstOrDefaultAsync(user => user.Email == requestBody.Email);
            if (currentUser == null) throw new ExceptionInfo($"Couldnt found account with \"{requestBody.Email}\" email", 400);
            if (currentUser.Password != requestBody.Password) throw new ExceptionInfo("Please check your credentials", 400);

            var token = "";
            var refreshToken = "";
            var claims = new List<Claim>
            {
                new(JwtClaimTypes.Id, currentUser.Id.ToString()),
                new(JwtClaimTypes.Email, currentUser.Email)
            };

            //claims.Add(new Claim("Claim1", "Value1"));
            //claims.Add(new Claim("Claim2", "Value2"));

            string newAccessToken = GetJwtToken(_jwtSettings, TimeSpan.FromMinutes(1), claims);
            string newRefreshToken = GetJwtToken(_jwtSettings, TimeSpan.FromMinutes(2), claims);

            ressult.Message = $"Welcome {requestBody.Email}";
            ressult.AccessToken = newAccessToken;
            ressult.RefreshToken = newRefreshToken;
            return ressult;
        }
        private string GetJwtToken(JwtSettings jwtSettings, TimeSpan expiration, IEnumerable<Claim> claims = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.IssuerSigningKey));
            var token = new JwtSecurityToken(
                issuer: jwtSettings.ValidIssuer,
                audience: jwtSettings.ValidAudience,
                expires: DateTime.UtcNow.Add(expiration),
                claims: claims,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return tokenHandler.WriteToken(token);
        }
    }
}
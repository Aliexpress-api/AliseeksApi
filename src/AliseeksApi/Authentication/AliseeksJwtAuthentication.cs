using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using AliseeksApi.Configuration;
using AliseeksApi.Utility.Extensions;
using SharpRaven.Core;
using SharpRaven.Core.Data;

namespace AliseeksApi.Authentication
{
    public class AliseeksJwtAuthentication : IJwtFactory
    {
        public static TokenValidationParameters TokenValidationParameters(string securityKey)
        {
            return new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(securityKey)),

                ValidateIssuer = true,
                ValidIssuer = issuer,

                ValidateAudience = true,
                ValidAudience = audience,

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };
        }

        const string issuer = "AliseeksIssuer";
        const string audience = "AliseeksUser";

        private readonly JwtOptions jwtOptions;
        private readonly IRavenClient raven;

        public AliseeksJwtAuthentication(IOptions<JwtOptions> jwtOptions, IRavenClient raven)
        {
            this.jwtOptions = jwtOptions.Value;
            this.raven = raven;
        }

        public string GenerateToken(Claim[] claims)
        {
            var securityKey = System.Text.Encoding.ASCII.GetBytes(jwtOptions.SecretKey);

            var handler = new JwtSecurityTokenHandler();
            var now = DateTime.UtcNow;

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = "AliseeksIssuer",
                Audience = "AliseeksUser",
                Expires = DateTime.Now.AddDays(14),
                NotBefore = now,
                IssuedAt = now,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(securityKey),
                    SecurityAlgorithms.HmacSha256)
            };

            try
            {
                var token = handler.CreateToken(tokenDescriptor);
                var tokenString = handler.WriteToken(token);
                return tokenString;
            }
            catch (Exception e)
            {
                var sentry = new SentryEvent(e);
                raven.CaptureNetCoreEventAsync(sentry).Wait(); //blocking i/o

                throw e;
            }
        }
    }
}

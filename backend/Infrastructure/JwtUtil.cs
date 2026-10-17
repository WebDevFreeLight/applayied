using Infrastructure.Extensions;
using Infrastructure.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Infrastructure
{
    /// <summary>
    /// 2023-8-29??WebApi???
    /// </summary>
    public class JwtUtil
    {
        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static TokenModel GetLoginUser(HttpContext httpContext)
        {
            string token = httpContext.GetToken();

            if (string.IsNullOrEmpty(token)) return null;

            var tokenModel = ValidateJwtToken(ParseToken(token));
            return tokenModel;
        }

        /// <summary>
        /// ??token
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        public static string GenerateJwtToken(List<Claim> claims)
        {
            JwtSettings jwtSettings = new();
            AppSettings.Bind("JwtSettings", jwtSettings);

            var authTime = DateTime.Now;
            var expiresAt = authTime.AddMinutes(jwtSettings.Expire);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);
            claims.Add(new Claim("Audience", jwtSettings.Audience));
            claims.Add(new Claim("Issuer", jwtSettings.Issuer));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = jwtSettings.Issuer,
                Audience = jwtSettings.Audience,
                IssuedAt = authTime,//token????
                Expires = expiresAt,
                //NotBefore = authTime,
                TokenType = jwtSettings.TokenType,
                //????,????
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        /// <summary>
        /// ??Token
        /// </summary>
        /// <returns></returns>
        public static TokenValidationParameters ValidParameters()
        {
            JwtSettings jwtSettings = new();
            AppSettings.Bind("JwtSettings", jwtSettings);

            if (jwtSettings == null || jwtSettings.SecretKey.IsEmpty())
            {
                throw new Exception("JwtSettings????");
            }
            var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

            var tokenDescriptor = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true,//????Token???,???????Token?Claims??NotBefore?Expires??
                ClockSkew = TimeSpan.FromSeconds(30)
                //RequireExpirationTime = true,//????
            };
            return tokenDescriptor;
        }
        /// <summary>
        /// ??????????
        /// </summary>
        /// <param name="token">??</param>
        /// <returns></returns>
        public static JwtSecurityToken? ParseToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validateParameter = ValidParameters();
            token = token.Replace("Bearer ", "");
            try
            {
                tokenHandler.ValidateToken(token, validateParameter, out SecurityToken validatedToken);

                return tokenHandler.ReadJwtToken(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // return null if validation fails
                return null;
            }
        }

        /// <summary>
        /// jwt token??
        /// </summary>
        /// <param name="jwtSecurityToken"></param>
        /// <returns></returns>
        public static TokenModel? ValidateJwtToken(JwtSecurityToken jwtSecurityToken)
        {
            try
            {
                if (jwtSecurityToken == null) return null;
                IEnumerable<Claim> claims = jwtSecurityToken?.Claims;
                TokenModel loginUser = null;

                var userData = claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;
                if (userData != null)
                {
                    loginUser = JsonConvert.DeserializeObject<TokenModel>(userData);
                    loginUser.ExpireTime = jwtSecurityToken.ValidTo;
                }
                return loginUser;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        ///??Claims
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static List<Claim> AddClaims(TokenModel user)
        {
            var claims = new List<Claim>()
                {
                    new(ClaimTypes.PrimarySid, user.UserId.ToString()),
                    new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new(ClaimTypes.Name, user.UserName),
                    new(ClaimTypes.GroupSid, user.DeptId.ToString()),
                    new(ClaimTypes.UserData, JsonConvert.SerializeObject(user)),
                };
            if (user?.TenantId != null)
            {
                //??ID
                claims.Add(new(ClaimTypes.PrimaryGroupSid, user.TenantId));
            }
            // ???????
            var sensitivePerms = user.Permissions?.Where(p => p.StartsWith("p:")).ToList();
            if (sensitivePerms != null && sensitivePerms.Count > 0)
            {
                claims.Add(new Claim("sensitivePerms", string.Join(',', sensitivePerms)));
            }
            return claims;
        }

    }
}


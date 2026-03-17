using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    public string CreateToken(AppUser user)
    {
       var tokenKey = config["TokenKey"] ?? throw new Exception("Cannot get token key");
       if(tokenKey.Length < 64){throw new Exception("Your token key needs to be > = 64 characters");}
       //Use encoding because we need a byte array in the constructor of the SymmetricSecurityKey
       var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)); //Use a symmetric key because this key doesn't leave the server
       
       var claims = new List<Claim> //A list of claims the user is making
       {
           new (ClaimTypes.Email, user.Email), 
           new(ClaimTypes.NameIdentifier, user.Id)
       };

       var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
       var tokenDescriptor = new SecurityTokenDescriptor
       {
           Subject = new ClaimsIdentity(claims),
           Expires = DateTime.UtcNow.AddDays(7),
           SigningCredentials = creds
        
       };

       var tokenHandler = new JwtSecurityTokenHandler(); //This class creates the token based on tokenDescriptor
       var token = tokenHandler.CreateToken(tokenDescriptor);
       return tokenHandler.WriteToken(token);
    }
}

using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(AppDbContext context, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")] //api/account/register
    public async Task<ActionResult<UserDto>> Register (RegisterDTO registerDto)
    {
        if(await EmailExists(registerDto.Email))
        {
            return BadRequest("Email taken.");
        }
        
        using var hmac = new HMACSHA512(); //Used to salt the password provided
        var user = new AppUser
        {
            DisplayName = registerDto.DisplayName,
            Email = registerDto.Email,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };
        
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user.ToDto(tokenService);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        //Check if the username exists in the database
        var user = await context.Users.SingleOrDefaultAsync(x => x.Email == loginDto.Email);
        if(user == null){return Unauthorized("Invalid email address");}
        //Check if the provided password matches the username + password stored for that user in the DB
        using var hmac = new HMACSHA512(user.PasswordSalt); //Use to create hash of login pswd to compare to db, required salt 
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
        //Hash is a byte array, so we need to loop through the array and compare each character
        for(int i = 0; i < computedHash.Length; i++)
        {
            if(computedHash[i]!= user.PasswordHash[i]){return Unauthorized("Invalid Password");}
        }
        return user.ToDto(tokenService);
    }

    private async Task<bool> EmailExists(string email)
    {
        return await context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
    }
}

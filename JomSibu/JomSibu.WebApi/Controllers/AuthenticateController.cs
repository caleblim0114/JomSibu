using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using JomSibu.WebApi.Services;
using System.Numerics;
using JomSibu.Shared.Models;
using Microsoft.EntityFrameworkCore;
using JomSibu.Shared.SystemModels;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace JomSibu.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private JomSibuDatabaseContext _database;
        private IFileStorageService _fileStorageService;

        public AuthenticateController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
            IConfiguration configuration, JomSibuDatabaseContext database, IFileStorageService fileStorageService)
        {
            _database = database;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _fileStorageService = fileStorageService;
        }

        //public class GenericResponse<T>
        //{
        //    public bool IsSuccess { get; set; }
        //    public int StatusCode { get; set; }
        //    public string Message { get; set; }
        //    public T Data { get; set; }
        //}

        //public async Task<GenericResponse<LoginResponse>> Testing()
        //{
        //    return new GenericResponse<LoginResponse>()
        //    {
        //        Data = new LoginResponse() {}
        //    };
        //}

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                return Ok(new GenericResponse<LoginResponse>
                {
                    IsSuccess = false,
                    StatusCode = CustomStatusCodes.WrongEmailOrPassword,
                    Message = "Email does not exist",
                    Data = new LoginResponse()
                });
            }
            var userDetail = await _database.GetUserByAspNetUserIdAsync(user.Id);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                if (userDetail.IsDeleted == 1)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new CustomResponse { StatusCode = CustomStatusCodes.VendorHavenVerified, Message = "Account deleted." });
                }
                
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.UtcNow.AddYears(1),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                var role = await _database.UserDetailsTables
                    .AsNoTracking()
                    .Select(x => new UserDetailsTable()
                    {
                        AspNetUserId = x.AspNetUserId
                    })
                    .FirstAsync(x => x.AspNetUserId == user.Id);

                return Ok(new GenericResponse<LoginResponse>
                {
                    IsSuccess = true,
                    Data = new LoginResponse
                    {
                        Token = new JwtSecurityTokenHandler().WriteToken(token),
                        Expiration = token.ValidTo,
                        UserId = role.Id
                    }
                });
            }

            return Ok(new GenericResponse<LoginResponse>
            {
                IsSuccess = false,
                StatusCode = CustomStatusCodes.WrongEmailOrPassword,
                Message = "Invalid email or password",
                Data = new LoginResponse()
            });
        }

        //todo please remove shouldnt be here
        //[HttpGet("RegisterHotels")]
        //public Task<List<HotelsTable>> GetAllHotels()
        //{
        //    return _database.HotelsTables.AsNoTracking()
        //        .ToListAsync();
        //}

        //[HttpGet("RegisterLocations")]
        //public Task<List<LocationsTable>> GetAllLocations()
        //{
        //    return _database.LocationsTables.AsNoTracking()
        //        .ToListAsync();
        //}

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Email);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new CustomResponse { StatusCode = CustomStatusCodes.AccountRegistered, Message = "User already exists!" });

            if (ModelState.IsValid)
            {
                using (var transaction = await _database.Database.BeginTransactionAsync())
                {
                    try
                    {
                        IdentityUser aspNetUser = new IdentityUser()
                        {
                            Email = model.Email,
                            SecurityStamp = Guid.NewGuid().ToString(),
                            UserName = model.Email,
                            PhoneNumber = model.PhoneNumber
                        };
                        var result = await _userManager.CreateAsync(aspNetUser, model.Password);
                        if (!result.Succeeded)
                            return StatusCode(StatusCodes.Status500InternalServerError, new Response
                            {
                                Status = "Error",
                                Message = string.Join("\n", result.Errors
                                    .Select(x => x.Description))
                            });
                    
                        if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
                        if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                            await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));
                        if (!await _roleManager.RoleExistsAsync(UserRoles.SystemAdmin))
                            await _roleManager.CreateAsync(new IdentityRole(UserRoles.SystemAdmin));

                        if (model.UserRoleId == (int)UserRoleEnum.User)
                        {
                            await _userManager.AddToRoleAsync(aspNetUser, UserRoles.User);
                        }
                        else
                        {
                            return BadRequest("User role does not exist!");
                        }

                        var newUser = new UserDetailsTable();
                        if (model.UserRoleId == (int)UserRoleEnum.User)
                        {
                            var user = new UserDetailsTable()
                            {
                                AspNetUserId = aspNetUser.Id,
                                FullName = model.FullName,
                                Email = model.Email,
                                DateJoined = DateTime.UtcNow.AddHours(8),
                                UserRoleId = model.UserRoleId,
                                PhoneNumber = model.PhoneNumber,
                                IsHalal = model.IsHalal,
                                IsVegetarian = model.IsVegeterian,
                                BudgetStatusId = model.BudgetStatusId,
                            };

                            newUser = user;
                        }
                        
                        await _database.UserDetailsTables.AddAsync(newUser);
                        await _database.SaveChangesAsync();

                        //var createdUser = _database.UserDetailsTables.FirstOrDefault(x => x.Email == model.Email);

                        //if (model.Preferences.Count() != 0)
                        //{
                        //    foreach (var pref in model.Preferences)
                        //    {
                        //        _database.UserPreferencesTables.Add(new UserPreferencesTable
                        //        {
                        //            UserId = createdUser.Id,
                        //            PreferenceId = pref.Id,
                        //        });
                        //    }
                        //}
                        //await _database.SaveChangesAsync();

                        await transaction.CommitAsync();
                        return StatusCode(StatusCodes.Status200OK, new CustomResponse { StatusCode = CustomStatusCodes.Ok, Message = "User created successfully!" });

                    }
                    catch (Exception e)
                    {
                        await transaction.RollbackAsync();
                        return StatusCode(StatusCodes.Status500InternalServerError, new CustomResponse { StatusCode = CustomStatusCodes.UnknownError, Message = "An error has occurred" });
                    }
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new CustomResponse
            {
                StatusCode = CustomStatusCodes.InvalidData,
                Message = string.Join("\n", ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage))
            });
        }

        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> Delete()
        {
            var aspNetUser = await _userManager.FindByNameAsync(User!.Identity!.Name);
            var userDetailInDb = await _database.UserDetailsTables
               .FirstAsync(x => x.AspNetUserId == aspNetUser.Id);

            var deleteDateTime = DateTime.UtcNow.AddHours(8);

            userDetailInDb.IsDeleted = 1;
            aspNetUser.UserName = $"DeletedUser_{aspNetUser.UserName}_{Guid.NewGuid().ToString()}";
            aspNetUser.Email = aspNetUser.UserName;

            await _userManager.UpdateAsync(aspNetUser);
            await _database.SaveChangesAsync();
            return NoContent();
        }
    }
}

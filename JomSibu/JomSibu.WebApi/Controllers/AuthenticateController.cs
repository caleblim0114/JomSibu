using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMCRecycle.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using JomSibu.WebApi.Services;
using System.Numerics;
using JomSibu.Shared.Models;
using Microsoft.EntityFrameworkCore;

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
        private IEmailSender _emailSender;
        private IFileStorageService _fileStorageService;

        public AuthenticateController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
            IConfiguration configuration, JomSibuDatabaseContext database, IEmailSender emailSender, IFileStorageService fileStorageService)
        {
            _emailSender = emailSender;
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
                if (userDetail.IsDeleted == true)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new CustomResponse { StatusCode = CustomStatusCodes.VendorHavenVerified, Message = "Account deleted." });
                }

                var ImageList = _database.UserIdentityImages.Where(x => x.UserDetailId == userDetail.Id);
                if (userDetail.UserRoleId == (int)UserRoleEnum.Vendor || userDetail.UserRoleId == (int)UserRoleEnum.Promoter)
                {
                    if (userDetail.HasIdentityImages == 0 || userDetail.HasIdentityImages == null)
                    {
                        //return Ok(new GenericResponse<LoginResponse>
                        //{
                        //    IsSuccess = false,
                        //    StatusCode = CustomStatusCodes.VendorHavenSubmitIdentity,
                        //    Message = "Please submit your identity for verification purpose.",
                        //    Data = new LoginResponse()
                        //    {
                        //        Role = userDetail.UserRoleId
                        //    }
                        //});

                        var tmpUserRoles = await _userManager.GetRolesAsync(user);

                        var tmpAuthClaims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        };

                        foreach (var userRole in tmpUserRoles)
                        {
                            tmpAuthClaims.Add(new Claim(ClaimTypes.Role, userRole));
                        }

                        var tmpAuthSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                        var tmpToken = new JwtSecurityToken(
                            issuer: _configuration["JWT:ValidIssuer"],
                            audience: _configuration["JWT:ValidAudience"],
                            expires: DateTime.UtcNow.AddYears(1),
                            claims: tmpAuthClaims,
                            signingCredentials: new SigningCredentials(tmpAuthSigningKey, SecurityAlgorithms.HmacSha256)
                            );

                        var tmpRole = await _database.UserDetailsTables
                            .AsNoTracking()
                            .Select(x => new UserDetailWithEmailAndPhone()
                            {
                                AspNetUserId = x.AspNetUserId,
                                UserRoleId = x.UserRoleId
                            })
                            .FirstAsync(x => x.AspNetUserId == user.Id);

                        return Ok(new GenericResponse<LoginResponse>
                        {
                            IsSuccess = false,
                            StatusCode = CustomStatusCodes.VendorHavenSubmitIdentity,
                            Message = "Please submit your identity for verification purpose.",
                            Data = new LoginResponse
                            {
                                Role = tmpRole.UserRoleId ?? 0,
                                Token = new JwtSecurityTokenHandler().WriteToken(tmpToken),
                                Expiration = tmpToken.ValidTo,
                                UserId = tmpRole.Id
                            }
                        });
                        //return StatusCode(StatusCodes.Status500InternalServerError, new CustomResponse { StatusCode = CustomStatusCodes.VendorHavenSubmitIdentity, Message = "Please submit your identity for verification purpose." });
                    }
                    else if (!userDetail.DateVerified.HasValue)
                    {
                        return Ok(new GenericResponse<LoginResponse>
                        {
                            IsSuccess = false,
                            StatusCode = CustomStatusCodes.VendorHavenVerified,
                            Message = "Please wait for the admin to approve your verification.",
                            Data = new LoginResponse()
                            {
                                Role = userDetail.UserRoleId
                            }
                        });
                        //return StatusCode(StatusCodes.Status500InternalServerError, new CustomResponse { StatusCode = CustomStatusCodes.VendorHavenVerified, Message = "Please wait for the admin to approve your verification." });
                    }
                }

                //todo because right now can only send email to 100 per day, ask them to verify only when they redeem rewards
                //if(userDetail.UserRoleId == (int)UserRoleEnum.User)
                //{
                //    if (!user.EmailConfirmed)
                //    {
                //        return Ok(new GenericResponse<LoginResponse>
                //        {
                //            IsSuccess = false,
                //            StatusCode = CustomStatusCodes.EmailHavenConfirm,
                //            Message = "Please verify your email before login.",
                //            Data = new LoginResponse()
                //            {
                //                Role = userDetail.UserRoleId
                //            }
                //        });
                //        //return StatusCode(StatusCodes.Status500InternalServerError, new CustomResponse { StatusCode = CustomStatusCodes.EmailHavenConfirm, Message = "Please verify your email before login." });
                //    }
                //}
                
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
                    .Select(x => new UserDetailWithEmailAndPhone()
                    {
                        AspNetUserId = x.AspNetUserId,
                        UserRoleId = x.UserRoleId
                    })
                    .FirstAsync(x => x.AspNetUserId == user.Id);

                return Ok(new GenericResponse<LoginResponse>
                {
                    IsSuccess = true,
                    Data = new LoginResponse
                    {
                        Role = role.UserRoleId ?? 0,
                        Token = new JwtSecurityTokenHandler().WriteToken(token),
                        Expiration = token.ValidTo,
                        UserId = role.Id
                    }
                });
                //return Ok(new LoginResponse
                //{
                //    Role = role.UserRoleId??0,
                //    Token = new JwtSecurityTokenHandler().WriteToken(token),
                //    Expiration = token.ValidTo
                //});
            }

            return Ok(new GenericResponse<LoginResponse>
            {
                IsSuccess = false,
                StatusCode = CustomStatusCodes.WrongEmailOrPassword,
                Message = "Invalid email or password",
                Data = new LoginResponse()
            });
            //return StatusCode(StatusCodes.Status500InternalServerError, new CustomResponse { StatusCode = CustomStatusCodes.WrongEmailOrPassword, Message = "Invalid email or password" });
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

        [HttpGet("RegisterResidentialZone")]
        public Task<List<ResidentialZones>> GetAllResidentialZones()
        {
            return _database.ResidentialZones.AsNoTracking()
                .ToListAsync();
        }

        [HttpGet("RegisterRecyclingCenter")]
        public Task<List<RecyclingCenters>> GetAllRecyclingCenters()
        {
            return _database.RecyclingCenters.AsNoTracking()
                .ToListAsync();
        }

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
                        if (!await _roleManager.RoleExistsAsync(UserRoles.Vendor))
                            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Vendor));
                        if (!await _roleManager.RoleExistsAsync(UserRoles.Promoter))
                            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Promoter));

                        if (model.UserRoleId == (int)UserRoleEnum.User)
                        {
                            await _userManager.AddToRoleAsync(aspNetUser, UserRoles.User);
                        }
                        else if (model.UserRoleId == (int)UserRoleEnum.Vendor)
                        {
                            await _userManager.AddToRoleAsync(aspNetUser, UserRoles.Vendor);
                        }
                        else if (model.UserRoleId == (int)UserRoleEnum.Promoter)
                        {
                            await _userManager.AddToRoleAsync(aspNetUser, UserRoles.Promoter);
                        }
                        else
                        {
                            return BadRequest("User role does not exist!");
                        }

                        var newUser = new UserDetails();
                        if (model.UserRoleId == (int)UserRoleEnum.User)
                        {
                            var user = new UserDetails()
                            {
                                AspNetUserId = aspNetUser.Id,
                                FullName = model.FullName,
                                DateJoined = DateTime.UtcNow.AddHours(8),
                                UserRoleId = model.UserRoleId,
                                ContactNumber = model.PhoneNumber,
                                ResidentialZoneId = model.ResidentialZoneId
                            };

                            newUser = user;
                        }
                        else if (model.UserRoleId == (int)UserRoleEnum.Vendor)
                        {
                            var user = new UserDetails()
                            {
                                AspNetUserId = aspNetUser.Id,
                                FullName = model.FullName,
                                DateJoined = DateTime.UtcNow.AddHours(8),
                                UserRoleId = model.UserRoleId,
                                ContactNumber = model.PhoneNumber,
                                RecyclingCenterId = model.RecyclingCenterId
                            };

                            newUser = user;
                        }
                        else if (model.UserRoleId == (int)UserRoleEnum.Promoter)
                        {
                            var user = new UserDetails()
                            {
                                AspNetUserId = aspNetUser.Id,
                                FullName = model.FullName,
                                DateJoined = DateTime.UtcNow.AddHours(8),
                                UserRoleId = model.UserRoleId,
                                ContactNumber = model.PhoneNumber,
                                OrganisationId = model.OrganisationId
                            };

                            newUser = user;
                        }
                        
                        await _database.UserDetails.AddAsync(newUser);
                        await _database.SaveChangesAsync();
                        await transaction.CommitAsync();
                        //Hide email verification for user for now, later reopen again for reward redemption
                        //try
                        //{
                        //    if (model.UserRoleId == (int)UserRoleEnum.User)
                        //    {
                        //        var code = await _userManager.GenerateEmailConfirmationTokenAsync(aspNetUser);
                        //        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        //        var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = aspNetUser.Id, code = code },
                        //            protocol: Request.Scheme);

                        //        //todo reformat the verification email
                        //        await _emailSender.SendEmailAsync(aspNetUser.Email, "Recycle App - Account Confirmation",
                        //            "Dear User, your account has been created successfully.<br/><br/>" +
                        //            "Please confirm your account by clicking <a href=\"" + callbackUrl +
                        //            "\">this</a> confirmation link.<br/><br/>" +
                        //            "Please do not reply this email. Thank you!");
                        //    }
                        //}
                        //catch (Exception e)
                        //{
                        //    Console.WriteLine(e);
                        //}
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

        [HttpPost]
        [Route("SubmitIdentity")]
        public async Task<IActionResult> SubmitIdentity()
        {
            var aspNetUser = await _userManager.FindByNameAsync(User!.Identity!.Name);
            var user = await _database.UserDetails
                .FirstAsync(x => x.AspNetUserId == aspNetUser.Id);
            var httpRequest = HttpContext.Request;

            if (!httpRequest.HasFormContentType)
            {
                return StatusCode(StatusCodes.Status415UnsupportedMediaType, new CustomResponse { StatusCode = CustomStatusCodes.InvalidMediaType, Message = "Invalid media type" });
            }

            if (httpRequest.Form.ContainsKey("model"))
            {
                if (httpRequest.Form.Files.Count > 0)
                {
                    foreach (var file in httpRequest.Form.Files)
                    {
                        if (file.Name == "files")
                        {
                            var newUserIdentityImages = new UserIdentityImages()
                            {
                                UserDetailId = user.Id,
                                ImagePath = await _fileStorageService.CreateFileAsync("UserIdentity/Image", file.FileName, file.OpenReadStream(), file.ContentType)
                            };

                            await _database.UserIdentityImages.AddAsync(newUserIdentityImages);
                        }
                    }
                }

                user.HasIdentityImages = 1;

                await _database.SaveChangesAsync();
                return Ok();
            }

            return BadRequest();
        }

        [HttpDelete]
        [Route("delete")]
        public async Task<IActionResult> Delete()
        {
            var aspNetUser = await _userManager.FindByNameAsync(User!.Identity!.Name);
            var userDetailInDb = await _database.UserDetails
               .FirstAsync(x => x.AspNetUserId == aspNetUser.Id);

            var deleteDateTime = DateTime.UtcNow.AddHours(8);

            userDetailInDb.IsDeleted = true;
            userDetailInDb.DateDeleted = deleteDateTime;
            aspNetUser.UserName = $"DeletedUser_{aspNetUser.UserName}_{Guid.NewGuid().ToString()}";
            aspNetUser.Email = aspNetUser.UserName;

            await _userManager.UpdateAsync(aspNetUser);
            await _database.SaveChangesAsync();
            return NoContent();
        }
    }
}

using JomSibu.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using JomSibu.WebApi.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using JomSibu.Shared.SystemModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace JomSibu.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserDetailController : ControllerBase
    {
        private readonly JomSibuDatabaseContext _database;
        private readonly UserManager<IdentityUser> _userManager;
        private IFileStorageService _fileStorageService;

        public UserDetailController(JomSibuDatabaseContext database, UserManager<IdentityUser> userManager, IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
            _database = database;
            _userManager = userManager;
        }
        
        [HttpGet("GetAllUserDetail")]
        [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.Admin}")]
        public async Task<List<UserDetailsTable>> GetAllUserDetail()
        {
            return await _database.UserDetailsTables
                .AsNoTracking()
                .Where(x => x.IsDeleted != 1)
                .Include(x => x.UserRoutesTables)
                .Select(x => new UserDetailsTable()
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    UserRoleId = x.UserRoleId,
                    DateJoined = x.DateJoined!.Value.AddHours(8),
                    ImagePath = x.ImagePath,
                    PhoneNumber = _database.Users.First(y => y.Id == x.AspNetUserId).PhoneNumber ?? string.Empty,
                    Email = _database.Users.First(y => y.Id == x.AspNetUserId).Email ?? string.Empty
                })
                .ToListAsync();
        }

        //[HttpGet("GetSystemDetails")]
        //public async Task<SystemDetailsModel> GetSystemDetails()
        //{
        //    var aspNetUser = await _userManager.FindByNameAsync(User!.Identity!.Name);
        //    var user = await _database.GetUserByAspNetUserIdAsync(aspNetUser.Id);
        //    var isVerified = false;
        //    if (user.DateVerified != null)
        //    {
        //        isVerified = true;
        //    }
        //    var hasIdentity = false;
        //    if (user.HasIdentityImages == 1)
        //    {
        //        hasIdentity = true;
        //    }

        //    var systemDetails = new SystemDetailsModel
        //    {
        //        Version = "1.0.17",
        //        IsVerified = isVerified,
        //        HasIdentity = hasIdentity,
        //        SystemUpdateMessage = "Please update your app to the latest version.",
        //    };

        //    return systemDetails;
        //}

        [HttpGet]
        public async Task<UserDetailsTable> GetUserDetail(int id = 0)
        {
            var aspNetUser = await _userManager.FindByNameAsync(User!.Identity!.Name);
            var user = await _database.GetUserByAspNetUserIdAsync(aspNetUser.Id);
            if (User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.SystemAdmin))
            {
                if (id == 0)
                {
                    id = user.Id;
                }
                return await _database.UserDetailsTables
                    .AsNoTracking()
                    .Select(x => new UserDetailsTable()
                    {
                        Id = x.Id,
                        FullName = x.FullName,
                        DateJoined = x.DateJoined!.Value.AddHours(8),
                        ImagePath = x.ImagePath,
                        PhoneNumber = _database.Users.First(y => y.Id == x.AspNetUserId).PhoneNumber ?? string.Empty,
                        Email = _database.Users.First(y => y.Id == x.AspNetUserId).Email ?? string.Empty,
                        })
                    .FirstAsync(x => x.Id == id);
            } 
            else 
            {
                return await _database.UserDetailsTables
                    .AsNoTracking()
                    .Select(x => new UserDetailsTable()
                    {
                        Id = x.Id,
                        FullName = x.FullName,
                        DateJoined = x.DateJoined!.Value.AddHours(8),
                        ImagePath = x.ImagePath,
                        PhoneNumber = aspNetUser.PhoneNumber,
                        Email = aspNetUser.Email,
                    })
                    .FirstAsync(x => x.Id == user.Id);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUserDetail()
        {
            var aspNetUser = await _userManager.FindByNameAsync(User!.Identity!.Name);
            var userDetailInDb = await _database.UserDetailsTables
                .FirstAsync(x => x.AspNetUserId == aspNetUser.Id);
            var httpRequest = HttpContext.Request;

            if (!httpRequest.HasFormContentType)
            {
                return StatusCode(StatusCodes.Status415UnsupportedMediaType, new CustomResponse { StatusCode = CustomStatusCodes.InvalidMediaType, Message = "Invalid media type" });
            }

            if (httpRequest.Form.ContainsKey("model"))
            {
                httpRequest.Form.TryGetValue("model", out StringValues model);
                var record = JsonConvert.DeserializeObject<EditProfileModels>(model);
                var context = new ValidationContext(record);
                var results = new List<ValidationResult>();
                var isValid = Validator.TryValidateObject(record, context, results, true);

                if (!isValid)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new CustomResponse
                    {
                        StatusCode = CustomStatusCodes.InvalidData,
                        Message = string.Join("\n", results
                            .Select(x => x.ErrorMessage))
                    });
                }

                aspNetUser.PhoneNumber = record.PhoneNumber;
                userDetailInDb.FullName = record.FullName;
                //userDetailInDb.Postcode = record.Postcode;
                //userDetailInDb.CityId = record.CityId;

                if (httpRequest.Form.Files.Count > 0)
                {
                    foreach (var file in httpRequest.Form.Files)
                    {
                        if (file.Name == "files")
                        {
                            var newPath = await _fileStorageService.CreateFileAsync("UserProfileImage/Image", file.FileName, file.OpenReadStream(), file.ContentType);
                            if (!string.IsNullOrWhiteSpace(userDetailInDb.ImagePath))
                            {
                                await _fileStorageService.DeleteFileIfExistsAsync(userDetailInDb.ImagePath);
                            }
                            userDetailInDb.ImagePath = newPath;
                            break;
                        }
                    }
                }

                await _database.SaveChangesAsync();
                return Ok(await GetUserDetail());
            }

            return BadRequest();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{UserRoles.SystemAdmin},{UserRoles.Admin}")]
        public async Task<IActionResult> DeleteUserDetail(long id)
        {
            var userDetail = await _database.UserDetailsTables.FindAsync(id);
            if (userDetail == null)
            {
                return NotFound();
            }

            userDetail.IsDeleted = 1;
            _database.UserDetailsTables.Update(userDetail);
            await _database.SaveChangesAsync();

            return NoContent();
        }
    }
}

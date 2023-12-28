using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using JomSibu.Shared.Models;
using JomSibu.Shared.SystemModels;

namespace SMCRecycle.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        private IEmailSender _emailSender;
        private UserManager<IdentityUser> _userManager;
        private JomSibuDatabaseContext _database;

        public AccountController(IEmailSender emailSender, UserManager<IdentityUser> userManager,JomSibuDatabaseContext database)
        {
            _database = database;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return BadRequest("Email not exists");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
#warning use deep link
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code },
                    protocol: Request.Scheme);
                await _emailSender.SendEmailAsync(user.Email, "JomSibu - Password Reset",
                    "Dear JomSibu's User, <br/><br/>" +
                    "Please reset your password by clicking <a href=\"" + HtmlEncoder.Default.Encode(callbackUrl) +
                    "\">this</a> password reset link.<br/><br/>" +
                    "Please do not reply this email. Thank you!");
#warning tgt with melayu version
                return Ok("Password reset link has been sent to your email, please click the link in your email to reset");
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new CustomResponse
            {
                StatusCode = CustomStatusCodes.InvalidData,
                Message = string.Join("\n", ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage))
            });
        }

        [AllowAnonymous]
        [HttpGet("ResetPassword")]
        public ActionResult ResetPassword(string? code)
        {
            return code == null ? NotFound() : View(new ResetPasswordModel()
            {
                Code = code
            });
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword([FromForm] ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound();
            }
            var result = await _userManager.ResetPasswordAsync(user, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code)), model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View();
        }

        [HttpGet("ResetPasswordConfirmation")]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [Route("ResetPasswordRequest")]
        public async Task<ActionResult> ResetPasswordRequest(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("Email not exists");
            }
            var result = await _userManager.ResetPasswordAsync(user, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code)), model.Password);
            if (result.Succeeded)
            {
                return Ok("Password reset successfully");
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new CustomResponse
            {
                StatusCode = CustomStatusCodes.InvalidData,
                Message = string.Join("\n", result.Errors
                    .Select(x => x.Description))
            });
        }

        [HttpPost]
        [Authorize]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var aspNetUser = await _userManager.FindByNameAsync(User!.Identity!.Name);
            var result = await _userManager.ChangePasswordAsync(aspNetUser, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return Ok("Password changed");
            }

            return BadRequest("Old password is incorrect");
        }
    }
}

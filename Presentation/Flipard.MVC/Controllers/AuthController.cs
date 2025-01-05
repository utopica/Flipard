using System.Threading.Tasks;
using Flipard.Domain.Identity;
using Flipard.Domain.Interfaces;
using Flipard.MVC.Services;
using Flipard.MVC.ViewModels;
using Flipard.MVC.ViewModels.Auth;
using Flipard.Persistence.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Flipard.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly INToastNotifyService _nToastNotifyService;
        private readonly IEmailService _emailService;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager,
            INToastNotifyService nToastNotifyService, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _nToastNotifyService = nToastNotifyService;
            _emailService = emailService;
        }
        
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            var registerViewModel = new AuthRegisterViewModel();
            return View(registerViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync(AuthRegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
            {
                _nToastNotifyService.AddErrorToastMessage("Please check the input fields.");
                return View(registerViewModel);
            }

            var userId = Guid.NewGuid();

            var user = new User()
            {
                Id = userId,
                Email = registerViewModel.Email,
                Birthdate = registerViewModel.Birthdate?.ToUniversalTime(),
                UserName = registerViewModel.Username,
                CreatedByUserId = userId.ToString(),
                CreatedOn = DateTimeOffset.UtcNow,
            };

            var identityResult = await _userManager.CreateAsync(user, registerViewModel.Password);

            if (!identityResult.Succeeded)
            {
                foreach (var error in identityResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                return View(registerViewModel);
            }

            TempData["SuccessMessage"] = "You've successfully registered to Flipard!";

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(Index), "Home");
            }

            var loginViewModel = new AuthLoginViewModel();

            if (TempData["SuccessMessage"] != null)
            {
                _nToastNotifyService.AddSuccessToastMessage(TempData["SuccessMessage"].ToString());
            }

            return View(loginViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(AuthLoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                _nToastNotifyService.AddErrorToastMessage("Please check the input fields.");
                return View(loginViewModel);
            }

            User user;

            if (loginViewModel.UserIdentifier.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(loginViewModel.UserIdentifier);
            }
            else
            {
                user = await _userManager.FindByNameAsync(loginViewModel.UserIdentifier);
            }

            if (user is null)
            {
                _nToastNotifyService.AddErrorToastMessage("Your email/username or password is incorrect.");
                return View(loginViewModel);
            }

            var loginResult = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, true, false);

            if (!loginResult.Succeeded)
            {
                _nToastNotifyService.AddErrorToastMessage("Your email/username or password is incorrect.");
                return View(loginViewModel);
            }

            _nToastNotifyService.AddSuccessToastMessage($"Welcome {user.UserName} to Flipard!");

            // if (user.Email != null) await _emailService.SendEmailAsync(user.Email, "flipard@elifokms.dev", "ornektir");

            return RedirectToAction(nameof(Index), "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }
    }
}
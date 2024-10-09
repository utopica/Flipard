using System.Threading.Tasks;
using Flipard.Domain.Identity;
using Flipard.Domain.Interfaces;
using Flipard.MVC.Services;
using Flipard.MVC.ViewModels;
using Flipard.Persistence.Services;
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

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, INToastNotifyService nToastNotifyService, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _nToastNotifyService = nToastNotifyService;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            _nToastNotifyService.AddInfoToastMessage("You got redirected");
            return View();
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
                return View(registerViewModel);
            }

            var userId = Guid.NewGuid();

            var user = new User()
            {
                Id = userId,
                Email = registerViewModel.Email,
                Birthdate = registerViewModel.Birthdate.Value.ToUniversalTime(),
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

            _nToastNotifyService.AddSuccessToastMessage("You've successfully registered to Flipard!");
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
            return View(loginViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(AuthLoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(loginViewModel);
            }

            var user = await _userManager.FindByEmailAsync(loginViewModel.Email);

            if (user == null)
            {
                _nToastNotifyService.AddErrorToastMessage("Your email or password is incorrect.");
                return View(loginViewModel);
            }

            var loginResult = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, true, false);

            if (!loginResult.Succeeded)
            {
                _nToastNotifyService.AddErrorToastMessage("Your email or password is incorrect.");
                return View(loginViewModel);
            }

            _nToastNotifyService.AddSuccessToastMessage($"Welcome {user.UserName} to Flipard!");

            if (user.Email != null) await _emailService.SendEmailAsync(user.Email, "flipard@elifokms.dev", "ornektir");

            return RedirectToAction(nameof(Index), "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var editProfileViewModel = new AuthEditProfileViewModel
            {
                Username = user.UserName,
                Email = user.Email,
                Birthdate = user.Birthdate.Value.ToUniversalTime(),
            };

            return View(editProfileViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfileAsync(AuthEditProfileViewModel editProfileViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(editProfileViewModel);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (editProfileViewModel.CurrentPassword != null && editProfileViewModel.NewPassword != null)
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, editProfileViewModel.CurrentPassword, editProfileViewModel.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(editProfileViewModel);
                }
            }

            user.UserName = editProfileViewModel.Username;
            user.Email = editProfileViewModel.Email;
            user.Birthdate = editProfileViewModel.Birthdate.Value.ToUniversalTime();

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(editProfileViewModel);
            }

            _nToastNotifyService.AddSuccessToastMessage("Your profile has been updated successfully.");
            return RedirectToAction(nameof(Index), "Home");
        }
    }
}

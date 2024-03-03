using Flipard.Domain.Identity;
using Flipard.MVC.Services;
using Flipard.MVC.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Flipard.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly INToastNotifyService _nToastNotifyService;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, INToastNotifyService nToastNotifyService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _nToastNotifyService = nToastNotifyService;
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
                return RedirectToAction(nameof(Index), controllerName: "Main");

            }

            var registerViewModel = new AuthRegisterViewModel();

            return View(registerViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync(AuthRegisterViewModel registerViewModel)
        {
            if(!ModelState.IsValid)
            {
                return View(registerViewModel);
            }

            var userId = Guid.NewGuid();

            var user = new User()
            {
                Id = userId,
                Email = registerViewModel.Email,
                FirstName = registerViewModel.FirstName,
                LastName = registerViewModel.LastName,
                Gender = registerViewModel.Gender,
                Birthdate = registerViewModel.Birthdate.Value.ToUniversalTime(),
                UserName = registerViewModel.Username,
                CreatedByUserId = userId.ToString(),
                CreatedOn = DateTimeOffset.UtcNow,

            };

            var identityResult = await _userManager.CreateAsync(user, registerViewModel.Password);

            if(!identityResult.Succeeded)
            {
                //throw new Exception("An error has occurred!");
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
                return RedirectToAction(nameof(Index), controllerName: "Main");

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

            if(user is null)
            {
                _nToastNotifyService.AddErrorToastMessage("Your email or password is incorrect.");

                return View(loginViewModel);

            }

            var loginResult = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, true, false);
            

            if(!loginResult.Succeeded)
            {
                _nToastNotifyService.AddErrorToastMessage("Your email or password is incorrect.");

                return View(loginViewModel);

            }
            _nToastNotifyService.AddSuccessToastMessage($"Welcome {user.UserName} to Flipard!");

            return RedirectToAction(nameof(Index),controllerName:"Main");
        }
    }
}

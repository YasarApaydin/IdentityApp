using IdentityApp.Models;
using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IdentityApp.Controllers
{
    public class AccountController:Controller
    {

        private  UserManager<AppUser> _userManager;
        private  RoleManager<AppRole> _roleManager;
        private  SignInManager<AppUser> _signInManager;
        private IEmailSender _emailSender;

        public AccountController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, SignInManager<AppUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            
        }

        public IActionResult Login()
        {
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if(user != null)
                {
                    await _signInManager.SignOutAsync();
                    if(!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError("","Hesabınızı Onaylayınız");
                        return View(model);
                    }



                    var result = await _signInManager.PasswordSignInAsync(user, model.Password,model.RememberMe,true);
                    if (result.Succeeded)
                    {
                        await _userManager.ResetAccessFailedCountAsync(user);
                        await _userManager.SetLockoutEndDateAsync(user,null);
                        return RedirectToAction("Index", "Home");
                    }else if (result.IsLockedOut)
                    {
                        var lockoutDate = await _userManager.GetLockoutEndDateAsync(user);
                        var timeLeft = lockoutDate.Value - DateTime.UtcNow;
                        ModelState.AddModelError("",$"Hesabınız kitlendi! Lütfen {timeLeft.Minutes} dakika sonra tekrar deneyiniz.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Hatalı Parola!");
                    }
                
                }
                else
                {
                    ModelState.AddModelError("","Hatalı Email!");
                }
            }
            return View(model);
        }


        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.Fullname
                };

                IdentityResult result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var tokken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var url = Url.Action("ConfirmEmail","Acccount", new {user.Id, tokken});
                    await _emailSender.SendEmailAsync(user.Email, "Hesap Onayı",$"Lütfen Email Hesabınızı Onaylamak için Linke <a href='https://localhost:44342{url}' >tıklayınız.</a>");


                    TempData["message"] = "Email Hesabınızdaki onay mailinize tıklayınız.";
                    return RedirectToAction("Login","Account");

                }
                foreach (IdentityError err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }


            }
            return View(model);
        }
        public async Task<IActionResult> ConfirmEmail(string id, string token)
        {
            if(id == null || token == null)
            {
                TempData["message"] = "Geçersiz tokken bilgisi!";
                return View();
            }

            var user = await _userManager.FindByIdAsync(id);
            if(user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user,token);
                if (result.Succeeded)
                {
                    TempData["message"] = "Hesabınız Onaylandı :)";
                    return RedirectToAction("Login","Account");
                }
            }

            TempData["message"] = "Kullanıcı Bulunamadı :(";
            return View();

        }



        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index");
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                TempData["message"] = "Eposta adresinizi giriniz.";
                return View();
            }
            var user = await _userManager.FindByEmailAsync(Email);
            if(user == null)
            {
                TempData["message"] = "Eposta adresiyle eşleşen bir kayıt yok.";
                return View();
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var url = Url.Action("ResetPassword", "Acccount", new { user.Id, token });


            await _emailSender.SendEmailAsync(Email,"Parola Sıfırlama",$"Parolanızı sıfırlamak için Linke <a href='https://localhost:44342{url}' >tıklayınız.</a>");
            TempData["message"] = "Eposta adresinize gönerilen şifrenizi sıfırlayabilirsiniz.";
            return View();
        }


        public IActionResult ResetPassword(string id, string token)
        {
            if(id == null || token == null)
            {
                RedirectToAction("Login");
            }
            var model = new ResetPasswordModel { Token = token };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if(user == null)
                {
                    TempData["message"] = "Bu mail adresiyle eşleşen kullanıcı yok.";
                    return RedirectToAction("Login");
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Token,model.Password);
                if (result.Succeeded)
                {
                    TempData["message"] = "Şifreniz Değiştirildi";
                    RedirectToAction("Login");
                }
                foreach (IdentityError err in result.Errors)
                {
                    ModelState.AddModelError("",err.Description);

                }
            }
            
            
            return View(model);

        }
    }
}

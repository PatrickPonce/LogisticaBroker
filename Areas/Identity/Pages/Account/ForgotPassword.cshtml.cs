// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using LogisticaBroker.Models;
// --- INICIO DEL CAMBIO ---
// Aseguramos que se use NUESTRO servicio de email y no el de Identity por defecto
using LogisticaBroker.Services;
// --- FIN DEL CAMBIO ---


namespace LogisticaBroker.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        // Esta interfaz ahora apunta a LogisticaBroker.Services.IEmailSender
        private readonly IEmailSender _emailSender;

        // El constructor se actualiza para recibir nuestro servicio
        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null) // Se quita la comprobación de IsEmailConfirmedAsync para permitir restablecer aunque no se haya confirmado.
                {
                    // No revelar que el usuario no existe. Siempre redirigir a la página de confirmación.
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                // --- INICIO DEL CAMBIO ---
                // Personalizamos el mensaje del correo
                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Restablecer tu contraseña", // Asunto en español
                    $"Hola,<br><br>Hemos recibido una solicitud para restablecer la contraseña de tu cuenta. Por favor, haz clic en el siguiente enlace para continuar:<br><br>" +
                    $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Restablecer mi contraseña</a>.<br><br>" +
                    "Si no solicitaste esto, puedes ignorar este correo de forma segura.<br><br>" +
                    "Saludos,<br>" +
                    "El equipo de LogisticaBroker");
                // --- FIN DEL CAMBIO ---

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
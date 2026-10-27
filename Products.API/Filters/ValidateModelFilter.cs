using Microsoft.AspNetCore.Mvc.Filters;
using Products.API.Exceptions;

namespace Products.API.Filters
{
    // Reemplaza la respuesta 400 automática de [ApiController] para que los errores
    // de validación pasen por ValidationExceptionHandler con el código PRD-002.
    public class ValidateModelFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid) return;

            var errores = new List<string>();
            foreach (var (key, entry) in context.ModelState)
            {
                foreach (var error in entry.Errors)
                {
                    // Errores de formato del JSON: vienen con clave "$.campo" y mensaje técnico en inglés
                    if (key.StartsWith("$"))
                        errores.Add(key.Length > 2
                            ? $"El campo '{key[2..]}' tiene un formato inválido"
                            : "El cuerpo de la request no es un JSON válido");
                    // "request" (o vacío) es el body completo; solo aporta si no hay otro error más preciso
                    else if (key is "request" or "")
                        continue;
                    else
                        errores.Add(error.ErrorMessage.TrimEnd('.'));
                }
            }

            if (errores.Count == 0)
                errores.Add("El cuerpo de la request es requerido");

            // Los problemas se listan separados por punto y coma (sección 3.1)
            throw new ValidationException(ErrorCodes.PRD_002,
                $"Los datos del producto son inválidos: {string.Join("; ", errores.Distinct())}.");
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}

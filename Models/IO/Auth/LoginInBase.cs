using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Api.Models.IO.Auth
{
	public class LoginInBase : IValidatableObject
	{
		[Required]
		[StringLength(30, MinimumLength = 5)]
		public string Username { get; set; }
		[Required]
		[StringLength(30, MinimumLength = 8)]
		public string Password { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (Username.Any(x => char.IsWhiteSpace(x)))
			{
				yield return new ValidationResult(null, new[] { nameof(Username) });
			}

			if (!Password.Any(x => char.IsLower(x))
				|| !Password.Any(x => char.IsUpper(x))
				|| !Password.Any(x => char.IsDigit(x))
				|| Password.Any(x => char.IsWhiteSpace(x))
			)
			{
				yield return new ValidationResult(null, new[] { nameof(Password) });
			}
			yield break;
		}
	}
}

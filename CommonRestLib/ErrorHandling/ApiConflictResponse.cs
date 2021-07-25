using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonRestLib.ErrorHandling
{
    public class ApiConflictResponse : ApiResponse
    {
        public IEnumerable<string> Errors { get; }

        public ApiConflictResponse(string text) : base(409, text)
        {
        }
        public ApiConflictResponse(ModelStateDictionary modelState)
            : base(409)
        {
            if (modelState.IsValid)
            {
                throw new ArgumentException("ModelState must be invalid", nameof(modelState));
            }

            Errors = modelState.SelectMany(x => x.Value.Errors)
                .Select(x => x.ErrorMessage).ToArray();
        }
    }
}

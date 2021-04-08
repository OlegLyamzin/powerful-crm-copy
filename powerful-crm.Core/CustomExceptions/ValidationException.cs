﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace powerful_crm.Core.CustomExceptions
{
    public class ValidationException : Exception
    {
        public int StatusCode { get; private set; }
        public string ErrorMessage { get; private set; }

        public ValidationException(ModelStateDictionary modelState)
        {
            StatusCode = (int)HttpStatusCode.Conflict;
            ErrorMessage = ""; 
            foreach (var state in modelState)
            {
                ErrorMessage += $"Invalid format {state.Key}{System.Environment.NewLine}";
            }
        }
    }
}

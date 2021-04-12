﻿using Microsoft.AspNetCore.Http;
using powerful_crm.Core;
using powerful_crm.Core.CustomExceptions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

namespace powerful_crm.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private const string GlobalErrorMessage = "An error occured while processing the request.";
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (ValidationException ex)
            {
                await HandleValidationExceptionAsync(httpContext, ex);
            }
            catch (SqlException ex)
            {
                await HandleSqlExceptionAsync(httpContext, ex);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
        {
            ModifyContextResponse(context, exception.StatusCode);

            return ConstructResponse(context, exception.StatusCode, exception.ErrorMessage);
        }
        private Task HandleSqlExceptionAsync(HttpContext context, SqlException exception)
        {
            ModifyContextResponse(context, (int)HttpStatusCode.BadRequest);
            var keys = new string[] { Constants.EMAIL_UNIQUE_CONSTRAINT, Constants.EMAIL_UNIQUE_CONSTRAINT };
            var result = keys.FirstOrDefault<string>(s => exception.Message.Contains(s));
            return result switch
            {
                Constants.LOGIN_UNIQUE_CONSTRAINT => ConstructResponse(context, 409, "This login is already in use."),
                Constants.EMAIL_UNIQUE_CONSTRAINT => ConstructResponse(context, 409, "This email is already in use."),
                _ => ConstructResponse(context, 400, GlobalErrorMessage),
            };
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            ModifyContextResponse(context, (int)HttpStatusCode.BadRequest);

            return ConstructResponse(context, (int)HttpStatusCode.BadRequest, GlobalErrorMessage);
        }

        private void ModifyContextResponse(HttpContext context, int statusCode)
        {
            context.Response.ContentType = MediaTypeNames.Application.Json;
            context.Response.StatusCode = statusCode;
        }

        private Task ConstructResponse(HttpContext context, int statusCode, string message)
        {
            var errorResponse = JsonSerializer.Serialize(
                new
                {
                    Code = statusCode,
                    Message = message
                });

            return context.Response.WriteAsync(errorResponse);
        }
    }
}
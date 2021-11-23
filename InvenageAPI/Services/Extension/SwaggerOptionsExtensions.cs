using InvenageAPI.Services.Constant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace InvenageAPI.Services.Extension
{
    public static class SwaggerOptionsExtensions
    {
        public static void GetDefaultSwaggerDoc(this SwaggerGenOptions c, IConfiguration config)
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

            c.SwaggerDoc("v1",
                new OpenApiInfo
                {
                    Title = "Invenage API",
                    Version = "v1",
                    Description = "This is the core functions API for project invenage.",
                    Contact = new()
                    {
                        Name = "Invenage",
                        Email = string.Empty,
                        Url = new Uri("https://www.invenage.com"),
                    },
                    License = new()
                    {
                        Name = "GNU General Public License v3.0",
                        Url = new Uri("https://github.com/invenage/invenage-api/blob/main/LICENSE"),
                    }
                });

            OpenApiSecurityScheme clientAccessScheme = new()
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Invenage API Access Client",
                },
                Description = "This is the Client Id required when you sending CORS request from external.",
                In = ParameterLocation.Header,
                Name = "client_id",
                Type = SecuritySchemeType.ApiKey,
            };
            OpenApiSecurityScheme apiAccessScheme = new()
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Invenage API Access Key",
                },
                Description = "This is the API Key required when you sending CORS request from external.",
                In = ParameterLocation.Header,
                Name = "apiKey",
                Type = SecuritySchemeType.ApiKey,
            };

            var scopes = AccessScope.GetScopesList().ToDictionary(x => x, x => "");

            var oAuthConfig = new OAuthConfig();
            config.GetSection("OAuth2").Bind(oAuthConfig);

            OpenApiSecurityScheme apiAuthScheme = new()
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Invenage API Authorization",
                },
                Description = "This is the Auth Key required when you operation restricted endpoint for user identification.",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(oAuthConfig.AuthorizationUrl),
                        RefreshUrl = new Uri(oAuthConfig.RefreshUrl),
                        TokenUrl = new Uri(oAuthConfig.TokenUrl),
                        Scopes = scopes
                    },
                    ClientCredentials = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(oAuthConfig.AuthorizationUrl),
                        TokenUrl = new Uri(oAuthConfig.TokenUrl),
                        Scopes = scopes
                    }
                }
            };

            c.AddSecurityDefinition("Invenage API Access Client", clientAccessScheme);
            c.AddSecurityDefinition("Invenage API Access Key", apiAccessScheme);
            c.AddSecurityDefinition("Invenage API Authorization", apiAuthScheme);

            c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
                { clientAccessScheme, new[] { "Invenage API Access Client" }},
                { apiAccessScheme, new[] { "Invenage API Access Key" }},
                { apiAuthScheme, new[] { "Invenage API Authorization" }}
            });
        }

        private class OAuthConfig
        {
            public string AuthorizationUrl { get; set; }
            public string RefreshUrl { get; set; }
            public string TokenUrl { get; set; }
        }
    }
}

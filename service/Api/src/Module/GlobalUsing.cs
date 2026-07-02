global using Carter;

global using FluentValidation;

global using MediatR;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Routing;
global using Microsoft.Extensions.Logging;

global using Shared.Application.Extensions.Results;
global using Shared.Application.Mediators.Commands;
global using Shared.Application.Mediators.Queries;
global using Shared.Application.Mappings;
global using Shared.Application.Models.Results;
global using Shared.Application.Models.Errors;
global using Shared.Operational.Persistence.Seeders;
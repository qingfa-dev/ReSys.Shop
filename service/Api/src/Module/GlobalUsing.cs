global using Carter;

global using FluentValidation;

global using MediatR;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Routing;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Logging;

global using Shared.Application.Extensions.Results;
global using Shared.Application.Mappings;
global using Shared.Application.Mediators.Commands;
global using Shared.Application.Mediators.Queries;
global using Shared.Application.Models.Errors;
global using Shared.Application.Models.Parameters;
global using Shared.Application.Models.Responses;
global using Shared.Application.Models.Results;
global using Shared.Application.Systems.SystemDateTimes;
global using Shared.Operational.Persistence.Data;
global using Shared.Operational.Persistence.Seeders;
global using Shared.Operational.Persistence.Specifications.Querying;
global using Shared.Security.Authentication.Contexts.Services;
global using Shared.Governance.Conventions;
global using Shared.Security.Authorization.Attributes;
global using Shared.Security.Authorization.Features;
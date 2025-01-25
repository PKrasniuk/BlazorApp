using System.Collections.Generic;
using BlazorApp.Common.Constants;
using BlazorApp.Common.Models.Security;
using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace BlazorApp.DAL.Infrastructure.Configurations;

public class IdentityServerConfiguration
{
    public static IEnumerable<IdentityResource> GetIdentityResources()
    {
        return new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Phone(),
            new IdentityResources.Email(),
            new(ScopeConstants.Roles, new List<string> { JwtClaimTypes.Role })
        };
    }

    public static IEnumerable<ApiResource> GetApiResources()
    {
        return new List<ApiResource>
        {
            new(CommonConstants.ApiName)
            {
                UserClaims =
                {
                    JwtClaimTypes.Name,
                    JwtClaimTypes.Email,
                    JwtClaimTypes.PhoneNumber,
                    JwtClaimTypes.Role,
                    ClaimConstants.Permission,
                    Policies.IsUser,
                    Policies.IsAdmin
                }
            }
        };
    }

    public static IEnumerable<Client> GetClients()
    {
        return new List<Client>
        {
            new()
            {
                AccessTokenType = AccessTokenType.Jwt,
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                AllowAccessTokensViaBrowser = true,
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Phone,
                    IdentityServerConstants.StandardScopes.Email,
                    ScopeConstants.Roles,
                    CommonConstants.ApiName
                },
                AllowRememberConsent = true,
                AllowOfflineAccess = true,
                ClientId = CommonConstants.AppClientId,
                ClientName = CommonConstants.ApiName,
                ClientSecrets = new List<Secret> { new() { Value = CommonConstants.AppName.Sha512() } },
                Enabled = true,
                RequireClientSecret = true,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                RefreshTokenUsage = TokenUsage.OneTimeOnly
            },
            new()
            {
                ClientId = CommonConstants.SwaggerClientId,
                ClientName = "Swagger UI",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                AllowAccessTokensViaBrowser = true,
                RequireClientSecret = false,
                AllowedScopes =
                {
                    CommonConstants.ApiName
                }
            }
        };
    }
}
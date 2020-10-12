using BlazorApp.Common.Constants;
using BlazorApp.Common.Models.Security;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace BlazorApp.DAL.Infrastructure.Configurations
{
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
                new IdentityResource(ScopeConstants.Roles, new List<string> {JwtClaimTypes.Role})
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource(CommonConstants.ApiName)
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
                new Client
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
                    ClientSecrets = new List<Secret> {new Secret {Value = CommonConstants.AppName.Sha512()}},
                    Enabled = true,
                    RequireClientSecret = true,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly
                },
                new Client
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
}
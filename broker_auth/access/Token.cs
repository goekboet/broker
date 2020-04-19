using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace cli
{
    public static class Token
    {
        static string RandomRwd => new string(
                Guid.NewGuid().ToString()
                    .Where(x => x != '-')
                    .Take(16)
                    .ToArray());
        public static async Task<Access> FetchToken(
            ServiceProvider sp,
            long now,
            Creds creds
        )
        {
            using var scope = sp.CreateScope();
            using var usrmgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser()
            {
                Id = Guid.NewGuid().ToString(),
                UserName = Guid.NewGuid().ToString(),
                Email = creds.Email
            };
            await usrmgr.CreateAsync(user, creds.Password);
            
            var idpclient = new HttpClient();
            var idpreply = await idpclient.RequestPasswordTokenAsync(
                new PasswordTokenRequest
                {
                    Address = "https://ids.ego/connect/token",

                    ClientId = "dev",
                    ClientSecret = "dev",
                    Scope = "openid bookings publish offline_access",

                    UserName = creds.Email,
                    Password = creds.Password
                });

            if (idpreply.IsError)
            {
                throw new Exception(idpreply.Error, idpreply.Exception);
            }

            return new Access
            {
                Accesstoken = idpreply.AccessToken,
                Refreshtoken = idpreply.RefreshToken,
                ValidTo = now + (idpreply.ExpiresIn * 1000)
            };
        }

        static async Task<Access> Refresh(Access a)
        {
            var client = new HttpClient();
            var idpreply = await client.RequestRefreshTokenAsync(
                new RefreshTokenRequest
                {
                    RefreshToken = a.Refreshtoken,
                    Scope = "openid bookings publish offline_access",
                    Address = "https://ids.ego/connect/token",

                    ClientId = "dev",
                    ClientSecret = "dev"
                }
            );

            if (idpreply.IsError)
            {
                throw new Exception(idpreply.Error, idpreply.Exception);
            }

            return new Access
            {
                Accesstoken = idpreply.AccessToken,
                Refreshtoken = idpreply.RefreshToken,
                ValidTo = DateTimeOffset.Now.AddSeconds(idpreply.ExpiresIn).ToUnixTimeMilliseconds()
            };
        }

       

        public static async Task<Access> GetAccess(
            ServiceProvider sp,
            long now,
            Creds creds)
        {
            var access = await TokenStore.Read(creds.Email);
            if (access == null)
            {
                access = await FetchToken(sp, now, creds);
                await TokenStore.Write(creds.Email, access);
            }

            var refreshThreshold = DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(5));
            if (access.ValidTo < refreshThreshold.ToUnixTimeMilliseconds())
            {
                access = await Refresh(access);
                await TokenStore.Write(creds.Email, access);
            }

            return access;
        }
    }
}
using System;
using System.Linq;

namespace endpointTests
{
    public class Creds
    {
        public static string Random => new string(
            Guid.NewGuid().ToString()
                .Where(x => x != '-')
                .Take(16)
                .ToArray());

        public static Creds RandomUser() => new Creds(
            $"{Random}@byappt.com",
            Random
        );

        private Creds(
            string email, 
            string pwd)
        {
            Email = email;
            Password = pwd;
        }
        public string Email {get;}
        public string Password {get;}
    }
}
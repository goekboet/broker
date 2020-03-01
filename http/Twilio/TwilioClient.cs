using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twilio.Jwt.AccessToken;

namespace http.Twilio
{
    public static class TwilioToken
    {
        public static string GetTwilioToken(
            this TwilioOptions opts,
            string name,
            string host,
            long start
        )
        {
            var grant = new VideoGrant() { Room = $"{host}/{start}" };
            var grants = new HashSet<IGrant> { grant };

            var nbf = DateTimeOffset.FromUnixTimeMilliseconds(start).Subtract(TimeSpan.FromMinutes(5));

            var token = new Token(
                accountSid: opts.AccountSid,
                signingKeySid: opts.ApiKeySid,
                secret: opts.ApiKeySecret,
                identity: name,
                expiration: nbf.AddMinutes(30).DateTime,
                nbf: nbf.DateTime,
                grants: grants);

            return token.ToJwt();
        }
    }
}
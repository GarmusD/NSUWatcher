using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher.Interfaces.NsuUsers;
using NSUWatcher.Services.NSUWatcherNet.NetMessenger.Processors;
using System.Collections.Generic;

namespace Tests.JWT
{
    [TestClass]
    public class Jwt
    {
        private const string Issuer = "NSUWatcher";
        private const string Audience = "nsuwatcher";
        private const string Secret = "nsuwatcher_secret";
        private const string UserName = "testUserName";

        [TestMethod]
        public void EncodeAndDecode()
        {
            var token = SysMsgProcessor.CreateJWT(new NsuUser() { Id = 1, UserName = UserName, UserType = NsuUserType.Admin }, Issuer, Audience, Secret);
            Assert.IsNotNull(token);
            var decodedToData = SysMsgProcessor.DecodeJWT(token, Issuer, Audience, Secret);
            Assert.IsNotNull(decodedToData);
            var jwtData = decodedToData.Value;
            Assert.AreEqual(Issuer, jwtData.Issuer);
            Assert.AreEqual(Audience, jwtData.Audience);
            Assert.AreEqual(1, jwtData.UserId);
            Assert.AreEqual(UserName, jwtData.Username);
            Assert.AreEqual((int)NsuUserType.Admin, jwtData.UserType);
        }

        [TestMethod]
        public void DecodeRandomString()
        {
            var decoded = SysMsgProcessor.DecodeJWT("just any random string", Issuer, Audience, Secret);
            Assert.IsNull(decoded);
        }

        private class NsuUser : INsuUser
        {
            public int Id { get; set; }
            public bool Enabled { get; set; }

            public string UserName { get; set;}

            public string Password { get; set;}

            public NsuUserType UserType { get; set;}

            public ICollection<string> Permissions { get; set;}
        }
    }
}

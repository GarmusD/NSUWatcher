using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher.Interfaces.NsuUsers;
using NSUWatcher.NSUWatcherNet.NetMessenger;
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
            var decoded = SysMsgProcessor.DecodeJWT(token, Issuer, Audience, Secret);
            Assert.IsNotNull(decoded);
            Assert.AreEqual(Issuer, (string)decoded["iss"]);
            Assert.AreEqual(Audience, (string)decoded["aud"]);
            Assert.AreEqual(1, (int)(long)decoded["userid"]);
            Assert.AreEqual(UserName, (string)decoded["username"]);
            Assert.AreEqual((int)NsuUserType.Admin, (int)(long)decoded["usertype"]);
        }

        private class NsuUser : INsuUser
        {
            public int Id { get; set; }

            public string UserName { get; set;}

            public string Password { get; set;}

            public NsuUserType UserType { get; set;}

            public ICollection<string> Permissions { get; set;}
        }
    }
}

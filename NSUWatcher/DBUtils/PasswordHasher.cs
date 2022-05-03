using System;
using System.Security.Cryptography;

namespace NSUWatcher.DBUtils
{
	public static class PasswordHasher
	{
		private static readonly byte[] salt = {0x94 ,0xa0 ,0x28 ,0xd9 ,0xf2 ,0xab ,0xff ,0xd2 ,0xd7 ,0x32 ,0x72 ,0x27 ,0x2f ,0xac ,0xb0 ,0x94 ,0x87 ,0x2b ,0x95};
		// 24 = 192 bits
		private const int SaltByteSize = 24;
		private const int HashByteSize = 24;
		private const int HasingIterationsCount = 10101;

		public static string ComputeHash(string password, int iterations = HasingIterationsCount, int hashByteSize = HashByteSize)
		{
			var hashGenerator = new Rfc2898DeriveBytes(password, salt);
			//hashGenerator.IterationCount = iterations;
			var bytes = hashGenerator.GetBytes(hashByteSize);
			return Convert.ToBase64String(bytes);
		}

		public static byte[] GenerateSalt(int saltByteSize = SaltByteSize)
		{
			var saltGenerator = new RNGCryptoServiceProvider();
			byte[] saltl = new byte[saltByteSize];
			saltGenerator.GetBytes(saltl);
			return saltl;
		}

		public static bool VerifyPassword(string password, string passwordHash)
		{
			string computedHash = ComputeHash(password);
			return AreHashesEqual(computedHash, passwordHash);
		}

		//Length constant verification - prevents timing attack
		private static bool AreHashesEqual(string firstHash, string secondHash)
		{
			return firstHash.Equals(secondHash, StringComparison.Ordinal);
			/*
			int minHashLenght = firstHash.Length <= secondHash.Length ? firstHash.Length : secondHash.Length;
			var xor = firstHash.Length ^ secondHash.Length;
			for(int i = 0; i < minHashLenght; i++)
				xor |= firstHash[i] ^ secondHash[i];
			return 0 == xor;
			*/
		}
	}
}
namespace Backend
{
    using System;
    using BCrypt.Net;
    using Backend.Models;
    using System.Threading.Tasks;
    using Backend.Controllers;

    internal class Program
    {
        public static async Task Main(string[] args)
        {   
            PassController passCon = new PassController();
            AccountController accountCon = new AccountController();
            // Use program for non-static methods
            var program = new Program();
            string pass = BCrypt.HashPassword("password");
            var accCreated = await accountCon.CreateAccount("Gweppysb", pass);
            Console.WriteLine(accCreated);

            byte[] key = accountCon.Login("Gweppysb", "password").Result;
            Console.WriteLine(Convert.ToBase64String(key));
            string addPassVerify = passCon.AddPassword("Gweppysb", "password", key, "www.booble.com", "username", "awagga@beemail", "myPassword").Result;
            var encPasswords = passCon.GetPasswords("Gweppysb", "password").Result; 
            foreach (var entry in encPasswords)
            {
                Console.WriteLine(entry);
            }
            var decPasswords = passCon.DecryptPasswords(key, encPasswords); 
            foreach (DecryptedPassword decPass in decPasswords)
            {
                Console.WriteLine($"Location: {decPass.plaintextLocation}");
                Console.WriteLine($"Username: {decPass.plaintextUsername}");
                Console.WriteLine($"Email: {decPass.plaintextEmail}");
                Console.WriteLine($"Password: {decPass.plaintextIVPass}");
            }
        }

        // TODO: Method for storing lockbox key and passwords in cache  
    }
}
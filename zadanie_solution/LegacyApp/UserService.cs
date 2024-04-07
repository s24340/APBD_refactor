using System;

namespace LegacyApp
{
    public class UserService
    {
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!IsValidName(firstName, lastName) || !IsValidEmail(email) || !IsAdult(dateOfBirth))
            {
                return false;
            }
        
            var client = GetClientById(clientId);
            if (client == null)
            {
                return false;
            }
        
            var user = CreateUser(firstName, lastName, email, dateOfBirth, client);
        
            AdjustCreditLimit(user, client);
        
            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }
        
            UserDataAccess.AddUser(user);
            return true;
        }
        
        private bool IsValidName(string firstName, string lastName)
        {
            return !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName);
        }
        
        private bool IsValidEmail(string email)
        {
            return email.Contains("@") && email.Contains(".");
        }
        
        private bool IsAdult(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
            {
                age--;
            }
            return age >= 21;
        }
        
        private Client GetClientById(int clientId)
        {
            var clientRepository = new ClientRepository();
            return clientRepository.GetById(clientId);
        }
        
        private User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, Client client)
        {
            return new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
        }
        
        private void AdjustCreditLimit(User user, Client client)
        {
            using (var userCreditService = new UserCreditService())
            {
                int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
        
                switch (client.Type)
                {
                    case "VeryImportantClient":
                        user.HasCreditLimit = false;
                        break;
                    case "ImportantClient":
                        creditLimit *= 2;
                        goto default;
                    default:
                        user.HasCreditLimit = true;
                        user.CreditLimit = creditLimit;
                        break;
                }
            }
        }

    }
}

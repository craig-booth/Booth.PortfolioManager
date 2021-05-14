using System;

using Booth.EventStore;
using Booth.EventStore.MongoDB;
using Booth.PortfolioManager.Domain.Users;
using Booth.PortfolioManager.Repository;

namespace Booth.PortfolioManager.DataMigration
{
    class Program
    {
        static void Main(string[] args)
        {
            MigrateUsers();

            Console.WriteLine("Hello World!");
        }

        static void MigrateUsers()
        {
            // Load users from Event Store
            var eventStore = new MongodbEventStore("mongodb://portfolio.boothfamily.id.au:27017", "PortfolioManager");
            var eventStream = eventStore.GetEventStream<User>("Users");
            var eventRepository = new Repository<User>(eventStream);

            var userRepository = new UserRepository("mongodb://portfolio.boothfamily.id.au:27017", "PortfolioManager2");

            foreach (var user in eventRepository.All())
            {                
                var newUser = new User(user.Id);
                newUser.Create2(user.UserName, user.Password);
                if (user.Administator)
                    newUser.AddAdministratorPrivilage();

                userRepository.Add(newUser);


                newUser.RemoveAdministratorPrivilage();
                newUser.AddAdministratorPrivilage();
                userRepository.Update(newUser);
            }


        }



    }
}

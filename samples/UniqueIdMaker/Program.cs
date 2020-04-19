using System;
using Unique.CSharp;

namespace UniqueIdMaker
{
    class Program
    {
        static void Main(string[] args)
        {
            // Use the pre-defined DNS namespace
            Guid uniqueIdForDns = NamedGuid.NewGuid(SpaceId.DNS, "example.com");
            Console.WriteLine($"DNS example.com ID is {uniqueIdForDns}");
            // OUTPUT: DNS example.com ID is cfbff0d1-9375-5685-968c-48ce8b15ae17

            // Use a custom namespace Guid
            Guid customGuid = Guid.Parse("AA0F4712-691F-4C72-B5EC-19730324EAFD");
            Guid uniqueIdForCustomSpace = NamedGuid.NewGuid(customGuid, "bob@builder.com");
            Console.WriteLine($"Custom bob@builder.com ID is {uniqueIdForCustomSpace}");
            // OUTPUT: Custom bob@builder.com ID is dbead1ff-3f86-5d73-b577-cb00ee3fccaf

            // Get the version of any Guid
            int version = NamedGuid.Version(uniqueIdForCustomSpace);
            Console.WriteLine($"Version is {version}");
            // OUTPUT: Version is 5
            // Since the default hash algorithm is SHA-1 the version is always 5
            // Use Unique.NamedGuid.newGuid to define the hash algorithm.

            version = NamedGuid.Version(Guid.NewGuid());
            Console.WriteLine($"Version is {version}");
            // OUTPUT: Version is 4

            Console.WriteLine("Press ENTER to exit.");
            Console.ReadLine();
        }
    }
}
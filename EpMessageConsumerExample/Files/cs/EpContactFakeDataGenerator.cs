namespace EP.EpMessageConsumerExample
{
    using System;
    using System.Collections.Generic;
    using Bogus;
    using Bogus.DataSets;
    using BPMSoft.Core;
    using BPMSoft.Core.Entities;
    using BPMSoft.Core.Factories;
    using Common.Logging;

    [DefaultBinding(typeof(IEpContactFakeDataGenerator))]
    internal class EpContactFakeDataGenerator : IEpContactFakeDataGenerator
    {
        private const string FallbackLocale = "en";

        private static readonly ILog s_logger = LogManager.GetLogger(nameof(EpContactFakeDataGenerator));

        private static readonly IReadOnlyDictionary<Name.Gender, Guid> s_genderMap = new Dictionary<Name.Gender, Guid>
        {
            { Name.Gender.Male, EpContactConsts.MaleGenderId },
            { Name.Gender.Female, EpContactConsts.FemaleGenderId },
        };

        public IReadOnlyList<Entity> Generate(UserConnection userConnection, int count)
        {
            if (userConnection == null)
                throw new ArgumentNullException(nameof(userConnection));
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count,
                    "Count must be greater than zero.");
            }

            EntitySchema schema = userConnection.EntitySchemaManager.GetInstanceByName(EpContactConsts.SchemaName);
            string primaryColumnValueName = schema.PrimaryColumn.ColumnValueName;
            string locale = ResolveLocale(userConnection.CurrentUser.Culture.TwoLetterISOLanguageName);
            Faker<Entity> faker = new Faker<Entity>(locale)
                .CustomInstantiator(f =>
                {
                    Entity entity = schema.CreateEntity(userConnection);
                    entity.SetDefColumnValue(primaryColumnValueName);
                    return entity;
                })
                .Rules((f, entity) =>
                {
                    _ = entity.SetColumnValue("GenderId", s_genderMap[f.Person.Gender]);
                    _ = entity.SetColumnValue("GivenName", f.Person.FirstName);
                    _ = entity.SetColumnValue("Surname", f.Person.LastName);
                    _ = entity.SetColumnValue("BirthDate", f.Person.DateOfBirth);
                    _ = entity.SetColumnValue("Email", f.Person.Email);
                    _ = entity.SetColumnValue("Phone", f.Person.Phone);
                    _ = entity.SetColumnValue("Confirmed", false);
                });
            return faker.Generate(count);
        }

        private static string ResolveLocale(string locale)
        {
            if (Bogus.Database.LocaleResourceExists(locale))
                return locale;
            s_logger.Warn($"Bogus has no data for locale '{locale}', falling back to '{FallbackLocale}'");
            return FallbackLocale;
        }
    }
}

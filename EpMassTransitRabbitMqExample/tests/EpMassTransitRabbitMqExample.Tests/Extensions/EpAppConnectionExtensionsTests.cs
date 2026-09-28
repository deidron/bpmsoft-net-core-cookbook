namespace EP.EpMassTransitRabbitMqExample.Tests
{
    using System;
    using System.Configuration;
    using System.IO;
    using Xunit;

    public sealed class EpAppConnectionExtensionsTests : IDisposable
    {
        private const string Name = EpAppConnectionExtensions.RabbitMqConnectionStringName;

        private readonly string _path = Path.Combine(Path.GetTempPath(), $"ep-{Guid.NewGuid():N}.config");

        public void Dispose() => File.Delete(_path);

        [Fact]
        public void ConnectionStringBecomesUri()
        {
            System.Configuration.Configuration configuration =
                Load($"<add name=\"{Name}\" connectionString=\"amqp://user:secret@localhost/vhost?heartbeat=20\" />");

            Uri address = configuration.GetConnectionStringUri(Name);

            Assert.Equal(new Uri("amqp://user:secret@localhost/vhost?heartbeat=20"), address);
        }

        [Fact]
        public void MissingConnectionStringThrowsWithItsName()
        {
            System.Configuration.Configuration configuration = Load(string.Empty);

            ConfigurationErrorsException exception = Assert.Throws<ConfigurationErrorsException>(
                () => configuration.GetConnectionStringUri(Name));

            Assert.Contains($"'{Name}'", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void EmptyConnectionStringThrows()
        {
            System.Configuration.Configuration configuration = Load($"<add name=\"{Name}\" connectionString=\" \" />");

            _ = Assert.Throws<ConfigurationErrorsException>(() => configuration.GetConnectionStringUri(Name));
        }

        [Fact]
        public void InvalidArgumentsAreRejected()
        {
            System.Configuration.Configuration configuration = Load(string.Empty);

            _ = Assert.Throws<ArgumentNullException>(() => ((System.Configuration.Configuration)null).GetConnectionStringUri(Name));
            _ = Assert.ThrowsAny<ArgumentException>(() => configuration.GetConnectionStringUri(string.Empty));
        }

        private System.Configuration.Configuration Load(string entries)
        {
            File.WriteAllText(
                _path,
                $"<?xml version=\"1.0\" encoding=\"utf-8\"?><configuration><connectionStrings>{entries}</connectionStrings></configuration>");
            return ConfigurationManager.OpenMappedExeConfiguration(
                new ExeConfigurationFileMap { ExeConfigFilename = _path }, ConfigurationUserLevel.None);
        }
    }
}

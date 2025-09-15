using System.Reactive.Subjects;
using FluentAssertions;
using Idasen.BluetoothLE.Linak;
using Idasen.BluetoothLE.Linak.Interfaces;
using NSubstitute;
using Serilog;

namespace Idasen.BluetoothLE.Linak.Tests
{
    [TestClass]
    public class ErrorManagerTests
    {
        [TestMethod]
        public void Publish_ForValidDetails_PushesToSubject()
        {
            var logger = Substitute.For<ILogger>();
            var subject = Substitute.For<ISubject<IErrorDetails>>();
            var sut = new ErrorManager(logger, subject);
            var details = Substitute.For<IErrorDetails>();

            sut.Publish(details);

            subject.Received(1).OnNext(details);
        }

        [TestMethod]
        public void PublishForMessage_ForValidMessage_PushesConstructedDetails()
        {
            var logger = Substitute.For<ILogger>();
            var subject = Substitute.For<ISubject<IErrorDetails>>();
            var sut = new ErrorManager(logger, subject);

            sut.PublishForMessage("message", caller: "caller");

            subject.Received(1)
                   .OnNext(Arg.Is<IErrorDetails>(d => d.Message == "message" && d.Caller == "caller"));
        }

        [TestMethod]
        public void ErrorChanged_ReturnsSubject()
        {
            var logger = Substitute.For<ILogger>();
            var subject = Substitute.For<ISubject<IErrorDetails>>();
            var sut = new ErrorManager(logger, subject);

            sut.ErrorChanged.Should().Be(subject);
        }
    }
}

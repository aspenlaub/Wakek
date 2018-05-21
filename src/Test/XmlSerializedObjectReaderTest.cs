using System;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test {
    [TestClass]
    public class XmlSerializedObjectReaderTest {
        [TestMethod]
        public void CanReadSerializedObject() {
            var componentProvider = new ComponentProvider();
            var sut = new XmlSerializedObjectReader(componentProvider);
            bool success;
            Type t;
            sut.IdentifyType("", out success, out t);
            Assert.IsFalse(success);
            sut.IdentifyType("<?xml", out success, out t);
            Assert.IsFalse(success);
            var objects = new object[] {
                new BenchmarkExecution { SequenceNumber = 24, BenchmarkDefinitionGuid = Guid.NewGuid().ToString() },
                new BenchmarkDefinition { BenchmarkExecutionType = BenchmarkExecutionType.CsNative, Description = "Not a benchmark", ExecutionTimeInSeconds = 24, Guid = Guid.NewGuid().ToString(), NumberOfCallsInParallel = 7, Url = "https://www.aspenlaub.net"}
            };
            var serializedObjects = new[] {
                componentProvider.XmlSerializer.Serialize(objects[0] as BenchmarkExecution),
                componentProvider.XmlSerializer.Serialize(objects[0] as BenchmarkDefinition)
            };

            for(var i = 0; i < objects.Length; i ++) {
                sut.IdentifyType(serializedObjects[i], out success, out t);
                Assert.IsTrue(success);
                Assert.AreEqual(objects[i].GetType(), t);
            }
        }
    }
}

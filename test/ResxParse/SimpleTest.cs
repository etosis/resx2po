using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace etosis.resx2po.test.ResxParse
{
    [TestClass]
    public class SimpleTest
    {
        [TestMethod]
        public void Parse()
        {
            ResxFile resx = ResxFile.Parse("ResxParse\\Simple.resx", null, new LanguageInfo("en"));
            Assert.AreEqual(1, resx.Strings.Count());
            Assert.AreEqual("Value0", resx["Id0"].Value);
            Assert.AreEqual("Comment0", resx["Id0"].Comment);
        }
    }
}

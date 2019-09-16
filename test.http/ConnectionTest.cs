using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using postgres;

namespace test.db
{
    [TestClass]
    public class ConnectionTest
    {
        public static PgresUser Broker => new PgresUser(
                "localhost",
                "5432",
                "broker",
                "trtLAqkGY3nE3DyA",
                "meets"
            );

        [TestMethod]
        public async Task GetTimes()
        {
            var r = 0;
            using (var c = Broker.ToConnection())
            {
                var cmd = c.SelectUnbookedTimesInWindow(
                    Guid.Parse("100c899e-d945-4bfc-95ef-891587ae686b"),
                    1567980000000,
                    1568584800000
                );

                var rs = await cmd.SubmitQuery(PGres.ToMeet);
                r = rs.Count();
            }

            Assert.IsTrue(r > 0);
        }

        [TestMethod]
        public async Task GetHosts()
        {
            var r = 0;
            using (var c = Broker.ToConnection())
            {
                var cmd = c.ListHosts();

                var rs = await cmd.SubmitQuery(PGres.ToHost);
                r = rs.Count();
            }

            Assert.IsTrue(r > 0);
        }
    }
}

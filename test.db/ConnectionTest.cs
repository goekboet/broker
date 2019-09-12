using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using postgres;

namespace test.db
{
    [TestClass]
    public class ConnectionTest
    {
        [TestMethod]
        public async Task TestConnection()
        {
            var user = new PgresUser(
                "localhost",
                "5432",
                "broker",
                "trtLAqkGY3nE3DyA",
                "meets"
            );

            var r = 0;
            using (var c = user.ToConnection())
            {
                var cmd = c.SelectUnbookedTimesInWindow(
                    Guid.Parse("100c899e-d945-4bfc-95ef-891587ae686b"),
                    1567980000000,
                    1568584800000
                );

                var rs = await cmd.GetResults(PGres.ToMeet);
                r = rs.Count();
            }

            Assert.IsTrue(r > 0);
        }
    }
}

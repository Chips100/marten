using System;
using Marten.Testing.CoreFunctionality;
using Marten.Testing.Documents;
using Marten.V4Internals;
using Xunit;
using Shouldly;

namespace Marten.Testing.V4Internals
{
    public class VersionTrackerTests
    {
        private StringDoc stringDoc = new StringDoc {Id = "bar"};
        private IntDoc intDoc = new IntDoc{Id = 4};
        private LongDoc longDoc = new LongDoc {Id = 5};
        private GuidDoc guidDoc = new GuidDoc {Id = Guid.NewGuid()};
        private VersionTracker theTracker = new VersionTracker();

        [Fact]
        public void can_get_version_when_it_is_empty_always_null()
        {
            theTracker.VersionFor<StringDoc, string>(stringDoc.Id)
                .ShouldBeNull();

            theTracker.VersionFor<IntDoc, int>(intDoc.Id)
                .ShouldBeNull();

            theTracker.VersionFor<LongDoc, long>(longDoc.Id)
                .ShouldBeNull();

            theTracker.VersionFor<GuidDoc, Guid>(guidDoc.Id)
                .ShouldBeNull();
        }

        [Fact]
        public void store_and_retrieve()
        {
            var intVersion = Guid.NewGuid();
            var stringVersion = Guid.NewGuid();
            theTracker.StoreVersion<IntDoc, int>(intDoc.Id, intVersion);
            theTracker.StoreVersion<StringDoc, string>(stringDoc.Id, stringVersion);

            theTracker.VersionFor<IntDoc, int>(intDoc.Id)
                .ShouldBe(intVersion);

            theTracker.VersionFor<StringDoc, string>(stringDoc.Id)
                .ShouldBe(stringVersion);
        }

        [Fact]
        public void override_the_version()
        {
            var intVersion = Guid.NewGuid();
            var intVersion2 = Guid.NewGuid();
            theTracker.StoreVersion<IntDoc, int>(intDoc.Id, intVersion);
            theTracker.StoreVersion<IntDoc, int>(intDoc.Id, intVersion2);

            theTracker.VersionFor<IntDoc, int>(intDoc.Id)
                .ShouldBe(intVersion2);

        }

        [Fact]
        public void store_and_then_clear()
        {
            var intVersion = Guid.NewGuid();
            var stringVersion = Guid.NewGuid();
            theTracker.StoreVersion<IntDoc, int>(intDoc.Id, intVersion);
            theTracker.StoreVersion<StringDoc, string>(stringDoc.Id, stringVersion);

            theTracker.ClearVersion<IntDoc, int>(intDoc.Id);

            theTracker.VersionFor<IntDoc, int>(intDoc.Id)
                .ShouldBeNull();

            // Not cleared
            theTracker.VersionFor<StringDoc, string>(stringDoc.Id)
                .ShouldBe(stringVersion);
        }
    }
}

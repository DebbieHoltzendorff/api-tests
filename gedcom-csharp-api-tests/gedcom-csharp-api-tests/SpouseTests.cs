using FamilySearch.Api.Ft;
using Gx.Common;
using Gx.Rs.Api;
using Gx.Rs.Api.Options;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Gedcomx.Rs.Api.Test
{
    [TestFixture]
    public class SpouseTests
    {
        private FamilySearchFamilyTree tree;

        [TestFixtureSetUp]
        public void Initialize()
        {
            tree = new FamilySearchFamilyTree(true);
            tree.AuthenticateViaOAuth2Password(Resources.TestUserName, Resources.TestPassword, Resources.TestClientId);
            Assert.DoesNotThrow(() => tree.IfSuccessful());
            Assert.IsNotNullOrEmpty(tree.CurrentAccessToken);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestCreateCoupleRelationship()
        {
            var husband = (PersonState)tree.AddPerson(TestBacking.GetCreateMalePerson()).Get();
            var wife = tree.AddPerson(TestBacking.GetCreateFemalePerson());
            var state = husband.AddSpouse(wife);

            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.Created, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, husband.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, wife.Response.StatusCode);
            wife.Delete();
            husband.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestCreateCoupleRelationshipSourceReference()
        {
            var husband = (PersonState)tree.AddPerson(TestBacking.GetCreateMalePerson()).Get();
            var wife = tree.AddPerson(TestBacking.GetCreateFemalePerson());
            var relationship = husband.AddSpouse(wife);
            var state = relationship.AddSourceReference(TestBacking.GetPersonSourceReference());

            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.Created, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, husband.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, wife.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, relationship.Response.StatusCode);
            wife.Delete();
            husband.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestCreateCoupleRelationshipConclusion()
        {
            var husband = (PersonState)tree.AddPerson(TestBacking.GetCreateMalePerson()).Get();
            var wife = tree.AddPerson(TestBacking.GetCreateFemalePerson());
            var relationship = husband.AddSpouse(wife);
            var state = relationship.AddFact(TestBacking.GetMarriageFact());

            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.NoContent, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, husband.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, wife.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, relationship.Response.StatusCode);
            wife.Delete();
            husband.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestCreateCoupleRelationshipNote()
        {
            var husband = (PersonState)tree.AddPerson(TestBacking.GetCreateMalePerson()).Get();
            var wife = tree.AddPerson(TestBacking.GetCreateFemalePerson());
            var relationship = husband.AddSpouse(wife);
            var state = relationship.AddNote(TestBacking.GetCreateNote());

            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.Created, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, husband.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, wife.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, relationship.Response.StatusCode);
            wife.Delete();
            husband.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadCoupleRelationship()
        {
            var husband = (PersonState)tree.AddPerson(TestBacking.GetCreateMalePerson()).Get();
            var wife = tree.AddPerson(TestBacking.GetCreateFemalePerson());
            var relationship = husband.AddSpouse(wife);
            var state = (RelationshipState)relationship.Get();

            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.IsNotNull(state.Relationship);
            Assert.AreEqual(HttpStatusCode.OK, husband.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, wife.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, relationship.Response.StatusCode);
            wife.Delete();
            husband.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadCoupleRelationshipConditional()
        {
            var husband = (PersonState)tree.AddPerson(TestBacking.GetCreateMalePerson()).Get();
            var wife = tree.AddPerson(TestBacking.GetCreateFemalePerson());
            var relationship = husband.AddSpouse(wife);
            var @get = (RelationshipState)relationship.Get();
            var cache = new CacheDirectives(@get.LastModified);
            var state = relationship.Get(cache);

            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.NotModified, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, husband.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, wife.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, relationship.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, @get.Response.StatusCode);
            wife.Delete();
            husband.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadCoupleRelationshipSourceReferences()
        {
            var husband = (PersonState)tree.AddPerson(TestBacking.GetCreateMalePerson()).Get();
            var wife = tree.AddPerson(TestBacking.GetCreateFemalePerson());
            var relationship = husband.AddSpouse(wife);
            relationship.AddSourceReference(TestBacking.GetPersonSourceReference());
            var state = ((RelationshipState)relationship.Get()).LoadSourceReferences();

            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.IsNotNull(state.SourceReference);
            Assert.AreEqual(HttpStatusCode.OK, husband.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, wife.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, relationship.Response.StatusCode);
            wife.Delete();
            husband.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadCoupleRelationshipNotes()
        {
            var husband = (PersonState)tree.AddPerson(TestBacking.GetCreateMalePerson()).Get();
            var wife = tree.AddPerson(TestBacking.GetCreateFemalePerson());
            var relationship = (RelationshipState)husband.AddSpouse(wife).Get();
            relationship.AddNote(TestBacking.GetCreateNote());
            var state = relationship.LoadNotes();

            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.IsNotNull(state.Relationship.Notes);
            Assert.AreEqual(1, state.Relationship.Notes.Count);
            Assert.AreEqual(HttpStatusCode.OK, husband.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, wife.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, relationship.Response.StatusCode);
            wife.Delete();
            husband.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadNonExistentCoupleRelationship()
        {
            var person = tree.AddPerson(TestBacking.GetCreateMalePerson());
            var relationship = TestBacking.GetCreateInvalidRelationship();
            var state = person.ReadRelationship(relationship);

            Assert.Throws<GedcomxApplicationException>(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.NotFound, state.Response.StatusCode);

            Assert.AreEqual(HttpStatusCode.Created, person.Response.StatusCode);
            person.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestHeadCoupleRelationship()
        {
            var husband = (PersonState)tree.AddPerson(TestBacking.GetCreateMalePerson()).Get();
            var wife = tree.AddPerson(TestBacking.GetCreateFemalePerson());
            var relationship = husband.AddSpouse(wife);

            var state = relationship.Head();
            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);

            Assert.AreEqual(HttpStatusCode.OK, husband.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, wife.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, relationship.Response.StatusCode);
            wife.Delete();
            husband.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestUpdatePersonsOfACoupleRelationship()
        {
            var husband = (PersonState)tree.AddPerson(TestBacking.GetCreateMalePerson()).Get();
            var wife = tree.AddPerson(TestBacking.GetCreateFemalePerson());
            var relationship = (RelationshipState)husband.AddSpouse(wife).Get();
            var wife2 = tree.AddPerson(TestBacking.GetCreateFemalePerson());
            relationship.Relationship.Person2 = new ResourceReference(wife2.GetSelfUri());
            var state = relationship.Post(relationship.Entity);

            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.NoContent, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, husband.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, wife.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, wife2.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, relationship.Response.StatusCode);
            wife.Delete();
            wife2.Delete();
            husband.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestUpdateCoupleRelationshipConclusion()
        {
            var husband = (PersonState)tree.AddPerson(TestBacking.GetCreateMalePerson()).Get();
            var wife = tree.AddPerson(TestBacking.GetCreateFemalePerson());
            var relationship = husband.AddSpouse(wife);
            var update = (RelationshipState)relationship.AddFact(TestBacking.GetMarriageFact()).Get();
            update.Fact.Date.Original = "4 Apr 1930";
            update.Fact.Attribution = new Attribution()
            {
                ChangeMessage = "Change message2",
            };
            var state = relationship.UpdateFact(update.Fact);

            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.NoContent, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, husband.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, wife.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, relationship.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, update.Response.StatusCode);
            wife.Delete();
            husband.Delete();

        }

        [Test]
        [Category("Regression Tests")]
        public void TestUpdateIllegalCoupleRelationship()
        {
            var husband = (PersonState)tree.AddPerson(TestBacking.GetCreateMalePerson()).Get();
            var wife = tree.AddPerson(TestBacking.GetCreateFemalePerson());
            var relationship = (RelationshipState)husband.AddSpouse(wife).Get();
            var invalid = tree.AddPerson(TestBacking.GetCreateMalePerson());
            relationship.Relationship.Person2 = new ResourceReference(invalid.GetSelfUri());
            var state = relationship.Post(relationship.Entity);

            Assert.Throws<GedcomxApplicationException>(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.BadRequest, state.Response.StatusCode);

            Assert.AreEqual(HttpStatusCode.OK, husband.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, wife.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, relationship.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, invalid.Response.StatusCode);
            wife.Delete();
            husband.Delete();
            invalid.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestDeleteCoupleRelationship()
        {
            var husband = (PersonState)tree.AddPerson(TestBacking.GetCreateMalePerson()).Get();
            var wife = tree.AddPerson(TestBacking.GetCreateFemalePerson());
            var relationship = husband.AddSpouse(wife);
            var state = relationship.Delete();

            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.NoContent, state.Response.StatusCode);

            Assert.AreEqual(HttpStatusCode.OK, husband.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, wife.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, relationship.Response.StatusCode);
            wife.Delete();
            husband.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestDeleteCoupleRelationshipConclusion()
        {
            var husband = (PersonState)tree.AddPerson(TestBacking.GetCreateMalePerson()).Get();
            var wife = tree.AddPerson(TestBacking.GetCreateFemalePerson());
            var relationship = husband.AddSpouse(wife);
            var fact = (RelationshipState)relationship.AddFact(TestBacking.GetMarriageFact()).Get();
            var state = fact.DeleteFact(fact.Fact);

            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.NoContent, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, husband.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, wife.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, relationship.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, fact.Response.StatusCode);
            wife.Delete();
            husband.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestRestoreCoupleRelationship()
        {
            var husband = (PersonState)tree.AddPerson(TestBacking.GetCreateMalePerson()).Get();
            var wife = tree.AddPerson(TestBacking.GetCreateFemalePerson());
            var relationship = (FamilyTreeRelationshipState)husband.AddSpouse(wife).Get();
            relationship = (FamilyTreeRelationshipState)relationship.Delete().IfSuccessful().Get();
            var state = relationship.Restore();

            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.NoContent, state.Response.StatusCode);

            Assert.AreEqual(HttpStatusCode.OK, husband.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, wife.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Gone, relationship.Response.StatusCode);
            wife.Delete();
            husband.Delete();
        }
    }
}



using FamilySearch.Api;
using FamilySearch.Api.Ft;
using FamilySearch.Api.Memories;
using Gedcomx.Support;
using Gx.Common;
using Gx.Conclusion;
using Gx.Links;
using Gx.Rs.Api;
using Gx.Rs.Api.Options;
using Gx.Rs.Api.Util;
using Gx.Source;
using Gx.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using KellermanSoftware.CompareNetObjects;

namespace Gedcomx.Rs.Api.Test
{
    [TestFixture]
    public class PersonTests
    {
        private static readonly String SANDBOX_URI = "https://sandbox.familysearch.org/platform/collections/tree";
        private static readonly String READ_PERSON_ID = "KWQ7-Y57";
        private static readonly String READ_PERSON_URI = "https://sandbox.familysearch.org/platform/tree/persons/" + READ_PERSON_ID;
        private static readonly String READ_PERSON_URI_NO_ID = "https://sandbox.familysearch.org/platform/tree/persons/";
        private static readonly String PERSON_WITH_DATA_ID = "KWWD-CMF";
        private static readonly String PERSON_WITH_DATA_URI = "https://sandbox.familysearch.org/platform/tree/persons/" + PERSON_WITH_DATA_ID;
        private CollectionState collection;
        private FamilySearchFamilyTree tree;

        [TestFixtureSetUp]
        public void Initialize()
        {
            collection = new CollectionState(new Uri(SANDBOX_URI));
            collection.AuthenticateViaOAuth2Password("sdktester", "1234sdkpass", "WCQY-7J1Q-GKVV-7DNM-SQ5M-9Q5H-JX3H-CMJK");
            Assert.DoesNotThrow(() => collection.IfSuccessful());
            Assert.IsNotNullOrEmpty(collection.CurrentAccessToken);

            tree = new FamilySearchFamilyTree(true);
            tree.AuthenticateWithAccessToken(collection.CurrentAccessToken);
        }

       

        [Test]
        [Category("Regression Tests")]
        public void TestCreatePerson()
        {
            var result = collection.AddPerson(TestBacking.GetCreateMalePerson());
            Assert.DoesNotThrow(() => result.IfSuccessful());
            var person = (PersonState)result.Get();
            Assert.IsNotNull(person.Person);
            Assert.IsNotNullOrEmpty(person.Person.Id);
            Assert.AreEqual(HttpStatusCode.Created, result.Response.StatusCode);
            result.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestCreatePersonSourceReference()
        {
            var result = collection.AddPerson(TestBacking.GetCreateMalePerson());
            Assert.DoesNotThrow(() => result.IfSuccessful());
            var state = (PersonState)result.Get();
            var result2 = state.AddSourceReference(TestBacking.GetPersonSourceReference());
            Assert.DoesNotThrow(() => result2.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.Created, result2.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, result.Response.StatusCode);
            result.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestCreatePersonConclusion()
        {
            var result = collection.AddPerson(TestBacking.GetCreateMalePerson());
            Assert.AreEqual(HttpStatusCode.Created, result.Response.StatusCode);
            var person = (PersonState)result.Get();
            var state = collection.ReadPerson(new Uri(READ_PERSON_URI_NO_ID + person.Person.Id));
            Person conclusion = TestBacking.GetCreatePersonConclusion(state.Person.Id);
            var state2 = state.UpdateConclusions(conclusion);
            Assert.IsNotNull(state2);
            Assert.DoesNotThrow(() => state2.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.NoContent, state2.Response.StatusCode);
            result.Delete();
        }

        [Test]
        [Ignore("DiscussionReference is defined in extension. Not ready to test.")]
        public void TestCreateDiscussionReference()
        {
            var state = collection.ReadPerson(new Uri(READ_PERSON_URI));
            var discussion = TestBacking.GetCreateDiscussionReference(state.Person.Id);
            throw new NotImplementedException();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestCreateNote()
        {
            var result = collection.AddPerson(TestBacking.GetCreateMalePerson());
            Assert.AreEqual(HttpStatusCode.Created, result.Response.StatusCode);
            var person = (PersonState)result.Get();
            var state = collection.ReadPerson(new Uri(READ_PERSON_URI_NO_ID + person.Person.Id));
            var note = TestBacking.GetCreateNote();
            var state2 = state.AddNote(note);
            Assert.DoesNotThrow(() => state2.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.Created, state2.Response.StatusCode);
            result.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadMergedPerson()
        {
            // KWWD-X35 was merged with KWWD-CMF
            var state = collection.ReadPerson(new Uri("https://sandbox.familysearch.org/platform/tree/persons/KWWD-X35"));
            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.MovedPermanently, state.Response.StatusCode);
            var link = state.GetLink("self");
            Assert.IsNotNull(link);
            Assert.AreEqual(PERSON_WITH_DATA_URI, link.Href);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadDeletedPerson()
        {
            /*Original Test
            var state = collection.ReadPerson(new Uri("https://sandbox.familysearch.org/platform/tree/persons/KWWD-ZM7"));
            Assert.AreEqual(HttpStatusCode.Gone, state.Response.StatusCode);*/

            var result = collection.AddPerson(TestBacking.GetCreateMalePerson());
            var person = (PersonState)result.Get();
            String personID = person.Person.Id;
            var state = collection.ReadPerson(new Uri(READ_PERSON_URI_NO_ID + personID));
            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            result.Delete();
            state = collection.ReadPerson(new Uri(READ_PERSON_URI_NO_ID + personID));
            Assert.AreEqual(HttpStatusCode.Gone, state.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadPerson()
        {
            var result = collection.AddPerson(TestBacking.GetCreateMalePerson());
            Assert.AreEqual(HttpStatusCode.Created, result.Response.StatusCode);
            var person = (PersonState)result.Get();
            var state = collection.ReadPerson(new Uri(READ_PERSON_URI_NO_ID + person.Person.Id));
            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.IsNotNull(state.Person);
            Assert.IsNotNullOrEmpty(state.Person.Id);
            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            result.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadPersonSourceReferences()
        {
            var state = collection.ReadPerson(new Uri(PERSON_WITH_DATA_URI));
            var state2 = state.LoadSourceReferences();
            Assert.DoesNotThrow(() => state2.IfSuccessful());
            Assert.IsNotNull(state2.Person);
            Assert.IsNotNull(state2.Person.Sources);
            
            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, state2.Response.StatusCode);
           
        }

        [Test]
        [Ignore("May not be needed. Covered by TestReadPersonSourceReferences()?")]
        public void TestReadPersonSources()
        {
            throw new NotImplementedException();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadRelationshipsToChildren()
        {
            var state = collection.ReadPerson(new Uri(PERSON_WITH_DATA_URI));
            var state2 = state.LoadChildRelationships();
            Assert.DoesNotThrow(() => state2.IfSuccessful());
            var children = state2.GetChildRelationships();
            Assert.IsNotNull(children);
            Assert.Greater(children.Count, 0);

            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, state2.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadRelationshipsToParents()
        {
            var state = collection.ReadPerson(new Uri("https://sandbox.familysearch.org/platform/tree/persons/KWWD-QV9"));
            var state2 = state.LoadParentRelationships();
            Assert.DoesNotThrow(() => state2.IfSuccessful());
            var parents = state2.GetParentRelationships();
            Assert.IsNotNull(parents);
            Assert.Greater(parents.Count, 0);

            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, state2.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadRelationshipsToSpouses()
        {
            var state = collection.ReadPerson(new Uri(READ_PERSON_URI));
            
            var state2 = state.LoadSpouseRelationships();
            Assert.DoesNotThrow(() => state2.IfSuccessful());
            var spouses = state2.GetSpouseRelationships();
            Assert.IsNotNull(spouses);
            Assert.Greater(spouses.Count, 0);

            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, state2.Response.StatusCode);
        }

        [Test(Description = "Matches example request here https://familysearch.org/developers/docs/api/tree/Read_Relationships_To_Spouses_with_Persons_usecase, but is either unneeded or the SDK needs to be updated to support this more directly.")]
        [Category("Regression Tests")]
        public void TestReadRelationshipsToSpousesWithPersons()
        {
            
            var query = new QueryParameter("persons", "");
            var state = collection.ReadPerson(new Uri(READ_PERSON_URI));
            var state2 = state.LoadSpouseRelationships(query);
            Assert.DoesNotThrow(() => state2.IfSuccessful());
            Assert.IsNotNull(state2.Entity != null);
            Assert.IsNotNull(state2.Entity.Persons);
            Assert.AreEqual(2, state2.Entity.Persons.Count);

            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, state2.Response.StatusCode);
        }

        [Test]
        [Ignore("DiscussionReference is defined in extension. Not ready to test.")]
        public void TestReadDiscussionReferences()
        {
            throw new NotImplementedException();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadChildrenOfAPerson()
        {
            var state = collection.ReadPerson(new Uri(PERSON_WITH_DATA_URI));
            var state2 = state.ReadChildren();
            Assert.DoesNotThrow(() => state2.IfSuccessful());
            Assert.IsNotNull(state2.Persons);
            Assert.Greater(state2.Persons.Count, 0);

            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, state2.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadNotFoundPerson()
        {
            var state = collection.ReadPerson(new Uri("https://sandbox.familysearch.org/platform/tree/persons/MMMM-MMM"));
            Assert.AreEqual(HttpStatusCode.NotFound, state.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadNotModifiedPerson()
        {
            var result = collection.AddPerson(TestBacking.GetCreateMalePerson());
            Assert.AreEqual(HttpStatusCode.Created, result.Response.StatusCode);
            var person = (PersonState)result.Get();
            var state = collection.ReadPerson(new Uri(READ_PERSON_URI_NO_ID + person.Person.Id));
            
            //var state = collection.ReadPerson(new Uri(READ_PERSON_URI));
            var cache = new CacheDirectives(state);
            var state2 = collection.ReadPerson(new Uri(READ_PERSON_URI_NO_ID + person.Person.Id), cache);
            
            //var state2 = collection.ReadPerson(new Uri(READ_PERSON_URI), cache);

            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.DoesNotThrow(() => state2.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.NotModified, state2.Response.StatusCode);
            result.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadNotes()
        {
            var state = collection.ReadPerson(new Uri(PERSON_WITH_DATA_URI));
            var state2 = state.LoadNotes();
            Assert.DoesNotThrow(() => state2.IfSuccessful());
            Assert.IsNotNull(state2.Person);
            Assert.IsNotNull(state2.Person.Notes);
            Assert.Greater(state2.Person.Notes.Count, 0);

            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, state2.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadParentsOfAPerson()
        {
            var state = collection.ReadPerson(new Uri(PERSON_WITH_DATA_URI));
            var state2 = state.ReadParents();
            Assert.DoesNotThrow(() => state2.IfSuccessful());
            Assert.IsNotNull(state2.Persons);
            Assert.Greater(state2.Persons.Count, 0);

            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, state2.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadSpousesOfAPerson()
        {
            var state = collection.ReadPerson(new Uri(PERSON_WITH_DATA_URI));
            var state2 = state.ReadSpouses();

            Assert.DoesNotThrow(() => state2.IfSuccessful());
            Assert.IsNotNull(state2.Persons);
            Assert.IsNotNull(state2.Relationships);
            Assert.Greater(state2.Persons.Count, 0);
            Assert.Greater(state2.Relationships.Count, 0);

            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, state2.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestHeadPerson()
        {
            var state = collection.ReadPerson(new Uri(PERSON_WITH_DATA_URI));
            var state2 = (PersonState)state.Head();
            Assert.DoesNotThrow(() => state2.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.OK, state2.Response.StatusCode);

            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, state2.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestUpdatePersonSourceReference()
        {
            var state = collection.ReadPerson(new Uri(PERSON_WITH_DATA_URI));
            state = state.LoadSourceReferences();
            var tag = state.Person.Sources[0].Tags.First();
            state.Person.Sources[0].Tags.Remove(tag);
            var state2 = state.UpdateSourceReferences(state.Person);
            Assert.DoesNotThrow(() => state2.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.NoContent, state2.Response.StatusCode);
            state.Person.Sources[0].Tags.Add(tag);
            state2 = state.UpdateSourceReferences(state.Person);
            Assert.DoesNotThrow(() => state2.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.NoContent, state2.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestUpdatePersonConclusion()
        {
            var result = collection.AddPerson(TestBacking.GetCreateMalePerson());
            Assert.AreEqual(HttpStatusCode.Created, result.Response.StatusCode);
            var person = (PersonState)result.Get();
            var state = collection.ReadPerson(new Uri(READ_PERSON_URI_NO_ID + person.Person.Id));
            
            //var state = collection.ReadPerson(new Uri(READ_PERSON_URI));
            state.Person.Facts.Add(TestBacking.GetBirthFact());
            var state2 = state.UpdateConclusions(state.Person);
            Assert.DoesNotThrow(() => state2.IfSuccessful());

            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.NoContent, state2.Response.StatusCode);

            result.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestUpdatePersonCustomNonEventFact()
        {
            var state = collection.ReadPerson(new Uri(PERSON_WITH_DATA_URI));
            var state2 = state.UpdateFact(TestBacking.GetUpdateFact());

            Assert.DoesNotThrow(() => state2.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.NoContent, state2.Response.StatusCode);
        }


        [Test]
        [Category("Regression Tests")]
        public void TestDeletePerson()
        {
            var state = collection.AddPerson(TestBacking.GetCreateMalePerson());
            var state2 = (PersonState)state.Delete();

            Assert.DoesNotThrow(() => state2.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.Created, state.Response.StatusCode);
            Assert.AreEqual(HttpStatusCode.NoContent, state2.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestDeletePersonSourceReference()
        {

            var state = collection.AddPerson(TestBacking.GetCreateMalePerson());
            Assert.AreEqual(HttpStatusCode.Created, state.Response.StatusCode);
            state = (PersonState)state.Get();
            
            state.AddSourceReference(TestBacking.GetPersonSourceReference());
            state.LoadSourceReferences();

            var state2 = state.DeleteSourceReference(state.Person.Sources.FirstOrDefault());
            Assert.DoesNotThrow(() => state2.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.NoContent, state2.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestDeletePersonWithPreconditions()
        {
            // Assume the ability to add a person is working
            var state = collection.AddPerson(TestBacking.GetCreateMalePerson());
            Assert.AreEqual(HttpStatusCode.Created, state.Response.StatusCode);
            state = (PersonState)state.Get();
            var cond = new Preconditions(state.LastModified);

            // Touch the record since the above date
            state.Update(state.Person);

            // This should fail
            var state2 = (PersonState)state.Delete(cond);
            Assert.Throws<GedcomxApplicationException>(() => state2.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.PreconditionFailed, state2.Response.StatusCode);
        }

        [Test]
        [Ignore("DiscussionReference is defined in extension. Not ready to test.")]
        public void TestDeleteDiscussionReference()
        {
            throw new NotImplementedException();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestRestorePerson()
        {
            // Assume the ability to add/delete a person works
            var state = collection.AddPerson(TestBacking.GetCreateMalePerson());
            var id = state.Headers.Get("X-ENTITY-ID").First().Value.ToString();
            state.Delete();

            var deletedPerson = tree.ReadPersonById(id);
            Assert.AreEqual(HttpStatusCode.Gone, deletedPerson.Response.StatusCode); // Ensure we have a deleted person
            var testState = deletedPerson.Restore();
            Assert.DoesNotThrow(() => testState.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.NoContent, testState.Response.StatusCode);
            deletedPerson.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadPreferredSpouseRelationship()
        {
            var me = tree.ReadCurrentUser();
            var person = tree.ReadPersonById(PERSON_WITH_DATA_ID);

            // Ensure the target relationship exists
            person.LoadSpouseRelationships();
            Assert.AreEqual(HttpStatusCode.OK, person.Response.StatusCode);
            var state = (PreferredRelationshipState)person.ReadRelationship(person.Entity.Relationships[0]);
            
            tree.UpdatePreferredSpouseRelationship(me.User.TreeUserId, PERSON_WITH_DATA_ID, state);

            var state2 = (FamilyTreeRelationshipState)tree.ReadPreferredSpouseRelationship(me.User.TreeUserId, PERSON_WITH_DATA_ID);
            Assert.AreEqual(HttpStatusCode.SeeOther, state2.Response.StatusCode);
            Assert.IsNotNull(state2.Headers.Get("Location").Single());
        }

        [Test]
        [Category("Regression Tests")]
        public void TestUpdatePreferredSpouseRelationship()
        {
            var me = tree.ReadCurrentUser();
            var person = tree.ReadPersonById(PERSON_WITH_DATA_ID);
            Assert.AreEqual(HttpStatusCode.OK, person.Response.StatusCode);

            person.LoadSpouseRelationships();
            Assert.AreEqual(HttpStatusCode.OK, person.Response.StatusCode);
            var state = (PreferredRelationshipState)person.ReadRelationship(person.Entity.Relationships[0]);
            var state2 = tree.UpdatePreferredSpouseRelationship(me.User.TreeUserId, PERSON_WITH_DATA_ID, state);
            Assert.AreEqual(HttpStatusCode.NoContent, state2.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestDeletePreferredSpouseRelationship()
        {

            var me = tree.ReadCurrentUser();
            var person = tree.ReadPersonById(PERSON_WITH_DATA_ID);
            Assert.AreEqual(HttpStatusCode.OK, person.Response.StatusCode);

            // Ensure the target relationship exists
            person.LoadSpouseRelationships();
            Assert.AreEqual(HttpStatusCode.OK, person.Response.StatusCode);
            var state = (PreferredRelationshipState)person.ReadRelationship(person.Entity.Relationships[0]);
            tree.UpdatePreferredSpouseRelationship(me.User.TreeUserId, PERSON_WITH_DATA_ID, state);

            var state2 = tree.DeletePreferredSpouseRelationship(me.User.TreeUserId, PERSON_WITH_DATA_ID);
            Assert.AreEqual(HttpStatusCode.NoContent, state2.Response.StatusCode);
            Assert.IsNotNull(state2.Headers.Get("Content-Location").Single());
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadPersonWithRelationships()
        {
            var state = tree.ReadPersonWithRelationshipsById(PERSON_WITH_DATA_ID);

            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.IsNotNull(state.Person);
            Assert.IsNotNull(state.ChildAndParentsRelationships);
            Assert.Greater(state.ChildAndParentsRelationships.Count, 0);
            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestUpdatePersonNotAMatchDeclarations()
        {
            var person1 = tree.ReadPersonById("KWWD-CMF");
            var person2 = tree.ReadPersonById("KW73-MB6");
            var state = person1.AddNonMatch(person2);
            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.NoContent, state.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestDeletePersonNotAMatch()
        {
            var person1 = tree.ReadPersonById("KWWD-CMF");
            var person2 = tree.ReadPersonById("KW73-MB6");
            var state = (PersonNonMatchesState)person1.AddNonMatch(person2).Get();
            state = state.RemoveNonMatch(state.Persons[0]);
            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.NoContent, state.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadPersonPortrait()
        {
            // Assume the ability to read a person by ID is working
            var result = collection.AddPerson(TestBacking.GetCreateMalePerson());
            Assert.AreEqual(HttpStatusCode.Created, result.Response.StatusCode);
            var person = (PersonState)result.Get();
            var state = tree.ReadPersonById(person.Person.Id);

            // This is BETA, and does not yet return a state. Test is based exclusively off response data.
            var response = state.ReadPortrait();
            Assert.IsTrue(!response.HasClientError() && !response.HasServerError());
            // NOTE: The READ_PERSON_ID user does not have images, thus the response should be 204.
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            person.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadPersonPortraitWithDefault()
        {
            var result = collection.AddPerson(TestBacking.GetCreateMalePerson());
            Assert.AreEqual(HttpStatusCode.Created, result.Response.StatusCode);
            var person = (PersonState)result.Get();
            var state = tree.ReadPersonById(person.Person.Id);
            var location = "http://i.imgur.com/d9J0gYA.jpg";
            var options = new QueryParameter("default", location);

            // This is BETA, and does not yet return a state. Test is based exclusively off response data.
            var response = state.ReadPortrait(options);
            Assert.IsTrue(!response.HasClientError() && !response.HasServerError());
            // NOTE: The READ_PERSON_ID user does not have images, but a default is specified, thus the response should be 307.
            Assert.AreEqual(HttpStatusCode.TemporaryRedirect, response.StatusCode);
            Assert.IsTrue(response.Headers.Get("Location").Any());
            Assert.IsNotNull(response.Headers.Get("Location").Single().Value);
            Assert.AreEqual(location, response.Headers.Get("Location").Single().Value.ToString());
            person.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadPersonChangeSummary()
        {
            var result = collection.AddPerson(TestBacking.GetCreateMalePerson());
            Assert.AreEqual(HttpStatusCode.Created, result.Response.StatusCode);
            var person = (PersonState)result.Get();
            var state = tree.ReadPersonById(person.Person.Id);
            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            var state2 = state.ReadChangeHistory();
            Assert.AreEqual(HttpStatusCode.OK, state2.Response.StatusCode);

            Assert.DoesNotThrow(() => state2.IfSuccessful());
            person.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadPersonPortraits()
        {
            // Assume the ability to read a person by ID is working
            var result = collection.AddPerson(TestBacking.GetCreateMalePerson());
            Assert.AreEqual(HttpStatusCode.Created, result.Response.StatusCode);
            var person = (PersonState)result.Get();
            var state = tree.ReadPersonById(person.Person.Id);
            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);

            var state2 = state.ReadPortraits();
            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.OK, state2.Response.StatusCode);
            person.Delete();
        }

        [Test]
        [Category("Regression Tests")]
        public void TestCreatePersonLifeSketch()
        {
            var person = tree.ReadPersonById(PERSON_WITH_DATA_ID);
            Assert.AreEqual(HttpStatusCode.OK, person.Response.StatusCode);
            var state = (FamilyTreePersonState)person.Post(TestBacking.GetCreatePersonLifeSketch(PERSON_WITH_DATA_ID));

            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.NoContent, state.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestUpdatePersonLifeSketch()
        {
            var person = tree.ReadPersonById(PERSON_WITH_DATA_ID);
            Assert.AreEqual(HttpStatusCode.OK, person.Response.StatusCode);
            var factId = TestBacking.GetFactId(person.Person, "http://familysearch.org/v1/LifeSketch");

            if (factId == null)
            {
                TestCreatePersonLifeSketch();
                person = tree.ReadPersonById(PERSON_WITH_DATA_ID);
                factId = TestBacking.GetFactId(person.Person, "http://familysearch.org/v1/LifeSketch");
            }
            var state = (FamilyTreePersonState)person.Post(TestBacking.GetUpdatePersonLifeSketch(PERSON_WITH_DATA_ID, factId));


            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.NoContent, state.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestDeletePersonConclusion()
        {
            var person = tree.ReadPersonById(PERSON_WITH_DATA_ID);
            Assert.AreEqual(HttpStatusCode.OK, person.Response.StatusCode);
            var sketchToDelete = person.Person.Facts.Where(x => x.Type == "http://familysearch.org/v1/LifeSketch").FirstOrDefault();

            if (sketchToDelete == null)
            {
                TestCreatePersonLifeSketch();
                person = tree.ReadPersonById(PERSON_WITH_DATA_ID);
                sketchToDelete = person.Person.Facts.Where(x => x.Type == "http://familysearch.org/v1/LifeSketch").FirstOrDefault();
            }

            Assert.IsNotNull(sketchToDelete);

            var state = person.DeleteFact(sketchToDelete);

            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.NoContent, state.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadPersonMemories()
        {
            var person = (FamilyTreePersonState)tree.AddPerson(TestBacking.GetCreateMalePerson()).Get();
            var dataSource = new BasicDataSource("Sample Memory", MediaTypes.TEXT_PLAIN_TYPE, Resources.MemoryTXT);
            person.AddArtifact(dataSource);
            person = (FamilyTreePersonState)person.Get();
            var state = person.ReadArtifacts();

            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadPersonMemoriesByType()
        {
            var person = (FamilyTreePersonState)tree.AddPerson(TestBacking.GetCreateMalePerson()).Get();
            var dataSource = new BasicDataSource("Sample Memory", MediaTypes.TEXT_PLAIN_TYPE, Resources.MemoryTXT);
            person.AddArtifact(dataSource);
            person = (FamilyTreePersonState)person.Get();
            var options = new QueryParameter[] { new QueryParameter("type", "story") };
            var state = person.ReadArtifacts(options);

            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
        }

        //[Test]
        ////[Ignore("Need to resolve RDF serialization issue.")]
        //public void TestUploadPhotoForPerson()
        //{
        //    var person = (FamilyTreePersonState)tree.AddPerson(TestBacking.createPerson(GenderType.Male)).Get();
        //    var converter = new ImageConverter();
        //    var bytes = (Byte[])converter.ConvertTo(Resources.PersonImage, typeof(Byte[]));
        //    var dataSource = TestBacking.GetDataSource("PersonImage", "image/jpeg", bytes);
        //    var state = person.AddArtifact(new SourceDescription() { Titles = new List<TextValue>() { new TextValue("PersonImage") }, Citations = new List<SourceCitation>() { new SourceCitation() { Value = "Citation for PersonImage" } } }, dataSource);

        //    Assert.DoesNotThrow(() => state.IfSuccessful());
        //    Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
        //}

        [Test]
        [Category("Regression Tests")]
        public void TestReadPreferredParentRelationship()
        {
            var me = tree.ReadCurrentUser();
            Assert.AreEqual(HttpStatusCode.OK, me.Response.StatusCode);
            var person = tree.ReadPersonById(PERSON_WITH_DATA_ID);
            Assert.AreEqual(HttpStatusCode.OK, person.Response.StatusCode);

            // Ensure the target relationship exists
            var relationship = ((FamilyTreePersonParentsState)person.ReadParents()).ChildAndParentsRelationships.First();
            var state = person.ReadChildAndParentsRelationship(relationship);
            tree.UpdatePreferredParentRelationship(me.User.TreeUserId, PERSON_WITH_DATA_ID, state);

            var state2 = (FamilyTreeRelationshipState)tree.ReadPreferredParentRelationship(me.User.TreeUserId, PERSON_WITH_DATA_ID);
            Assert.AreEqual(HttpStatusCode.SeeOther, state2.Response.StatusCode);
            Assert.IsNotNull(state2.Headers.Get("Location").Single());
        }

        [Test]
        [Category("Regression Tests")]
        public void TestUpdatePreferredParentRelationship()
        {
            var me = tree.ReadCurrentUser();
            Assert.AreEqual(HttpStatusCode.OK, me.Response.StatusCode);
            var person = tree.ReadPersonById(PERSON_WITH_DATA_ID);
            Assert.AreEqual(HttpStatusCode.OK, person.Response.StatusCode);

            // Ensure the target relationship exists
            var relationship = ((FamilyTreePersonParentsState)person.ReadParents()).ChildAndParentsRelationships.First();
            var state = person.ReadChildAndParentsRelationship(relationship);


            var state2 = tree.UpdatePreferredParentRelationship(me.User.TreeUserId, PERSON_WITH_DATA_ID, state);
            Assert.AreEqual(HttpStatusCode.NoContent, state2.Response.StatusCode);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestDeletePreferredParentRelationship()
        {
            var me = tree.ReadCurrentUser();
            Assert.AreEqual(HttpStatusCode.OK, me.Response.StatusCode);
            var person = tree.ReadPersonById(PERSON_WITH_DATA_ID);
            Assert.AreEqual(HttpStatusCode.OK, person.Response.StatusCode);

            // Ensure the target relationship exists
            var relationship = ((FamilyTreePersonParentsState)person.ReadParents()).ChildAndParentsRelationships.First();
            var state = person.ReadChildAndParentsRelationship(relationship);
            tree.UpdatePreferredParentRelationship(me.User.TreeUserId, PERSON_WITH_DATA_ID, state);

            var state2 = tree.DeletePreferredParentRelationship(me.User.TreeUserId, PERSON_WITH_DATA_ID);
            Assert.AreEqual(HttpStatusCode.NoContent, state2.Response.StatusCode);
            Assert.IsNotNull(state2.Headers.Get("Content-Location").Single());
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadPersonMergeAnalysis()
        {
            var person1 = tree.ReadPersonById(PERSON_WITH_DATA_ID); 
            Assert.AreEqual(HttpStatusCode.OK, person1.Response.StatusCode);
            var person2 = tree.ReadPersonById("KWWX-JKF");
            Assert.AreEqual(HttpStatusCode.OK, person2.Response.StatusCode);

            var state = person1.ReadMergeAnalysis(person2);
            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.IsNotNull(state.Analysis);
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadPersonMergeConstraintCanMergeAnyOrder()
        {
            var person1 = tree.ReadPersonById(PERSON_WITH_DATA_ID);
            Assert.AreEqual(HttpStatusCode.OK, person1.Response.StatusCode);
            var person2 = tree.ReadPersonById("KWWX-JKF");
            Assert.AreEqual(HttpStatusCode.OK, person2.Response.StatusCode);

            var state = person1.ReadMergeOptions(person2);
            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.IsNotNull(state.GetLink(FamilySearch.Api.Rel.MERGE_MIRROR));
        }

        [Test]
        [Category("Regression Tests")]
        public void TestReadPersonMergeConstraintCanMergeOtherOrderOnly()
        {
            
            var person1 = tree.ReadPersonById(PERSON_WITH_DATA_ID);
            Assert.AreEqual(HttpStatusCode.OK, person1.Response.StatusCode);
            var resultPerson2 = collection.AddPerson(TestBacking.GetCreateFemalePerson());
            var personState = (PersonState)resultPerson2.Get(); 
            var person2 = tree.ReadPersonById(personState.Person.Id);
            Assert.AreEqual(HttpStatusCode.OK, person2.Response.StatusCode);

            var state = person1.ReadMergeOptions(person2);
            Assert.DoesNotThrow(() => state.IfSuccessful());
            Assert.AreEqual(HttpStatusCode.OK, state.Response.StatusCode);
            Assert.IsFalse(state.IsAllowed);
        }

        
    }
}

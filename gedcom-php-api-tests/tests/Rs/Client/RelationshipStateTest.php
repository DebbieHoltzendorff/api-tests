<?php

namespace Gedcomx\ApiTests\Rs\Client;

use Gedcomx\Common\Attribution;
use Gedcomx\Common\ResourceReference;
use Gedcomx\Conclusion\DateInfo;
use Gedcomx\Conclusion\Fact;
use Gedcomx\Conclusion\Relationship;
use Gedcomx\Extensions\FamilySearch\Rs\Client\FamilyTree\FamilyTreeStateFactory;
use Gedcomx\Rs\Client\Options\HeaderParameter;
use Gedcomx\Rs\Client\RelationshipState;
use Gedcomx\Rs\Client\Util\HttpStatus;
use Gedcomx\Source\SourceReference;
use Gedcomx\Tests\ApiTestCase;
use Gedcomx\Tests\FactBuilder;
use Gedcomx\Tests\NoteBuilder;
use Gedcomx\Types\FactType;

class RelationshipStateTest extends ApiTestCase
{
    /**
     * @link https://familysearch.org/developers/docs/api/tree/Create_Couple_Relationship_usecase
     * @link https://familysearch.org/developers/docs/api/tree/Read_Couple_Relationship_usecase
     * @link https://familysearch.org/developers/docs/api/tree/Update_Persons_of_a_Couple_Relationship_usecase
     * @link https://familysearch.org/developers/docs/api/tree/Delete_Couple_Relationship_usecase
     */
    
    /**
     * @link https://familysearch.org/developers/docs/api/tree/Create_Couple_Relationship_Note_usecase
     */
    public function testCreateCoupleRelationshipNote()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        $this->collectionState($factory);

        $person1 = $this->createPerson('male')->get();
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $person1->getResponse());
        $person2 = $this->createPerson('female')->get();
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $person2->getResponse());

        /* Create Relationship */
        /** @var $relation RelationshipState */
        $relation = $this->collectionState()->addSpouseRelationship($person1, $person2)->get();
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $relation->getResponse(), $this->buildFailMessage(__METHOD__."(addSpouse)", $relation));

        $note = NoteBuilder::createNote();
        $updated = $relation->addNote($note);
        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $updated->getResponse(), $this->buildFailMessage(__METHOD__."(addSpouse)", $updated));

        $relation->delete();
        $person2->delete();
        $person1->delete();
    }

    /**
     * @link https://familysearch.org/developers/docs/api/tree/Read_Couple_Relationship_Notes_usecase
     */
    public function testReadCoupleRelationshipNotes()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        $this->collectionState($factory);

        $person1 = $this->createPerson('male')->get();
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $person1->getResponse());
        $person2 = $this->createPerson('female')->get();
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $person2->getResponse());

        /* Create Relationship */
        /** @var $relation RelationshipState */
        $relation = $this->collectionState()->addSpouseRelationship($person1, $person2)->get();
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $relation->getResponse(), $this->buildFailMessage(__METHOD__."(addSpouse)", $relation));

        $note = NoteBuilder::createNote();
        $updated = $relation->addNote($note);
        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $updated->getResponse(), $this->buildFailMessage(__METHOD__."(addSpouse)", $updated));

        $relation->loadNotes();
        $this->assertNotEmpty($relation->getRelationship()->getNotes());
        
        $relation->delete();
        $person1->delete();
        $person2->delete();
    }

    /**
     * @link https://familysearch.org/developers/docs/api/tree/Couple_Relationship_Note_resource
     */
    public function testReadCoupleRelationshipNote()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        $this->collectionState($factory);

        $person1 = $this->createPerson('male')->get();
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $person1->getResponse());
        $person2 = $this->createPerson('female')->get();
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $person2->getResponse());

        /* Create Relationship */
        /** @var $relation RelationshipState */
        $relation = $this->collectionState()->addSpouseRelationship($person1, $person2)->get();
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $relation->getResponse(), $this->buildFailMessage(__METHOD__."(addSpouse)", $relation));

        $note = NoteBuilder::createNote();
        $updated = $relation->addNote($note);
        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $updated->getResponse(), $this->buildFailMessage(__METHOD__."(addSpouse)", $updated));

        $relation->loadNotes();
        $notes = $relation->getRelationship()->getNotes();
        $noted = $relation->readNote($notes[0]);
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $noted->getResponse(), $this->buildFailMessage(__METHOD__."(addSpouse)", $noted));
        
        $relation->delete();
        $person1->delete();
        $person2->delete();
    }

    /**
     * @link https://familysearch.org/developers/docs/api/tree/Update_Couple_Relationship_Note_usecase
     */
    public function testUpdateCoupleRelationshipNote()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        $this->collectionState($factory);

        $person1 = $this->createPerson('male')->get();
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $person1->getResponse());
        $person2 = $this->createPerson('female')->get();
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $person2->getResponse());

        /* Create Relationship */
        /** @var $relation RelationshipState */
        $relation = $this->collectionState()->addSpouseRelationship($person1, $person2)->get();
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $relation->getResponse(), $this->buildFailMessage(__METHOD__."(addSpouse)", $relation));

        $note = NoteBuilder::createNote();
        $noted = $relation->addNote($note);
        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $noted->getResponse(), $this->buildFailMessage(__METHOD__."(addSpouse)", $noted));

        $relation->loadNotes();
        $notes = $relation->getRelationship()->getNotes();
        $notes[0]->setText($this->faker->sentence(12));
        $noted = $relation->updateNote($notes[0]);
        $this->assertAttributeEquals(HttpStatus::NO_CONTENT, "statusCode", $noted->getResponse(), $this->buildFailMessage(__METHOD__."(addSpouse)", $noted));
    
        $relation->delete();
        $person1->delete();
        $person2->delete();
    }

    /**
     * @link https://familysearch.org/developers/docs/api/tree/Update_Couple_Relationship_Note_usecase
     */
    public function testDeleteCoupleRelationshipNote()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        $this->collectionState($factory);

        $person1 = $this->createPerson('male')->get();
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $person1->getResponse());
        $person2 = $this->createPerson('female')->get();
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $person2->getResponse());

        /* Create Relationship */
        /** @var $relation RelationshipState */
        $relation = $this->collectionState()->addSpouseRelationship($person1, $person2)->get();
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $relation->getResponse(), $this->buildFailMessage(__METHOD__."(addSpouse)", $relation));

        $note = NoteBuilder::createNote();
        $noted = $relation->addNote($note);
        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $noted->getResponse(), $this->buildFailMessage(__METHOD__."(addSpouse)", $noted));

        $relation->loadNotes();
        $notes = $relation->getRelationship()->getNotes();
        $noted = $relation->deleteNote($notes[0]);
        $this->assertAttributeEquals(HttpStatus::NO_CONTENT, "statusCode", $noted->getResponse(), $this->buildFailMessage(__METHOD__."(addSpouse)", $noted));
    
        $relation->delete();
        $person1->delete();
        $person2->delete();
    }

    
}
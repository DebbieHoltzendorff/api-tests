<?php

namespace Gedcomx\ApiTests\Extensions\FamilySearch\Rs\Client\FamilyTree;

use Gedcomx\Common\Attribution;
use Gedcomx\Common\ResourceReference;
use Gedcomx\Extensions\FamilySearch\Platform\Tree\ChildAndParentsRelationship;
use Gedcomx\Extensions\FamilySearch\Rs\Client\FamilyTree\ChildAndParentsRelationshipState;
use Gedcomx\Extensions\FamilySearch\Rs\Client\FamilyTree\FamilyTreeCollectionState;
use Gedcomx\Extensions\FamilySearch\Rs\Client\FamilyTree\FamilyTreeStateFactory;
use Gedcomx\Rs\Client\Util\HttpStatus;
use Gedcomx\Source\SourceReference;
use Gedcomx\Tests\ApiTestCase;
use Gedcomx\Tests\FactBuilder;
use Gedcomx\Tests\NoteBuilder;

class ChildAndParentsRelationshipStateTest extends ApiTestCase
{
    private $states;

    
    

    

    /**
     * @link https://familysearch.org/developers/docs/api/tree/Create_Child-and-Parents_Relationship_Note_usecase
     */
    public function testCreateChildAndParentsRelationshipNote()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        /** @var FamilyTreeCollectionState $collection */
        $this->collectionState($factory);

        $relation = $this->createRelationship();
        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $relation->getResponse(), $this->buildFailMessage(__METHOD__, $relation));

        $note = NoteBuilder::createNote();
        $noteState = $relation->addNote($note);
        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $noteState->getResponse(), $this->buildFailMessage(__METHOD__, $noteState));

        $this->cleanup();
    }

    

    /**
     * @link https://familysearch.org/developers/docs/api/tree/Read_Child-and-Parents_Relationship_Notes_usecase
     */
    public function testReadChildAndParentsRelationshipNotes()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        /** @var FamilyTreeCollectionState $collection */
        $this->collectionState($factory);

        /** @var ChildAndParentsRelationshipState $relation */
        $relation = $this->createRelationship();

        $note = NoteBuilder::createNote();
        $noteState = $relation->addNote($note);
        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $noteState->getResponse(), $this->buildFailMessage(__METHOD__, $noteState));

        $relation = $relation->get();
        $relation->loadNotes();
        $this->assertNotEmpty($relation->getRelationship()->getNotes());

        $this->cleanup();
    }

    /**
     * @link https://familysearch.org/developers/docs/api/tree/Read_Child-and-Parents_Relationship_Note_usecase
     */
    public function testReadChildAndParentsRelationshipNote()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        $this->collectionState($factory);

        /** @var ChildAndParentsRelationshipState $relation */
        $relation = $this->createRelationship();
        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $relation->getResponse());
        $note = NoteBuilder::createNote();
        $relation->addNote($note);

        $relation = $relation->get()->loadNotes();
        $notes = $relation->getRelationship()->getNotes();
        $noted = $relation->readNote($notes[0]);
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $noted->getResponse(), $this->buildFailMessage(__METHOD__."(addSpouse)", $noted));

        $this->cleanup();
    }

    
    /**
     * @link https://familysearch.org/developers/docs/api/tree/Update_Child-and-Parents_Relationship_Note_usecase
     */
    public function testUpdateChildAndParentsRelationshipNote()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        $this->collectionState($factory);

        /** @var ChildAndParentsRelationshipState $relation */
        $relation = $this->createRelationship();
        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $relation->getResponse());
        $note = NoteBuilder::createNote();
        $relation->addNote($note);

        $relation = $relation->get()->loadNotes();
        $notes = $relation->getRelationship()->getNotes();
        $notes[0]->setText($this->faker->sentence(12));
        $noted = $relation->updateNote($notes[0]);
        $this->assertAttributeEquals(HttpStatus::NO_CONTENT, "statusCode", $noted->getResponse(), $this->buildFailMessage(__METHOD__."(addSpouse)", $noted));

        $this->cleanup();
    }


    /**
     * @link https://familysearch.org/developers/docs/api/tree/Delete_Child-and-Parents_Relationship_Note_usecase
     */
    public function testDeleteChildAndParentRelationshipNote()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        $this->collectionState($factory);

        /** @var ChildAndParentsRelationshipState $relation */
        $relation = $this->createRelationship();
        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $relation->getResponse());
        $note = NoteBuilder::createNote();
        $relation->addNote($note);

        $relation = $relation->get()->loadNotes();
        $notes = $relation->getRelationship()->getNotes();
        $noted = $relation->deleteNote($notes[0]);
        $this->assertAttributeEquals(HttpStatus::NO_CONTENT, "statusCode", $noted->getResponse(), $this->buildFailMessage(__METHOD__, $noted));

        $this->cleanup();
    }
    
    /**
     * @link https://familysearch.org/developers/docs/api/tree/Create_Child-and-Parents_Relationship_usecase
     */
    public function testCreateChildAndParentsRelationship()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        /** @var FamilyTreeCollectionState $collection */
        $this->collectionState($factory);
        /** @var ChildAndParentsRelationshipState $relation */
        $relation = $this->createRelationship();

        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $relation->getResponse(), $this->buildFailMessage(__METHOD__, $relation));

        $this->cleanup();
    }

    /**
     * @link https://familysearch.org/developers/docs/api/tree/Create_Child-and-Parents_Relationship_Source_Reference_usecase
     */
    public function testCreateChildAndParentsRelationshipSourceReferences()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        /** @var FamilyTreeCollectionState $collection */
        $this->collectionState($factory);

        /** @var ChildAndParentsRelationshipState $relation */
        $relation = $this->createRelationship();
        $sourceState = $this->createSource();
        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $sourceState->getResponse() );

        $reference = new SourceReference();
        $reference->setDescriptionRef($sourceState->getSelfUri());
        $reference->setAttribution( new Attribution( array(
            "changeMessage" => $this->faker->sentence(6)
        )));
        $newState = $relation->addSourceReference($reference);
        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $relation->getResponse(), $this->buildFailMessage(__METHOD__, $relation));

        $sourceState->delete();
        $this->cleanup();
    }

    /**
     * @link https://familysearch.org/developers/docs/api/tree/Create_Child-and-Parents_Relationship_Conclusion_usecase
     */
    public function testCreateChildAndParentsRelationshipConclusion()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        /** @var FamilyTreeCollectionState $collection */
        $this->collectionState($factory);

        /** @var ChildAndParentsRelationshipState $relation */
        $relation = $this->createRelationship();

        $fact = FactBuilder::adoptiveParent();
        $factState = $relation->addFatherFact($fact);
        $this->assertAttributeEquals(HttpStatus::NO_CONTENT, "statusCode", $factState->getResponse(), $this->buildFailMessage(__METHOD__, $factState));

        $this->cleanup();
    }

    
    /**
     * @link https://familysearch.org/developers/docs/api/tree/Read_Child-and-Parents_Relationship_usecase
     */
    public function testReadChildAndParentsRelationship()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        $this->collectionState($factory);

        $relation = $this->createRelationship();
        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $relation->getResponse(), $this->buildFailMessage(__METHOD__, $relation) );

        /** @var ChildAndParentsRelationshipState $relation */
        $relation = $relation->get();
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $relation->getResponse(), $this->buildFailMessage(__METHOD__, $relation) );

        /** @var ChildAndParentsRelationship $entity */
        $entity = $relation->getRelationship();
        $data_check = $entity->getFather() instanceof ResourceReference
            && $entity->getMother() instanceof ResourceReference
            && $entity->getChild() instanceof ResourceReference;
        $this->assertTrue( $data_check );

        $this->cleanup();
    }

    /**
     * @link https://familysearch.org/developers/docs/api/tree/Read_Child-and-Parents_Relationship_Source_References_usecase
     * @link https://familysearch.org/developers/docs/api/tree/Read_Child-and-Parents_Relationship_Sources_usecase
     */
    public function testReadChildAndParentsRelationshipSourceReferences()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        /** @var FamilyTreeCollectionState $collection */
        $this->collectionState($factory);

        /** @var ChildAndParentsRelationshipState $relation */
        $relation = $this->createRelationship();
        $sourceState = $this->createSource();

        $reference = new SourceReference();
        $reference->setDescriptionRef($sourceState->getSelfUri());
        $reference->setAttribution( new Attribution( array(
            "changeMessage" => $this->faker->sentence(6)
        )));
        $relation->addSourceReference($reference);

        $relation = $relation->get();
        $relation->loadSourceReferences();
        $this->assertNotEmpty($relation->getRelationship()->getSources());

        $sourceState->delete();
        $this->cleanup();
    }

    /**
     * @link https://familysearch.org/developers/docs/api/tree/Update_Child-and-Parents_Relationship_usecase
     */
    public function testUpdateChildAndParentRelationship()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        $this->collectionState($factory);

        $relation = $this->createRelationship();
        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $relation->getResponse(), $this->buildFailMessage(__METHOD__."(create)", $relation) );

        $relation = $relation->get();
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $relation->getResponse(), $this->buildFailMessage(__METHOD__."(read)", $relation) );

        $mother = $this->createPerson('female')->get();
        $updated = $relation->updateMotherWithPersonState($mother);
        $this->assertAttributeEquals(HttpStatus::NO_CONTENT, "statusCode", $updated->getResponse(), $this->buildFailMessage(__METHOD__."(update)", $updated) );

        $this->cleanup();
    }

    /**
     * @link https://familysearch.org/developers/docs/api/tree/Update_Child-and-Parents_Relationship_Conclusion_usecase
     * @link https://familysearch.org/developers/docs/api/tree/Restore_Child-and-Parents_Relationship_usecase
     */
    public function testDeleteAndRestoreChildAndParentsRelationship()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        $this->collectionState($factory);

        /** @var ChildAndParentsRelationshipState $relation */
        $relation = $this->createRelationship();
        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $relation->getResponse(), $this->buildFailMessage(__METHOD__."(create)", $relation));

        /* DELETE */
        $deleted = $relation->delete();
        $this->assertAttributeEquals(HttpStatus::NO_CONTENT, "statusCode", $deleted->getResponse(), $this->buildFailMessage(__METHOD__."(delete)", $deleted));

        $missing = $deleted->get();
        $this->assertAttributeEquals(HttpStatus::GONE, "statusCode", $missing->getResponse(), $this->buildFailMessage(__METHOD__."(read)", $missing));

        $restored = $missing->restore();
        $this->assertAttributeEquals(HttpStatus::NO_CONTENT, "statusCode", $restored->getResponse(), $this->buildFailMessage(__METHOD__."(restore)", $restored));

        $this->cleanup();
    }

    /**
     * @link https://familysearch.org/developers/docs/api/tree/Delete_Child-and-Parents_Relationship_Parent_usecase
     */
    public function testDeleteParentFromRelationship()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        $this->collectionState($factory);

        /** @var ChildAndParentsRelationshipState $relation */
        $relation = $this->createRelationship();
        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $relation->getResponse(), $this->buildFailMessage(__METHOD__."(create)", $relation) );

        $relation = $relation->get();
        $updated = $relation->deleteFather();
        $this->assertAttributeEquals(HttpStatus::NO_CONTENT, "statusCode", $updated->getResponse(), $this->buildFailMessage(__METHOD__."(update)", $updated) );

        $relation = $relation->get();
        $this->assertEmpty($relation->getRelationship()->getFather(), "Father should have been deleted" );

        $this->cleanup();
    }

    /**
     * @link https://familysearch.org/developers/docs/api/tree/Update_Child-and-Parents_Relationship_Conclusion_usecase
     */
    public function testUpdateChildAndParentsRelationshipConclusion()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        $this->collectionState($factory);

        /** @var ChildAndParentsRelationshipState $relation */
        $relation = $this->createRelationship();
        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $relation->getResponse(), $this->buildFailMessage(__METHOD__."(create)", $relation));

        $fact = FactBuilder::adoptiveParent();
        $relation = $relation->addFatherFact($fact);
        $this->assertAttributeEquals(HttpStatus::NO_CONTENT, "statusCode", $relation->getResponse(), $this->buildFailMessage(__METHOD__."(addFact)", $relation));

        $relation = $relation->get();
        $facts = $relation->getRelationship()->getFatherFacts();
        $factState = $relation->updateFatherFact($facts[0]);
        $this->assertAttributeEquals(HttpStatus::NO_CONTENT, "statusCode", $factState->getResponse(), $this->buildFailMessage(__METHOD__."(updateFact)", $factState));

        $this->cleanup();
    }

    /**
     * @link https://familysearch.org/developers/docs/api/tree/Delete_Child-and-Parents_Relationship_Conclusion_usecase
     */
    public function testDeleteChildAndParentsRelationshipConclusion(){
        
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        $this->collectionState($factory);

        /** @var ChildAndParentsRelationshipState $relation */
        $relation = $this->createRelationship();
        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $relation->getResponse(), $this->buildFailMessage(__METHOD__."(create)", $relation));

        $fact = FactBuilder::adoptiveParent();
        $relation = $relation->addFatherFact($fact);
        $this->assertAttributeEquals(HttpStatus::NO_CONTENT, "statusCode", $relation->getResponse(), $this->buildFailMessage(__METHOD__."(addFact)", $relation));

        $relation = $relation->get();
        $facts = $relation->getRelationship()->getFatherFacts();
        $factState = $relation->deleteFact($facts[0]);
        $this->assertAttributeEquals(HttpStatus::NO_CONTENT, "statusCode", $factState->getResponse(), $this->buildFailMessage(__METHOD__."(updateFact)", $factState));

        $this->cleanup();
    }
    /**
     * @link https://familysearch.org/developers/docs/api/tree/Delete_Child-and-Parents_Relationship_Source_Reference_usecase
     */
    public function testDeleteChildAndParentsRelationshipSourceReference()
    {
        //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        /** @var FamilyTreeCollectionState $collection */
        $this->collectionState($factory);

        /** @var ChildAndParentsRelationshipState $relation */
        $relation = $this->createRelationship();
        $sourceState = $this->createSource();

        $reference = new SourceReference();
        $reference->setDescriptionRef($sourceState->getSelfUri());
        $reference->setAttribution( new Attribution( array(
            "changeMessage" => $this->faker->sentence(6)
        )));
        $relation->addSourceReference($reference);

        $relation = $relation->get();
        $relation->loadSourceReferences();
        $sources = $relation->getRelationship()->getSources();
        $deleted = $relation->deleteSourceReference($sources[0]);
        $this->assertAttributeEquals(HttpStatus::NO_CONTENT, "statusCode", $deleted->getResponse(), $this->buildFailMessage(__METHOD__."(updateFact)", $deleted));

        $sourceState->delete();
        $this->cleanup();
    }


    /**
     * @return \Gedcomx\Extensions\FamilySearch\Rs\Client\FamilyTree\ChildAndParentsRelationshipState
     * @throws \Gedcomx\Rs\Client\Exception\GedcomxApplicationException
     */
    private function createRelationship()
    {
        $father = $this->createPerson('male')->get();
        $mother = $this->createPerson('female')->get();
        $child = $this->createPerson()->get();

        $rel = new ChildAndParentsRelationship();
        $rel->setChild($child->getResourceReference());
        $rel->setFather($father->getResourceReference());
        $rel->setMother($mother->getResourceReference());

        $rState = $this->collectionState()->addChildAndParentsRelationship($rel);

        $this->states[] = $father;
        $this->states[] = $child;
        $this->states[] = $mother;
        $this->states[] = $rState;

        return $rState;
    }

    private function cleanup(){
        foreach ($this->states as $s ){
            $s->delete();
        }
    }
}
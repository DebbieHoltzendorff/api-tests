<?php


namespace Gedcomx\ApiTests\Rs\Client;


use Gedcomx\Common\ResourceReference;
use Gedcomx\Extensions\FamilySearch\Platform\Tree\ChildAndParentsRelationship;
use Gedcomx\Extensions\FamilySearch\Rs\Client\FamilyTree\FamilyTreeStateFactory;
use Gedcomx\Rs\Client\Util\HttpStatus;
use Gedcomx\Tests\ApiTestCase;
use Gedcomx\Tests\FactBuilder;

class RelationshipsStateTest extends ApiTestCase {

	/**
	 * @link https://familysearch.org/developers/docs/api/tree/Create_Couple_Relationship_usecase
	 */
	public function testCreateCoupleRelationship()
	{
            //QA Reviewed
        $factory = new FamilyTreeStateFactory();
        $this->collectionState($factory);

        $person1 = $this->createPerson('male')->get();
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $person1->getResponse());
        $person2 = $this->createPerson('female')->get();
        $this->assertAttributeEquals(HttpStatus::OK, "statusCode", $person2->getResponse());
        

        $relation = $this->collectionState()->addSpouseRelationship($person1, $person2);

        $this->assertAttributeEquals(HttpStatus::CREATED, "statusCode", $relation->getResponse(), $this->buildFailMessage(__METHOD__, $relation));

        $relation = $relation->get();
        $entity = $relation->getRelationship();

        $data_check = $entity->getPerson1() instanceof ResourceReference
                          && $entity->getPerson2() instanceof ResourceReference;
        $this->assertTrue( $data_check );
        
        $person1->delete();
        $person2->delete();
	}

   
} 
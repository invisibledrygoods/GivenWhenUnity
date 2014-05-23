GivenWhenUnity
==============

A Test Driven Development tool for Unity3D. You can use this if you're sick of manually verifying things, or changing one thing on one side of your code and one month later discovering that something on the other side silently broke.

Why Write Tests
---------------

I had a whole thing about the philosophy of test driven development here, [but links to other peoples blogs do a better job of that](http://blog.orfjackal.net/2009/10/tdd-is-not-test-first-tdd-is-specify.html), so instead just take a look at these two bad boys for convincing on how tests can help games:

####Combat Balance

    Given("there is an AI monk")
      .And("there is an AI barbarian")
      .When("the monk engages the barbarian")
      .ThenWithin("20 seconds", "the barbarian should be dead")
      .AndWithin("20 seconds", "the monk should have less than 30 percent hitpoints")
      .Because("monks are only slightly buffer than barbarians but fights still shouldn't drag out");

####Crowd Collision

    Given("there are 10 units to the north")
      .And("there are 10 units to the south")
      .When("the units to the north are told to move south")
      .And("the units to the south are told to move north")
      .ThenWithin("20 seconds", "the units to the north should all make it to the south")
      .AndWithin("20 seconds", "the units to the south should all make it to the north")
      .Because("pathfinding should be able to deal with crowd collision");

And I should probably tear down the most common misconception while I'm still on my soapbox: your tests aren't a safety net, they're a self-verifying design document. Write them with your game designer, or even better, let your designer write them herself.

How It Works
------------

Making a class that extends TestBehaviour is enough to run the test, clicking run or finishing compile with "run after compile" checked will build a test scene and use reflection to execute every `TestBehaviour` in the codebase.

All test descriptions go inside of a `public override void Spec()` method, and are specified in a Given/When/Then format similar to [cucumber](http://cukes.info/).

To write the code behind a specification, make a `public void` method by snake-casing the specification string. Numbers and single quoted strings in the specification need to be replaced by two underscores in the method name and supplied as arguments to the method in their original order.

Test output will be color coded yellow if the test method does not exist or a prior test failed, red if the test threw an error, and green if the test passed.

The `Because` specification is only used by output but every test should be backed by a feature and every feature needs a reason to exist so fill that in here.

Real Life Example
-----------------

    using UnityEngine;
    using System.Collections;
    using Require;
    using Shouldly;
    using System;
    
    public class DealsPointsInCollisionTest : TestBehaviour
    {
        DealsPointsInCollision it;
        GameObject butthead;
    
        public override void Spec()
        {
            Given("it deals 3 'damage' in collisions")
                .And("a butthead with 5 hp is nearby")
                .When("it collides with the butthead")
                .ThenWithin("3 frames", "the butthead should have 2 hp")
                .Because("it should follow basic expected behaviour");
    
            Given("it deals 1 'damage' in collisions")
                .And("it is destroyed after dealing")
                .And("a butthead with 5 hp is nearby")
                .When("it collides with the butthead")
                .ThenWithin("3 frames", "it should be destroyed")
                .Because("it should be destroyed after dealing when destroyAfterDealing is true");
    
            Given("it deals 1 'damage' in collisions")
                .And("it is deactivated after dealing")
                .And("a butthead with 5 hp is nearby")
                .When("it collides with the butthead")
                .ThenWithin("3 frames", "it should be deactivated")
                .Because("it should be deactivated after dealing if deactivateAfterDealing is true");
        }
    
        public void ItDeals____InCollisions(float amount, string source)
        {
            it = new GameObject().transform.Require<DealsPointsInCollision>();
            it.transform.position = transform.position;
            it.transform.Require<BoxCollider>().isTrigger = true;
            it.transform.Require<Rigidbody>().isKinematic = true;
            it.source = source;
            it.amount = amount;
        }
    
        public void ItIsDestroyedAfterDealing()
        {
            it.afterDealing = DealsPointsInCollision.AfterDealing.Destroy;
        }
    
        public void ItIsDeactivatedAfterDealing()
        {
            it.afterDealing = DealsPointsInCollision.AfterDealing.Deactivate;
        }
    
        public void AButtheadWith__HpIsNearby(float hp)
        {
            butthead = new GameObject();
            butthead.transform.Require<BoxCollider>().isTrigger = true;
            butthead.transform.Require<Rigidbody>().isKinematic = true;
            butthead.transform.position = transform.position + Vector3.left * 5;
    
            HasPoints buttheadPoints = butthead.transform.Require<HasPoints>();
            buttheadPoints.Set("hp", hp);
            buttheadPoints.SetModifier("hp", "damage", -1);
        }
    
        public void ItCollidesWithTheButthead()
        {
            butthead.transform.position = transform.position;
        }
    
        public void TheButtheadShouldHave__Hp(float hp)
        {
            butthead.transform.Require<HasPoints>().Get("hp").ShouldBe(hp);
        }
    
        public void ItShouldBeDestroyed()
        {
            if (it != null)
            {
                throw new Exception("Expected it to be destroyed but it wasn't");
            }
        }
    
        public void ItShouldBeDeactivated()
        {
            it.gameObject.activeSelf.ShouldBe(false);
        }
    }

Physics
-------

Because these tests operate on realtime physics they do some weird things.

They all have to run in the same physics scene thanks to unity, and they all operate in realtime so running serially is impractical. To get around this, they all run in parallel at random locations 100 meters apart. If you want a safe place to put stuff in the physics space that won't bump into other things, use offsets from your `TestBehaviour`'s `transform.position`.

Also avoid the crap out of finding all of something in the scene, you should be avoiding the crap out of that anyway. If you need to do some kind of search, do it inside your `TestBehaviour`'s children.

In Progress
-----------

 - Stochastic tests would be nice for probabilistic scenarios. Something like "this test should pass more than 90 out of 100 tries"
